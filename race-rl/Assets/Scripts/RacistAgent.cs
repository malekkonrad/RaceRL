using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;
using Unity.MLAgents.Sensors;

public class RacistAgent : Agent
{

    [SerializeField] private TrackCheckpoints trackCheckpoints;
    [SerializeField] private Transform spawnPosition;

    [SerializeField] private float flippedEndDelay = 0.75f;        // ile sekund warunek ma trwać
    [SerializeField, Range(-1f, 1f)] private float upsideDownDotThreshold = -0.2f; // < 0 znaczy "głową w dół"
    private float flippedTimer = 0f;

    private SimpleCar carDriver;


    private void Awake()
    {
        carDriver = GetComponent<SimpleCar>();
    }


    private void Start()
    {
        trackCheckpoints.OnCarCorrectCheckpoint += TrackCheckpoints_OnCarCorrectCheckpoint;
        trackCheckpoints.OnCarWrongCheckpoint += TrackCheckpoints_OnWrongCorrectCheckpoint;
    }

    private void TrackCheckpoints_OnCarCorrectCheckpoint(object sender, TrackCheckpoints.CarCheckpointEventArgs e)
    {
        if (e.carTransform == transform)
        {
            AddReward(1f);
        }
    }

    private void TrackCheckpoints_OnWrongCorrectCheckpoint(object sender, TrackCheckpoints.CarCheckpointEventArgs e)
    {
        if (e.carTransform == transform)
        {
            AddReward(-1f);
        }
    }


    public override void OnEpisodeBegin()
    {
        transform.position = spawnPosition.position + new Vector3(0, 0, Random.Range(-5f, +5f));
        transform.forward = spawnPosition.forward;
        trackCheckpoints.ResetCheckpoint(transform);
        carDriver.StopCompletely();

        flippedTimer = 0f;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 checkpointForward = trackCheckpoints.GetNexCheckpoint(transform).transform.forward;
        float directionDot = Vector3.Dot(transform.forward, checkpointForward);
        sensor.AddObservation(directionDot);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float forwardAmount = 0f;
        float turnAmount = 0f;

        switch (actions.DiscreteActions[0])
        {
            case 0: forwardAmount = 0f; break;
            case 1: forwardAmount = 1f; break;
            case 2: forwardAmount = -1f; break;
        }

        switch (actions.DiscreteActions[1])
        {
            case 0: turnAmount = 0f; break;
            case 1: turnAmount = 1f; break;
            case 2: turnAmount = -1f; break;
        }

        Debug.Log($"ML Action: Forward={forwardAmount}, Turn={turnAmount}");

        carDriver.SetInputs(forwardAmount, turnAmount);
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int forwardAction = 0;
        if (Input.GetKey(KeyCode.W)) forwardAction = 1;
        if (Input.GetKey(KeyCode.S)) forwardAction = 2;

        int turnAction = 0;
        if (Input.GetKey(KeyCode.A)) turnAction = 1;
        if (Input.GetKey(KeyCode.D)) turnAction = 2;

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = forwardAction;
        discreteActions[1] = turnAction;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            AddReward(-0.5f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            AddReward(-0.1f);
        }
    }


    private void FixedUpdate()
    {
        if (carDriver != null)
        {
            SimpleWheel[] wheels = { carDriver.frontLeft, carDriver.frontRight, carDriver.rearLeft, carDriver.rearRight };
            int counter = 0;
            foreach (var wheel in wheels)
            {
                if (!wheel.IsGrounded())
                {
                    counter++;
                }
            }

            float upDot = Vector3.Dot(transform.up, Vector3.up); // 1 = prosto, -1 = do góry nogami
            bool upsideDown = upDot < upsideDownDotThreshold;

            if (counter == 4 && upsideDown)
            {
                flippedTimer += Time.fixedDeltaTime;
                if (flippedTimer >= flippedEndDelay)
                {
                    AddReward(-1f);
                    EndEpisode();
                }
            }
            else
            {
                flippedTimer = 0f;
            }
        }
    }


}
