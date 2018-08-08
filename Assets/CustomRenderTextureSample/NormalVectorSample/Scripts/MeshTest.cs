using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTest : MonoBehaviour
{
    [SerializeField]
    private Transform[] _points;

    [SerializeField]
    private Material _material;

    private GameObject _obj;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        if (_obj != null)
        {
            Destroy(_obj);
        }

        Mesh mesh = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        verts.Add(_points[0].position); 
        verts.Add(_points[1].position); 
        verts.Add(_points[2].position); 

        mesh.SetVertices(verts);
        mesh.SetIndices(new int[] { 0, 1, 2, }, MeshTopology.Triangles, 0);

        List<Vector2> uvs = new List<Vector2>();
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        mesh.SetUVs(0, uvs);

        _obj = new GameObject("TestMesh", typeof(MeshFilter), typeof(MeshRenderer));
        MeshFilter filter = _obj.GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer renderer = _obj.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = _material;

        _obj.AddComponent<MeshCollider>();
    }
}
