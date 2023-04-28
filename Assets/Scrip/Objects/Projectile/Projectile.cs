using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ProjectileType
    {
        Dart,
        Curved,
        Follow,
        FollowCurved,
        Instant,
    }

    internal int Pierce = 1;
    int pierceCount;
    internal int Damage = 1;
    public float deathTime;
    internal Rigidbody2D rb;
    public List<GameObject> targets = new List<GameObject>();
    private List<GameObject> hitObjs = new List<GameObject>();

    public ProjectileType projectileType;


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

        if (collision.gameObject.layer != 0 && projectileType != ProjectileType.Follow)
        {
            Destroy(gameObject);
        }

        pierceCount--;
    }

    private void Update()
    {
        Destroy(gameObject, deathTime);

        if (projectileType != ProjectileType.Follow && (pierceCount <= 0 || hitObjs.Count >= Pierce))
        {
            Destroy(gameObject);
            hitObjs.Clear();
        }
    }

    public void Move(Vector3 dir, bool normalize, float projectileSpeed)
    {
        switch (projectileType)
        {
            case (ProjectileType.Dart):
                rb.AddForce(((normalize) ? (dir - transform.position).normalized : dir) * projectileSpeed, ForceMode2D.Impulse);
                break;

            case (ProjectileType.Curved):

                break;

            case (ProjectileType.Follow):
                StartCoroutine(follow(projectileSpeed));
                Debug.Log(targets.Count);

                break;

            case (ProjectileType.FollowCurved):

                break;

            case (ProjectileType.Instant):

                break;
        }
    }

    IEnumerator follow(float projectileSpeed)
    {
        int i = 0;

        while (gameObject != null)
        {
            if (hitObjs.Contains(targets[i]) && targets.Count > 1 && targets[i++] != null) i++;

            rb.AddForce((targets[i].transform.position - transform.position).normalized * projectileSpeed, ForceMode2D.Impulse);
            rb.velocity = Vector2.ClampMagnitude(rb.velocity, projectileSpeed);
            yield return new WaitForEndOfFrame();
        }
    }


}
