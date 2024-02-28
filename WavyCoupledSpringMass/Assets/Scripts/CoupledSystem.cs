using UnityEngine;

public class CoupledSystem : MonoBehaviour
{

    [Header("System Information")]
    [SerializeField]
    Body bodyPrefab;

    [SerializeField]
    Spring springPrefab;

    [SerializeField]
    [Min(2)]
    int amountOfBodies = 10;

    [SerializeField]
    Transform startPoint;

    [SerializeField]
    Transform endPoint;

    [Header("Physical Properties")]
    [SerializeField]
    float gravitionalAcceleration = 9.81f;

    [SerializeField]
    [Min(0)]
    float omegaSquared = 1f;

    [SerializeField]
    [Min(0)]
    float gamma = 1f;

    [Header("Driving Force in X Direction")]
    [SerializeField]
    [Min(0)]
    float xDrivingAmplitude = 0f;

    [SerializeField]
    [Min(0)]
    float xDrivingAngularFrequency = 0f;

    [Header("Driving Force in Y Direction")]
    [SerializeField]
    [Min(0)]
    float yDrivingAmplitude = 0f;

    [SerializeField]
    [Min(0)]
    float yDrivingAngularFrequency = 0f;

    [Header("Driving Force in Z Direction")]
    [SerializeField]
    [Min(0)]
    float zDrivingAmplitude = 0f;

    [SerializeField]
    [Min(0)]
    float zDrivingAngularFrequency = 0f;

    float restLength;
    Body[] bodies;
    Spring[] springs;

    // Start is called before the first frame update
    void Start()
    {
        // Set up bodies and strings
        bodies = new Body[amountOfBodies];
        springs = new Spring[amountOfBodies + 1];

        // Vector from start to end points
        Vector3 vectorAlongSystem = endPoint.position - startPoint.position;
        Vector3 systemDirection = vectorAlongSystem.normalized;

        // Divide the entire span. The restlength is the size of each division
        float offset = vectorAlongSystem.magnitude / (amountOfBodies + 1);

        float restLengthFactor = 0.5f; // Lower value means more tension 

        restLength = restLengthFactor * offset;

        Vector3 bodiesStartPosition = startPoint.position + systemDirection * offset;

        // Add bodies with given offset
        for (int i = 0; i < amountOfBodies; i++)
        {
            Vector3 position = bodiesStartPosition + i * offset * systemDirection;
            Body body = Instantiate(bodyPrefab, position, Quaternion.identity);
            body.Position = position;
            bodies[i] = body;
        }

        // Set up springs at start and end points
        springs[0] = Instantiate(springPrefab);
        springs[0].SetConnectionPoints(startPoint, bodies[0].transform);
        springs[amountOfBodies] = Instantiate(springPrefab);
        springs[amountOfBodies].SetConnectionPoints(bodies[amountOfBodies - 1].transform, endPoint);

        // Add other springs
        for (int i = 1; i < amountOfBodies; i++)
        {
            Spring spring = Instantiate(springPrefab);
            spring.SetConnectionPoints(bodies[i - 1].transform, bodies[i].transform);
            springs[i] = spring;
        }
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        float halfdt = 0.5f * dt; // Used for Runge-Kutta 4

        for (int i = 0; i < amountOfBodies; i++)
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
        // Start with only gravitional force
        Vector3 acceleration = Vector3.down * gravitionalAcceleration;

        Vector3 positionLeft;
        Vector3 positionRight;

        // Get positions of left and right points connected to the body
        if (i == 0) // Start point
        {
            positionLeft = startPoint.position;
            positionRight = bodies[i + 1].Position;
        }
        else if (i == amountOfBodies - 1) // End point
        {
            positionRight = endPoint.position;
            positionLeft = bodies[i - 1].Position;
        }
        else // Any other point
        {
            positionLeft = bodies[i - 1].Position;
            positionRight = bodies[i + 1].Position;
        }

        // Spring Forces
        Vector3 deltaToLeft = positionLeft - position;
        Vector3 deltaToRight = positionRight - position;
        float extensionLeft = deltaToLeft.magnitude - restLength;
        float extensionRight = deltaToRight.magnitude - restLength;
        acceleration += omegaSquared * extensionLeft * deltaToLeft.normalized;
        acceleration += omegaSquared * extensionRight * deltaToRight.normalized;

        // Drag Force
        acceleration += -gamma * velocity;

        // Driving Force (Only on the first body)
        if (i == 0)
        {
            float oscillationX = Mathf.Cos(xDrivingAngularFrequency * (Time.fixedTime + dt));
            acceleration += xDrivingAmplitude * oscillationX * Vector3.left;

            float oscillationY = Mathf.Cos(yDrivingAngularFrequency * (Time.fixedTime + dt));
            acceleration += yDrivingAmplitude * oscillationY * Vector3.up;

            float oscillationZ = Mathf.Cos(zDrivingAngularFrequency * (Time.fixedTime + dt));
            acceleration += zDrivingAmplitude * oscillationZ * Vector3.forward;
        }

        return acceleration;
    }
}
