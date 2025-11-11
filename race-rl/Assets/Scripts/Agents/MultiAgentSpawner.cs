using System.Collections.Generic;
using UnityEngine;

public class MultiAgentSpawner : MonoBehaviour
{
    [SerializeField] private GameObject agentPrefab;              
    [SerializeField] private TrackCheckpoints trackCheckpoints;
    [SerializeField] private Transform[] spawnPoints;             // to potencjalne do pozycji startowych w wyścigach - na razie nie ma większeog znaczenia ale nie zapomnieć
    [SerializeField] private int agentsToSpawn = 3;               // liczba spanów


    private List<GameObject> spawned = new List<GameObject>();

    // private void Start()
    // {
    //     int count = Mathf.Min(agentsToSpawn, spawnPoints.Length);
    //     for (int i = 0; i < count; i++)
    //     {
    //         var go = Instantiate(agentPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
    //         var agent = go.GetComponent<RacistAgent>();
    //         agent.Init(trackCheckpoints, spawnPoints[i]);
    //     }
    // }
    // Remove auto-spawn from Start: LevelManager powinien wywołać RespawnAll po ustawieniu toru.
    private void Start()
    {
        // Intentionally left blank
    }

    private void SpawnAgents()
    {
        ClearSpawned();
        int count = Mathf.Min(agentsToSpawn, spawnPoints.Length);
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(agentPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
            var agent = go.GetComponent<RacistAgent>();
            agent?.Init(trackCheckpoints, spawnPoints[i]);
            spawned.Add(go);
        }
    }

    private void ClearSpawned()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] != null) Destroy(spawned[i]);
        }
        spawned.Clear();
    }

    public void RespawnAll()
    {
        SpawnAgents();
    }

    // public API: LevelManager ustawia aktualny tor i punkty startowe
    public void SetTrack(TrackCheckpoints tc, Transform[] spawns)
    {
        trackCheckpoints = tc;
        spawnPoints = spawns ?? new Transform[0];
    }
}