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
    public float suspensionDistance = 0.1f;
    public float springStrength = 1f;
    public float springDamper = .2f;

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
            float springForce = (compression * springStrength) + (velocity * springDamper);     // zwiększyć springForce !!!! -> poszuakć jak to działa gdzie indziej
            
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
// //     public float suspensionDistance = 0.3f;
// //     public float springStrength = 25000f;
// //     public float springDamper = 2500f;

// //     // new section
// //     [Header("Tire Model (Simplified Pacejka)")]
// //     public float maxLateralGrip = 3.0f;  // Multiplier na lateral force
// //     public float maxLongitudinalGrip = 1.5f;  // Multiplier na motor/brake
// //     public float slipAngleSensitivity = 0.05f;  // Jak szybko rośnie slip

// //     [Header("Debug")]
// //     public bool showDebug = true;



// //     // private 

// //     private Rigidbody carRb;
// //     private float lastLength;
// //     private bool isGrounded;

// //     private float normalForce;  // Ważne dla obliczenia grip'u
// //     private float slipAngle;



// //     private float smoothedLateralForce = 0f;  // Dodaj do private fields
// //     private float lastLateralForce = 0f;



// //     void Start()
// //     {
// //         carRb = GetComponentInParent<Rigidbody>();
// //         lastLength = suspensionDistance;
// //     }

// //     void FixedUpdate()
// //     {
// //         // Raycast - sprawdza czy dotyka koło ziemi - wszyscy tak robią
// //         if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, suspensionDistance + radius))
// //         {
// //             isGrounded = true;

// //             // Obliczamy jak bardzo sprężyna jest ściśnięta - inne rozwiązanie niż w SrcWheel ale działa tak samo dobrze a ma mniej zmiennych więc IMO lepiej chyba - przynajmniej jak na razie
// //             // nie ma pierdolenia się z ustalanie różnych specyficznych wartości - a to po prostu działa
// //             float currentLength = hit.distance - radius;
// //             float compression = suspensionDistance - currentLength;

// //             // Prędkość kompresji (dla dampera)
// //             float velocity = (lastLength - currentLength) / Time.fixedDeltaTime;

// //             // Siła sprężyny (w górę) ->  NORMAL FORCE - TO JEST KLUCZOWE DLA GRIP'U
// //             float springForce = (compression * springStrength) + (velocity * springDamper);
// //             normalForce = Mathf.Max(0, springForce);  // Zapisz normal force
// //             carRb.AddForceAtPosition(transform.up * springForce, hit.point);

// //             // Siła boczna (friction) - GRIP - bo inaczej się bolid ślizga
// //             Vector3 wheelVelocity = carRb.GetPointVelocity(hit.point);
// //             float lateralVelocity = Vector3.Dot(wheelVelocity, transform.right);
// //             float forwardVelocity = Vector3.Dot(wheelVelocity, transform.forward);


// //             // Slip angle w radianach, bezpośrednio do obliczenia grip'u
// //             slipAngle = Mathf.Atan2(lateralVelocity, Mathf.Max(2f, Mathf.Abs(forwardVelocity))) * Mathf.Rad2Deg;
// //             float gripMultiplier = Mathf.Clamp01(1f - (Mathf.Abs(slipAngle) / 90f));


// //             // Lateral force - zależy od normal force i grip multiplier'a
// //             float maxLateralForce = normalForce * maxLateralGrip * gripMultiplier;



// //             float targetLateralForce = -Mathf.Sign(lateralVelocity) * maxLateralForce;
// //             smoothedLateralForce = Mathf.Lerp(smoothedLateralForce, targetLateralForce, Time.fixedDeltaTime * 10f);



// //             carRb.AddForceAtPosition(transform.right * smoothedLateralForce, hit.point);

// //             lastLength = currentLength;

// //             if (showDebug && isGrounded)
// //                 Debug.Log($"Normal: {normalForce}, Lateral: {lateralVelocity}, Grip: {gripMultiplier}");
// //         }
// //         else
// //         {
// //             isGrounded = false;
// //             lastLength = suspensionDistance;
// //             normalForce = 0;
// //             smoothedLateralForce = 0;
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
// //     public float GetNormalForce() => normalForce;
// //     public float GetSlipAngle() => slipAngle;
// // }


// using UnityEngine;

// public class SimpleWheel : MonoBehaviour
// {
//     public bool isSteering;     // zaznacz TRUE dla przednich kół
//     public Transform steerParent; // obiekt nad tym kołem który skręca (np. pusty GameObject)

//     public void Rotate(float wheelRotation)
//     {
//         transform.localRotation = Quaternion.Euler(wheelRotation, 0f, 0f);
//     }

//     public void Steer(float steerAngle)
//     {
//         if (isSteering && steerParent != null)
//             steerParent.localRotation = Quaternion.Euler(0f, steerAngle, 0f);
//     }
// }
