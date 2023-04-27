using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using UnityEditor;
using System.IO;

[CustomEditor(typeof(TowerHandler))]
[CanEditMultipleObjects]

public class TowerViewEditor : Editor
{
    SerializedProperty bulletCount,
                       currentStateFire,
                       projectile,
                       projectileSpeed,
                       viewRadius,
                       FOV,
                       findDelay;

    SerializedProperty projectileDeathTime,
                       projectilePierce,
                       targetLayer,
                       objectLayer;

    SerializedProperty projectileDamage;

    private void OnEnable()
    {
        bulletCount = serializedObject.FindProperty("bulletCount");
        currentStateFire = serializedObject.FindProperty("fireType");
        projectile = serializedObject.FindProperty("projectile");
        projectileSpeed = serializedObject.FindProperty("projectileSpeed");
        viewRadius = serializedObject.FindProperty("viewRadius");
        FOV = serializedObject.FindProperty("viewAngle");
        findDelay = serializedObject.FindProperty("findDelay");
        projectileDeathTime = serializedObject.FindProperty("projectileDeathTime");
        targetLayer = serializedObject.FindProperty("targetMask");
        objectLayer = serializedObject.FindProperty("objectMask");
        projectilePierce = serializedObject.FindProperty("projectilePierce");
        projectileDamage = serializedObject.FindProperty("projectileDamage");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Tower Projectile Settings", EditorStyles.boldLabel);

        EditorGUILayout.Separator();

        EditorGUILayout.ObjectField(projectile);

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(currentStateFire);
        TowerHandler.FireType st = (TowerHandler.FireType)currentStateFire.enumValueIndex;

        EditorGUILayout.Separator();

        switch (st)
        {
            case (TowerHandler.FireType.Single):

                EditorGUILayout.Separator();

                EditorGUILayout.Slider(findDelay, 0, 3, "         Fire Rate");
                EditorGUILayout.Slider(projectileSpeed, 0, 100, "         Bullet Speed");
                EditorGUILayout.Slider(projectileDeathTime, 0, 2, "         Bullet Death Time");

                EditorGUILayout.Separator();

                EditorGUILayout.IntSlider(projectileDamage, 1, 100, "         Projectile Damage");
                EditorGUILayout.IntSlider(projectilePierce, 1, 10, "         Projectile Pierce");

                break;

            case (TowerHandler.FireType.Multi):

                EditorGUILayout.Separator();

                EditorGUILayout.IntSlider(bulletCount, 0, 25, "         Bullet Count");

                EditorGUILayout.Slider(findDelay, 0, 3, "         Fire Rate");
                EditorGUILayout.Slider(projectileSpeed, 0, 100, "         Bullet Speed");
                EditorGUILayout.Slider(projectileDeathTime, 0, 2, "         Bullet Death Time");
                break;

            case (TowerHandler.FireType.Radial):

                EditorGUILayout.Separator();

                EditorGUILayout.IntSlider(bulletCount, 0, 200, "         Bullet Count");
                EditorGUILayout.Slider(findDelay, 0, 3, "         Fire Rate");
                EditorGUILayout.Slider(projectileSpeed, 0, 100, "         Bullet Speed");
                EditorGUILayout.Slider(projectileDeathTime, 0, 2, "         Bullet Death Time"); 
                break;

            case (TowerHandler.FireType.AOE):

                break;
        }
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(targetLayer);
        EditorGUILayout.PropertyField(objectLayer);

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        TowerHandler fov = (TowerHandler)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.forward, Vector3.right, 360, fov.viewRadius);

        Vector3 angleA = fov.AngleDir(-fov.viewAngle / 2, false),
                angleB = fov.AngleDir(fov.viewAngle / 2, false);

        Handles.DrawLine(fov.transform.position, fov.transform.position + angleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + angleB * fov.viewRadius);

        if (fov.fireType == TowerHandler.FireType.Radial)
        {
            for (int i = 0; i < fov.bulletCount; i++)
            {
                float degreesPer = Mathf.PI * 2 / fov.bulletCount;
                Vector2 dir = new Vector2((float)Mathf.Cos(i * degreesPer), (float)Mathf.Sin(i * degreesPer));

                Debug.DrawLine(fov.transform.position, fov.transform.position + (Vector3)dir, Color.red);
            }
        }
    }

}


// Keep tac shooter bullets within range by destroying thems
public class TowerHandler : MonoBehaviour
{

    #region Parameters

    public enum TargetType
    {
        First,
        Last,
        Random,
        Closest
    }

    public enum FireType
    {
        Single,
        Multi,
        Radial,
        AOE,
    }

    #endregion

    [HideInInspector]
    public TargetType targetType;

    [HideInInspector]
    public FireType fireType;

    private PathCreator creator;

    [HideInInspector]
    public int bulletCount;

    [HideInInspector]
    public float projectileSpeed;

    [HideInInspector]
    public float overshootFix;

