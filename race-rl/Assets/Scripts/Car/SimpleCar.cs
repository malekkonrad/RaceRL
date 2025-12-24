// // using UnityEngine;


// // /// <summary>
// // /// Prosty samochód z napędem na tylne koła, prostą fizyką i sterowaniem
// // /// Pomysł na rozbudowę:
// // /// - lepsza fizyka (opory, downforce, stabilizacja, itp)
// // /// - lepsze sterowanie (ackerman, płynne zmiany biegów, itp)
// // /// - lepsze zawieszenie (anti-roll bar, itp)
// // /// - lepsze hamowanie (ABS, itp)
// // /// - lepszy silnik (charakterystyka, itp)
// // /// - lepsze koła (temperatura, przyczepność, itp) -> patrz SimpleWheel (pacejka)
// // /// - lepsza aerodynamika (skrzydła, itp)
// // /// - lepsza kamera (follow, itp) - można łatwo przeciągnąć i podąża za bolidem
// // /// </summary>


// // [RequireComponent(typeof(Rigidbody))]
// // public class SimpleCar : MonoBehaviour
// // {
// //     [Header("Wheels")]
// //     public SimpleWheel frontLeft;
// //     public SimpleWheel frontRight;
// //     public SimpleWheel rearLeft;
// //     public SimpleWheel rearRight;
    
// //     [Header("Engine")]
// //     public float motorForce = 500f;
    
// //     [Header("Steering")]
// //     public float maxSteerAngle = 30f;
// //     public float steerSpeed = 5f;

// //     [Header("Brakes")]
// //     public float brakeForce = 3000f;
    
// //     [Header("Stability")]
// //     public float antiRollForce = 5000f;
    
// //     private Rigidbody rb;
// //     private float currentSteerAngle;
    
// //     void Start()
// //     {
// //         rb = GetComponent<Rigidbody>();
// //         rb.centerOfMass = new Vector3(0, -0.5f, 0);
// //     }

    
// //     public void SetInputs(float forwardAmount, float turnAmount)
// //     {
// //         // Wywoływane przez ML-Agent (a jeśli testowanie ręczene to przez Heuristic) - zmiana z FixedUpdate i Update - teraz wszystko tutaj się dzieje
// //         float throttle = forwardAmount;
// //         float steer = turnAmount;
        
// //         // Skręcanie
// //         float targetSteer = steer * maxSteerAngle;
// //         currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteer, Time.fixedDeltaTime * steerSpeed);
        
// //         // Obróć przednie koła
// //         if (frontLeft != null)
// //             frontLeft.transform.localRotation = Quaternion.Euler(0, currentSteerAngle, 0);
// //         if (frontRight != null)
// //             frontRight.transform.localRotation = Quaternion.Euler(0, currentSteerAngle, 0);

// //         // Silnik
// //         if (throttle > 0.1f)
// //         {
// //             ApplyMotor(rearLeft, throttle);
// //             ApplyMotor(rearRight, throttle);
// //         }
        
// //         // Hamulce
// //         if (throttle < -0.1f)
// //         {
// //             ApplyBrake(frontLeft, Mathf.Abs(throttle));
// //             ApplyBrake(frontRight, Mathf.Abs(throttle));
// //             ApplyBrake(rearLeft, Mathf.Abs(throttle));
// //             ApplyBrake(rearRight, Mathf.Abs(throttle));
// //         }
// //     }

// //     void ApplyMotor(SimpleWheel wheel, float input)
// //     {
// //         if (wheel != null && wheel.IsGrounded())
// //         {
// //             float carSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);

// //             float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / 1000f);

// //             float availableTorque = powerCurve(normalizedSpeed) * input * motorForce;

// //             // Vector3 force = transform.forward * motorForce * input;
// //             rb.AddForceAtPosition(transform.forward * availableTorque, wheel.transform.position);
// //         }
// //     }
    
    
// //     /// TODO pobawić się można z jakimiś krzywymi - gdzieś takie coś widziałem na necie - 
// //     /// na razie prowizorycznie ręczenie liczone (ale można je jakoś rysować czyc coś)
// //     float powerCurve(float t)
// //     {
// //         // Prosty wykres mocy: pełna moc przy 0 prędkości, spada do 0 przy maksymalnej prędkości
// //         return Mathf.Clamp01(1f - t);
// //     }

// //     void ApplyBrake(SimpleWheel wheel, float input)
// //     {
// //         if (wheel != null && wheel.IsGrounded())
// //         {
// //             Vector3 brakeDirection = -transform.forward * Mathf.Sign(Vector3.Dot(rb.linearVelocity, transform.forward));
// //             Vector3 force = brakeDirection * (brakeForce * input);
// //             rb.AddForceAtPosition(force, wheel.transform.position);
// //         }
// //     }
    

