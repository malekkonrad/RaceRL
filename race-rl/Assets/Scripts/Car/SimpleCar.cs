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

    
    public void SetInputs(float forwardAmount, float turnAmount)
    {
        // Wywoływane przez ML-Agent (a jeśli testowanie ręczene to przez Heuristic) - zmiana z FixedUpdate i Update - teraz wszystko tutaj się dzieje
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

    void ApplyMotor(SimpleWheel wheel, float input)
    {
        if (wheel != null && wheel.IsGrounded())
        {
            float carSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);

            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / 1000f);

            float availableTorque = powerCurve(normalizedSpeed) * input * motorForce;

            // Vector3 force = transform.forward * motorForce * input;
            rb.AddForceAtPosition(transform.forward * availableTorque, wheel.transform.position);
        }
    }
    
    
    /// TODO pobawić się można z jakimiś krzywymi - gdzieś takie coś widziałem na necie - 
    /// na razie prowizorycznie ręczenie liczone (ale można je jakoś rysować czyc coś)
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
