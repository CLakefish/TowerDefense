using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollow : MonoBehaviour
{
    public PathCreator pathCreator;
    public float speed = 5;
    public int j;

    private void Start()
    {
        pathCreator = FindObjectOfType<PathCreator>().GetComponent<PathCreator>();
        StartCoroutine(Move());
    }

    public IEnumerator Move()
    {
        transform.position = pathCreator.path[0];

        while (true)
        {
            for (j = 0; j < pathCreator.pos.Length; j++)
            {
                transform.position = pathCreator.pos[j];

                yield return new WaitForSeconds(speed);
            }

            if (!pathCreator.path.isClosed)
            {
                Destroy(gameObject);
                break;
            }
        }

        yield return null;
    }
}
