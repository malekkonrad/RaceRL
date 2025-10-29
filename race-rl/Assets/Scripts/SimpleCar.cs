using UnityEngine;


/// <summary>
/// Prosty samochód z napędem na tylne koła, prostą fizyką i sterowaniem
/// Pomysł na rozbudowę:
/// - lepsza fizyka (opory, downforce, stabilizacja, itp)
/// - lepsze sterowanie (ackerman, płynne zmiany biegów, itp)
/// - lepsze zawieszenie (anti-roll bar, itp)
/// - lepsze hamowanie (ABS, itp)
/// - lepszy silnik (charakterystyka, itp)
/// - lepsze koła (temperatura, przyczepność, itp) -> patrz SimpleWheel (pacejka)
/// - lepsza aerodynamika (skrzydła, itp)
/// - lepsza kamera (follow, itp) - można łatwo przeciągnąć i podąża za bolidem
/// </summary>


[RequireComponent(typeof(Rigidbody))]
public class SimpleCar : MonoBehaviour
{
    [Header("Wheels")]
    public SimpleWheel frontLeft;
    public SimpleWheel frontRight;
    public SimpleWheel rearLeft;
    public SimpleWheel rearRight;
    
    [Header("Engine")]
    public float motorForce = 500f;
    
    [Header("Steering")]
    public float maxSteerAngle = 30f;
    public float steerSpeed = 5f;

    [Header("Brakes")]
    public float brakeForce = 3000f;
    
    [Header("Stability")]
    public float antiRollForce = 5000f;
    
