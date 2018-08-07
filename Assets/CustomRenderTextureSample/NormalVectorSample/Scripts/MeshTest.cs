using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTest : MonoBehaviour
{
    [SerializeField]
    private Transform[] _points;

    [SerializeField]
    private Material _material;

    private void Start()
    {
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

        GameObject obj = new GameObject("TestMesh", typeof(MeshFilter), typeof(MeshRenderer));
        MeshFilter filter = obj.GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = _material;

        obj.AddComponent<MeshCollider>();
    }
}