    [HideInInspector]
    public float projectileDeathTime;

    [HideInInspector]
    public LayerMask targetMask,
                 objectMask;

    List<PathFollow> targets = new List<PathFollow>();

    [HideInInspector]
    public float viewRadius,
                 viewAngle,
                 findDelay;

    [HideInInspector]
    public int projectileDamage;

    [HideInInspector]
    public int projectilePierce;

    [HideInInspector]
    public Projectile projectile;
    Projectile p;
    bool isActive = false;

    // Start is called before the first frame update
    void Start()
    {
        creator = GameObject.FindGameObjectWithTag("PathCreator").GetComponent<PathCreator>();

        if (!isActive) StartCoroutine(TargetFind(findDelay));
    }

    IEnumerator TargetFind(float delay)
    {
        while (true)
        {
            isActive = true;

            if (isActive)
                ShootProjectile();

            yield return new WaitForSeconds(delay);
            isActive = false;
        }
    }

    void SpawnProjectile(Vector3 dir, bool normalize = true)
    {
        p = Instantiate(projectile, gameObject.transform.position, Quaternion.identity);

        p.Damage = projectileDamage;
        p.Pierce = projectilePierce;
        p.deathTime = projectileDeathTime;

        p.rb.AddForce(((normalize) ? (dir - transform.position).normalized : dir) * projectileSpeed, ForceMode2D.Impulse);
    }

    void ShootProjectile()
    {
        switch (fireType)
        {
            case (FireType.Single):
                FindTargets();
                break;

            case (FireType.Multi):
                FindTargets();
                break;

            case (FireType.Radial):
                FindTargets();

                for (int i = 0; i < bulletCount; i++)
                {
                    float degreesPer = Mathf.PI * 2 / bulletCount;
                    Vector2 dir = new Vector2((float)Mathf.Cos(i * degreesPer), (float)Mathf.Sin(i * degreesPer));

                    SpawnProjectile(dir, false);
                }

                break;

            case (FireType.AOE):
                FindTargets();
                break;
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

        if (targets.Count <= 0) return;

        Vector3 dir = targets[0].transform.position;

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

        SpawnProjectile(dir);
    }

    public Vector3 AngleDir(float angle, bool globalAngle)
    {
        if (!globalAngle)
        {
            angle += transform.eulerAngles.z;
        }

        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), -Mathf.Cos(angle * Mathf.Deg2Rad), 0);
    }


    #region EnemyDetection

    public Vector3 GetFirstEnemy(List<PathFollow> targets, GameObject g)
    {
        PathFollow farthest = targets[0];

        for (int i = 0; i < targets.Count; i++)
        {
            farthest = PathDetection.GreaterPathProgress(farthest, targets[i]);
        }

        Debug.DrawRay(g.transform.position, (farthest.transform.position - g.transform.position).normalized, Color.green, 0.1f);

        return PathDetection.CalculateBulletAhead(farthest, creator, 2.6f);
    }

    public Vector3 GetLastEnemy(List<PathFollow> targets, GameObject g)
    {
        PathFollow closest = targets[0];

        for (int i = 0; i < targets.Count; i++)
        {
            closest = PathDetection.LeastPathProgress(closest, targets[i]);
        }

        Debug.DrawRay(g.transform.position, (closest.transform.position - g.transform.position).normalized, Color.magenta, 0.1f);

        return PathDetection.CalculateBulletAhead(closest, creator, 2.6f);
    }

    public Vector3 GetRandomEnemy(List<PathFollow> targets, GameObject g)
    {
        PathFollow random = targets[Random.Range(0, targets.Count)];

        Debug.DrawRay(g.transform.position, (random.transform.position - g.transform.position).normalized, new Color(Random.Range(0f, 5.0f), Random.Range(0f, 5.0f), Random.Range(0f, 5.0f), 1), 0.1f);

        return PathDetection.CalculateBulletAhead(random, creator, 2.6f);
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

        return PathDetection.CalculateBulletAhead(closest, creator, 2.6f);
    }

    #endregion
}


// Detection logic
public static class PathDetection
{
    public static PathFollow GreaterPathProgress(PathFollow p, PathFollow p2)
    {
        if (p.j > p2.j) return p;
        else if (p.j < p2.j) return p2;
        else return p;
    }

    public static PathFollow LeastPathProgress(PathFollow p, PathFollow p2)
    {
        if (p.j > p2.j) return p2;
        else if (p.j < p2.j) return p;
        else return p;
    }

    public static Vector3 CalculateBulletAhead(PathFollow p, PathCreator creator, float overshootFix)
    {
        if (p.j >= creator.pos.Length || (p.j + Mathf.CeilToInt((p.speed * 10f) - overshootFix)) >= creator.pos.Length) return creator.pos[creator.pos.Length - 1];
        else return creator.pos[p.j + Mathf.CeilToInt((p.speed * 10f) - overshootFix)];
    }
}