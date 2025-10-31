using System;
using System.Collections.Generic;
using UnityEngine;

public class TrackCheckpoints : MonoBehaviour
{
    // [SerializeField] private List<Transform> carTransformList;  // TODO:
    //  problem że trzeba ręcznie dodawać model z bolidu - bo to ona ma box collider a nie sam empty więc trochę chujnia 


    private List<Transform> carTransformList = new List<Transform>();
    private List<int> nextCheckpointIndexList = new List<int>();
    private List<CheckpointSingle> checkpointSingleList = new List<CheckpointSingle>();
    
    private readonly Dictionary<Transform, int> carIndexLookup = new Dictionary<Transform, int>();

    public event EventHandler<CarCheckpointEventArgs> OnCarCorrectCheckpoint;
    public event EventHandler<CarCheckpointEventArgs> OnCarWrongCheckpoint;

    

    public class CarCheckpointEventArgs : EventArgs
    {
        public Transform carTransform;
    }


    private void Awake()
    {
        Transform checkpointsTransform = transform.Find("Checkpoints");

        foreach (Transform checkpointSingleTransform in checkpointsTransform)
        {
            CheckpointSingle checkpointSingle = checkpointSingleTransform.GetComponent<CheckpointSingle>();
            checkpointSingle.SetTrackCheckpoints(this);
            checkpointSingleList.Add(checkpointSingle);
        }


        // foreach (Transform carTransform in carTransformList)
        // {
        //     nextCheckpointIndexList.Add(0);
        // }
    }

    public void RegisterCar(Transform carTransform)
    {
        if (carTransform == null) return;
        if (carIndexLookup.ContainsKey(carTransform)) return;

        carTransformList.Add(carTransform);
        int idx = carTransformList.Count - 1;
        carIndexLookup[carTransform] = idx;
        nextCheckpointIndexList.Add(0);
    }

    public void UnregisterCar(Transform carTransform)
    {
        if (carTransform == null) return;
        if (!carIndexLookup.TryGetValue(carTransform, out int index)) return;

        carTransformList.RemoveAt(index);
        nextCheckpointIndexList.RemoveAt(index);
        carIndexLookup.Remove(carTransform);

        // Przemapuj indeksy po wycięciu
        for (int i = index; i < carTransformList.Count; i++)
        {
            carIndexLookup[carTransformList[i]] = i;
        }
    }





    public void AgentThroughCheckpoint(CheckpointSingle checkpointSingle, Transform carTransform)
    {
        if (!carIndexLookup.TryGetValue(carTransform, out int idx))
        {
            // Opcjonalnie: auto-rejestracja jeśli zapomniano
            RegisterCar(carTransform);
            if (!carIndexLookup.TryGetValue(carTransform, out idx)) return;
        }


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
