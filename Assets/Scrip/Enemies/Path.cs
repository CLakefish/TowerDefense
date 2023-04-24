using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{
    [SerializeField, HideInInspector]
    List<Vector2> points;

    public bool isClosed;

    public Path(Vector2 centre)
    {
        points = new List<Vector2>
        {
            centre+Vector2.left,
            centre+(Vector2.left+Vector2.up)*.5f,
            centre + (Vector2.right+Vector2.down)*.5f,
            centre + Vector2.right
        };
    }

    public Vector2 this[int i]
    {
        get
        {
            return points[i];
        }
    }

    public bool IsClosed
    {
        get
        {
            return isClosed;
        }
        set
        {
            isClosed = IsClosed;
        }
    }

    public int NumPoints
    {
        get
        {
            return points.Count;
        }
    }

    public int NumSegments
    {
        get
        {
            return points.Count / 3;
        }
    }

    public void AddSegment(Vector2 anchorPos)
    {
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add((points[points.Count - 1] + anchorPos) * .5f);
        points.Add(anchorPos);
    }

    public void DeleteSegment(int i)
    {
        if (NumSegments <= 2 || isClosed && NumSegments <= 1) return;

        if (i == 0)
        {
            if (isClosed)
            {
                points[points.Count - 1] = points[2];
            }
            points.RemoveRange(0, 3);
        }
        else if (i == points.Count - 1 && !isClosed)
        {
            points.RemoveRange(i - 2, 3);
        }
        else
        {
            points.RemoveRange(i - 1, 3);
        }
    }

    public Vector2[] GetPointsInSegment(int i)
    {
        return new Vector2[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[Index(i * 3 + 3)] };
    }

    public void MovePoint(int i, Vector2 pos)
    {
        Vector2 deltaMove = pos - points[i];
        points[i] = pos;

        if (i % 3 == 0)
        {
            if (i + 1 < points.Count || isClosed)
            {
                points[Index(i + 1)] += deltaMove;
            }
            if (i - 1 >= 0 || isClosed)
            {
                points[Index(i - 1)] += deltaMove;
            }
        }
        else
        {
            bool nextPointIsAnchor = (i + 1) % 3 == 0;
            int correspondingControlIndex = (nextPointIsAnchor) ? i + 2 : i - 2;
            int anchorIndex = (nextPointIsAnchor) ? i + 1 : i - 1;

            if (correspondingControlIndex >= 0 && correspondingControlIndex < points.Count || isClosed)
            {
                float dst = (points[Index(anchorIndex)] - points[Index(correspondingControlIndex)]).magnitude;
                Vector2 dir = (points[Index(anchorIndex)] - pos).normalized;
                points[Index(correspondingControlIndex)] = points[Index(anchorIndex)] + dir * dst;
            }
        }
    }

    public Vector2[] PointSpacing(float spacing, float resolution = 1)
    {
        List<Vector2> pointsE = new List<Vector2>();

        pointsE.Add(points[0]);

        Vector2 prevPoint = points[0];
        float pointDist = 0;

        for (int sI = 0; sI < NumSegments; sI++)
        {
            Vector2[] p = GetPointsInSegment(sI);

            float cNL = Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2]) + Vector2.Distance(p[2], p[3]);
            float eCL = Vector2.Distance(p[0], p[3]) + cNL / 2f;
            int d = Mathf.CeilToInt(eCL * resolution * 10);

            float t = 0;
            while (t <= 1)
            {
                t += 1f/d;

                Vector2 pointC = MathHelp.Cubic(p[0], p[1], p[2], p[3], t);
                pointDist += Vector2.Distance(prevPoint, pointC);

                while (pointDist >= spacing)
                {
                    float pointDistOvershoot = pointDist - spacing;
                    Vector2 newP = pointC + (prevPoint - pointC).normalized * pointDistOvershoot;
                    pointsE.Add(newP);
                    pointDist = pointDistOvershoot;
                    prevPoint = newP;
                }

                prevPoint = pointC;
            }
        }

        return pointsE.ToArray();
    }

    int Index(int i)
    {
        return (i + points.Count) % points.Count;
    }

    public void ToggleClosed()
    {
        isClosed = !isClosed;

        if (isClosed)
        {
            points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
            points.Add(points[0] * 2 - points[1]);
        }
        else
        {
            points.RemoveRange(points.Count - 2, 2);
        }
    }
}

public static class MathHelp
{
    public static Vector2 Quadratic(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 p0 = Vector2.Lerp(a, b, t);
        Vector2 p1 = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(p0, p1, t);
    }

    public static Vector2 Cubic(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        Vector2 p0 = Quadratic(a, b, c, t);
        Vector2 p1 = Quadratic(b, c, d, t);

        return Vector2.Lerp(p0, p1, t);
    }
}