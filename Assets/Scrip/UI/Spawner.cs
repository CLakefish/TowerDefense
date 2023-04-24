using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject spawnObj;

    public void SpawnObj()
    {
        for (int i = 0; i < 5; i++)
        {
            Instantiate(spawnObj);
        }
    }
}
