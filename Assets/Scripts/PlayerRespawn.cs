using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    void Start()
    {
        if (Data.checkpointPosition.HasValue)
        {
            transform.position = Data.checkpointPosition.Value;
        }
    }
}
