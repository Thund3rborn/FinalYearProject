using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeshGenerator_TESTING : MonoBehaviour
{
    public Material material;
    public Vector3[] vertices = new Vector3[4];

    void Start()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        vertices = new Vector3[] {
            new Vector3(-0.97f, -12.99f, 6.92f),
            new Vector3(3.35f, -12.99f, 7.12f),
            new Vector3(3.54f, -12.99f, 2.94f),
            new Vector3(-0.73f, -12.99f, 2.62f)
        };

        mesh.vertices = vertices;
        mesh.triangles = new int[] { 0, 1, 2, 2, 3, 0 };


        GameObject gameObject = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer));
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        gameObject.GetComponent<MeshRenderer>().material = material;
    }

    private void Update()
    {
        
    }
}
