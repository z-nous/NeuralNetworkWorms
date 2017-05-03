using UnityEngine;
using System.Collections;

public class Food : MonoBehaviour {

    void OnCollisionEnter(Collision collision)
    {

        GameObject master = GameObject.FindGameObjectWithTag("GameMaster");
        master.GetComponent<GameMaster>().MinusFood(this);

    }
}
