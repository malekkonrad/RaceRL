// // using UnityEngine;


// // /// <summary>
// // /// Proste koło z zawieszeniem - używane w SimpleCar
// // /// Pomysł na rozbudowe:
// // /// - pacejka model koła - wtedy temperatura to mogło by być ciekawe
// // /// </summary>


// // public class SimpleWheel : MonoBehaviour
// // {
// //     [Header("Wheel Settings")]
// //     public float radius = 0.34f;
// //     public float suspensionDistance = 0.1f;
// //     public float springStrength = 1f;
// //     public float springDamper = .2f;

// //     [Header("Ground Filter")]
// //     public LayerMask groundLayers;



// //     [Header("Debug")]
// //     public bool showDebug = true;

// //     private Rigidbody carRb;
// //     private float lastLength;
// //     private bool isGrounded;

// //     void Start()
// //     {
// //         carRb = GetComponentInParent<Rigidbody>();
// //         lastLength = suspensionDistance;
// //     }

// //     void FixedUpdate()
// //     {
// //         /// NEW zmiana - sprawdzamy tylko odległość od konkretnych warstw - trzeba o tym pamiętać!!!!!!!!!
// //         // Raycast - sprawdza czy dotyka koło ziemi - wszyscy tak robią
// //         if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, suspensionDistance + radius, groundLayers, QueryTriggerInteraction.Ignore))
// //         {
// //             isGrounded = true;

// //             // Obliczamy jak bardzo sprężyna jest ściśnięta - inne rozwiązanie niż w SrcWheel ale działa tak samo dobrze a ma mniej zmiennych więc IMO lepiej chyba - przynajmniej jak na razie
// //             // nie ma pierdolenia się z ustalanie różnych specyficznych wartości - a to po prostu działa
// //             float currentLength = hit.distance - radius;
// //             float compression = suspensionDistance - currentLength;

// //             // Prędkość kompresji (dla dampera)
// //             float velocity = (lastLength - currentLength) / Time.fixedDeltaTime;

// //             // Siła sprężyny (w górę) 
// //             float springForce = (compression * springStrength) + (velocity * springDamper);     // zwiększyć springForce !!!! -> poszuakć jak to działa gdzie indziej
            
// //             carRb.AddForceAtPosition(transform.up * springForce, hit.point);

// //             // Siła boczna (friction) - GRIP - bo inaczej się bolid ślizga
// //             Vector3 wheelVelocity = carRb.GetPointVelocity(hit.point);
// //             float lateralVelocity = Vector3.Dot(wheelVelocity, transform.right);

// //             // Im większa prędkość na boki, tym większa siła hamująca (grip) - działa spoko wsm
// //             float lateralForce = -lateralVelocity * springForce * 0.5f;
// //             carRb.AddForceAtPosition(transform.right * lateralForce, hit.point);

// //             lastLength = currentLength;
// //         }
// //         else
// //         {
// //             isGrounded = false;
// //             lastLength = suspensionDistance;
// //         }
// //     }


// //     // Wizualizacja w edytorze - pomocna - dużo pierdolenia miałem z ustaleniem pozycji kół XD
// //     void OnDrawGizmos()
// //     {
// //         if (!showDebug) return;

// //         // Linia zawieszenia
// //         Gizmos.color = isGrounded ? Color.green : Color.red;
// //         Gizmos.DrawLine(transform.position, transform.position - transform.up * (suspensionDistance + radius));

// //         // Koło
// //         Gizmos.color = Color.yellow;
// //         Gizmos.DrawWireSphere(transform.position - transform.up * suspensionDistance, radius);
// //     }

// //     public bool IsGrounded() => isGrounded;
// // }



// // -------------------------------------------------------------------------------------------------------------------------------


using UnityEngine;

public class SimpleWheel : MonoBehaviour
{
    [Header("Wheel Settings")]
    public float radius = 0.34f;
    public float suspensionDistance = 0.2f; // Zwiększyłem nieco dla stabilności
    public float springStrength = 35000f;   // Zwiększ jeśli auto jest ciężkie (dla 1500kg masy)
    public float springDamper = 2500f;      // Tłumienie drgań

    [Header("Grip Settings")]
    // Jak mocno opona trzyma na boki (zwiększ dla efektu F1, zmniejsz dla driftu)
    public float sideStiffness = 1.5f; 
    // Krzywa tarcia: Oś X to poślizg (0-1), Oś Y to siła. 
    // Ustaw w inspektorze klucz (0,0), (0.2, 1), (1, 0.8) dla realistycznego zachowania
    public AnimationCurve frictionCurve = AnimationCurve.Linear(0, 0, 1, 1); 

    [Header("Ground Filter")]
    public LayerMask groundLayers;
    public bool showDebug = true;

    private Rigidbody carRb;
    private bool isGrounded;
    
    // Publiczny dostęp do stopnia kompresji dla Anti-Roll Bara w SimpleCar
    public float CompressionRatio { get; private set; } 

