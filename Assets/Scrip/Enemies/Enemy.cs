using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : PathFollow
{
    [Header("Variables")]
    public int HealthCount;

    // Update is called once per frame
    void Update()
    {
        if (HealthCount <= 0)
        {
            Destroy(gameObject);
        }
    }
}
