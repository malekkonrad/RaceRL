using UnityEngine;


/// <summary>
/// Proste koło z zawieszeniem - używane w SimpleCar
/// Pomysł na rozbudowe:
/// - pacejka model koła - wtedy temperatura to mogło by być ciekawe
/// </summary>



public class SimpleWheel : MonoBehaviour
{
    [Header("Wheel Settings")]
    public float radius = 0.34f;
    public float suspensionDistance = 0.3f;
    public float springStrength = 25000f;
    public float springDamper = 2500f;

    [Header("Debug")]
    public bool showDebug = true;

    private Rigidbody carRb;
    private float lastLength;
    private bool isGrounded;

    void Start()
    {
        carRb = GetComponentInParent<Rigidbody>();
        lastLength = suspensionDistance;
    }

    void FixedUpdate()
    {
        // Raycast - sprawdza czy dotyka koło ziemi - wszyscy tak robią
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, suspensionDistance + radius))
        {
            isGrounded = true;

            // Obliczamy jak bardzo sprężyna jest ściśnięta - inne rozwiązanie niż w SrcWheel ale działa tak samo dobrze a ma mniej zmiennych więc IMO lepiej chyba - przynajmniej jak na razie
            // nie ma pierdolenia się z ustalanie różnych specyficznych wartości - a to po prostu działa
            float currentLength = hit.distance - radius;
            float compression = suspensionDistance - currentLength;

            // Prędkość kompresji (dla dampera)
            float velocity = (lastLength - currentLength) / Time.fixedDeltaTime;

            // Siła sprężyny (w górę) 
            float springForce = (compression * springStrength) + (velocity * springDamper);
            carRb.AddForceAtPosition(transform.up * springForce, hit.point);

            // Siła boczna (friction) - GRIP - bo inaczej się bolid ślizga
            Vector3 wheelVelocity = carRb.GetPointVelocity(hit.point);
            float lateralVelocity = Vector3.Dot(wheelVelocity, transform.right);

            // Im większa prędkość na boki, tym większa siła hamująca (grip) - działa spoko wsm
            float lateralForce = -lateralVelocity * springForce * 0.5f;
            carRb.AddForceAtPosition(transform.right * lateralForce, hit.point);

            lastLength = currentLength;
        }
        else
        {
            isGrounded = false;
            lastLength = suspensionDistance;
        }
    }


    // Wizualizacja w edytorze - pomocna - dużo pierdolenia miałem z ustaleniem pozycji kół XD
    void OnDrawGizmos()
    {
        if (!showDebug) return;

        // Linia zawieszenia
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position - transform.up * (suspensionDistance + radius));

        // Koło
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position - transform.up * suspensionDistance, radius);
    }

    public bool IsGrounded() => isGrounded;
}