// //     // pomoc do wizualizacji informacji o położeniu kół - obecnie nie potrzebne ale przy rozbudownie fizyki może się przydać 
// //     void OnGUI()
// //     {
// //         GUIStyle style = new GUIStyle();
// //         style.fontSize = 20;
// //         style.normal.textColor = Color.white;

// //         GUI.Label(new Rect(10, 10, 300, 30), $"Speed: {rb.linearVelocity.magnitude * 3.6f:F0} km/h", style);
// //         GUI.Label(new Rect(10, 35, 300, 30), $"Throttle: W/S  Steer: A/D", style);

// //         int y = 70;
// //         SimpleWheel[] wheels = { frontLeft, frontRight, rearLeft, rearRight };
// //         string[] names = { "FL", "FR", "RL", "RR" };

// //         for (int i = 0; i < wheels.Length; i++)
// //         {
// //             if (wheels[i] != null)
// //             {
// //                 style.normal.textColor = wheels[i].IsGrounded() ? Color.green : Color.red;
// //                 GUI.Label(new Rect(10, y, 300, 30), $"{names[i]}: {(wheels[i].IsGrounded() ? "GROUND" : "AIR")}", style);
// //                 y += 25;
// //             }
// //         }
// //     }
    
    
// //     public void StopCompletely()
// //     {
// //         if (rb != null)
// //         {
// //             rb.linearVelocity = Vector3.zero;
// //             rb.angularVelocity = Vector3.zero;
// //         }
// //         currentSteerAngle = 0f;
        
// //         // Zresetuj kąty kół
// //         if (frontLeft != null)
// //             frontLeft.transform.localRotation = Quaternion.identity;
// //         if (frontRight != null)
// //             frontRight.transform.localRotation = Quaternion.identity;
// //     }



// // }



