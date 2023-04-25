using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;


[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    // Followed along with this: 

    PathCreator creator;
    Path path;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.TextArea("Usage: \n > Left-Click and hold Left-Shift to add points.\n > Right-Click to remove points.\n > Click and drag the white orbs to change curve");
        GUILayout.Space(5);

        if (GUILayout.Button("Create new"))
        {
            creator.CreatePath();
            path = creator.path;
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Toggle closed"))
        {
            Undo.RecordObject(creator, "Toggle closed");
            path.ToggleClosed();
        }
    }

    void OnSceneGUI()
    {
        Input();
        Draw();
    }

    // Mouse Input
    void Input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            Undo.RecordObject(creator, "Add segment");
            path.AddSegment(mousePos);
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float minDist = .05f;
            int closestI = -1;

            for (int i = 0; i < path.NumPoints; i += 3)
            {
                float dst = Vector2.Distance(mousePos, path[i]);
                if (dst < minDist)
                {
                    closestI = i;
                    minDist = dst;
                }
            }

            if (closestI != 1)
            {
                Undo.RecordObject(creator, "Delete segment");
                path.DeleteSegment(closestI);
            }
            else if (closestI == -1)
            {
                Debug.Log("Outside of Array. Stop");
            }
        }
    }

    void Draw()
    {
        // Path drawing
        for (int i = 0; i < path.NumSegments; i++)
        {
            Vector2[] points = path.GetPointsInSegment(i);
            Handles.color = Color.black;
            Handles.DrawLine(points[1], points[0]);
            Handles.DrawLine(points[2], points[3]);
            Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.green, null, 2);
        }

        // Point drawing
        Handles.color = Color.white;
        for (int i = 0; i < path.NumPoints; i++)
        {
            Vector3 newPos = Handles.FreeMoveHandle(path[i], Quaternion.identity, .1f, Vector3.zero, Handles.CylinderHandleCap);
            if (path[i] != newPos)
            {
                Undo.RecordObject(creator, "Move point");
                path.MovePoint(i, newPos);
            }
        }
    }

    // Setting path points
    void OnEnable()
    {
        creator = (PathCreator)target;
        if (creator.path == null)
        {
            creator.CreatePath();
        }
        path = creator.path;
    }
}

public class PathCreator : MonoBehaviour
{
    public Vector3[] pos;
    public GameObject Holder;

    [HideInInspector]
    public Path path;

    private void Awake()
    {
        pos = path.PointSpacing(0.25f);

        for (int j = 0; j < pos.Length; j += 6)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p.transform.position = new Vector3(pos[j].x, pos[j].y, 20f);
            p.transform.localScale = new Vector3(.25f, .25f, 1);
            p.transform.parent = Holder.transform;
        }
    }

    public void CreatePath()
    {
        path = new Path(transform.position);
    }
}