    private Rigidbody rb;
    private float currentSteerAngle;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }
    
    void Update()
    {
        // Input (WSAD, strzałki chyba też)
        float throttle = Input.GetAxis("Vertical");
        float steer = Input.GetAxis("Horizontal");
        
        // Skręcanie, żeby było płynnie - to nie ma przeskoków tylko gładko zmienia się kąt 
        float targetSteer = steer * maxSteerAngle;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteer, Time.deltaTime * steerSpeed);
        
        // Obróć przednie koła - zastanawaim się czy ten ackerman nie jest potrzebny, ale tak też działa
        if (frontLeft != null)
            frontLeft.transform.localRotation = Quaternion.Euler(0, currentSteerAngle, 0);
        if (frontRight != null)
            frontRight.transform.localRotation = Quaternion.Euler(0, currentSteerAngle, 0);
    }
    
    void FixedUpdate()
    {
        // float throttle = Input.GetAxis("Vertical");
        
        // // Silnik - tylko tylne koła - bo w F1 tak jest - nie wiedziałem
        // if (throttle > 0.1f)
        // {
        //     ApplyMotor(rearLeft, throttle);
        //     ApplyMotor(rearRight, throttle);
        // }
        
        // // Hamulce - tu już na wszystkie koła
        // if (throttle < -0.1f)
        // {
        //     ApplyBrake(frontLeft, Mathf.Abs(throttle));
        //     ApplyBrake(frontRight, Mathf.Abs(throttle));
        //     ApplyBrake(rearLeft, Mathf.Abs(throttle));
        //     ApplyBrake(rearRight, Mathf.Abs(throttle));
        // }
        
        // // Prosta aerodynamika (opór + downforce) - TODO: do rozbudowy
        // float speed = rb.linearVelocity.magnitude;
        // float drag = speed * speed * 0.5f;
        // float downforce = speed * speed * 2f;
        
        // rb.AddForce(-rb.linearVelocity.normalized * drag);
        // rb.AddForce(-transform.up * downforce);

        // ApplyAntiRoll(); - pomysł na rozbudowę ale na razie działa, bo wystarczyło dopasować wysokość kół i zawieszenia i nie ma jakiś dziwnych obrotów w wyskich zakrętach
    }

    void ApplyMotor(SimpleWheel wheel, float input)
    {
        if (wheel != null && wheel.IsGrounded())
        {
            float carSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);

            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / 1000f); // Zakładamy, że 100 m/s to maksymalna prędkość dla pełnej mocy

            float availableTorque = powerCurve(normalizedSpeed) * input* motorForce;

            // Vector3 force = transform.forward * motorForce * input;
            rb.AddForceAtPosition(transform.forward * availableTorque, wheel.transform.position);
        }
    }
    
    float powerCurve(float t)
    {
        // Prosty wykres mocy: pełna moc przy 0 prędkości, spada do 0 przy maksymalnej prędkości
        return Mathf.Clamp01(1f - t);
    }

    void ApplyBrake(SimpleWheel wheel, float input)
    {
        if (wheel != null && wheel.IsGrounded())
        {
            Vector3 brakeDirection = -transform.forward * Mathf.Sign(Vector3.Dot(rb.linearVelocity, transform.forward));
            Vector3 force = brakeDirection * (brakeForce * input);
            rb.AddForceAtPosition(force, wheel.transform.position);
        }
    }
    


    void ApplyAntiRoll()
    {
        
        // From chat - TODO: sprawdzić i się zastanowić

        // Sprawdź różnicę wysokości między lewą a prawą stroną (przód)
        if (frontLeft != null && frontRight != null && frontLeft.IsGrounded() && frontRight.IsGrounded())
        {
            float leftHeight = GetWheelHeight(frontLeft);
            float rightHeight = GetWheelHeight(frontRight);
            float rollDifference = leftHeight - rightHeight;
            
            Vector3 antiRollForce = transform.right * rollDifference * this.antiRollForce;
            rb.AddForceAtPosition(-antiRollForce, frontLeft.transform.position);
            rb.AddForceAtPosition(antiRollForce, frontRight.transform.position);
        }
        
        // To samo dla tyłu
        if (rearLeft != null && rearRight != null && rearLeft.IsGrounded() && rearRight.IsGrounded())
        {
            float leftHeight = GetWheelHeight(rearLeft);
            float rightHeight = GetWheelHeight(rearRight);
            float rollDifference = leftHeight - rightHeight;
            
            Vector3 antiRollForce = transform.right * rollDifference * this.antiRollForce;
            rb.AddForceAtPosition(-antiRollForce, rearLeft.transform.position);
            rb.AddForceAtPosition(antiRollForce, rearRight.transform.position);
        }
    }

    float GetWheelHeight(SimpleWheel wheel)
    {
        if (Physics.Raycast(wheel.transform.position, -wheel.transform.up, out RaycastHit hit, 2f))
        {
            return hit.distance;
        }
        return 2f;
    }



    // pomoc do wizualizacji informacji o położeniu kół - obecnie nie potrzebne ale przy rozbudownie fizyki może się przydać 
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 300, 30), $"Speed: {rb.linearVelocity.magnitude * 3.6f:F0} km/h", style);
        GUI.Label(new Rect(10, 35, 300, 30), $"Throttle: W/S  Steer: A/D", style);

        int y = 70;
        SimpleWheel[] wheels = { frontLeft, frontRight, rearLeft, rearRight };
        string[] names = { "FL", "FR", "RL", "RR" };

        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i] != null)
            {
                style.normal.textColor = wheels[i].IsGrounded() ? Color.green : Color.red;
                GUI.Label(new Rect(10, y, 300, 30), $"{names[i]}: {(wheels[i].IsGrounded() ? "GROUND" : "AIR")}", style);
                y += 25;
            }
        }
    }
    


    public void SetInputs(float forwardAmount, float turnAmount)
    {
        // Zastąp input z klawiatury inputem z ML-Agents
        float throttle = forwardAmount;
        float steer = turnAmount;
        
        // Skręcanie
        float targetSteer = steer * maxSteerAngle;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteer, Time.fixedDeltaTime * steerSpeed);
        
        // Obróć przednie koła
        if (frontLeft != null)
            frontLeft.transform.localRotation = Quaternion.Euler(0, currentSteerAngle, 0);
        if (frontRight != null)
            frontRight.transform.localRotation = Quaternion.Euler(0, currentSteerAngle, 0);

        // Silnik
        if (throttle > 0.1f)
        {
            ApplyMotor(rearLeft, throttle);
            ApplyMotor(rearRight, throttle);
        }
        
        // Hamulce
        if (throttle < -0.1f)
        {
            ApplyBrake(frontLeft, Mathf.Abs(throttle));
            ApplyBrake(frontRight, Mathf.Abs(throttle));
            ApplyBrake(rearLeft, Mathf.Abs(throttle));
            ApplyBrake(rearRight, Mathf.Abs(throttle));
        }
    }

    public void StopCompletely()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        currentSteerAngle = 0f;
        
        // Zresetuj kąty kół
        if (frontLeft != null)
            frontLeft.transform.localRotation = Quaternion.identity;
        if (frontRight != null)
            frontRight.transform.localRotation = Quaternion.identity;
    }



}



