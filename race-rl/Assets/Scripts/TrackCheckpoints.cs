using System.Collections.Generic;
using UnityEngine;

public class TrackCheckpoints : MonoBehaviour
{
    [SerializeField] private List<Transform> carTransformList;  // TODO:
    //  problem że trzeba ręcznie dodawać model z bolidu - bo to ona ma box collider a nie sam empty więc trochę chujnia 
    private List<CheckpointSingle> checkpointSingleList;
    private List<int> nextCheckpointIndexList;

    private void Awake()
    {
        Transform checkpointsTransform = transform.Find("Checkpoints");

        checkpointSingleList = new List<CheckpointSingle>();

        foreach (Transform checkpointSingleTransform in checkpointsTransform)
        {
            CheckpointSingle checkpointSingle = checkpointSingleTransform.GetComponent<CheckpointSingle>();

            checkpointSingle.SetTrackCheckpoints(this);

            checkpointSingleList.Add(checkpointSingle);
        }

        nextCheckpointIndexList = new List<int>();
        foreach (Transform carTransform in carTransformList)
        {
            nextCheckpointIndexList.Add(0);
        }
    }


    public void AgentThroughCheckpoint(CheckpointSingle checkpointSingle, Transform carTransform)
    {
        int nextCheckpointIndex = nextCheckpointIndexList[carTransformList.IndexOf(carTransform)];
        if (checkpointSingleList.IndexOf(checkpointSingle) == nextCheckpointIndex)
        {
            Debug.Log("Correct");
            nextCheckpointIndexList[carTransformList.IndexOf(carTransform)] = (nextCheckpointIndex + 1) % checkpointSingleList.Count;   
        }
        else
        {
            Debug.Log("Wrong:");
        }
    }

}
