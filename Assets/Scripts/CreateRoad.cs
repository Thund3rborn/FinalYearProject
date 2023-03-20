using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Net;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

public class CreateRoad : MonoBehaviour
{
    public MousePos mousePos;
    public Material material;
    public float roadWidth = 1f;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Button button;

    //private bool preview = false;
    private bool creatingRoad = false;

    private bool curvedBuildingMode, straightBuildingMode = false;

    private Vector3 startPoint = Vector3.zero;
    private Vector3 controlPoint = Vector3.zero;
    private Vector3 endPoint = Vector3.zero;
    private List<List<Vector3>> listOfPositionLists = new List<List<Vector3>>();

    int sizeOfArr;
    Vector3[] theLine;

    // Start is called before the first frame update
    void Start()
    {
        sizeOfArr = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (straightBuildingMode)
        {
            StraightRoad();
        }
        if (curvedBuildingMode)
        {
            CurvedRoad();
        }

        ProcessInput();
    }

    void CurvedRoad()
    {
        //Create a line along which the road will be created
        //bool preview = false;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        //sizeOfArr = 60;
        Vector2[] line = new Vector2[sizeOfArr];
        //sizeOfArr = line.Length;


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
            }
        }
        else if (startPoint != Vector3.zero && controlPoint != Vector3.zero && endPoint != Vector3.zero)
        {
            //float distance = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));

            double distance = GetDistanceBetweenPoints();
            sizeOfArr = (int)Math.Round(distance);

            for (int i = 0; i < sizeOfArr; i++)
            {
                double t = (double)i / sizeOfArr;
                line[i] = quadratic(new Vector2 (startPoint.x, startPoint.z), new Vector2(controlPoint.x, controlPoint.z), new Vector2(endPoint.x, endPoint.z), (float)t);

            }

            theLine = new Vector3[sizeOfArr];

            for(int i = 0; i < line.Length; i++) 
            {
                theLine[i].x = line[i].x;
                theLine[i].y = SnapPointToTerrainBelow(theLine[i]);
                theLine[i].z = line[i].y;
            }

            
            LineRenderer lineDraw = new GameObject("Line " + listOfPositionLists.Count.ToString()).AddComponent<LineRenderer>();

            lineDraw.startColor = Color.white;
            lineDraw.endColor = Color.black;
            lineDraw.startWidth = 0.1f;
            lineDraw.endWidth = 0.1f;
            lineDraw.positionCount = theLine.Length;
            lineDraw.useWorldSpace = true;

            lineDraw.SetPositions(theLine);

            CreateCurvedRoad();

            startPoint = Vector3.zero; endPoint = Vector3.zero; controlPoint = Vector3.zero;
        }   
    }

    void CreateCurvedRoad()
    {
        for (int i = 1; i < sizeOfArr; i++)
        {
            Vector3[] vertices = new Vector3[4];
            Vector2[] uv = new Vector2[4];
            int[] triangles = new int[6];

            Vector3 direction = (theLine[i] - theLine[i - 1]).normalized;
            Vector3 normal = Vector3.Cross(direction, Vector3.up).normalized * roadWidth;

            vertices[0] = theLine[i - 1] - normal + new Vector3(0, 0.05f, 0);
            vertices[1] = theLine[i - 1] + normal + new Vector3(0, 0.05f, 0);
            vertices[2] = theLine[i ] + normal + new Vector3(0, 0.05f, 0);
            vertices[3] = theLine[i ] - normal + new Vector3(0, 0.05f, 0);

            uv[3] = new Vector2(0f, 0f);
            uv[0] = new Vector2(0f, 1f);
            uv[1] = new Vector2(1f, 1f);
            uv[2] = new Vector2(1f, 0f);

            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;

            Mesh mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GameObject gameObject = new GameObject("Mesh" + listOfPositionLists.Count.ToString(), typeof(MeshFilter), typeof(MeshRenderer));
            gameObject.GetComponent<MeshFilter>().mesh = mesh;
            gameObject.GetComponent<MeshRenderer>().material = material;
        }
    }
    private double GetDistanceBetweenPoints()
    {
        //distance between first two points
        double distance1 = Math.Sqrt(Math.Pow((controlPoint.x - startPoint.x), 2) + Math.Pow((controlPoint.z - startPoint.z), 2));
        //distance between second and third point
        double distance2 = Math.Sqrt(Math.Pow((endPoint.x - controlPoint.x), 2) + Math.Pow((endPoint.z - controlPoint.z), 2));

        return distance1 + distance2;
    }

    void StraightRoad()
    {
        //Create a line along which the road will be created

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        Vector2[] line = new Vector2[sizeOfArr];

        //get first coordinate, activate and display preview
        if (Input.GetMouseButtonDown(0) && !creatingRoad && Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask) && startPoint == Vector3.zero)
        {
            Debug.Log("firstPos");
            startPoint = raycastHit.point;
            endPoint = startPoint;              //temporary location 

            CreateStraightLine();

            //DrawRoad();

            creatingRoad = true;
            //preview = true;
        }
        //get all next coords, display preview
        else if (Input.GetMouseButtonDown(0) && creatingRoad && Physics.Raycast(ray, out raycastHit, float.MaxValue, layerMask))
        {
            List<Vector3> positionsList = new List<Vector3>();

            Debug.Log("create");
            endPoint = raycastHit.point;
            positionsList.Add(endPoint);
            positionsList.Add(startPoint);
            listOfPositionLists.Add(positionsList);

            GenerateStraightRoadMesh();

            for (int i = 0; i < listOfPositionLists.Count; i++)
            {
                List<Vector3> list = listOfPositionLists[i];
                Debug.Log("List " + i + ": " + string.Join(", ", list));
            }

            startPoint = Vector3.zero;

            startPoint = endPoint;
            endPoint = raycastHit.point;              //temporary location 

            CreateStraightLine();

        }
        //when rightclicked, deactivate preview, destroy preview mesh, reset the variables
        else if (Input.GetMouseButtonDown(1) && creatingRoad)
        {
            GameObject.Destroy(GameObject.Find("Line " + listOfPositionLists.Count));

            creatingRoad = false;
            //preview = false;

            startPoint = Vector3.zero;
            endPoint = Vector3.zero;
        }

        //preview update when on
        else if (creatingRoad && Physics.Raycast(ray, out raycastHit, float.MaxValue, layerMask))
        {
            GameObject.Destroy(GameObject.Find("Line " + listOfPositionLists.Count));

            endPoint = raycastHit.point;

            CreateStraightLine();
        }
    }

    private void CreateStraightLine()
    {
        LineRenderer theLine = new GameObject("Line " + listOfPositionLists.Count.ToString()).AddComponent<LineRenderer>();

        theLine.startColor = Color.white;
        theLine.endColor = Color.black;
        theLine.startWidth = 0.1f;
        theLine.endWidth = 0.1f;
        theLine.positionCount = 2;
        theLine.useWorldSpace = true;

        theLine.SetPosition(0, startPoint);
        theLine.SetPosition(1, endPoint);
    }

    private void GenerateStraightRoadMesh()
    {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        Vector3 direction = (endPoint - startPoint).normalized;
        Vector3 normal = Vector3.Cross(direction, Vector3.up).normalized * roadWidth;

        vertices[0] = startPoint - normal + new Vector3(0, 0.05f, 0);
        vertices[1] = startPoint + normal + new Vector3(0, 0.05f, 0);
        vertices[2] = endPoint + normal + new Vector3(0, 0.05f, 0);
        vertices[3] = endPoint - normal + new Vector3(0, 0.05f, 0);

        uv[3] = new Vector2(0f, 0f);
        uv[0] = new Vector2(0f, 1f);
        uv[1] = new Vector2(1f, 1f);
        uv[2] = new Vector2(1f, 0f);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        GameObject gameObject = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer));
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        gameObject.GetComponent<MeshRenderer>().material = material;

    }

    private void ProcessInput()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1) && !straightBuildingMode && !curvedBuildingMode) 
        {
            Debug.Log("STRAIGHTBuildingMode set to ON");
            straightBuildingMode = true;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha1) && straightBuildingMode)
        {
            Debug.Log("STRAIGHTBuildingMode set to OFF");
            straightBuildingMode = false;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && !curvedBuildingMode && !straightBuildingMode)
        {
            Debug.Log("CURVEDBuildingMode set to ON");
            curvedBuildingMode = true;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2) && curvedBuildingMode)
        {
            Debug.Log("CURVEDBuildingMode set to OFF");
            curvedBuildingMode = false;
        }
    }

    private float lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    //private Vector2 linear(Vector2 a, Vector2 b, float t)
    //{
    //    return new Vector2(lerp(a.x, b.x, t), lerp(a.y, b.y, t));
    //}

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
            return hit.point.y;
        }
        else
        {
            return point.y;
        }
    }
}
