using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (FieldOfView))]
public class Tower : MonoBehaviour
{
    public enum TargetType
    {
        First, 
        Last,
        Random,
        Closest
    }

    public TargetType targetType;
    public PathCreator creator;
    public float projectileSpeed;
    public float overshootFix;

    void Start()
    {
        creator = GameObject.FindGameObjectWithTag("PathCreator").GetComponent<PathCreator>();
    }

    public Vector3 GetFirstEnemy(List<PathFollow> targets, GameObject g)
    {
        PathFollow farthest = targets[0];

        for (int i = 0; i < targets.Count; i++)
        {
            farthest = PathDetection.GreaterPathProgress(farthest, targets[i]);
        }

        Debug.DrawRay(g.transform.position, (farthest.transform.position - g.transform.position).normalized, Color.green, 0.1f);

        return PathDetection.CalculateBulletAhead(farthest, creator, overshootFix);
    }

    public Vector3 GetLastEnemy(List<PathFollow> targets, GameObject g)
    {
        PathFollow closest = targets[0];

        for (int i = 0; i < targets.Count; i++)
        {
            closest = PathDetection.LeastPathProgress(closest, targets[i]);
        }

        Debug.DrawRay(g.transform.position, (closest.transform.position - g.transform.position).normalized, Color.magenta, 0.1f);

        return PathDetection.CalculateBulletAhead(closest, creator, overshootFix);
    }

    public Vector3 GetRandomEnemy(List<PathFollow> targets, GameObject g)
    {
        PathFollow random = targets[Random.Range(0, targets.Count)];

        Debug.DrawRay(g.transform.position, (random.transform.position - g.transform.position).normalized, new Color(Random.Range(0f, 5.0f), Random.Range(0f, 5.0f), Random.Range(0f, 5.0f), 1), 0.1f);

        return PathDetection.CalculateBulletAhead(random, creator, overshootFix);
    }

    public Vector3 GetClosestEnemy(List<PathFollow> targets, GameObject g)
    {
        float closeDist = Mathf.Infinity;
        PathFollow closest = null;

        foreach (var t in targets)
        {
            float newDist = (t.gameObject.transform.position - g.transform.position).sqrMagnitude;


            if (newDist < closeDist)
            {
                closeDist = newDist;
                closest = t;
            }
        }

        Debug.DrawRay(g.transform.position, (closest.transform.position - g.transform.position).normalized, new Color(255, 102, 0, 1), 0.1f);

        return PathDetection.CalculateBulletAhead(closest, creator, overshootFix);
    }
}