// using UnityEngine;


// /// <summary>
// /// Prosty samochód z napędem na tylne koła, prostą fizyką i sterowaniem
// /// Pomysł na rozbudowę:
// /// - lepsza fizyka (opory, downforce, stabilizacja, itp)
// /// - lepsze sterowanie (ackerman, płynne zmiany biegów, itp)
// /// - lepsze zawieszenie (anti-roll bar, itp)
// /// - lepsze hamowanie (ABS, itp)
// /// - lepszy silnik (charakterystyka, itp)
// /// - lepsze koła (temperatura, przyczepność, itp) -> patrz SimpleWheel (pacejka)
// /// - lepsza aerodynamika (skrzydła, itp)
// /// - lepsza kamera (follow, itp) - można łatwo przeciągnąć i podąża za bolidem
// /// </summary>


// [RequireComponent(typeof(Rigidbody))]
// public class SimpleCar : MonoBehaviour
// {
//     [Header("Wheels")]
//     public SimpleWheel frontLeft;
//     public SimpleWheel frontRight;
//     public SimpleWheel rearLeft;
//     public SimpleWheel rearRight;

//     [Header("Engine")]
//     public float motorForce = 5000f;
//     public float maxMotorForce = 8000f;  // Ograniczenie dla RL
    
//     [Header("Steering")]
//     public float maxSteerAngle = 30f;
//     public float steerSpeed = 5f;

//     [Header("Brakes")]
//     public float brakeForce = 3000f;

//     [Header("Aerodynamics")]
//     public float dragCoeff = 0.3f;      // Zwiększ dla więcej drag'u
//     public float downforceCoeff = 0.5f;  // Zmniejszy dla bardziej realnej dynamiki

//     [Header("Weight Transfer")]
//     public bool useWeightTransfer = true;
    
//     // [Header("Stability")]
//     // public float antiRollForce = 5000f;
    
//     private Rigidbody rb;
//     private float currentSteerAngle;
//     private float carMass;
    
//     void Start()
//     {
//         rb = GetComponent<Rigidbody>();
//         carMass = rb.mass;
//         rb.centerOfMass = new Vector3(0, -0.5f, 0);
//     }
    
//     void Update()
//     {
//         // Input (WSAD, strzałki chyba też)
//         float throttle = Input.GetAxis("Vertical");
//         float steer = Input.GetAxis("Horizontal");
        
//         // Skręcanie, żeby było płynnie - to nie ma przeskoków tylko gładko zmienia się kąt 
//         float targetSteer = steer * maxSteerAngle;
//         currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteer, Time.deltaTime * steerSpeed);
        
//         // Obróć przednie koła - zastanawaim się czy ten ackerman nie jest potrzebny, ale tak też działa
//         if (frontLeft != null)
//             frontLeft.transform.localRotation = Quaternion.Euler(0, currentSteerAngle, 0);
//         if (frontRight != null)
//             frontRight.transform.localRotation = Quaternion.Euler(0, currentSteerAngle, 0);
//     }
    
//     void FixedUpdate()
//     {
//         float throttle = Input.GetAxis("Vertical");
        
//         // Silnik - tylko tylne koła - bo w F1 tak jest - nie wiedziałem
//         if (throttle > 0.1f)
//         {
//             ApplyMotor(rearLeft, throttle);
//             ApplyMotor(rearRight, throttle);
//         }
        
