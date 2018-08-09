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

    /// <summary>
    /// メッシュの頂点郡から、与えられた点に一番近い頂点を探す
    /// </summary>
    /// <param name="mesh">対象メッシュ</param>
    /// <param name="point">調べる点</param>
    /// <param name="nearestPoints">一番近い点が含まれるポリゴンの頂点リスト</param>
    /// <param name="nearestUVs">一番近い点が含まれるポリゴンのUVリスト</param>
    static public void GetNearestPointsInMesh(Mesh mesh, Vector3 point, out Vector3[] nearestPoints, out Vector2[] nearestUVs)
    {
        List<Vector3> nearestPointsList = new List<Vector3>();
        List<Vector2> nearestUVList = new List<Vector2>();

        float sqrMinDist = float.MaxValue;
        int nearestIndex = -1;

        #region ### 一番近い頂点を探す ###
        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            int idx = mesh.triangles[i];

            Vector3 p0 = mesh.vertices[idx];
            Vector3 delta = p0 - point;

            float sqrd = delta.sqrMagnitude;
            if (sqrd >= sqrMinDist)
            {
                continue;
            }

            sqrMinDist = sqrd;

            nearestIndex = idx;
        }
        #endregion ### 一番近い頂点を探す ###

        #region ### 見つかった一番近い頂点のindexからポリゴン頂点のリストを生成する ###
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

            nearestPointsList.Add(mesh.vertices[mesh.triangles[idx0]]);
            nearestPointsList.Add(mesh.vertices[mesh.triangles[idx1]]);
            nearestPointsList.Add(mesh.vertices[mesh.triangles[idx2]]);

            nearestUVList.Add(mesh.uv[mesh.triangles[idx0]]);
            nearestUVList.Add(mesh.uv[mesh.triangles[idx1]]);
            nearestUVList.Add(mesh.uv[mesh.triangles[idx2]]);
        }
        #endregion ### 見つかった一番近い頂点のindexからポリゴン頂点のリストを生成する ###

        // Variables out.
        nearestPoints = nearestPointsList.ToArray();
        nearestUVs = nearestUVList.ToArray();
    }
}
