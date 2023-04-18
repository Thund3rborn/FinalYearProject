using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class CreateRoad : MonoBehaviour
{
    public MousePos mousePos;
    public Material material;
    //public float roadWidth = 1f;
    public float roadHeightOffset = 0f;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Button button;

    private bool curvedBuildingMode, straightBuildingMode;

    private Vector3 startPoint = Vector3.zero;
    private Vector3 controlPoint = Vector3.zero;
    private Vector3 endPoint = Vector3.zero;
    private Vector3 keepTrackOfEndPoint = Vector3.zero;
    private List<List<Vector3>> listOfPositionLists = new List<List<Vector3>>();
    int counter = 1;

    int sizeOfArr;
    Vector3[] theLine;

    public float sphereRadius = 0.5f;



    private Vector3[] roadPoints;
    public float roadWidth = 2f;
    private GameObject roadPreview;
    private MeshFilter meshFilter;
    public Mesh roadMesh;

    // Start is called before the first frame update
    void Start()
    {
        // Get the MeshFilter component
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        roadPreview = new GameObject("Road Preview", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));

        // Create the road mesh
        //CreateRoadMesh();
        sizeOfArr = 0;
    }

    // Update is called once per frame
    void Update()
    {
        {
            CurvedRoad();
        }

        ProcessInput();
    }

    //create curved road
    void CurvedRoad()
    {
        //create a line along which the road will be created
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        Vector2[] line = new Vector2[sizeOfArr];
        //Mesh roadMesh = new Mesh();

        if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask))
        {
            if (startPoint == Vector3.zero)
            {
                startPoint = raycastHit.point;
            }
            else if (controlPoint == Vector3.zero)
            {
                controlPoint = raycastHit.point;
            }
            else if (endPoint == Vector3.zero)
            {
                endPoint = raycastHit.point;
                keepTrackOfEndPoint = endPoint;
                CreateRoadObject();
            }
        }
        else if (startPoint != Vector3.zero && controlPoint != Vector3.zero && endPoint != Vector3.zero)
        {
            double distance = GetDistanceBetweenPoints();
            sizeOfArr = (int)Math.Round(distance) + 1;

            for (int i = 0; i < sizeOfArr; i++)
            {
                double t = i / (double)(sizeOfArr - 1);
                line[i] = quadratic(new Vector2(startPoint.x, startPoint.z), new Vector2(controlPoint.x, controlPoint.z), new Vector2(endPoint.x, endPoint.z), (float)t);
            }

            roadPoints = new Vector3[sizeOfArr];

            for (int i = 0; i < line.Length; i++)
            {
                roadPoints[i].x = line[i].x;
                roadPoints[i].y = SnapPointToTerrainBelow(roadPoints[i]) + 0.05f;
                roadPoints[i].z = line[i].y;
            }

            startPoint = endPoint;
            endPoint = Vector3.zero;
            controlPoint = Vector3.zero;
        }
        else
        //update preview
        {
            if (startPoint != Vector3.zero && controlPoint == Vector3.zero && endPoint == Vector3.zero && Physics.Raycast(ray, out raycastHit, float.MaxValue, layerMask))
            {
                double distance = GetDistanceBetweenPoints();
                sizeOfArr = (int)Math.Round(distance) + 1;

                for (int i = 0; i < sizeOfArr; i++)
                {
                    double t = i / (double)(sizeOfArr - 1);
                    line[i] = linear(new Vector2(startPoint.x, startPoint.z), new Vector2(raycastHit.point.x, raycastHit.point.z), (float)t);
                }

                roadPoints = new Vector3[sizeOfArr];

                for (int i = 0; i < line.Length; i++)
                {
                    roadPoints[i].x = line[i].x;
                    roadPoints[i].y = SnapPointToTerrainBelow(roadPoints[i]) + 0.05f;
                    roadPoints[i].z = line[i].y;
                }

                PreviewMeshUpdate();
            }
            else if (startPoint != Vector3.zero && controlPoint != Vector3.zero && Physics.Raycast(ray, out raycastHit, float.MaxValue, layerMask))
            {

                double distance = GetDistanceBetweenPoints();
                sizeOfArr = (int)Math.Round(distance) + 1;

                for (int i = 0; i < sizeOfArr; i++)
                {
                    double t = i / (double)(sizeOfArr - 1);
                    line[i] = quadratic(new Vector2(startPoint.x, startPoint.z), new Vector2(controlPoint.x, controlPoint.z), new Vector2(raycastHit.point.x, raycastHit.point.z), (float)t);
                }

                roadPoints = new Vector3[sizeOfArr];

                for (int i = 0; i < line.Length; i++)
                {
                    roadPoints[i].x = line[i].x;
                    roadPoints[i].y = SnapPointToTerrainBelow(roadPoints[i]) + 0.05f;
                    roadPoints[i].z = line[i].y;
                }

                PreviewMeshUpdate();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            startPoint = Vector3.zero;
            endPoint = Vector3.zero;
            controlPoint = Vector3.zero;
            roadMesh.Clear();
        }
    }

    void CreateRoadObject()
    {
        roadMesh.name = "Road Mesh";

        // Get the number of points in the road
        int numPoints = roadPoints.Length;

        // Calculate the number of vertices and triangles needed
        int numVertices = numPoints * 2;
        int numTriangles = (numPoints - 1) * 2;

        // Create arrays to hold the vertices, triangles, and UVs
        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numTriangles * 3];
        Vector2[] uvs = new Vector2[numVertices];

        // Loop through the points in the road
        for (int i = 0; i < numPoints; i++)
        {
            // Calculate the index of the vertices for this segment
            int vertexIndex = i * 2;

            // Calculate the position of the vertices for this segment
            Vector3 segmentDirection = (i < numPoints - 1) ? (roadPoints[i + 1] - roadPoints[i]).normalized : (roadPoints[i] - roadPoints[i - 1]).normalized;
            Vector3 segmentNormal = Vector3.Cross(segmentDirection, Vector3.up * 2).normalized;
            Vector3 vertex1 = roadPoints[i] + segmentNormal * roadWidth / 2f;
            Vector3 vertex2 = roadPoints[i] - segmentNormal * roadWidth / 2f;

            // Add the vertices to the array
            vertices[vertexIndex] = vertex1;
            vertices[vertexIndex + 1] = vertex2;

            // Calculate the UVs for the vertices
            float uvX = (float)i / (float)(numPoints - 1);
            uvs[vertexIndex] = new Vector2(0, uvX);
            uvs[vertexIndex + 1] = new Vector2(1, uvX);

            // Add the triangles to the array
            if (i < numPoints - 1)
            {
                int triangleIndex = i * 6;
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + 1;
                triangles[triangleIndex + 2] = vertexIndex + 3;
                triangles[triangleIndex + 3] = vertexIndex + 3;
                triangles[triangleIndex + 4] = vertexIndex + 2;
                triangles[triangleIndex + 5] = vertexIndex;
            }
        }

        // Set the vertices, triangles, and UVs on the mesh
        roadMesh.vertices = vertices;
        roadMesh.triangles = triangles;
        roadMesh.uv = uvs;

        // Recalculate the normals and bounds of the mesh
        roadMesh.RecalculateNormals();
        roadMesh.RecalculateBounds();

        GameObject roadSegment = new GameObject("Segment" + counter, typeof(MeshFilter), typeof(MeshRenderer));
        roadSegment.GetComponent<MeshFilter>().mesh = Instantiate(roadMesh);
        roadSegment.GetComponent<MeshRenderer>().material = material;
        roadSegment.tag = "Road";
        roadSegment.transform.SetParent(gameObject.transform, true);
        counter++;
    }

    void PreviewMeshUpdate()
    {
        roadMesh.Clear();
        // Create a new mesh
        //roadMesh = new Mesh();
        roadMesh.name = "Road Mesh";

        // Get the number of points in the road
        int numPoints = roadPoints.Length;

        // Calculate the number of vertices and triangles needed
        int numVertices = numPoints * 2;
        int numTriangles = (numPoints - 1) * 2;

        // Create arrays to hold the vertices, triangles, and UVs
        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numTriangles * 3];
        Vector2[] uvs = new Vector2[numVertices];

        // Loop through the points in the road
        for (int i = 0; i < numPoints; i++)
        {
            // Calculate the index of the vertices for this segment
            int vertexIndex = i * 2;

            // Calculate the position of the vertices for this segment
            Vector3 segmentDirection = (i < numPoints - 1) ? (roadPoints[i + 1] - roadPoints[i]).normalized : (roadPoints[i] - roadPoints[i - 1]).normalized;
            Vector3 segmentNormal = Vector3.Cross(segmentDirection, Vector3.up * 2).normalized;
            Vector3 vertex1 = roadPoints[i] + segmentNormal * roadWidth / 2f;
            Vector3 vertex2 = roadPoints[i] - segmentNormal * roadWidth / 2f;

            // Add the vertices to the array
            vertices[vertexIndex] = vertex1;
            vertices[vertexIndex + 1] = vertex2;

            // Calculate the UVs for the vertices
            float uvX = (float)i / (float)(numPoints - 1);
            uvs[vertexIndex] = new Vector2(0, uvX);
            uvs[vertexIndex + 1] = new Vector2(1, uvX);

            // Add the triangles to the array
            if (i < numPoints - 1)
            {
                int triangleIndex = i * 6;
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + 1;
                triangles[triangleIndex + 2] = vertexIndex + 3;
                triangles[triangleIndex + 3] = vertexIndex + 3;
                triangles[triangleIndex + 4] = vertexIndex + 2;
                triangles[triangleIndex + 5] = vertexIndex;
            }

        }

        // Set the vertices, triangles, and UVs on the mesh
        roadMesh.vertices = vertices;
        roadMesh.triangles = triangles;
        roadMesh.uv = uvs;

        // Recalculate the normals and bounds of the mesh
        roadMesh.RecalculateNormals();
        roadMesh.RecalculateBounds();

        meshFilter.mesh = roadMesh;

        roadPreview.GetComponent<MeshFilter>().mesh = roadMesh;
        roadPreview.GetComponent<MeshRenderer>().material = material;
        roadPreview.tag = "Preview";
    }

    // Merge two meshes together
    private Mesh MergeMeshes(Mesh mesh1, Mesh mesh2)
    {
        // Combine the vertices, triangles, and UVs of both meshes
        Vector3[] combinedVertices = mesh1.vertices.Concat(mesh2.vertices).ToArray();
        int[] combinedTriangles = mesh1.triangles.Concat(mesh2.triangles.Select(index => index + mesh1.vertexCount)).ToArray();
        Vector2[] combinedUVs = mesh1.uv.Concat(mesh2.uv).ToArray();

        // Find the intersection point where the two meshes meet
        Vector3 intersectionPoint = Vector3.zero;
        for (int i = 0; i < mesh1.vertices.Length; i++)
        {
            for (int j = 0; j < mesh2.vertices.Length; j++)
            {
                if (mesh1.vertices[i] == mesh2.vertices[j])
                {
                    intersectionPoint = mesh1.vertices[i];
                    break;
                }
            }
        }

        // Modify the vertices to create the junction
        for (int i = 0; i < combinedVertices.Length; i++)
        {
            if (combinedVertices[i] == intersectionPoint)
            {
                combinedVertices[i] = new Vector3(intersectionPoint.x, intersectionPoint.y + 1f, intersectionPoint.z);
            }
        }

        // Create a new mesh and set its vertices, triangles, and UVs
        Mesh mergedMesh = new Mesh();
        mergedMesh.vertices = combinedVertices;
        mergedMesh.triangles = combinedTriangles;
        mergedMesh.uv = combinedUVs;

        // Recalculate the normals and bounds of the merged mesh
        mergedMesh.RecalculateNormals();
        mergedMesh.RecalculateBounds();

        // Create a new GameObject for the merged mesh
        //GameObject newlyMergedSegment = new GameObject("Merged Segment", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        //newlyMergedSegment.GetComponent<MeshFilter>().mesh = mergedMesh;
        //newlyMergedSegment.GetComponent<MeshRenderer>().material = material;
        //newlyMergedSegment.tag = "Road";
        //newlyMergedSegment.transform.SetParent(gameObject.transform, true);

        //return newlyMergedSegment;
        return mergedMesh;
    }

    private double GetDistanceBetweenPoints()
    {
        //distance between first two points
        double distance1 = Math.Sqrt(Math.Pow((controlPoint.x - startPoint.x), 2) + Math.Pow((controlPoint.z - startPoint.z), 2));
        //distance between second and third point
        double distance2 = Math.Sqrt(Math.Pow((endPoint.x - controlPoint.x), 2) + Math.Pow((endPoint.z - controlPoint.z), 2));

        return distance1 + distance2;
    }

    private void ProcessInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && !straightBuildingMode && !curvedBuildingMode)
        {
            Debug.Log("STRAIGHTBuildingMode set to ON");
            straightBuildingMode = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) && straightBuildingMode)
        {
            Debug.Log("STRAIGHTBuildingMode set to OFF");
            straightBuildingMode = false;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && !curvedBuildingMode && !straightBuildingMode)
        {
            Debug.Log("CURVEDBuildingMode set to ON");
            curvedBuildingMode = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && curvedBuildingMode)
        {
            Debug.Log("CURVEDBuildingMode set to OFF");
            curvedBuildingMode = false;
        }

        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            roadHeightOffset += 0.5f;
            Debug.Log("Page up key pressed");
        }
        else if (Input.GetKeyDown(KeyCode.PageDown))
        {
            roadHeightOffset -= 0.5f;
            Debug.Log("Page down key pressed");
        }
    }

    private float lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    private Vector2 linear(Vector2 a, Vector2 b, float t)
    {
        return new Vector2(lerp(a.x, b.x, t), lerp(a.y, b.y, t));
    }

    private Vector2 quadratic(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 one, two;
        one.x = lerp(a.x, b.x, t);
        one.y = lerp(a.y, b.y, t);
        two.x = lerp(b.x, c.x, t);
        two.y = lerp(b.y, c.y, t);

        return new Vector2(lerp(one.x, two.x, t), lerp(one.y, two.y, t));
    }

    float SnapPointToTerrainBelow(Vector3 point)
    {
        RaycastHit hit;
        if (Physics.Raycast(point, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            return hit.point.y + roadHeightOffset;
        }
        else
        {
            return point.y;
        }
    }
    bool AreMeshesIntersecting(Mesh mesh1, Mesh mesh2)
    {
        // Get the triangles of each mesh
        int[] triangles1 = mesh1.triangles;
        int[] triangles2 = mesh2.triangles;

        // Get the vertices of each mesh
        Vector3[] vertices1 = mesh1.vertices;
        Vector3[] vertices2 = mesh2.vertices;

        // Check if any triangle of mesh1 intersects with any triangle of mesh2
        for (int i = 0; i < triangles1.Length; i += 3)
        {
            Vector3 v1 = vertices1[triangles1[i]];
            Vector3 v2 = vertices1[triangles1[i + 1]];
            Vector3 v3 = vertices1[triangles1[i + 2]];
            Plane plane1 = new Plane(v1, v2, v3);

            for (int j = 0; j < triangles2.Length; j += 3)
            {
                Vector3 w1 = vertices2[triangles2[j]];
                Vector3 w2 = vertices2[triangles2[j + 1]];
                Vector3 w3 = vertices2[triangles2[j + 2]];
                Plane plane2 = new Plane(w1, w2, w3);

                if (ArePlanesIntersecting(plane1, plane2))
                {
                    return true;
                }
            }
        }

        return false;
    }

    bool ArePlanesIntersecting(Plane plane1, Plane plane2)
    {
        Vector3 normal1 = plane1.normal;
        Vector3 normal2 = plane2.normal;

        // Check if the planes are parallel
        if (Vector3.Dot(normal1, normal2) == 1f)
        {
            return false;
        }

        // Find the intersection line between the planes
        Vector3 lineDirection = Vector3.Cross(normal1, normal2);
        Vector3 linePoint = (-(plane1.distance * normal2) + (plane2.distance * normal1)) / lineDirection.sqrMagnitude;
        Ray intersectionLine = new Ray(linePoint, lineDirection);

        // Check if the intersection line is inside both planes
        if (!plane1.Raycast(intersectionLine, out float enter1) || !plane2.Raycast(intersectionLine, out float enter2))
        {
            return false;
        }

        return true;
    }
}


