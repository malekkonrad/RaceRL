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


// using UnityEngine;

// public class SimpleWheel : MonoBehaviour
// {
//     [Header("Wheel Settings")]
//     public float radius = 0.34f;
//     public float suspensionDistance = 0.2f; // Zwiększyłem nieco dla stabilności
//     public float springStrength = 35000f;   // Zwiększ jeśli auto jest ciężkie (dla 1500kg masy)
//     public float springDamper = 2500f;      // Tłumienie drgań

//     [Header("Grip Settings")]
//     // Jak mocno opona trzyma na boki (zwiększ dla efektu F1, zmniejsz dla driftu)
//     public float sideStiffness = 1.5f; 
//     // Krzywa tarcia: Oś X to poślizg (0-1), Oś Y to siła. 
//     // Ustaw w inspektorze klucz (0,0), (0.2, 1), (1, 0.8) dla realistycznego zachowania
//     public AnimationCurve frictionCurve = AnimationCurve.Linear(0, 0, 1, 1); 

//     [Header("Ground Filter")]
//     public LayerMask groundLayers;
//     public bool showDebug = true;

//     private Rigidbody carRb;
//     private bool isGrounded;

//     // WPISZ TO:
//     float castRadius = 0.15f; // Promień "szukania" ziemi (trochę mniejszy niż szerokość opony)
    
//     // Publiczny dostęp do stopnia kompresji dla Anti-Roll Bara w SimpleCar
//     public float CompressionRatio { get; private set; } 

//     void Start()
//     {
//         carRb = GetComponentInParent<Rigidbody>();
        
//         // Domyślna krzywa jeśli nie ustawiona w inspektorze (tzw. Tarmac curve)
//         if (frictionCurve.length == 0)
//         {
//             frictionCurve = new AnimationCurve(
//                 new Keyframe(0f, 0f),
//                 new Keyframe(0.2f, 1.0f), // Peak grip
//                 new Keyframe(1.0f, 0.6f)  // Sliding grip
//             );
//         }
//     }

//     void FixedUpdate()
//     {
//         if (Physics.SphereCast(transform.position, castRadius, -transform.up, out RaycastHit hit, suspensionDistance + radius, groundLayers, QueryTriggerInteraction.Ignore))
//         {

//             float currentDistanceToGround = Vector3.Distance(transform.position, hit.point);
            
//             isGrounded = true;

//             // 1. SUSPENSION (Zawieszenie)
//             Vector3 springDir = transform.up;
//             Vector3 tireWorldVel = carRb.GetPointVelocity(transform.position);

//             float offset = suspensionDistance - (currentDistanceToGround - radius);
//             CompressionRatio = Mathf.Clamp01(offset / suspensionDistance);

//             float vel = Vector3.Dot(springDir, tireWorldVel);
//             float force = (offset * springStrength) - (vel * springDamper);
            
//             // Aplikujemy siłę zawieszenia tylko jeśli jest dodatnia (nie przyciągamy do ziemi sprężyną)
//             if (force > 0)
//                 carRb.AddForceAtPosition(springDir * force, hit.point);

//             // 2. LATERAL FRICTION (Przyczepność boczna)
//             // Oblicz prędkość boczną (ślizganie się na boki)
//             float steeringAngle = Vector3.Dot(transform.right, tireWorldVel);
            
//             // Normalizujemy poślizg względem prędkości (żeby przy małej prędkości nie szalało)
//             // Im szybciej jedziemy, tym mniejszy kąt powoduje utratę przyczepności
//             float slipFactor = Mathf.Clamp01(Mathf.Abs(steeringAngle) / (0.1f + carRb.linearVelocity.magnitude * 0.05f));
            
//             // Pobieramy współczynnik z krzywej tarcia
//             float gripFactor = frictionCurve.Evaluate(slipFactor);

//             // Obliczamy siłę przeciwdziałającą poślizgowi
//             // force (nacisk na oponę) * gripFactor * stiffness
//             float desiredSideForce = -steeringAngle * force * sideStiffness * gripFactor;

//             // Aplikujemy siłę (tarcie)
//             carRb.AddForceAtPosition(transform.right * desiredSideForce, hit.point);
//         }
//         else
//         {
//             isGrounded = false;
//             CompressionRatio = 0f;
//         }
//     }

//     void OnDrawGizmos()
//     {
//         if (!showDebug) return;
//         Gizmos.color = isGrounded ? Color.green : Color.red;
//         Gizmos.DrawLine(transform.position, transform.position - transform.up * (suspensionDistance + radius));
//     }