//         // Hamulce - tu już na wszystkie koła
//         if (throttle < -0.1f)
//         {
//             ApplyBrake(frontLeft, Mathf.Abs(throttle));
//             ApplyBrake(frontRight, Mathf.Abs(throttle));
//             ApplyBrake(rearLeft, Mathf.Abs(throttle));
//             ApplyBrake(rearRight, Mathf.Abs(throttle));
//         }
        
//         // Prosta aerodynamika (opór + downforce) - TODO: do rozbudowy
//         float speed = rb.linearVelocity.magnitude;
//         float drag = speed * speed * dragCoeff;
//         float downforce = speed * speed * downforceCoeff;

//         // float yawDamping = 500f; // Dodaj do fields
//         // rb.angularVelocity = new Vector3(rb.angularVelocity.x, rb.angularVelocity.y * (1f - yawDamping * Time.fixedDeltaTime), rb.angularVelocity.z);
        
//         rb.AddForce(-rb.linearVelocity.normalized * drag);
//         rb.AddForce(-transform.up * downforce);

//         // ApplyAntiRoll(); - pomysł na rozbudowę ale na razie działa, bo wystarczyło dopasować wysokość kół i zawieszenia i nie ma jakiś dziwnych obrotów w wyskich zakrętach
//     }
    
//     void ApplyMotor(SimpleWheel wheel, float input)
//     {
//         if (wheel != null && wheel.IsGrounded())
//         {
//             float normalForce = wheel.GetNormalForce();
//             float effectiveForce = Mathf.Min(motorForce * input, maxMotorForce) * Mathf.Clamp01(normalForce / (carMass * 10f));
            
//             Vector3 force = transform.forward * effectiveForce;
//             rb.AddForceAtPosition(force, wheel.transform.position);
//         }
//     }

//     void ApplyBrake(SimpleWheel wheel, float input)
//     {
//         if (wheel != null && wheel.IsGrounded())
//         {
//             // Hamuj zawsze przeciwko kierunkowi ruchu
//             float forwardVelocity = Vector3.Dot(rb.linearVelocity, transform.forward);
//             Vector3 brakeDirection = -transform.forward * Mathf.Sign(forwardVelocity);

//             float normalForce = wheel.GetNormalForce();
//             float effectiveBrake = brakeForce * input * Mathf.Clamp01(normalForce / (carMass * 10f));
            
//             Vector3 force = brakeDirection * effectiveBrake;
//             rb.AddForceAtPosition(force, wheel.transform.position);
//         }
//     }
    


//     // void ApplyAntiRoll()
//     // {
        
//     //     // From chat - TODO: sprawdzić i się zastanowić

//     //     // Sprawdź różnicę wysokości między lewą a prawą stroną (przód)
//     //     if (frontLeft != null && frontRight != null && frontLeft.IsGrounded() && frontRight.IsGrounded())
//     //     {
//     //         float leftHeight = GetWheelHeight(frontLeft);
//     //         float rightHeight = GetWheelHeight(frontRight);
//     //         float rollDifference = leftHeight - rightHeight;
            
//     //         Vector3 antiRollForce = transform.right * rollDifference * this.antiRollForce;
//     //         rb.AddForceAtPosition(-antiRollForce, frontLeft.transform.position);
//     //         rb.AddForceAtPosition(antiRollForce, frontRight.transform.position);
//     //     }
        
//     //     // To samo dla tyłu
//     //     if (rearLeft != null && rearRight != null && rearLeft.IsGrounded() && rearRight.IsGrounded())
//     //     {
//     //         float leftHeight = GetWheelHeight(rearLeft);
//     //         float rightHeight = GetWheelHeight(rearRight);
//     //         float rollDifference = leftHeight - rightHeight;
            
//     //         Vector3 antiRollForce = transform.right * rollDifference * this.antiRollForce;
//     //         rb.AddForceAtPosition(-antiRollForce, rearLeft.transform.position);
//     //         rb.AddForceAtPosition(antiRollForce, rearRight.transform.position);
//     //     }
//     // }

