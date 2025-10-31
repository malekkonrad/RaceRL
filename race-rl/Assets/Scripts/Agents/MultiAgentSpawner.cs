using UnityEngine;

public class MultiAgentSpawner : MonoBehaviour
{
    [SerializeField] private GameObject agentPrefab;              
    [SerializeField] private TrackCheckpoints trackCheckpoints;
    [SerializeField] private Transform[] spawnPoints;             // to potencjalne do pozycji startowych w wyścigach - na razie nie ma większeog znaczenia ale nie zapomnieć
    [SerializeField] private int agentsToSpawn = 8;               // liczba spanów

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