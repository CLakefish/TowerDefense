using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    internal int Pierce = 1;
    int pierceCount;
    internal int Damage = 1;
    internal Rigidbody2D rb;
    private List<GameObject> hitObjs = new List<GameObject>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        pierceCount = Pierce;
        Destroy(gameObject, .7f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 0)
        {
            if (hitObjs.Contains(collision.gameObject)) return;

            hitObjs.Add(collision.gameObject);

            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            enemy.HealthCount -= Damage;
        }

        if (collision.gameObject.layer != 0)
        {
            Destroy(gameObject);
        }

        pierceCount--;
    }

    private void Update()
    {
        if (pierceCount == 0 || hitObjs.Count == Pierce)
        {
            Destroy(gameObject);
        }

        if (hitObjs.Count == Pierce)
        {
            hitObjs.Clear();
        }
    }
}