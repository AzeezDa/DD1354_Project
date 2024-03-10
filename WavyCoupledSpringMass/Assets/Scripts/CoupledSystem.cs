using System.IO;
using System.Text;
using UnityEngine;

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
    DynamicsType oldDynamicsType = DynamicsType.ForceBasedDynamics; // Used for switch handling at runtime

    [Header("General Physical Properties")]
    [SerializeField]
    float gravitionalAcceleration = 9.81f;

    [SerializeField]
    [Min(0)]
    float yDrivingAmplitude = 0f;

    [SerializeField]
    [Min(0)]
    float yDrivingAngularFrequency = 0f;

    [Header("FBD Specific Parameters")]
    [SerializeField]
    [Min(0)]
    float omegaSquared = 1f;

    [SerializeField]
    [Min(0)]
    float gamma = 1f;
    [SerializeField]
    Integrator integrator;

    [Header("PBD Specific Parameters")]
    [SerializeField]
    [Range(0, 1)]
    float springStiffness = 1f;

    [SerializeField]
    [Range(0, 1)]
    float dampingFactor = 1f;

    float restLength;
    Body[] bodies;
    Spring[] springs;

    ForceBasedDynamics forceBasedDynamics;
    PositionBasedDynamics positionBasedDynamics;

    StreamWriter recordStreamWriter = null;
    Body first;
    Body mid;
    Body last;

    double averageTimePerStep = 0;
    double totalTimePerStep = 0;
    int iterations = 0;
    const int RECORD_ITERATIONS_MAX = 2000;


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
        float restLengthFactor = 0.3f; // Lower value means more tension 
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
            gamma,
            yDrivingAmplitude,
            yDrivingAngularFrequency,
            restLength,
            bodies,
            integrator);

        positionBasedDynamics = new PositionBasedDynamics(
            bodies,
            amountOfBodies,
            springStiffness,
            restLength,
            dampingFactor,
            gravitionalAcceleration,
            yDrivingAmplitude,
            yDrivingAngularFrequency);

        StartRecorder();
        oldDynamicsType = dynamicsType;
    }

    void FixedUpdate()
    {
        double timeStart = Time.realtimeSinceStartup;
        if (dynamicsType == DynamicsType.ForceBasedDynamics)
        {
            forceBasedDynamics.Step();
        }
        else if (dynamicsType == DynamicsType.PositionBasedDynamics)
        {
            positionBasedDynamics.Step();
        }


        // Timekeeping
        totalTimePerStep += Time.realtimeSinceStartup - timeStart;
        iterations += 1;
        averageTimePerStep = totalTimePerStep / iterations;
        if (iterations % RECORD_ITERATIONS_MAX == 0)
        {
            Debug.Log($"Average Step Time-Cost: {averageTimePerStep * 1000}ms");
        }

        // Recording
        if (recordStreamWriter == null)
        {
            return;
        }

        if (iterations <= RECORD_ITERATIONS_MAX)
        {
            recordStreamWriter.WriteLine($"{Time.fixedTime},{first.Position.y},{mid.Position.y},{last.Position.y}");
        }
        else if (iterations == RECORD_ITERATIONS_MAX + 1)
        {
            recordStreamWriter.Flush();
            recordStreamWriter.Close();
            recordStreamWriter = null;
        }
    }

    void OnValidate()
    {
        positionBasedDynamics?.UpdateParameters(
            gravitionalAcceleration,
            springStiffness,
            dampingFactor,
            yDrivingAmplitude,
            yDrivingAngularFrequency);

        forceBasedDynamics?.UpdateParameters(
            gravitionalAcceleration,
            omegaSquared,
            gamma,
            yDrivingAmplitude,
            yDrivingAngularFrequency,
            integrator);

        if (oldDynamicsType != dynamicsType)
        {
            Debug.Log("Dynamics Type Switched. Counters and timers resetted!");
            iterations = 0;
            totalTimePerStep = 0;
            averageTimePerStep = 0;
            recordStreamWriter?.Close();
            oldDynamicsType = dynamicsType;
        }
    }

    void StartRecorder()
    {
        StringBuilder fileName = new($"Records/{yDrivingAmplitude}_{yDrivingAngularFrequency}_{gravitionalAcceleration}");
        if (dynamicsType == DynamicsType.ForceBasedDynamics)
        {
            fileName.Append($"_fbd_{omegaSquared}_{gamma}");
        }
        else if (dynamicsType == DynamicsType.PositionBasedDynamics)
        {
            fileName.Append($"_pbd_{springStiffness}_{dampingFactor}");
        }
        fileName.Append(".csv");

        recordStreamWriter = new StreamWriter(fileName.ToString());
        recordStreamWriter.WriteLine("time,y1,ymid,yn");

        first = bodies[1];
        mid = bodies[amountOfBodies % 2 == 0 ? amountOfBodies / 2 : amountOfBodies / 2 + 1];
        last = bodies[amountOfBodies];

    }

    void OnDestroy()
    {
        recordStreamWriter?.Close();
    }
}
