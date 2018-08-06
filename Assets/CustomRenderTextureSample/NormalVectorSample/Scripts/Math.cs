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
}