//     public bool IsGrounded() => isGrounded;
// }






using UnityEngine;

public class SimpleWheel : MonoBehaviour
{
    [Header("Wheel Settings")]
    public float radius = 0.34f;        // Wizualny promień koła
    public float castRadius = 0.15f;    // Promień "czujnika" (fizycznej kuli)
    public float suspensionDistance = 0.2f;
    public float springStrength = 35000f;
    public float springDamper = 2500f;

    [Header("Grip Settings")]
    public float sideStiffness = 1.5f;
    public AnimationCurve frictionCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Ground Filter")]
    public LayerMask groundLayers;
    public bool showDebug = true;

    private Rigidbody carRb;
    private bool isGrounded;

    public float CompressionRatio { get; private set; }

    void Start()
    {
        carRb = GetComponentInParent<Rigidbody>();

        // Zabezpieczenie krzywej
        if (frictionCurve.length == 0)
        {
            frictionCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.2f, 1.0f),
                new Keyframe(1.0f, 0.6f)
            );
        }
    }

    void FixedUpdate()
    {
        // POPRAWKA 1: Długość SphereCasta. 
        // Chcemy, żeby spód kuli sięgał tam, gdzie spód koła przy max wyproście.
        // Dystans środka = (max_zasięg_koła) - (promień_kuli_czujnika)
        float maxRayLength = (suspensionDistance + radius) - castRadius;

        if (Physics.SphereCast(transform.position, castRadius, -transform.up, out RaycastHit hit, maxRayLength, groundLayers, QueryTriggerInteraction.Ignore))
        {
            isGrounded = true;
            
            // Obliczamy faktyczną odległość od środka koła do punktu styku
            float currentDistanceToGround = hit.distance + castRadius; 
            // Alternatywnie Vector3.Distance(transform.position, hit.point) też jest OK, 
            // ale hit.distance jest bardziej stabilny przy SphereCast na płaskim.

            // 1. SUSPENSION
            Vector3 springDir = transform.up;
            Vector3 tireWorldVel = carRb.GetPointVelocity(transform.position);

            // Ile sprężyna jest ściśnięta?
            float offset = suspensionDistance - (currentDistanceToGround - radius);
            CompressionRatio = Mathf.Clamp01(offset / suspensionDistance);

            float vel = Vector3.Dot(springDir, tireWorldVel);
            float force = (offset * springStrength) - (vel * springDamper);

            // --- POPRAWKA 2 (KLUCZOWA): LIMIT SIŁY ---
            // Zapobiega wystrzeleniu w kosmos, jeśli offset błędnie wyjdzie ogromny.
            // Limit = Masa Auta * Grawitacja * Margines (np. 15G przeciążenia)
            // Zakładając masę auta ~1500kg, maxForce ~ 220,000. To wystarczy, by skakać, ale nie by zbugować Unity.
            float maxForce = carRb.mass * 15f * 9.81f; 
            force = Mathf.Clamp(force, 0f, maxForce); 
            // -----------------------------------------

            if (force > 0)
                carRb.AddForceAtPosition(springDir * force, hit.point);

            // 2. LATERAL FRICTION
            float steeringAngle = Vector3.Dot(transform.right, tireWorldVel);
            
            // Normalizacja poślizgu
            float slipFactor = Mathf.Clamp01(Mathf.Abs(steeringAngle) / (0.1f + carRb.linearVelocity.magnitude * 0.05f));
            float gripFactor = frictionCurve.Evaluate(slipFactor);

            // Siła boczna
            float desiredSideForce = -steeringAngle * force * sideStiffness * gripFactor;
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

        // Rysujemy linię maksymalnego zasięgu
        Vector3 endPos = transform.position - transform.up * (suspensionDistance + radius);
        Gizmos.DrawLine(transform.position, endPos);

        // Rysujemy kulę (SphereCast) w miejscu trafienia lub na końcu
        if (isGrounded)
        {
            // Rysujemy tam gdzie trafiło (symulacja SphereCasta)
            // Musimy odjąć castRadius, żeby narysować środek kuli w dobrym miejscu
            // (ponieważ SphereCast zwraca hit.distance do środka kuli)
            // Ale dla uproszczenia wizualizacji w Gizmos:
             Gizmos.DrawWireSphere(endPos, castRadius); // To pokazuje cel
        }
        else
        {
             Gizmos.DrawWireSphere(endPos, castRadius);
        }
    }

    public bool IsGrounded() => isGrounded;
}