    void Start()
    {
        carRb = GetComponentInParent<Rigidbody>();
        
        // Domyślna krzywa jeśli nie ustawiona w inspektorze (tzw. Tarmac curve)
        if (frictionCurve.length == 0)
        {
            frictionCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.2f, 1.0f), // Peak grip
                new Keyframe(1.0f, 0.6f)  // Sliding grip
            );
        }
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, suspensionDistance + radius, groundLayers, QueryTriggerInteraction.Ignore))
        {
            isGrounded = true;

            // 1. SUSPENSION (Zawieszenie)
            Vector3 springDir = transform.up;
            Vector3 tireWorldVel = carRb.GetPointVelocity(transform.position);

            float offset = suspensionDistance - (hit.distance - radius);
            CompressionRatio = Mathf.Clamp01(offset / suspensionDistance);

            float vel = Vector3.Dot(springDir, tireWorldVel);
            float force = (offset * springStrength) - (vel * springDamper);
            
            // Aplikujemy siłę zawieszenia tylko jeśli jest dodatnia (nie przyciągamy do ziemi sprężyną)
            if (force > 0)
                carRb.AddForceAtPosition(springDir * force, hit.point);

            // 2. LATERAL FRICTION (Przyczepność boczna)
            // Oblicz prędkość boczną (ślizganie się na boki)
            float steeringAngle = Vector3.Dot(transform.right, tireWorldVel);
            
            // Normalizujemy poślizg względem prędkości (żeby przy małej prędkości nie szalało)
            // Im szybciej jedziemy, tym mniejszy kąt powoduje utratę przyczepności
            float slipFactor = Mathf.Clamp01(Mathf.Abs(steeringAngle) / (0.1f + carRb.linearVelocity.magnitude * 0.05f));
            
            // Pobieramy współczynnik z krzywej tarcia
            float gripFactor = frictionCurve.Evaluate(slipFactor);

            // Obliczamy siłę przeciwdziałającą poślizgowi
            // force (nacisk na oponę) * gripFactor * stiffness
            float desiredSideForce = -steeringAngle * force * sideStiffness * gripFactor;

            // Aplikujemy siłę (tarcie)
            carRb.AddForceAtPosition(transform.right * desiredSideForce, hit.point);
        }
        else
        {
            isGrounded = false;
            CompressionRatio = 0f;
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebug) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position - transform.up * (suspensionDistance + radius));
    }

    public bool IsGrounded() => isGrounded;
}
















// using UnityEngine;

// public class SimpleWheel : MonoBehaviour
// {
//     [Header("Suspension")]
//     public float suspensionDistance = 0.2f;
//     public float springStrength = 40000f;
//     public float springDamper = 3000f;
//     public float wheelRadius = 0.34f;

//     [Header("Grip Settings")]
//     [Range(0f, 1f)] 
//     public float forwardGrip = 0.9f; // 1.0 = zero buksowania kół
//     [Range(0f, 1f)] 
//     public float sidewaysGrip = 0.95f; // 1.0 = jazda jak po szynach (zero driftu)

//     [Header("Setup")]
//     public LayerMask groundLayer;
//     public bool showDebug = true;

//     private Rigidbody rb;
//     private bool isGrounded;
    
//     // Potrzebne do obliczeń
//     public bool IsGrounded() => isGrounded;

//     void Start()
//     {
//         rb = GetComponentInParent<Rigidbody>();
//     }

//     void FixedUpdate()
//     {
//         // Raycast startujemy nieco wyżej, żeby nie gubić ziemi przy dobiciu zawieszenia
//         Vector3 rayOrigin = transform.position + transform.up * 0.1f;
//         float rayDist = suspensionDistance + wheelRadius + 0.1f;

//         if (Physics.Raycast(rayOrigin, -transform.up, out RaycastHit hit, rayDist, groundLayer))
//         {
//             isGrounded = true;

//             // 1. ZAWIESZENIE (Uproszczone, ale stabilne)
//             Vector3 springDir = transform.up;
//             Vector3 tireVel = rb.GetPointVelocity(transform.position);
//             float offset = suspensionDistance - (hit.distance - wheelRadius - 0.1f);
//             float vel = Vector3.Dot(springDir, tireVel);
            
//             // Siła sprężyny tylko w górę
//             float force = (offset * springStrength) - (vel * springDamper);
//             if (force > 0)
//                 rb.AddForceAtPosition(springDir * force, transform.position); // Aplikujemy w punkcie mocowania koła dla stabilności

//             // 2. KASOWANIE POŚLIZGU (ARCADE PHYSICS)
//             // Obliczamy prędkość koła w jego lokalnym układzie
//             Vector3 localVel = transform.InverseTransformDirection(tireVel);

//             // Tłumienie prędkości bocznej (X) - to eliminuje "Ice Skating"
//             // Obliczamy siłę potrzebną do całkowitego zatrzymania poślizgu w tej klatce
//             float desiredXCorrection = -localVel.x * sidewaysGrip;
            
//             // Aplikujemy zmianę prędkości (ChangeVelocity ignoruje masę, więc jest super stabilne)
//             // Uwaga: Używamy AddForceAtPosition z ForceMode.VelocityChange dla natychmiastowej reakcji
//             // Dzielimy przez liczbę kół (4), żeby siła była rozłożona równomiernie
//             Vector3 lateralForce = transform.right * desiredXCorrection / Time.fixedDeltaTime;
//             rb.AddForceAtPosition(lateralForce * (rb.mass / 4f), transform.position);

//             // Tłumienie prędkości wzdłużnej (Z) przy puszczonym gazie (opcjonalne, dla lepszego hamowania)
//             // To działa jak naturalny opór toczenia
//             // float desiredZCorrection = -localVel.z * (1f - forwardGrip) * 0.1f;
//             // rb.AddForceAtPosition(transform.forward * desiredZCorrection * (rb.mass / 4f) / Time.fixedDeltaTime, transform.position);
//         }
//         else
//         {
//             isGrounded = false;
//         }
//     }

//     void OnDrawGizmos()
//     {
//         if (!showDebug) return;
//         Gizmos.color = Color.yellow;
//         Gizmos.DrawLine(transform.position, transform.position - transform.up * (suspensionDistance + wheelRadius));
//         if(isGrounded) Gizmos.DrawSphere(transform.position - transform.up * (suspensionDistance + wheelRadius), 0.1f);
//     }
// }