using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject spawnObj;

    public void SpawnObj()
    {
        Instantiate(spawnObj);
    }
}
