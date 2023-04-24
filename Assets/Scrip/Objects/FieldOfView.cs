using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(FieldOfView))]

public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        FieldOfView fov = (FieldOfView)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.forward, Vector3.right, 360, fov.viewRadius);

        Vector3 angleA = fov.AngleDir(-fov.viewAngle / 2, false),
                angleB = fov.AngleDir(fov.viewAngle / 2, false);

        Handles.DrawLine(fov.transform.position, fov.transform.position + angleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + angleB * fov.viewRadius);
    }

}

[RequireComponent (typeof (Tower))]
public class FieldOfView : Tower
{
    public LayerMask targetMask,
                     objectMask;

    public List<PathFollow> targets = new List<PathFollow>();

    public float viewRadius,
                 viewAngle,
                 findDelay;

    public Projectile projectile;
    Transform dir;
    bool isActive = false;

    IEnumerator TargetFind(float delay)
    {
        while (true)
        {
            isActive = true;

            if (isActive)
                FindTargets();

            yield return new WaitForSeconds(delay);
            isActive = false;
        }
    }

    void FindTargets()
    {
        targets.Clear();
        Collider2D[] targetsInRad = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInRad.Length; i++)
        {
            Transform target = targetsInRad[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics2D.Raycast(transform.position, dirToTarget, dstToTarget, objectMask))
                {
                    targets.Add(target.GetComponent<PathFollow>());
                }
            }
        }

        if (targets.Count >= 2)
        {
            switch (targetType)
            {
                case (TargetType.First):
                    dir = GetFirstEnemy(targets, gameObject);
                    break;

                case (TargetType.Last):
                    dir = GetLastEnemy(targets, gameObject);
                    break;

                case (TargetType.Random):
                    dir = GetRandomEnemy(targets, gameObject);
                    break;

                case (TargetType.Closest):
                    dir = GetClosestEnemy(targets, gameObject);
                    break;
            }
        }
        else if (targets.Count == 1)
        {
            dir = targets[0].transform;
            Debug.DrawRay(transform.position, (targets[0].transform.position - transform.position).normalized, Color.red, .3f);
        }
        else return;

        Projectile p = Instantiate(projectile, gameObject.transform.position, Quaternion.identity);
        p.rb.AddForce((dir.transform.position - transform.position).normalized * 50f, ForceMode2D.Impulse);
    }

    public Vector3 AngleDir(float angle, bool globalAngle)
    {
        if (!globalAngle)
        {
            angle += transform.eulerAngles.z;
        }

        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), -Mathf.Cos(angle * Mathf.Deg2Rad), 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!isActive) StartCoroutine(TargetFind(findDelay));
    }
}
