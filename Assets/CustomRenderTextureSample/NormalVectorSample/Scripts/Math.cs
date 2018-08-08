using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Math
{
    private const float TOLERANCE = 1E-2f;

    static public bool PointOnPlane(Vector3 p, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 v1 = p2 - p1;
        Vector3 v2 = p3 - p1;
        Vector3 vp = p - p1;

        Vector3 nv = Vector3.Cross(v1, v2);
        float val = Vector3.Dot(nv.normalized, vp.normalized);

        return -TOLERANCE < val && val < TOLERANCE;
    }

    static public bool PointInTriangle(Vector3 p, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        //Vector3 a = Vector3.Cross(p2 - p1, p - p1).normalized;
        //Vector3 b = Vector3.Cross(p3 - p2, p - p2).normalized;
        //Vector3 c = Vector3.Cross(p1 - p3, p - p3).normalized;
        var a = Vector3.Cross(p1 - p3, p - p1).normalized;
        var b = Vector3.Cross(p2 - p1, p - p2).normalized;
        var c = Vector3.Cross(p3 - p2, p - p3).normalized;

        float dab = Vector3.Dot(a, b);
        float dbc = Vector3.Dot(b, c);

        bool bdab = (1 - TOLERANCE) < dab;
        bool bdbc = (1 - TOLERANCE) < dbc;

        return bdab && bdbc;
    }

    static public Vector3[] GetNearestPointsInMesh(Mesh mesh, Vector3 p)
    {
        List<Vector3> nearestPoints = new List<Vector3>();

        float sqrMinDist = float.MaxValue;
        int nearestIndex = -1;

        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            int idx = mesh.triangles[i];

            Vector3 p0 = mesh.vertices[idx];
            Vector3 delta = p0 - p;

            float sqrd = delta.sqrMagnitude;
            if (sqrd >= sqrMinDist)
            {
                continue;
            }

            sqrMinDist = sqrd;

            nearestIndex = idx;
        }

        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            if (mesh.triangles[i] != nearestIndex)
            {
                continue;
            }

            int m = i % 3;

            int idx0 = 0;
            int idx1 = 0;
            int idx2 = 0;

            switch (m)
            {
                case 0:
                    idx0 = i + 0;
                    idx1 = i + 1;
                    idx2 = i + 2;
                    break;

                case 1:
                    idx0 = i - 1;
                    idx1 = i + 0;
                    idx2 = i + 1;
                    break;

                case 2:
                    idx0 = i - 2;
                    idx1 = i - 1;
                    idx2 = i + 0;
                    break;
            }

            nearestPoints.Add(mesh.vertices[mesh.triangles[idx0]]);
            nearestPoints.Add(mesh.vertices[mesh.triangles[idx1]]);
            nearestPoints.Add(mesh.vertices[mesh.triangles[idx2]]);
        }

        return nearestPoints.ToArray();
    }
}
