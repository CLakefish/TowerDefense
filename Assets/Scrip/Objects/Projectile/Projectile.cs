using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    internal int Pierce = 1;
    int pierceCount;
    internal int Damage = 1;
    public float deathTime;
    internal Rigidbody2D rb;
    private List<GameObject> hitObjs = new List<GameObject>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        pierceCount = Pierce;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 0)
        {
            if (!hitObjs.Contains(collision.gameObject))
            {
                hitObjs.Add(collision.gameObject);

                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                enemy.HealthCount -= Damage;
            }

        }

        if (collision.gameObject.layer != 0)
        {
            Destroy(gameObject);
        }

        pierceCount--;
    }

    private void Update()
    {
        Destroy(gameObject, deathTime);

        if (pierceCount <= 0 || hitObjs.Count >= Pierce)
        {
            Destroy(gameObject);
            hitObjs.Clear();
        }
    }
}
