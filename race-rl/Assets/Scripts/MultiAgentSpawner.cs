using UnityEngine;

public class MultiAgentSpawner : MonoBehaviour
{
    [SerializeField] private GameObject agentPrefab;              // Prefab z komponentem RacistAgent + SimpleCar
    [SerializeField] private TrackCheckpoints trackCheckpoints;   // Ten sam dla wszystkich agentów (ok)
    [SerializeField] private Transform[] spawnPoints;             // Różne spawny dla agentów
    [SerializeField] private int agentsToSpawn = 8;               // Ilu agentów postawić (<= liczba spawnów)

    private void Start()
    {
        int count = Mathf.Min(agentsToSpawn, spawnPoints.Length);
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(agentPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
            var agent = go.GetComponent<RacistAgent>();
            agent.Init(trackCheckpoints, spawnPoints[i]);
        }
    }
}