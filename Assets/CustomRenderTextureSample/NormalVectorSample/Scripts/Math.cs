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

    static public Vector2 GetPerspectiveCollectedUV(Vector2[] uvs, Vector3 p, Vector3[] points, Matrix4x4 mvp)
    {
        // 各点をProjectionSpaceへ変換
        Vector4 p1_p = mvp * points[0];
        Vector4 p2_p = mvp * points[1];
        Vector4 p3_p = mvp * points[2];
        Vector4 p_p = mvp * p;

        // 通常座標（同次座標）への変換（w除算）
        Vector2 p1_n = new Vector2(p1_p.x, p1_p.y) / p1_p.w;
        Vector2 p2_n = new Vector2(p2_p.x, p2_p.y) / p2_p.w;
        Vector2 p3_n = new Vector2(p3_p.x, p3_p.y) / p3_p.w;
        Vector2 p_n = new Vector2(p_p.x, p_p.y) / p_p.w;

        // 頂点のなす三角形を点Pにより分割し、必要になる面積を計算
        float s = 0.5f * ((p2_n.x - p1_n.x) * (p3_n.y - p1_n.y) - (p2_n.y - p1_n.y) * (p3_n.x - p1_n.x));
        float s1 = 0.5f * ((p3_n.x - p_n.x) * (p1_n.y - p_n.y) - (p3_n.y - p_n.y) * (p1_n.x - p_n.x));
        float s2 = 0.5f * ((p1_n.x - p_n.x) * (p2_n.y - p_n.y) - (p1_n.y - p_n.y) * (p2_n.x - p_n.x));

        // 面積比からUVを補間
        float u = s1 / s;
        float v = s2 / s;
        float w = ((1f - u - v) * 1f / p1_p.w) + (u * 1f / p2_p.w) + (v * 1f / p3_p.w);
        float invW = 1f / w;

        Vector2 uv = (((1f - u - v) * uvs[0] / p1_p.w) + (u * uvs[1] / p2_p.w) + (v * uvs[2] / p3_p.w)) * invW;

        return uv;
    }

    static public Vector3[] GetTangentSpaceVectors(Vector3[] p, Vector2[] uv)
    {
        // UVベクトルを求めるための3頂点を、UV座標を利用して定義する

        // X方向
        Vector3[] cp0 = new[]
        {
            new Vector3(p[0].x, uv[0].x, uv[0].y),
            new Vector3(p[0].y, uv[0].x, uv[0].y),
            new Vector3(p[0].z, uv[0].x, uv[0].y),
        };

        // Y方向
        Vector3[] cp1 = new[]
        {
            new Vector3(p[1].x, uv[1].x, uv[1].y),
            new Vector3(p[1].y, uv[1].x, uv[1].y),
            new Vector3(p[1].z, uv[1].x, uv[1].y),
        };

        // Z方向
        Vector3[] cp2 = new[]
        {
            new Vector3(p[2].x, uv[2].x, uv[2].y),
            new Vector3(p[2].y, uv[2].x, uv[2].y),
            new Vector3(p[2].z, uv[2].x, uv[2].y),
        };

        // UV方向の接ベクトル、順法線ベクトルを計算する
        Vector3 u = Vector3.zero;
        Vector3 v = Vector3.zero;

        // U,VベクトルそれぞれのX, Y, Z要素を計算する
        // 考え方は、辺の外積が法線方向を向くことを利用して
        // 平面の方程式から各パラメータを算出することで求める
        for (int i = 0; i < 3; i++)
        {
            Vector3 v1 = cp1[i] - cp0[i];
            Vector3 v2 = cp2[i] - cp0[i];
            Vector3 ABC = Vector3.Cross(v1, v2).normalized;

            if (ABC.x == 0)
            {
                Debug.LogWarning("ポリゴンかUV上のポリゴンが縮退しています");
                return new[] { Vector3.zero, Vector3.zero };
            }

            u[i] = -(ABC.y / ABC.x);
            v[i] = -(ABC.z / ABC.x);
        }

        u.Normalize();
        v.Normalize();

        return new[] { u, v };
    }
}