//     // float GetWheelHeight(SimpleWheel wheel)
//     // {
//     //     if (Physics.Raycast(wheel.transform.position, -wheel.transform.up, out RaycastHit hit, 2f))
//     //     {
//     //         return hit.distance;
//     //     }
//     //     return 2f;
//     // }
    


//     // pomoc do wizualizacji informacji o położeniu kół - obecnie nie potrzebne ale przy rozbudownie fizyki może się przydać 
//     void OnGUI()
//     {
//         GUIStyle style = new GUIStyle();
//         style.fontSize = 20;
//         style.normal.textColor = Color.white;
        
//         float speed = rb.linearVelocity.magnitude;
//         GUI.Label(new Rect(10, 10, 300, 30), $"Speed: {speed * 3.6f:F0} km/h", style);
//         GUI.Label(new Rect(10, 35, 300, 30), $"Throttle: W/S  Steer: A/D", style);
        
//         int y = 70;
//         SimpleWheel[] wheels = { frontLeft, frontRight, rearLeft, rearRight };
//         string[] names = { "FL", "FR", "RL", "RR" };
        
//         for (int i = 0; i < wheels.Length; i++)
//         {
//             if (wheels[i] != null)
//             {
//                 style.normal.textColor = wheels[i].IsGrounded() ? Color.green : Color.red;
//                 float slip = wheels[i].GetSlipAngle();
//                 GUI.Label(new Rect(10, y, 400, 30), 
//                     $"{names[i]}: {(wheels[i].IsGrounded() ? "GROUND" : "AIR")} | Slip: {slip:F1}°", style);
//                 y += 25;
//             }
//         }
//     }
// }


// using UnityEngine;

// public class SimpleCar : MonoBehaviour
// {
//     [Header("Wheels")]
//     public SimpleWheel FL;
//     public SimpleWheel FR;
//     public SimpleWheel RL;
//     public SimpleWheel RR;

//     [Header("Car Settings")]
//     public float acceleration = 600f;
//     public float maxSpeed = 80f;
//     public float steeringSensitivity = 2.5f;
//     public float steeringMaxAngle = 30f;
//     public float dragOnRelease = 3f;
//     public float antiRollStability = 5000f;

//     private Rigidbody rb;
//     private float currentSpeed;
//     private float rotationSpeed;

//     void Start()
//     {
//         rb = GetComponent<Rigidbody>();
//         rb.centerOfMass = new Vector3(0, -0.5f, 0); // stabilizuje auto
//     }

//     void FixedUpdate()
//     {
//         // G A Z
//         float forwardInput = Input.GetAxis("Vertical");
//         if (forwardInput != 0f && rb.linearVelocity.magnitude < maxSpeed)
//             rb.AddForce(transform.forward * forwardInput * acceleration);

//         // HAMOWANIE DYNAMICZNE
//         if (forwardInput == 0f)
//             rb.linearDamping = dragOnRelease;
//         else
//             rb.linearDamping = 0f;

//         // S K R Ę T
//         float steerInput = Input.GetAxis("Horizontal");
//         float targetRotation = steerInput * steeringMaxAngle;
//         rotationSpeed = Mathf.Lerp(rotationSpeed, targetRotation, Time.fixedDeltaTime * steeringSensitivity);
//         transform.Rotate(0, rotationSpeed * Time.fixedDeltaTime, 0);

//         // ANTY-PRZEWRÓCENIE (jak F1)
//         rb.AddForce(-transform.up * rb.linearVelocity.magnitude * antiRollStability);

//         // ANIMACJA KÓŁ
//         float wheelRotationVisual = rb.linearVelocity.magnitude * 20f;
//         FL.Rotate(wheelRotationVisual);
//         FR.Rotate(wheelRotationVisual);
//         RL.Rotate(wheelRotationVisual);
//         RR.Rotate(wheelRotationVisual);

//         FL.Steer(targetRotation);
//         FR.Steer(targetRotation);
//     }
// }
