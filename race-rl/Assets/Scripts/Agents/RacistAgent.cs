using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(DecisionRequester))]
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

    [SerializeField] public int lapsPerEpisode = 10;
    private int currentLap;

    [SerializeField] private bool ignoreAgentCollisions = true;

    public void Init(TrackCheckpoints checkpoints, Transform spawn)
    {
        trackCheckpoints = checkpoints;
        spawnPosition = spawn;

        // auto register
        // trackCheckpoints?.RegisterCar(transform);

        // auto register
        // trackCheckpoints?.RegisterCar(transform);
        // If already registered to another TrackCheckpoints, unregister first
        if (trackCheckpoints != null)
        {
            trackCheckpoints.OnCarCorrectCheckpoint -= TrackCheckpoints_OnCarCorrectCheckpoint;
            trackCheckpoints.OnCarWrongCheckpoint -= TrackCheckpoints_OnWrongCorrectCheckpoint;
            trackCheckpoints.OnCarLapCompleted -= TrackCheckpoints_OnCarLapCompleted;
            trackCheckpoints.UnregisterCar(transform);
        }

        trackCheckpoints = checkpoints;
        spawnPosition = spawn;

        // Register and subscribe to events on the new TrackCheckpoints (if any)
        if (trackCheckpoints != null)
        {
            trackCheckpoints.RegisterCar(transform);
            trackCheckpoints.OnCarCorrectCheckpoint += TrackCheckpoints_OnCarCorrectCheckpoint;
            trackCheckpoints.OnCarWrongCheckpoint += TrackCheckpoints_OnWrongCorrectCheckpoint;
            trackCheckpoints.OnCarLapCompleted += TrackCheckpoints_OnCarLapCompleted;
        }
    }



    protected override void Awake()
    {
        carDriver = GetComponent<SimpleCar>();
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

    private void TrackCheckpoints_OnCarLapCompleted(object sender, TrackCheckpoints.CarCheckpointEventArgs e) {
        if (e.carTransform == transform)
        {
            currentLap++;
            Debug.Log("Lap "+ currentLap + "/" + lapsPerEpisode);
            AddReward(5.0f);                // nagroda za całe okrążenie

            if (currentLap >= lapsPerEpisode)
            {
                AddReward(2.0f);
                EndEpisode();       // kończymy epizod
            }
        }
        
    }


    public override void OnEpisodeBegin()
    {
        if (spawnPosition == null)
        {
            Debug.LogWarning($"{name}: spawnPosition not set in OnEpisodeBegin — using current transform as fallback.");
            spawnPosition = transform;
        }
        transform.position = spawnPosition.position; 
        transform.forward = spawnPosition.forward;
        trackCheckpoints?.ResetCheckpoint(transform);
        carDriver?.StopCompletely();
        
        // Zmienne
        currentLap = 0;
        flippedTimer = 0f;


        // curriculum na liczbę okrążeń (opcjonalne, patrz YAML poniżej)
        var envParams = Academy.Instance.EnvironmentParameters;
        int lapsFromEnv = (int)envParams.GetWithDefault("laps_per_episode", lapsPerEpisode);
        lapsPerEpisode = Mathf.Max(1, lapsFromEnv);

    }


    public override void CollectObservations(VectorSensor sensor)
    {
        var next = trackCheckpoints?.GetNexCheckpoint(transform);
        float directionDot = 0f;
        if (next != null)
        {
            Vector3 checkpointForward = next.transform.forward;
            directionDot = Vector3.Dot(transform.forward, checkpointForward);
        }
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
            AddReward(-1f);
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


    /// <summary>
    /// Zamiast metody Start
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable(); // KLUCZOWE: inicjalizuje ML-Agents (sensors, policy itd.)

        // Ustaw warstwę "Agent" na całym prefabie i wyłącz kolizje Agent↔Agent
        EnsureAgentLayerAndIgnoreSelf();



        if (trackCheckpoints == null)
            trackCheckpoints = FindFirstObjectByType<TrackCheckpoints>();

        trackCheckpoints?.RegisterCar(transform);

        if (trackCheckpoints != null)
        {
            trackCheckpoints.OnCarCorrectCheckpoint += TrackCheckpoints_OnCarCorrectCheckpoint;
            trackCheckpoints.OnCarWrongCheckpoint += TrackCheckpoints_OnWrongCorrectCheckpoint;
            trackCheckpoints.OnCarLapCompleted += TrackCheckpoints_OnCarLapCompleted;
        }

        var dr = GetComponent<DecisionRequester>();
        if (dr != null)
        {
            if (dr.DecisionPeriod <= 0) dr.DecisionPeriod = 5;
            dr.TakeActionsBetweenDecisions = true;
        }
    }

    protected override void OnDisable()
    {
        if (trackCheckpoints != null)
        {
            trackCheckpoints.OnCarCorrectCheckpoint -= TrackCheckpoints_OnCarCorrectCheckpoint;
            trackCheckpoints.OnCarWrongCheckpoint -= TrackCheckpoints_OnWrongCorrectCheckpoint;
            trackCheckpoints.OnCarLapCompleted -= TrackCheckpoints_OnCarLapCompleted;
            trackCheckpoints.UnregisterCar(transform);
        }

        base.OnDisable(); // KLUCZOWE: czyści rejestracje w Academy
    }


    // TODO REFACTOR

    private static int s_AgentLayer = -1;
    private static bool s_IgnoredSelfCollision = false;

    private void EnsureAgentLayerAndIgnoreSelf()
    {
        if (s_AgentLayer == -1)
            s_AgentLayer = LayerMask.NameToLayer("Agent");

        if (s_AgentLayer == -1)
        {
            Debug.LogError("Brak warstwy 'Agent'. Dodaj ją w Project Settings > Tags and Layers.");
            return;
        }

        // Ustaw warstwę na całym obiekcie (root + dzieci)
        SetLayerRecursively(gameObject, s_AgentLayer);

        // Raz na proces wyłącz kolizje tej warstwy z samą sobą
        if (!s_IgnoredSelfCollision)
        {
            Physics.IgnoreLayerCollision(s_AgentLayer, s_AgentLayer, ignoreAgentCollisions);
            s_IgnoredSelfCollision = ignoreAgentCollisions;
        }
    }

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            SetLayerRecursively(obj.transform.GetChild(i).gameObject, layer);
        }
    }

}
