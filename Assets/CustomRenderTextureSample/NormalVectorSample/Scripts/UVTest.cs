using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVTest : MonoBehaviour
{
    private Vector3? _lastHit = null;
    private List<Vector3> _verts = new List<Vector3>();

    private void CheckUV()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit))
        {
            return;
        }

        _verts.Clear();
        _lastHit = null;

        Vector3 p = hit.transform.InverseTransformPoint(hit.point);
        _lastHit = p;

        MeshFilter filter = hit.transform.GetComponent<MeshFilter>();
        Mesh mesh = filter.sharedMesh;

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            #region 1. ある点Pがポリゴン平面に存在するか
            int idx0 = i + 0;
            int idx1 = i + 1;
            int idx2 = i + 2;

            Vector3 p1 = mesh.vertices[mesh.triangles[idx0]];
            Vector3 p2 = mesh.vertices[mesh.triangles[idx1]];
            Vector3 p3 = mesh.vertices[mesh.triangles[idx2]];

            if (!Math.PointOnPlane(p, p1, p2, p3))
            {
                continue;
            }
            #endregion 1. ある点Pがポリゴン平面に存在するか

            #region 2. 同一平面上に存在する点Pが三角形内部に存在するか
            _verts.Add(p1);
            _verts.Add(p2);
            _verts.Add(p3);

            if(!Math.PointInTriangle(p, p1, p2, p3))
            {
                continue;
            }
            #endregion 2. 同一平面上に存在する点Pが三角形内部に存在するか

            #region 3. 点PのUV座標を求める
            Vector2 uv1 = mesh.uv[mesh.triangles[idx0]];
            Vector2 uv2 = mesh.uv[mesh.triangles[idx1]];
            Vector2 uv3 = mesh.uv[mesh.triangles[idx2]];

            // PerspectiveCollect（投資射影を考慮したUV補間）
            Matrix4x4 mvp = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix * hit.transform.localToWorldMatrix;

            // 各点をProjectionSpaceへ変換
            Vector4 p1_p = mvp * p1;
            Vector4 p2_p = mvp * p2;
            Vector4 p3_p = mvp * p3;
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

            Vector2 uv = (((1f - u - v) * uv1 / p1_p.w) + (u * uv2 / p2_p.w) + (v * uv3 / p3_p.w)) * invW;
            #endregion 3. 点PのUV座標を求める

            Debug.Log(uv + " : " + hit.textureCoord);

            _lastHit = p;

            return;
        }
    }

    private void CheckUV2()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {
            var meshRenderer = hitInfo.transform.GetComponent<MeshFilter>();
            var mesh = meshRenderer.sharedMesh;
            for (var i = 0; i < mesh.triangles.Length; i += 3)
            {
                #region 1.ある点pが与えられた3点において平面上に存在するか

                var index0 = i + 0;
                var index1 = i + 1;
                var index2 = i + 2;

                var p1 = mesh.vertices[mesh.triangles[index0]];
                var p2 = mesh.vertices[mesh.triangles[index1]];
                var p3 = mesh.vertices[mesh.triangles[index2]];
                var p = hitInfo.transform.InverseTransformPoint(hitInfo.point);

                var v1 = p2 - p1;
                var v2 = p3 - p1;
                var vp = p - p1;

                var nv = Vector3.Cross(v1, v2);
                var val = Vector3.Dot(nv, vp);
                //適当に小さい少数値で誤差をカバー
                var suc = -0.00001f < val && val < 0.00001f;

                #endregion 1.ある点pが与えられた3点において平面上に存在するか

                #region 2.同一平面上に存在する点pが三角形内部に存在するか

                if (!suc)
                    continue;
                else
                {
                    var a = Vector3.Cross(p1 - p3, p - p1).normalized;
                    var b = Vector3.Cross(p2 - p1, p - p2).normalized;
                    var c = Vector3.Cross(p3 - p2, p - p3).normalized;

                    var d_ab = Vector3.Dot(a, b);
                    var d_bc = Vector3.Dot(b, c);

                    suc = 0.999f < d_ab && 0.999f < d_bc;
                }

                #endregion 2.同一平面上に存在する点pが三角形内部に存在するか

                #region 3.点pのUV座標を求める

                if (!suc)
                    continue;
                else
                {
                    var uv1 = mesh.uv[mesh.triangles[index0]];
                    var uv2 = mesh.uv[mesh.triangles[index1]];
                    var uv3 = mesh.uv[mesh.triangles[index2]];

                    //PerspectiveCollect(透視投影を考慮したUV補間)
                    Matrix4x4 mvp = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix * hitInfo.transform.localToWorldMatrix;
                    //各点をProjectionSpaceへの変換
                    Vector4 p1_p = mvp * p1;
                    Vector4 p2_p = mvp * p2;
                    Vector4 p3_p = mvp * p3;
                    Vector4 p_p = mvp * p;
                    //通常座標への変換(ProjectionSpace)
                    Vector2 p1_n = new Vector2(p1_p.x, p1_p.y) / p1_p.w;
                    Vector2 p2_n = new Vector2(p2_p.x, p2_p.y) / p2_p.w;
                    Vector2 p3_n = new Vector2(p3_p.x, p3_p.y) / p3_p.w;
                    Vector2 p_n = new Vector2(p_p.x, p_p.y) / p_p.w;
                    //頂点のなす三角形を点pにより3分割し、必要になる面積を計算
                    var s = 0.5f * ((p2_n.x - p1_n.x) * (p3_n.y - p1_n.y) - (p2_n.y - p1_n.y) * (p3_n.x - p1_n.x));
                    var s1 = 0.5f * ((p3_n.x - p_n.x) * (p1_n.y - p_n.y) - (p3_n.y - p_n.y) * (p1_n.x - p_n.x));
                    var s2 = 0.5f * ((p1_n.x - p_n.x) * (p2_n.y - p_n.y) - (p1_n.y - p_n.y) * (p2_n.x - p_n.x));
                    //面積比からuvを補間
                    var u = s1 / s;
                    var v = s2 / s;
                    var w = 1 / ((1 - u - v) * 1 / p1_p.w + u * 1 / p2_p.w + v * 1 / p3_p.w);
                    var uv = w * ((1 - u - v) * uv1 / p1_p.w + u * uv2 / p2_p.w + v * uv3 / p3_p.w);

                    //uvが求まったよ!!!!
                    Debug.Log("2: " + uv + ":" + hitInfo.textureCoord);
                    return;
                }

                #endregion 3.点pのUV座標を求める
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckUV();
            //CheckUV2();
        }
    }

    private void OnDrawGizmos()
    {
        if (_lastHit == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_lastHit.Value, 0.01f);

        Gizmos.color = Color.cyan;
        for (int i = 0; i < _verts.Count; i++)
        {
            Gizmos.DrawWireSphere(_verts[i], 0.005f);
        }
    }
}
