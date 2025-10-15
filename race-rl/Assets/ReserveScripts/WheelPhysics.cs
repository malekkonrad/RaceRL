using UnityEngine;

public class WheelPhysics : MonoBehaviour
{
    [Header("Wheel Properties")]
    public float radius = 0.34f; // Typowy promień F1: 34cm
    public float mass = 12f; // Masa koła z oponą
    public float suspensionTravel = 0.1f; // 10cm ruchu zawieszenia
    public float suspensionStiffness = 35000f; // Sztywność sprężyny (N/m)
    public float damperRate = 3500f; // Tłumienie
    
    [Header("Tire Grip - Simplified Pacejka")]
    public float peakSlipRatio = 0.12f; // ~12% poślizgu dla max grip
    public float peakSlipAngle = 8f; // ~8° dla max lateral grip
    public float longitudinalStiffness = 2.0f;
    public float lateralStiffness = 1.8f;
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    
    // Runtime data
    private Rigidbody carRigidbody;
    private float suspensionLength;
    private float previousSuspensionLength;
    private Vector3 wheelVelocityLS; // Local space velocity
    private bool isGrounded;
    
    void Start()
    {
        carRigidbody = GetComponentInParent<Rigidbody>();
        suspensionLength = suspensionTravel;
        previousSuspensionLength = suspensionLength;
    }
    
    void FixedUpdate()
    {

        RaycastHit hit;
        Vector3 rayStart = transform.position;
        Vector3 rayDirection = -transform.up;
        float rayLength = 1.0f; // Prosty test
        
        isGrounded = Physics.Raycast(rayStart, rayDirection, out hit, rayLength);
        
        if (isGrounded)
        {
            // Bardzo prosta siła w górę
            float distance = hit.distance;
            float desiredDistance = 0.5f; // 50cm nad ziemią
            
            if (distance < desiredDistance)
            {
                float compressionAmount = desiredDistance - distance;
                float force = compressionAmount * 10000f; // Prosty mnożnik
                
                carRigidbody.AddForceAtPosition(transform.up * force, hit.point);
                
                Debug.Log($"{gameObject.name}: Distance={distance:F2}, Force={force:F0}");
            }
        }
        
        previousSuspensionLength = suspensionLength;


        // return;
        // 1. Raycast w dół od pozycji koła
        // WAŻNE: rayStart musi być w centrum zawieszenia (góra), nie w centrum koła!
        // RaycastHit hit;
        // Vector3 rayStart = transform.position + transform.up * suspensionTravel;
        // Vector3 rayDirection = -transform.up;
        // float rayLength = suspensionTravel + radius;
        
        // isGrounded = Physics.Raycast(rayStart, rayDirection, out hit, rayLength);
        
        // if (isGrounded)
        // {
        //     // 2. Oblicz kompresję zawieszenia
        //     float groundDistance = hit.distance;
        //     suspensionLength = Mathf.Clamp(groundDistance - radius, 0, suspensionTravel);
        //     float compression = suspensionTravel - suspensionLength;
        //     float compressionRatio = compression / suspensionTravel;
            
        //     // 3. Oblicz prędkość kompresji (dla dampera)
        //     float compressionVelocity = (previousSuspensionLength - suspensionLength) / Time.fixedDeltaTime;
            
        //     // 4. Siła zawieszenia (sprężyna + damper)
        //     float springForce = suspensionStiffness * compression;
        //     float damperForce = damperRate * compressionVelocity;
        //     float suspensionForce = springForce + damperForce;
            
        //     // 5. Dodaj masę koła do obciążenia
        //     float wheelLoad = suspensionForce + (mass * 9.81f);
            
        //     // 6. Oblicz prędkość koła w local space
        //     Vector3 contactPointVelocity = carRigidbody.GetPointVelocity(hit.point);
        //     wheelVelocityLS = transform.InverseTransformDirection(contactPointVelocity);
            
        //     // 7. Oblicz slip ratio (poślizg podłużny) i slip angle (poślizg boczny)
        //     float forwardSpeed = wheelVelocityLS.z;
        //     float lateralSpeed = wheelVelocityLS.x;
            
        //     // Slip ratio: (wheel_speed - ground_speed) / ground_speed
        //     // Dla uproszczenia zakładamy, że koło nie ma własnej prędkości obrotowej na razie
        //     float slipRatio = 0; // To rozbudujemy później z silnikiem/hamulcami
        //     if (Mathf.Abs(forwardSpeed) > 0.1f)
        //     {
        //         slipRatio = Mathf.Clamp(-forwardSpeed / Mathf.Abs(forwardSpeed), -1f, 1f);
        //     }
            
        //     // Slip angle w stopniach
        //     float slipAngle = 0;
        //     if (Mathf.Abs(forwardSpeed) > 0.1f)
        //     {
        //         slipAngle = -Mathf.Atan2(lateralSpeed, Mathf.Abs(forwardSpeed)) * Mathf.Rad2Deg;
        //     }
            
        //     // 8. Uproszczony model Pacejka (funkcja sinus)
        //     float normalizedSlipRatio = slipRatio / peakSlipRatio;
        //     float normalizedSlipAngle = slipAngle / peakSlipAngle;
            
        //     // Grip multiplier używając sin dla uproszczenia
        //     float longitudinalGrip = Mathf.Sin(normalizedSlipRatio * Mathf.PI * 0.5f) * longitudinalStiffness;
        //     float lateralGrip = Mathf.Sin(Mathf.Clamp(normalizedSlipAngle, -1f, 1f) * Mathf.PI * 0.5f) * lateralStiffness;
            
        //     // 9. Oblicz siły w local space
        //     float longitudinalForce = longitudinalGrip * wheelLoad;
        //     float lateralForce = lateralGrip * wheelLoad;
            
        //     // 10. Konwertuj siły do world space i aplikuj
        //     Vector3 suspensionForceVector = transform.up * suspensionForce;
        //     Vector3 longitudinalForceVector = transform.forward * longitudinalForce;
        //     Vector3 lateralForceVector = transform.right * lateralForce;
            
        //     Vector3 totalForce = suspensionForceVector + longitudinalForceVector + lateralForceVector;
        //     carRigidbody.AddForceAtPosition(totalForce, hit.point);
            
        //     previousSuspensionLength = suspensionLength;
        // }
        // else
        // {
        //     // Koło w powietrzu - zawieszenie w pełnym rozciągnięciu
        //     suspensionLength = suspensionTravel;
        //     previousSuspensionLength = suspensionLength;
        // }
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Wizualizacja zawieszenia
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 rayStart = transform.position;
        Vector3 rayEnd = rayStart - transform.up * (suspensionTravel + radius);
        Gizmos.DrawLine(rayStart, rayEnd);
        
        // Wizualizacja koła
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rayStart - transform.up * (suspensionLength + radius), radius);
    }
    
    // Publiczne metody do kontroli z zewnątrz (dla silnika/hamulców)
    public void ApplyMotorTorque(float torque)
    {
        // TODO: Implementacja w następnym kroku
    }
    
    public void ApplyBrakeTorque(float torque)
    {
        // TODO: Implementacja w następnym kroku
    }
    
    public bool IsGrounded => isGrounded;
    public float WheelLoad => isGrounded ? suspensionStiffness * (suspensionTravel - suspensionLength) : 0;
}