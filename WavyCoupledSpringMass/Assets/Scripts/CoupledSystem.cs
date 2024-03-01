using UnityEngine;
using UnityEngine.Assertions;

enum DynamicsType
{
    ForceBasedDynamics,
    PositionBasedDynamics
}

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
    Wall startPoint;

    [SerializeField]
    Wall endPoint;

    [SerializeField]
    DynamicsType dynamicsType = DynamicsType.ForceBasedDynamics;

    [Header("General Physical Properties")]
    [SerializeField]
    float gravitionalAcceleration = 9.81f;

    [SerializeField]
    [Min(0)]
    float yDrivingAmplitude = 0f;

    [SerializeField]
    [Min(0)]
    float yDrivingAngularFrequency = 0f;

    [SerializeField]
    [Min(0)]
    float dampingFactor = 1f;

    [Header("FBD Specific Parameters")]
    [SerializeField]
    [Min(0)]
    float omegaSquared = 1f;

    [Header("PBD Specific Parameters")]
    [SerializeField]
    [Min(0)]
    float springStiffness = 1f;

    float restLength;
    Body[] bodies;
    Spring[] springs;

    ForceBasedDynamics forceBasedDynamics;
    PositionBasedDynamics positionBasedDynamics;

    // Start is called before the first frame update
    void Start()
    {
        // Set up bodies and strings
        bodies = new Body[amountOfBodies + 2];
        bodies[0] = startPoint;
        bodies[amountOfBodies + 1] = endPoint;

        springs = new Spring[amountOfBodies + 1];

        // Vector from start to end points
        Vector3 vectorAlongSystem = endPoint.Position - startPoint.Position;
        Vector3 systemDirection = vectorAlongSystem.normalized;

        // Divide the entire span. The restlength is the size of each division
        float offset = vectorAlongSystem.magnitude / (amountOfBodies + 1);
        float restLengthFactor = 0.5f; // Lower value means more tension 
        restLength = restLengthFactor * offset;

        // Add bodies with given offset
        for (int i = 1; i < amountOfBodies + 1; i++)
        {
            Vector3 position = startPoint.Position + i * offset * systemDirection;
            Body body = Instantiate(bodyPrefab, position, Quaternion.identity);
            body.Position = position;
            bodies[i] = body;
        }

        // Add other springs
        for (int i = 0; i < amountOfBodies + 1; i++)
        {
            Spring spring = Instantiate(springPrefab);
            spring.SetConnectionPoints(bodies[i].transform, bodies[i + 1].transform);
            springs[i] = spring;
        }


        forceBasedDynamics = new ForceBasedDynamics(
            amountOfBodies,
            gravitionalAcceleration,
            omegaSquared,
            dampingFactor,
            yDrivingAmplitude,
            yDrivingAngularFrequency,
            restLength,
            bodies);

        positionBasedDynamics = new PositionBasedDynamics(
            bodies,
            amountOfBodies,
            springStiffness,
            restLength,
            dampingFactor,
            gravitionalAcceleration,
            yDrivingAmplitude,
            yDrivingAngularFrequency);
    }

    void FixedUpdate()
    {
        if (dynamicsType == DynamicsType.ForceBasedDynamics)
        {
            forceBasedDynamics.UpdateParameters(gravitionalAcceleration, omegaSquared, dampingFactor, yDrivingAmplitude, yDrivingAngularFrequency);
            forceBasedDynamics.Step();

        }
        else if (dynamicsType == DynamicsType.PositionBasedDynamics)
        {

            positionBasedDynamics.UpdateParameters(gravitionalAcceleration, springStiffness, dampingFactor, yDrivingAmplitude, yDrivingAngularFrequency);
            positionBasedDynamics.Step();
        }
    }

}
