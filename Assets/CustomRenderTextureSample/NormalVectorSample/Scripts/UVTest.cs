using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVTest : MonoBehaviour
{
    [SerializeField]
    private Transform _testPoint;

    [SerializeField]
    private Transform _testTarget;

    private Vector3? _lastHit = null;
    private List<Vector3> _verts = new List<Vector3>();

    private Vector3[] _triangle = new Vector3[3];
    private Vector3 _n;
    private Vector3 _delta;
    private bool _hasDone = false;

    private IEnumerator CheckUV()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit))
        {
            //return;
            yield break;
        }

        _verts.Clear();
        _lastHit = null;
        _hasDone = false;

        //Vector3 p = _testTarget.InverseTransformPoint(_testPoint.position);
        //MeshFilter filter = _testTarget.transform.GetComponent<MeshFilter>();

        Vector3 p = hit.transform.InverseTransformPoint(hit.point);
        MeshFilter filter = hit.transform.GetComponent<MeshFilter>();

        Mesh mesh = filter.sharedMesh;

        _lastHit = p;

        Vector3[] nearestPoints = Math.GetNearestPointsInMesh(mesh, p);
        _verts.AddRange(nearestPoints);

        for (int i = 0; i < nearestPoints.Length; i += 3)
        {
            #region 1. 同一平面上に存在する点Pが三角形内部に存在するか
            int idx0 = i + 0;
            int idx1 = i + 1;
            int idx2 = i + 2;

            Vector3 p0 = nearestPoints[idx0];
            Vector3 p1 = nearestPoints[idx1];
            Vector3 p2 = nearestPoints[idx2];

            Plane plane = new Plane(p0, p1, p2);
            Vector3 projected = plane.ClosestPointOnPlane(p);

            _triangle[0] = p0;
            _triangle[1] = p1;
            _triangle[2] = p2;
            _n = plane.normal;
            _lastHit = projected;

            if (!Math.PointInTriangle(projected, p0, p1, p2))
            {
                yield return new WaitForSeconds(1f);
                continue;
            }
            #endregion 1. 同一平面上に存在する点Pが三角形内部に存在するか

            #region 3. 点PのUV座標を求める
            Vector2 uv1 = mesh.uv[mesh.triangles[idx0]];
            Vector2 uv2 = mesh.uv[mesh.triangles[idx1]];
            Vector2 uv3 = mesh.uv[mesh.triangles[idx2]];

            // PerspectiveCollect（投資射影を考慮したUV補間）
            Matrix4x4 mvp = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix * _testTarget.transform.localToWorldMatrix;

            // 各点をProjectionSpaceへ変換
            Vector4 p1_p = mvp * p0;
            Vector4 p2_p = mvp * p1;
            Vector4 p3_p = mvp * p2;
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
            #endregion 2. 点PのUV座標を求める

            Debug.Log(uv + " : " + hit.textureCoord);

            _hasDone = true;

            //return;
            yield break;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(CheckUV());
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

        Gizmos.color = _hasDone ? Color.blue : Color.cyan;
        for (int i = 0; i < _verts.Count; i++)
        {
            Gizmos.DrawWireSphere(_verts[i], 0.005f);
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < _triangle.Length; i++)
        {
            Gizmos.DrawLine(_triangle[i], _triangle[i] + _n * 0.1f);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_testPoint.position, _testPoint.position - _delta);
    }
}
