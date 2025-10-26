using UnityEngine;

public class CheckpointSingle : MonoBehaviour
{
    private TrackCheckpoints trackCheckpoints;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log("Checkpoint tag reached at position: " + this.transform.position);
            trackCheckpoints.AgentThroughCheckpoint(this, other.transform);
        }

        // if (other.TryGetComponent<SimpleCar>(out var player))
        // {
        //     // CheckpointManager.Instance.SetCurrentCheckpoint(this.transform);
        //     Debug.Log("Checkpoint reached at position: " + this.transform.position);
        // }
    }

    public void SetTrackCheckpoints(TrackCheckpoints trackCheckpoints)
    {
        this.trackCheckpoints = trackCheckpoints;
    }
}
