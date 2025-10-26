using UnityEngine;

public class TrackDetector : MonoBehaviour
{
    public Transform[] wheels;
    public LayerMask roadLayer;

    public int WheelsOnRoad()
    {
        int wheelsOnRoad = 0;

        foreach (var wheel in wheels)
        {
            if (Physics.Raycast(wheel.position, Vector3.down, 1f, roadLayer))
            {
                wheelsOnRoad++;
            }
        }

        return wheelsOnRoad;
    }

    void Update()
    {
        if (WheelsOnRoad() == 0)
        {
            Debug.Log("Wszystkie koła poza torem!");
        }
        else
        {
            Debug.Log("Koła na torze: " + WheelsOnRoad());
        }
    }
}

