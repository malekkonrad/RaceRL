

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Wheel References")]
    public WheelPhysics frontLeftWheel;
    public WheelPhysics frontRightWheel;
    public WheelPhysics rearLeftWheel;
    public WheelPhysics rearRightWheel;

    [Header("Engine")]
    public float maxEngineTorque = 500f; // Nm
    public float maxRPM = 15000f; // F1: ~15000 RPM
    public AnimationCurve engineTorqueCurve; // Torque vs RPM ratio

    [Header("Steering")]
    public float maxSteerAngle = 25f; // Maksymalny kąt skrętu
    public float steerSpeed = 3f; // Szybkość skręcania

    [Header("Brakes")]
    public float maxBrakeTorque = 8000f; // Nm
    public float brakeBias = 0.6f; // 60% na przód, 40% na tył

    [Header("Downforce - Podstawowa")]
    public float downforceCoefficient = 2.5f; // Mnożnik downforce
    public float dragCoefficient = 0.3f;
    public float frontalArea = 1.5f; // m²

    // Runtime
    private Rigidbody rb;
    private float currentSteerAngle;
    private float throttleInput;
    private float brakeInput;
    private float steerInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Ustawienia Rigidbody dla samochodu wyścigowego
        rb.mass = 798f; // Minimum F1 bez paliwa: 798kg
        rb.centerOfMass = new Vector3(0, 0.2f, 0); // Nisko, dla stabilności
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Domyślna krzywa momentu (jeśli nie ustawiona)
        if (engineTorqueCurve == null || engineTorqueCurve.length == 0)
        {
            engineTorqueCurve = AnimationCurve.Linear(0, 0.5f, 1, 1);
        }
    }

    void Update()
    {
        // Input (później podmienisz na ML-Agents)
        throttleInput = Input.GetAxis("Vertical");
        throttleInput = Mathf.Clamp01(throttleInput); // Tylko 0-1

        brakeInput = Input.GetAxis("Vertical");
        brakeInput = Mathf.Clamp(brakeInput, -1, 0); // Tylko -1-0
        brakeInput = Mathf.Abs(brakeInput);

        steerInput = Input.GetAxis("Horizontal");

        // Płynne skręcanie
        float targetSteerAngle = steerInput * maxSteerAngle;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, Time.deltaTime * steerSpeed);

        // Aplikuj kąt skrętu do przednich kół (transform rotation)
        frontLeftWheel.transform.localRotation = Quaternion.Euler(0, currentSteerAngle, 0);
        frontRightWheel.transform.localRotation = Quaternion.Euler(0, currentSteerAngle, 0);
    }

    void FixedUpdate()
    {
        ApplyEngineForce();
        ApplyBrakes();
        ApplyAerodynamics();
    }

    void ApplyEngineForce()
    {
        if (throttleInput > 0.01f)
        {
            // Uproszczona symulacja RPM bazująca na prędkości
            float speed = Vector3.Dot(rb.linearVelocity, transform.forward);
            float rpmRatio = Mathf.Clamp01(Mathf.Abs(speed) / 100f); // Zakładamy max ~100 m/s

            // Pobierz moment z krzywej
            float torqueMultiplier = engineTorqueCurve.Evaluate(rpmRatio);
            float currentTorque = maxEngineTorque * torqueMultiplier * throttleInput;

            // Aplikuj moment do tylnych kół (RWD dla F1)
            float torquePerWheel = currentTorque / 2f;

            // Proste przełożenie moment -> siła
            // F = Torque / radius
            float forcePerWheel = torquePerWheel / rearLeftWheel.radius;

            if (rearLeftWheel.IsGrounded)
            {
                rb.AddForceAtPosition(transform.forward * forcePerWheel, rearLeftWheel.transform.position);
            }
            if (rearRightWheel.IsGrounded)
            {
                rb.AddForceAtPosition(transform.forward * forcePerWheel, rearRightWheel.transform.position);
            }
        }
    }

    void ApplyBrakes()
    {
        if (brakeInput > 0.01f)
        {
            // Oblicz siłę hamowania
            float frontBrakeTorque = maxBrakeTorque * brakeBias * brakeInput;
            float rearBrakeTorque = maxBrakeTorque * (1f - brakeBias) * brakeInput;

            float frontBrakeForce = frontBrakeTorque / frontLeftWheel.radius;
            float rearBrakeForce = rearBrakeTorque / rearLeftWheel.radius;

            // Aplikuj siłę przeciwną do ruchu
            Vector3 brakeDirection = -transform.forward * Mathf.Sign(Vector3.Dot(rb.linearVelocity, transform.forward));

            if (frontLeftWheel.IsGrounded)
                rb.AddForceAtPosition(brakeDirection * frontBrakeForce, frontLeftWheel.transform.position);
            if (frontRightWheel.IsGrounded)
                rb.AddForceAtPosition(brakeDirection * frontBrakeForce, frontRightWheel.transform.position);
            if (rearLeftWheel.IsGrounded)
                rb.AddForceAtPosition(brakeDirection * rearBrakeForce, rearLeftWheel.transform.position);
            if (rearRightWheel.IsGrounded)
                rb.AddForceAtPosition(brakeDirection * rearBrakeForce, rearRightWheel.transform.position);
        }
    }

    void ApplyAerodynamics()
    {
        float speed = rb.linearVelocity.magnitude;
        float speedSquared = speed * speed;

        // Siła oporu (drag) - zawsze przeciwna do kierunku ruchu
        float dragForce = 0.5f * 1.225f * dragCoefficient * frontalArea * speedSquared;
        Vector3 dragVector = -rb.linearVelocity.normalized * dragForce;
        rb.AddForce(dragVector);

        // Downforce - siła dociskająca w dół
        float downforce = 0.5f * 1.225f * downforceCoefficient * frontalArea * speedSquared;
        rb.AddForce(-transform.up * downforce);

        // Rozkład downforce: 40% przód, 60% tył (typowo dla F1)
        Vector3 frontDownforce = -transform.up * (downforce * 0.4f);
        Vector3 rearDownforce = -transform.up * (downforce * 0.6f);

        Vector3 frontCenter = (frontLeftWheel.transform.position + frontRightWheel.transform.position) / 2f;
        Vector3 rearCenter = (rearLeftWheel.transform.position + rearRightWheel.transform.position) / 2f;

        rb.AddForceAtPosition(frontDownforce, frontCenter);
        rb.AddForceAtPosition(rearDownforce, rearCenter);
    }

    // Metody do ML-Agents (później)
    public void SetInputs(float throttle, float brake, float steer)
    {
        throttleInput = Mathf.Clamp01(throttle);
        brakeInput = Mathf.Clamp01(brake);
        steerInput = Mathf.Clamp(steer, -1f, 1f);
    }

    // Gettery dla obserwacji ML
    public float GetSpeed() => rb.linearVelocity.magnitude;
    public float GetRPMRatio() => Mathf.Clamp01(Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.forward)) / 100f);




















}






/// ------------------------------------------------------------------------------------
/// OLD


// using UnityEngine;

// public class CarController : MonoBehaviour
// {
//     public float speed = 5.0f; // Set player's movement speed.
//     public float rotationSpeed = 120.0f; // Set player's rotation speed.

//     private Rigidbody rb; // Reference to player's Rigidbody.

//     // Start is called before the first frame update
//     private void Start()
//     {
//         rb = GetComponent<Rigidbody>(); // Access player's Rigidbody.
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }

//     // Handle physics-based movement and rotation.
//     private void FixedUpdate()
//     {
//         // Move player based on vertical input.
//         float moveVertical = Input.GetAxis("Vertical");
//         Vector3 movement = transform.forward * moveVertical * speed * Time.fixedDeltaTime;
//         rb.MovePosition(rb.position + movement);

//         // Rotate player based on horizontal input.
//         float turn = Input.GetAxis("Horizontal") * rotationSpeed * Time.fixedDeltaTime;
//         Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
//         rb.MoveRotation(rb.rotation * turnRotation);
//     }
// }
