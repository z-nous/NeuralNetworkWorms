using UnityEngine;
using System.Collections;

public class WormTail : MonoBehaviour {

    public Transform TailSpawnPoint;

    public Transform GetSpawnPoint()
    {
        return TailSpawnPoint;
    }
    void Update()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }
}
