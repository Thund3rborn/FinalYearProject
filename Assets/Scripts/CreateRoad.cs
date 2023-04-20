using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CreateRoad : MonoBehaviour
{
    public MousePos mousePos;
    public Material material;
    public Material previewMaterial1;
    public Material previewMaterial2;
    //public float roadWidth = 1f;
    public float roadHeightOffset = 0f;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Button button;

    public static bool curvedBuildingMode, straightBuildingMode;

    private Vector3 startPoint = Vector3.zero;
    private Vector3 controlPoint = Vector3.zero;
    private Vector3 endPoint = Vector3.zero;
    private Vector3 keepTrackOfEndPoint = Vector3.zero;
    public static List<List<Vector3>> listOfPositionLists = new List<List<Vector3>>();
    
    private int counter = 1;
    private int sizeOfArr = 0;
    private Vector3[] line;
    private Vector3[] roadPoints;
    public float roadWidth = 2f;
    private GameObject roadPreview;
    private MeshFilter meshFilter;
    public Mesh roadMesh;


    //terrain
    public Terrain terrain;

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
    }

    // Update is called once per frame
    void Update()
    {
        Road();

        ProcessInput();
    }

    //create curved road
    void Road()
    {
        //create a line along which the road will be created
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0) && (straightBuildingMode || curvedBuildingMode) && Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask))
        {
            if (startPoint == Vector3.zero)
            {
                startPoint = raycastHit.point;
            }
            else if (controlPoint == Vector3.zero && !straightBuildingMode)
            {
                controlPoint = raycastHit.point;
            }
            else if (endPoint == Vector3.zero)
            {
                endPoint = raycastHit.point;
                keepTrackOfEndPoint = endPoint;
            }
        }
        else if ((startPoint != Vector3.zero && endPoint != Vector3.zero) 
            && (controlPoint != Vector3.zero || straightBuildingMode))
        {

            CreateRoadObject();


            List<Vector3> points = new List<Vector3>();
            points.AddRange(roadPoints);
            listOfPositionLists.Add(points);

            startPoint = endPoint;
            endPoint = Vector3.zero;
            controlPoint = Vector3.zero;
        }
        else if (straightBuildingMode || curvedBuildingMode)
        //update preview
        {
            float offsetcopy = roadHeightOffset;
            if (startPoint != Vector3.zero && controlPoint == Vector3.zero 
                && endPoint == Vector3.zero && Physics.Raycast(ray, out raycastHit, float.MaxValue, layerMask))
            {
                double distance = GetDistanceBetweenPoints();
                sizeOfArr = (int)Math.Round(distance) + 1;

                line = new Vector3[sizeOfArr];

                for (int i = 0; i < sizeOfArr; i++)
                {
                    double t = i / (double)(sizeOfArr - 1);
                    line[i] = linear(startPoint, raycastHit.point, (float)t);
                }

                roadPoints = new Vector3[sizeOfArr];

                for (int i = 0; i < line.Length; i++)
                {
                    roadPoints[i].x = line[i].x;
                    roadPoints[i].y = line[i].y + 0.05f + roadHeightOffset;
                    roadPoints[i].z = line[i].z;
                }

                PreviewMeshUpdate();
            }
            else if (startPoint != Vector3.zero && controlPoint != Vector3.zero && !straightBuildingMode && Physics.Raycast(ray, out raycastHit, float.MaxValue, layerMask))
            {

                double distance = GetDistanceBetweenPoints();
                sizeOfArr = (int)Math.Round(distance) + 1;

                line = new Vector3[sizeOfArr];

                for (int i = 0; i < sizeOfArr; i++)
                {
                    double t = i / (double)(sizeOfArr - 1);
                    line[i] = quadratic(startPoint, controlPoint, raycastHit.point, (float)t);
                }

                roadPoints = new Vector3[sizeOfArr];

                for (int i = 0; i < line.Length; i++)
                {
                    roadPoints[i].x = line[i].x;
                    roadPoints[i].y = line[i].y + 0.05f + roadHeightOffset;
                    roadPoints[i].z = line[i].z;
                }

                PreviewMeshUpdate();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            ResetRoadPreviewMesh();
        }
    }

    void CreateRoadObject()
    {
        roadMesh.name = "Road Mesh";

        //get the number of points in the road
        int numPoints = roadPoints.Length;

        //calculate the number of vertices and triangles needed
        int numVertices = numPoints * 2;
        int numTriangles = (numPoints - 1) * 2;

        //create arrays to hold the vertices, triangles, and UVs
        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numTriangles * 3];
        Vector2[] uvs = new Vector2[numVertices];

        //loop through the points in the road
        for (int i = 0; i < numPoints; i++)
        {
            //calculate the index of the vertices for this segment
            int vertexIndex = i * 2;

            //calculate the position of the vertices for this segment
            Vector3 segmentDirection = (i < numPoints - 1) ? (roadPoints[i + 1] - roadPoints[i]).normalized : (roadPoints[i] - roadPoints[i - 1]).normalized;
            Vector3 segmentNormal = Vector3.Cross(segmentDirection, Vector3.up * 2).normalized;
            Vector3 vertex1 = roadPoints[i] + segmentNormal * roadWidth / 2f;
            Vector3 vertex2 = roadPoints[i] - segmentNormal * roadWidth / 2f;

            //add the vertices to the array
            vertices[vertexIndex] = vertex1;
            vertices[vertexIndex + 1] = vertex2;

            //calculate the UVs for the vertices
            float uvX = (float)i / (float)(numPoints - 1);
            uvs[vertexIndex] = new Vector2(0, uvX);
            uvs[vertexIndex + 1] = new Vector2(1, uvX);

            //add the triangles to the array
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

        //set the vertices, triangles, and UVs on the mesh
        roadMesh.vertices = vertices;
        roadMesh.triangles = triangles;
        roadMesh.uv = uvs;

        //recalculate the normals and bounds of the mesh
        roadMesh.RecalculateNormals();
        roadMesh.RecalculateBounds();

        GameObject roadSegment = new GameObject("Segment" + counter, typeof(MeshFilter), typeof(MeshRenderer));
        roadSegment.GetComponent<MeshFilter>().mesh = Instantiate(roadMesh);
        roadSegment.GetComponent<MeshRenderer>().material = material;
        roadSegment.tag = "Road";
        roadSegment.transform.SetParent(gameObject.transform, true);
        counter++;

        RoadManager.roadSegments.Add(roadSegment);
    }

    void PreviewMeshUpdate()
    {
        roadMesh.Clear();
        roadMesh.name = "Road Mesh";

        //get the number of points in the road
        int numPoints = roadPoints.Length;

        //calculate the number of vertices and triangles needed
        int numVertices = numPoints * 2;
        int numTriangles = (numPoints - 1) * 2;

        //create arrays to hold the vertices, triangles, and UVs
        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numTriangles * 3];
        Vector2[] uvs = new Vector2[numVertices];

        //loop through the points in the road
        for (int i = 0; i < numPoints; i++)
        {
            //calculate the index of the vertices for this segment
            int vertexIndex = i * 2;

            //calculate the position of the vertices for this segment
            Vector3 segmentDirection = (i < numPoints - 1) ? (roadPoints[i + 1] - roadPoints[i]).normalized : (roadPoints[i] - roadPoints[i - 1]).normalized;
            Vector3 segmentNormal = Vector3.Cross(segmentDirection, Vector3.up * 2).normalized;
            Vector3 vertex1 = roadPoints[i] + segmentNormal * roadWidth / 2f;
            Vector3 vertex2 = roadPoints[i] - segmentNormal * roadWidth / 2f;

            //add the vertices to the array
            vertices[vertexIndex] = vertex1;
            vertices[vertexIndex + 1] = vertex2;

            //calculate the UVs for the vertices
            float uvX = (float)i / (float)(numPoints - 1);
            uvs[vertexIndex] = new Vector2(0, uvX);
            uvs[vertexIndex + 1] = new Vector2(1, uvX);

            //add the triangles to the array
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

        //set the vertices, triangles, and UVs on the mesh
        roadMesh.vertices = vertices;
        roadMesh.triangles = triangles;
        roadMesh.uv = uvs;

        //recalculate the normals and bounds of the mesh
        roadMesh.RecalculateNormals();
        roadMesh.RecalculateBounds();

        meshFilter.mesh = roadMesh;

        roadPreview.GetComponent<MeshFilter>().mesh = roadMesh;
        if(straightBuildingMode || !straightBuildingMode && controlPoint != Vector3.zero)
          roadPreview.GetComponent<MeshRenderer>().material = previewMaterial2;
        else
            roadPreview.GetComponent<MeshRenderer>().material = previewMaterial1;
        roadPreview.tag = "Preview";
    }

    void ResetRoadPreviewMesh()
    {
        startPoint = Vector3.zero;
        endPoint = Vector3.zero;
        controlPoint = Vector3.zero;
        roadMesh.Clear();
    }

    void CreateBridge()
    {
        int numPoints = roadPoints.Length;

        Vector3[] vertices = new Vector3[numPoints * 2];
        int[] triangles = new int[(numPoints - 1) * 6];

        // Loop through each road point
        for (int i = 0; i < numPoints; i++)
        {
            Vector3 point = roadPoints[i];

            //determine the height of the terrain at the road point
            float terrainHeight = Terrain.activeTerrain.SampleHeight(point);

            //define the vertices of the mesh at the road point
            vertices[i * 2] = new Vector3(point.x - roadWidth / 2, terrainHeight, point.z);
            vertices[i * 2 + 1] = new Vector3(point.x + roadWidth / 2, terrainHeight, point.z);

            //define the vertices of the mesh for the bridge segment at the road point
            Vector3 bridgeSegmentStart = new Vector3(point.x, terrainHeight - 5, point.z);
            Vector3 bridgeSegmentEnd = new Vector3(point.x, terrainHeight - 5 - 10, point.z);
            vertices[(numPoints * 2) + i * 2] = bridgeSegmentStart;
            vertices[(numPoints * 2) + i * 2 + 1] = bridgeSegmentEnd;

            //define the triangles of the mesh at the road point
            if (i > 0)
            {
                int triIndex = (i - 1) * 6;
                int vertIndex = i * 2;

                //define the triangles of the road mesh
                triangles[triIndex] = vertIndex - 2;
                triangles[triIndex + 1] = vertIndex - 1;
                triangles[triIndex + 2] = vertIndex;

                triangles[triIndex + 3] = vertIndex;
                triangles[triIndex + 4] = vertIndex - 1;
                triangles[triIndex + 5] = vertIndex + 1;

                //define the triangles of the bridge segment mesh
                int bridgeSegmentVertIndex = (numPoints * 4) + i * 2;
                triangles[(numPoints - 1) * 6 + triIndex] = bridgeSegmentVertIndex - 2;
                triangles[(numPoints - 1) * 6 + triIndex + 1] = bridgeSegmentVertIndex - 1;
                triangles[(numPoints - 1) * 6 + triIndex + 2] = bridgeSegmentVertIndex;

                triangles[(numPoints - 1) * 6 + triIndex + 3] = bridgeSegmentVertIndex;
                triangles[(numPoints - 1) * 6 + triIndex + 4] = bridgeSegmentVertIndex - 1;
                triangles[(numPoints - 1) * 6 + triIndex + 5] = bridgeSegmentVertIndex + 1;
            }
        }

        //create the mesh object for the road
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GameObject bridge = new GameObject("Bridge" + counter);
        bridge.transform.position = Vector3.zero;
        bridge.transform.rotation = Quaternion.identity;

        // Create the mesh object for the bridge segment
        Mesh bridgeMesh = new Mesh();
        bridgeMesh.vertices = vertices.Skip(numPoints * 2).ToArray();
        bridgeMesh.triangles = triangles.Skip((numPoints - 1) * 6).ToArray();
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
            ResetRoadPreviewMesh();
            Debug.Log("STRAIGHTBuildingMode set to ON");
            straightBuildingMode = true;

        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) && straightBuildingMode)
        {
            ResetRoadPreviewMesh();
            Debug.Log("STRAIGHTBuildingMode set to OFF");
            straightBuildingMode = false;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && !curvedBuildingMode && !straightBuildingMode)
        {
            ResetRoadPreviewMesh();
            Debug.Log("CURVEDBuildingMode set to ON");
            curvedBuildingMode = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && curvedBuildingMode)
        {
            ResetRoadPreviewMesh();
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

    //helpers

    private static float lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    private static Vector3 linear(Vector3 a, Vector3 b, float t)
    {
        return new Vector3(lerp(a.x, b.x, t), lerp(a.y, b.y, t), lerp(a.z, b.z, t));
    }

    private static Vector3 quadratic(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        Vector3 one, two;
        one.x = lerp(a.x, b.x, t);
        one.y = lerp(a.y, b.y, t);
        one.z = lerp(a.z, b.z, t);
        two.x = lerp(b.x, c.x, t);
        two.y = lerp(b.y, c.y, t);
        two.z = lerp(b.z, c.z, t);

        return new Vector3(lerp(one.x, two.x, t), lerp(one.y, two.y, t), lerp(one.z, two.z, t));
    }
}


