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
    // Reference to the terrain object
    public Terrain terrain;

    // Reference to the object we want to match
    //public GameObject objectToMatch;

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


        
        //Mesh roadMesh = new Mesh();

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
                //CreateRoadObject();
            }
        }
        else if ((startPoint != Vector3.zero && endPoint != Vector3.zero) 
            && (controlPoint != Vector3.zero || straightBuildingMode))
        {

            CreateRoadObject();
            //CreateBridge();
            //AdjustTerrain();


            List<Vector3> points = new List<Vector3>();
            points.AddRange(roadPoints);
            listOfPositionLists.Add(points);

            //CreateRoadObject();
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

                //if(roadHeightOffset != 0)
                //{
                //    roadPoints[0].y += roadHeightOffset;
                //}

                PreviewMeshUpdate();
            }
            else if (startPoint != Vector3.zero && controlPoint != Vector3.zero && !straightBuildingMode && Physics.Raycast(ray, out raycastHit, float.MaxValue, layerMask))
            {

                double distance = GetDistanceBetweenPoints();
                sizeOfArr = (int)Math.Round(distance) + 1;

                line = new Vector3[sizeOfArr];
                //if (roadHeightOffset != 0)
                //{
                //    startPoint.y += roadHeightOffset;
                //}

                for (int i = 0; i < sizeOfArr; i++)
                {
                    double t = i / (double)(sizeOfArr - 1);
                    line[i] = quadratic(startPoint, controlPoint, raycastHit.point, (float)t);
                }

                //RaycastHit hit;
                //if (Physics.Raycast(transform.position, transform.forward, out hit))
                //{
                //    if (hit.collider.tag == "Road")
                //    {
                //        // Calculate the middle point of the width of the object collided
                //        Vector3 middlePoint = hit.collider.bounds.center;
                //        middlePoint.y = transform.position.y;

                //        endPoint = middlePoint;
                //    }
                //}

                roadPoints = new Vector3[sizeOfArr];

                for (int i = 0; i < line.Length; i++)
                {
                    roadPoints[i].x = line[i].x;
                    roadPoints[i].y = line[i].y + 0.05f + roadHeightOffset;
                    roadPoints[i].z = line[i].z;
                }

                //if (roadHeightOffset != offsetcopy)
                //{
                //    roadPoints[line.Length].y += roadHeightOffset;
                //}

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

        RoadManager.roadSegments.Add(roadSegment);
       //AdjustTerrain();
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

            // Determine the height of the terrain at the road point
            float terrainHeight = Terrain.activeTerrain.SampleHeight(point);

            // Define the vertices of the mesh at the road point
            vertices[i * 2] = new Vector3(point.x - roadWidth / 2, terrainHeight, point.z);
            vertices[i * 2 + 1] = new Vector3(point.x + roadWidth / 2, terrainHeight, point.z);

            // Define the vertices of the mesh for the bridge segment at the road point
            Vector3 bridgeSegmentStart = new Vector3(point.x, terrainHeight - 5, point.z);
            Vector3 bridgeSegmentEnd = new Vector3(point.x, terrainHeight - 5 - 10, point.z);
            vertices[(numPoints * 2) + i * 2] = bridgeSegmentStart;
            vertices[(numPoints * 2) + i * 2 + 1] = bridgeSegmentEnd;

            // Define the triangles of the mesh at the road point
            if (i > 0)
            {
                int triIndex = (i - 1) * 6;
                int vertIndex = i * 2;

                // Define the triangles of the road mesh
                triangles[triIndex] = vertIndex - 2;
                triangles[triIndex + 1] = vertIndex - 1;
                triangles[triIndex + 2] = vertIndex;

                triangles[triIndex + 3] = vertIndex;
                triangles[triIndex + 4] = vertIndex - 1;
                triangles[triIndex + 5] = vertIndex + 1;

                // Define the triangles of the bridge segment mesh
                int bridgeSegmentVertIndex = (numPoints * 4) + i * 2;
                triangles[(numPoints - 1) * 6 + triIndex] = bridgeSegmentVertIndex - 2;
                triangles[(numPoints - 1) * 6 + triIndex + 1] = bridgeSegmentVertIndex - 1;
                triangles[(numPoints - 1) * 6 + triIndex + 2] = bridgeSegmentVertIndex;

                triangles[(numPoints - 1) * 6 + triIndex + 3] = bridgeSegmentVertIndex;
                triangles[(numPoints - 1) * 6 + triIndex + 4] = bridgeSegmentVertIndex - 1;
                triangles[(numPoints - 1) * 6 + triIndex + 5] = bridgeSegmentVertIndex + 1;
            }
        }

        // Create the mesh object for the road
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


    void AdjustTerrain()
    {
        // Loop through the array of path points
        foreach (Vector3 pathPoint in roadPoints)
        {
            // Get the terrain data
            TerrainData terrainData = terrain.terrainData;

            // Get the size of the terrain
            int heightmapWidth = terrainData.heightmapResolution;
            int heightmapHeight = terrainData.heightmapResolution;

            // Get the position of the path point in terrain coordinates
            Vector3 terrainPosition = terrain.transform.position;
            Vector3 pathPointInTerrainSpace = pathPoint - terrainPosition;

            // Calculate the position in terrain coordinates as a percentage of the terrain size
            float terrainX = pathPointInTerrainSpace.x / terrainData.size.x;
            float terrainZ = pathPointInTerrainSpace.z / terrainData.size.z;

            // Calculate the index of the heightmap array for the desired position
            int xIndex = Mathf.RoundToInt(terrainX * (heightmapWidth - 1));
            int zIndex = Mathf.RoundToInt(terrainZ * (heightmapHeight - 1));

            // Set the height at the desired position
            float[,] heights = terrainData.GetHeights(xIndex, zIndex, 1, 1);
            heights[0, 0] = pathPoint.y;
            terrainData.SetHeights(xIndex, zIndex, heights);
        }
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


