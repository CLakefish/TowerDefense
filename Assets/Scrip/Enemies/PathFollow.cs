using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollow : MonoBehaviour
{
    public PathCreator pathCreator;
    public float speed = 5;
    public float pauseSeconds;
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
                transform.position = Vector3.Lerp(transform.position, pathCreator.pos[j], speed);

                yield return new WaitForSeconds(pauseSeconds);
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