using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleCar : MonoBehaviour
{
    [Header("Wheels")]
    public SimpleWheel frontLeft;
    public SimpleWheel frontRight;
    public SimpleWheel rearLeft;
    public SimpleWheel rearRight;
    
    [Header("Engine & Performance")]
    public float motorForce = 1500f; // Zwiększone, bo fizyka opon teraz lepiej hamuje
    public float maxSpeed = 80f;     // m/s (ok 280 km/h)
    public AnimationCurve torqueCurve = AnimationCurve.Linear(0, 1, 1, 0.5f); // Krzywa momentu

    [Header("Steering")]
    public float maxSteerAngle = 35f;
    public float steerSpeed = 10f;

    [Header("Brakes")]
    public float brakeForce = 6000f;
    
    [Header("Physics Tweaks (F1 Style)")]
    public float downforce = 100f;      // Docisk aerodynamiczny
    public float antiRollForce = 10000f;// Sztywność stabilizatora
    [Range(0,1)] public float steerHelper = 0.3f; // Pomaga utrzymać kierunek (fake physics dla lepszego feelingu)

    private Rigidbody rb;
    private float currentSteerAngle;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Bardzo ważne: obniżenie środka ciężkości, żeby auto się nie wywracało
        rb.centerOfMass = new Vector3(0, -0.6f, 0.2f); 
        
        // Ustawienia masy dla stabilności (sugerowane: 1500)
        if(rb.mass < 500) rb.mass = 1500f; 
        
        // Zmniejszamy opór kątowy, żeby auto chętniej skręcało
        rb.angularDamping = 1.0f;
    }

    public void SetInputs(float forwardAmount, float turnAmount)
    {
        // ZABEZPIECZENIE: Jeśli z jakiegoś powodu rb nie istnieje, nie rób nic
        if (rb == null) return;

        float throttle = forwardAmount;
        float steer = turnAmount;
        
        HandleSteering(steer);
        HandleEngine(throttle);
        HandleAerodynamics();
        HandleStabilizers(); // Anti-roll bar
    }

    void HandleSteering(float steerInput)
    {
        // Płynne skręcanie
        float targetAngle = steerInput * maxSteerAngle;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetAngle, Time.fixedDeltaTime * steerSpeed);

        // Geometria Ackermanna (wewnętrzne koło skręca mocniej)
        if (frontLeft != null && frontRight != null)
        {
            float farAngle = currentSteerAngle;
            float nearAngle = currentSteerAngle;
            
            // Prosta symulacja Ackermanna - zwiększamy kąt wewnętrznego koła
            if(steerInput > 0) nearAngle *= 1.1f; // Skręt w prawo, prawe koło mocniej
            else farAngle *= 1.1f; // Skręt w lewo, lewe koło mocniej
            
            frontLeft.transform.localRotation = Quaternion.Euler(0, steerInput > 0 ? farAngle : nearAngle, 0);
            frontRight.transform.localRotation = Quaternion.Euler(0, steerInput > 0 ? nearAngle : farAngle, 0);
        }

        // Steer Helper - sztuczna siła obracająca auto w stronę skrętu
        // Pomaga ML Agentom szybciej "zrozumieć" skręcanie bez driftowania
        if (Mathf.Abs(currentSteerAngle) > 1f && rb.linearVelocity.magnitude > 5f)
        {
            rb.AddRelativeTorque(Vector3.up * currentSteerAngle * steerHelper * rb.linearVelocity.magnitude);
        }
    }

    void HandleEngine(float throttle)
    {
        // Napęd (RWD)
        if (throttle > 0.1f)
        {
            ApplyMotor(rearLeft, throttle);
            ApplyMotor(rearRight, throttle);
        }
        // Hamowanie
        else if (throttle < -0.1f)
        {
            ApplyBrake(frontLeft, Mathf.Abs(throttle) * 0.7f); // Przód hamuje słabiej (balans hamulców)
            ApplyBrake(frontRight, Mathf.Abs(throttle) * 0.7f);
            ApplyBrake(rearLeft, Mathf.Abs(throttle));
            ApplyBrake(rearRight, Mathf.Abs(throttle));
        }
        else
        {
            // Hamowanie silnikiem (Drag)
            // ApplyBrake(rearLeft, 0.1f);
            // ApplyBrake(rearRight, 0.1f);
        }
    }

    void HandleAerodynamics()
    {
        if (rb == null) return;
        
        // Obliczamy siłę docisku
        float speed = rb.linearVelocity.magnitude;
        // Debug.Log($"Speed: {rb.linearVelocity.magnitude * 3.6f:F0} km/h");
        float downforceAmount = downforce * speed;

        // Opcja A (Realistyczna - auto siada):
        // rb.AddForce(-transform.up * downforceAmount);

        // Opcja B (Stabilna - docisk aplikowany bezpośrednio nad kołami):
        // Rozkładamy siłę na 4 koła, żeby docisnąć opony, ale mniej wpływać na przechyły budy
        // (Wymaga referencji do kół w tej metodzie, albo prostszego triku poniżej)

        // Opcja C (Najlepsza do ML-Agents - "Fake Gravity"):
        // Zamiast dociskać auto w dół, po prostu zwiększamy mu masę/grawitację TYLKO dla celów trakcji
        // Ale najprościej dla Ciebie będzie po prostu ograniczyć docisk, żeby nie był nieskończony:
        
        float clampedDownforce = Mathf.Clamp(downforceAmount, 0, 50000f); // Limit max siły
        rb.AddForce(-transform.up * clampedDownforce);
    }

    void HandleStabilizers()
    {
        // Anti-Roll Bar: Przenosi siłę z koła ściśniętego na koło odciążone
        ApplyAntiRoll(frontLeft, frontRight);
        ApplyAntiRoll(rearLeft, rearRight);
    }

    void ApplyAntiRoll(SimpleWheel wl, SimpleWheel wr)
    {
        if (wl == null || wr == null) return;

        float travelL = wl.CompressionRatio;
        float travelR = wr.CompressionRatio;

        float antiRollForceAmount = (travelL - travelR) * antiRollForce;

        if (wl.IsGrounded())
            rb.AddForceAtPosition(wl.transform.up * -antiRollForceAmount, wl.transform.position);
        
        if (wr.IsGrounded())
            rb.AddForceAtPosition(wr.transform.up * antiRollForceAmount, wr.transform.position);
    }

    void ApplyMotor(SimpleWheel wheel, float input)
    {
        if (wheel != null && wheel.IsGrounded())
        {
            // Ogranicznik prędkości maksymalnej
            float currentSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);
            if (currentSpeed > maxSpeed) return;

            // Krzywa momentu obrotowego (więcej mocy na starcie, mniej przy V-max)
            float speedRatio = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
            float availableTorque = torqueCurve.Evaluate(speedRatio) * motorForce * input;

            // Aplikujemy siłę w kierunku, w który patrzy koło (ważne dla RWD przy poślizgach)
            // Dla tylnych kół transform.forward bolidu jest ok, ale wheel.transform.forward jest bezpieczniejsze
            rb.AddForceAtPosition(wheel.transform.forward * availableTorque, wheel.transform.position);
        }
    }

    void ApplyBrake(SimpleWheel wheel, float input)
    {
        if (wheel != null && wheel.IsGrounded())
        {
            // Hamujemy przeciwnie do ruchu auta
            Vector3 velocity = rb.GetPointVelocity(wheel.transform.position);
            Vector3 brakeDir = -velocity.normalized;
            
            // Aplikujemy hamulec
            rb.AddForceAtPosition(brakeDir * brakeForce * input, wheel.transform.position);
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
    }
}









