using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ForceBasedDynamics
{
    readonly int amountOfBodies;
    float gravitionalAcceleration;
    float omegaSquared;
    float dampingFactor;
    float yDrivingAmplitude;
    float yDrivingAngularFrequency;
    readonly float restLength;
    readonly Body[] bodies;

    public ForceBasedDynamics(
        int amountOfBodies,
        float gravitionalAcceleration,
        float omegaSquared,
        float dampingFactor,
        float yDrivingAmplitude,
        float yDrivingAngularFrequency,
        float restLength,
        Body[] bodies)
    {

        this.amountOfBodies = amountOfBodies;
        this.gravitionalAcceleration = gravitionalAcceleration;
        this.omegaSquared = omegaSquared;
        this.dampingFactor = dampingFactor;
        this.yDrivingAmplitude = yDrivingAmplitude;
        this.yDrivingAngularFrequency = yDrivingAngularFrequency;
        this.restLength = restLength;
        this.bodies = bodies;
    }

    public void Step()
    {
        float dt = Time.fixedDeltaTime;
        float halfdt = 0.5f * dt; // Used for Runge-Kutta 4

        for (int i = 1; i < amountOfBodies + 1; i++)
        {
            Body body = bodies[i];

            // Runge-Kutta 4
            Vector3 v1 = body.Velocity;
            Vector3 a1 = GetAccelerationFor(i, 0, body.Position, v1);

            Vector3 v2 = v1 + halfdt * a1;
            Vector3 a2 = GetAccelerationFor(i, halfdt, body.Position + halfdt * v1, v1);

            Vector3 v3 = v2 + halfdt * a2;
            Vector3 a3 = GetAccelerationFor(i, halfdt, body.Position + halfdt * v2, v2);

            Vector3 v4 = v3 + halfdt * a3;
            Vector3 a4 = GetAccelerationFor(i, dt, body.Position + dt * v3, v3);

            Vector3 velocity = (v1 + v2 + v2 + v3 + v3 + v4) / 6f;
            Vector3 acceleration = (a1 + a2 + a2 + a3 + a3 + a4) / 6f;

            body.Position += velocity * dt;
            body.Velocity += acceleration * dt;
        }
    }

    Vector3 GetAccelerationFor(int i, float dt, Vector3 position, Vector3 velocity)
    {
        Assert.IsTrue(0 < i && i < amountOfBodies + 1);

        // Start with only gravitional force
        Vector3 acceleration = Vector3.down * gravitionalAcceleration;

        Body left = bodies[i - 1];
        Body right = bodies[i + 1];

        // Spring Forces
        Vector3 deltaToLeft = left.Position - position;
        Vector3 deltaToRight = right.Position - position;
        float extensionLeft = deltaToLeft.magnitude - restLength;
        float extensionRight = deltaToRight.magnitude - restLength;
        acceleration += omegaSquared * extensionLeft * deltaToLeft.normalized;
        acceleration += omegaSquared * extensionRight * deltaToRight.normalized;

        // Drag Force
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
        float gravitionalAcceleration,
        float omegaSquared,
        float dampingFactor,
        float yDrivingAmplitude,
        float yDrivingAngularFrequency)
    {
        this.gravitionalAcceleration = gravitionalAcceleration;
        this.omegaSquared = omegaSquared;
        this.dampingFactor = dampingFactor;
        this.yDrivingAmplitude = yDrivingAmplitude;
        this.yDrivingAngularFrequency = yDrivingAngularFrequency;
    }
}
