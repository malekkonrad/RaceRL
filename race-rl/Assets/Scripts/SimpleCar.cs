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
    public float motorForce = 5000f;
    
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
        float throttle = Input.GetAxis("Vertical");
        
        // Silnik - tylko tylne koła - bo w F1 tak jest - nie wiedziałem
        if (throttle > 0.1f)
        {
            ApplyMotor(rearLeft, throttle);
            ApplyMotor(rearRight, throttle);
        }
        
        // Hamulce - tu już na wszystkie koła
        if (throttle < -0.1f)
        {
            ApplyBrake(frontLeft, Mathf.Abs(throttle));
            ApplyBrake(frontRight, Mathf.Abs(throttle));
            ApplyBrake(rearLeft, Mathf.Abs(throttle));
            ApplyBrake(rearRight, Mathf.Abs(throttle));
        }
        
        // Prosta aerodynamika (opór + downforce) - TODO: do rozbudowy
        float speed = rb.linearVelocity.magnitude;
        float drag = speed * speed * 0.5f;
        float downforce = speed * speed * 2f;
        
        rb.AddForce(-rb.linearVelocity.normalized * drag);
        rb.AddForce(-transform.up * downforce);

        // ApplyAntiRoll(); - pomysł na rozbudowę ale na razie działa, bo wystarczyło dopasować wysokość kół i zawieszenia i nie ma jakiś dziwnych obrotów w wyskich zakrętach
    }
    
    void ApplyMotor(SimpleWheel wheel, float input)
    {
        if (wheel != null && wheel.IsGrounded())
        {
            Vector3 force = transform.forward * motorForce * input;
            rb.AddForceAtPosition(force, wheel.transform.position);
        }
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
}