using UnityEngine;

public class TrackDetector : MonoBehaviour
{
    public LayerMask roadLayer; // ustaw warstwę „Road” w Unity dla toru

    void Update()
    {
        bool isOnTrack = Physics.Raycast(transform.position, Vector3.down, 3f, roadLayer);
        Debug.Log(isOnTrack ? "✅ On track" : "❌ OFF track!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 3f);
    }
}

