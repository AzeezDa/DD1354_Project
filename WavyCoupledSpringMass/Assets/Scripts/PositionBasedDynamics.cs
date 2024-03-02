using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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

            Vector3 bodyToBodyVector;

            if (i == 0)
            {
                bodyToBodyVector = bodies[0].Position - body2.Position;
            }
            else if (i == amountOfBodies)
            {
                bodyToBodyVector = body1.Position - bodies[amountOfBodies + 1].Position;
            }
            else
            {
                bodyToBodyVector = body1.Position - body2.Position;
            }

            float bodyToBodyDistance = bodyToBodyVector.magnitude;

            if (i > 0)
            {
                Vector3 deltaEstimate1 = -0.5f * springStiffness * (bodyToBodyDistance - restLength) * bodyToBodyVector.normalized;
                estimatedPositions[i - 1] += deltaEstimate1;
            }
            if (i < amountOfBodies)
            {
                Vector3 deltaEstimate2 = 0.5f * springStiffness * (bodyToBodyDistance - restLength) * bodyToBodyVector.normalized;
                estimatedPositions[i] += deltaEstimate2;
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
        Assert.IsTrue(0 < i && i < amountOfBodies + 1);
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
