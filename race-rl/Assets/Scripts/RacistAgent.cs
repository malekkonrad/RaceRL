using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;
using Unity.MLAgents.Sensors;

public class RacistAgent : Agent
{

    [SerializeField] private TrackCheckpoints trackCheckpoints;
    [SerializeField] private Transform spawnPosition;

    // dodatkowe pola żeby zapobiegać przewracaniu się - w teorii teraz już nie powinno być z tym problemu ale myślę 
    // że jak się dołoży kilku agentów i zderzenia to może być różnie
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
        // Continuous actions: [0] = throttle (-1..1), [1] = steer (-1..1)
        float forwardAmount = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float turnAmount = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        carDriver.SetInputs(forwardAmount, turnAmount);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuous = actionsOut.ContinuousActions;
        continuous[0] = Input.GetAxis("Vertical");   // throttle: W/S
        continuous[1] = Input.GetAxis("Horizontal"); // steer: A/D
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            AddReward(-0.5f);
            // na razie kończymy epizod jak zderzy się z ścianą
            EndEpisode();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            AddReward(-0.1f);
            EndEpisode();
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
