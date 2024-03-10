using UnityEngine;

public class PositionBasedDynamics
{
    readonly Body[] bodies;
    readonly int amountOfBodies;
    readonly Vector3[] estimatedPositions;
    float springStiffness;
    readonly float restLength;
    float dampingFactor;
    float gravitionalAcceleration;
    float yDrivingAmplitude;
    float yDrivingAngularFrequency;

    public PositionBasedDynamics(
        Body[] bodies,
        int amountOfBodies,
        float springStiffness,
        float restLength,
        float dampingFactor,
        float gravitionalAcceleration,
        float yDrivingAmplitude,
        float yDrivingAngularFrequency
    )
    {
        this.bodies = bodies;
        this.amountOfBodies = amountOfBodies;
        this.springStiffness = springStiffness;
        this.restLength = restLength;
        this.dampingFactor = dampingFactor;
        this.gravitionalAcceleration = gravitionalAcceleration;
        this.yDrivingAmplitude = yDrivingAmplitude;
        this.yDrivingAngularFrequency = yDrivingAngularFrequency;

        estimatedPositions = new Vector3[amountOfBodies];

    }

    public void Step()
    {
        float dt = Time.fixedDeltaTime;

        // Apply External Forces, Damping, and calculate estimated positions
        for (int i = 1; i < amountOfBodies + 1; i++)
        {
            Body body = bodies[i];
            body.Velocity += dt * GetAccelerationFor(i);
            body.Velocity *= dampingFactor;
            estimatedPositions[i - 1] = body.Position + dt * bodies[i].Velocity;
        }

        // Optimize Estimations
        for (int i = 0; i < amountOfBodies + 1; i++)
        {
            Body body1 = bodies[i];
            Body body2 = bodies[i + 1];

            Vector3 bodyToBodyVector = body1.Position - body2.Position;
            float bodyToBodyDistance = bodyToBodyVector.magnitude;

            Vector3 deltaEstimate = -0.5f * springStiffness * (bodyToBodyDistance - restLength) * bodyToBodyVector.normalized;
            if (i > 0)
            {
                estimatedPositions[i - 1] += deltaEstimate;
            }
            if (i < amountOfBodies)
            {
                estimatedPositions[i] += -deltaEstimate;
            }
        }

        for (int i = 1; i < amountOfBodies + 1; i++)
        {
            Body body = bodies[i];

            body.Velocity = (estimatedPositions[i - 1] - body.Position) / dt;
            body.Position = estimatedPositions[i - 1];
        }

    }

    Vector3 GetAccelerationFor(int i)
    {
        Vector3 acceleration = gravitionalAcceleration * Vector3.down;

        // Driving Force (Only on the first body)
        if (i == 1)
        {
            float oscillationY = Mathf.Cos(yDrivingAngularFrequency * (Time.fixedTime + Time.fixedDeltaTime));
            acceleration += yDrivingAmplitude * oscillationY * Vector3.up;
        }

        return acceleration;
    }

    public void UpdateParameters(
        float gravitionalAcceleration,
        float springStiffness,
        float dampingFactor,
        float yDrivingAmplitude,
        float yDrivingAngularFrequency
    )
    {
        this.gravitionalAcceleration = gravitionalAcceleration;
        this.springStiffness = springStiffness;
        this.dampingFactor = dampingFactor;
        this.yDrivingAmplitude = yDrivingAmplitude;
        this.yDrivingAngularFrequency = yDrivingAngularFrequency;
    }
}
