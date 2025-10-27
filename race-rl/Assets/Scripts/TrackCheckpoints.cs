using System;
using System.Collections.Generic;
using UnityEngine;

public class TrackCheckpoints : MonoBehaviour
{
    [SerializeField] private List<Transform> carTransformList;  // TODO:
    //  problem że trzeba ręcznie dodawać model z bolidu - bo to ona ma box collider a nie sam empty więc trochę chujnia 
    private List<CheckpointSingle> checkpointSingleList;
    private List<int> nextCheckpointIndexList;



    public event EventHandler<CarCheckpointEventArgs> OnCarCorrectCheckpoint;
    public event EventHandler<CarCheckpointEventArgs> OnCarWrongCheckpoint;

    public class CarCheckpointEventArgs : EventArgs
    {
        public Transform carTransform;
    }


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
            OnCarCorrectCheckpoint?.Invoke(this, new CarCheckpointEventArgs { carTransform = carTransform });
        }
        else
        {
            Debug.Log("Wrong:");
            OnCarWrongCheckpoint?.Invoke(this, new CarCheckpointEventArgs { carTransform = carTransform });
        }
    }

    public CheckpointSingle GetNexCheckpoint(Transform carTransform)
    {
        int carIndex = carTransformList.IndexOf(carTransform);
        if (carIndex == -1) return checkpointSingleList[0];
        
        return checkpointSingleList[nextCheckpointIndexList[carIndex]];
    }

    public void ResetCheckpoint(Transform carTransform)
    {
        int carIndex = carTransformList.IndexOf(carTransform);
        if (carIndex != -1)
        {
            nextCheckpointIndexList[carIndex] = 0;
        }
    }

}
