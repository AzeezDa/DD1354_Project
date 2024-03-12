using UnityEngine;

public enum Integrator
{
    RungeKutta4,
    EulerForward
}

public class ForceBasedDynamics
{
    readonly int amountOfBodies;
    float gravitationalAcceleration;
    float omegaSquared;
    float dampingFactor;
    float yDrivingAmplitude;
    float yDrivingAngularFrequency;
    readonly float restLength;
    readonly Body[] bodies;
    readonly Vector3[] deltaPositions;
    Integrator integrator;

    public ForceBasedDynamics(
        int amountOfBodies,
        float gravitationalAcceleration,
        float omegaSquared,
        float dampingFactor,
        float yDrivingAmplitude,
        float yDrivingAngularFrequency,
        float restLength,
        Body[] bodies,
        Integrator integrator)
    {

        this.amountOfBodies = amountOfBodies;
        this.gravitationalAcceleration = gravitationalAcceleration;
        this.omegaSquared = omegaSquared;
        this.dampingFactor = dampingFactor;
        this.yDrivingAmplitude = yDrivingAmplitude;
        this.yDrivingAngularFrequency = yDrivingAngularFrequency;
        this.restLength = restLength;
        this.bodies = bodies;
        this.integrator = integrator;
        deltaPositions = new Vector3[amountOfBodies];
    }

    public void Step()
    {
        for (int i = 1; i < amountOfBodies + 1; i++)
        {
            Body body = bodies[i];

            Vector3 deltaPosition = Vector3.zero;
            Vector3 deltaVelocity = Vector3.zero;

            switch (integrator)
            {
                case Integrator.EulerForward:
                    EulerForwardStep(i, out deltaPosition, out deltaVelocity);
                    break;
                case Integrator.RungeKutta4:
                    RK4Step(i, out deltaPosition, out deltaVelocity);
                    break;
            }

            deltaPositions[i - 1] = deltaPosition;
            body.Velocity += deltaVelocity;
        }

        for (int i = 1; i < amountOfBodies + 1; i++)
        {
            bodies[i].Position += deltaPositions[i - 1];
        }
    }

    Vector3 GetAccelerationOnBody(int i, float dt, Vector3 position, Vector3 velocity)
    {
        // Start with only gravitational force
        Vector3 acceleration = Vector3.down * gravitationalAcceleration;

        Body left = bodies[i - 1];
        Body right = bodies[i + 1];

        // Spring Forces
        Vector3 deltaToLeft = left.Position - position;
        Vector3 deltaToRight = right.Position - position;
        float extensionLeft = deltaToLeft.magnitude - restLength;
        float extensionRight = deltaToRight.magnitude - restLength;
        acceleration += omegaSquared * extensionLeft * deltaToLeft.normalized;
        acceleration += omegaSquared * extensionRight * deltaToRight.normalized;

        // Damping Force
        acceleration += -dampingFactor * velocity;

        // Driving Force (Only on the first body)
        if (i == 1)
        {
            float oscillationY = Mathf.Cos(yDrivingAngularFrequency * (Time.fixedTime + dt));
            acceleration += yDrivingAmplitude * oscillationY * Vector3.up;
        }

        return acceleration;
    }

    public void UpdateParameters(
        float gravitationalAcceleration,
        float omegaSquared,
        float dampingFactor,
        float yDrivingAmplitude,
        float yDrivingAngularFrequency,
        Integrator integrator)
    {
        this.gravitationalAcceleration = gravitationalAcceleration;
        this.omegaSquared = omegaSquared;
        this.dampingFactor = dampingFactor;
        this.yDrivingAmplitude = yDrivingAmplitude;
        this.yDrivingAngularFrequency = yDrivingAngularFrequency;
        this.integrator = integrator;
    }

    void RK4Step(int i, out Vector3 deltaPosition, out Vector3 deltaVelocity)
    {
        Body body = bodies[i];

        float dt = Time.fixedDeltaTime;
        float halfdt = 0.5f * dt; // Used for Runge-Kutta 4

        Vector3 v1 = body.Velocity;
        Vector3 a1 = GetAccelerationOnBody(i, 0, body.Position, v1);

        Vector3 v2 = v1 + halfdt * a1;
        Vector3 a2 = GetAccelerationOnBody(i, halfdt, body.Position + halfdt * v1, v2);

        Vector3 v3 = v2 + halfdt * a2;
        Vector3 a3 = GetAccelerationOnBody(i, halfdt, body.Position + halfdt * v2, v3);

        Vector3 v4 = v3 + dt * a3;
        Vector3 a4 = GetAccelerationOnBody(i, dt, body.Position + dt * v3, v4);

        deltaPosition = dt / 6f * (v1 + v2 + v2 + v3 + v3 + v4);
        deltaVelocity = dt / 6f * (a1 + a2 + a2 + a3 + a3 + a4);
    }

    void EulerForwardStep(int i, out Vector3 deltaPosition, out Vector3 deltaVelocity)
    {
        float dt = Time.fixedDeltaTime;

        Body body = bodies[i];

        deltaPosition = dt * body.Velocity;
        deltaVelocity = dt * GetAccelerationOnBody(i, dt, body.Position, body.Velocity);
    }
}
