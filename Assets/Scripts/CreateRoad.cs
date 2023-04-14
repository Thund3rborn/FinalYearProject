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
    public float roadHeightOffset = 0f;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Button button;
    //private GameObject roadGrid;

    //private bool preview = false;
    //private bool creatingRoad = false;

    private bool curvedBuildingMode, straightBuildingMode, previewOn = false;

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

    //create curved road
    void CurvedRoad()
    {
        //create a line along which the road will be created
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        Vector2[] line = new Vector2[sizeOfArr];


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
            double distance = GetDistanceBetweenPoints();
            sizeOfArr = (int)Math.Round(distance) + 1;
            //sizeOfArr = 10;

            for (int i = 0; i < sizeOfArr; i++)
            {
                double t = i / (double)(sizeOfArr - 1);
                line[i] = quadratic(new Vector2 (startPoint.x, startPoint.z), new Vector2(controlPoint.x, controlPoint.z), new Vector2(endPoint.x, endPoint.z), (float)t);
            }

            
            //line[sizeOfArr - 1] = new Vector2(endPoint.x, endPoint.z);

            theLine = new Vector3[sizeOfArr];

            for(int i = 0; i < line.Length; i++) 
            {
                theLine[i].x = line[i].x;
                theLine[i].y = SnapPointToTerrainBelow(theLine[i]);
                theLine[i].z = line[i].y;
            }
            LineRenderer lineDraw = new GameObject("Road " + listOfPositionLists.Count.ToString()).AddComponent<LineRenderer>();
            lineDraw.transform.SetParent(transform, true);

            lineDraw.material.color = Color.red;
            lineDraw.startWidth = 0.1f;
            lineDraw.endWidth = 0.1f;
            lineDraw.positionCount = theLine.Length;
            lineDraw.useWorldSpace = true;

            lineDraw.SetPositions(theLine);

            CreateRoadMesh(lineDraw);

            startPoint = Vector3.zero; endPoint = Vector3.zero; controlPoint = Vector3.zero;
        }   
    }

    //create straight road
    void StraightRoad()
    {
        //create a line along which the road will be created
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        Vector2[] line = new Vector2[sizeOfArr];


        if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask))
        {
            if (startPoint == Vector3.zero)
            {
                startPoint = raycastHit.point;
                //previewOn = true;
            }
            else if (endPoint == Vector3.zero && !previewOn)
            {
                endPoint = raycastHit.point;
            }
        }
        //else if(previewOn && Physics.Raycast(ray, out raycastHit, float.MaxValue, layerMask))
        //{
        //    endPoint = raycastHit.point;
        //}
        else if (startPoint != Vector3.zero && endPoint != Vector3.zero)
        {
            //GameObject roadObject = new GameObject("Road" + listOfPositionLists.Count.ToString(), typeof(MeshFilter), typeof(MeshRenderer));

            double distance = GetDistanceBetweenPoints();
            sizeOfArr = (int)Math.Round(distance) + 1;
            //sizeOfArr = 5;

            for (int i = 0; i < sizeOfArr; i++)
            {
                double t = (double)i / sizeOfArr;
                line[i] = linear(new Vector2(startPoint.x, startPoint.z), new Vector2(endPoint.x, endPoint.z), (float)t);

            }
            line[sizeOfArr-1] = new Vector2(endPoint.x, endPoint.z);

            theLine = new Vector3[sizeOfArr];

            for (int i = 0; i < line.Length; i++)
            {
                theLine[i].x = line[i].x;
                theLine[i].y = SnapPointToTerrainBelow(theLine[i]);
                theLine[i].z = line[i].y;
            }


            LineRenderer lineDraw = new GameObject("Road " + listOfPositionLists.Count.ToString()).AddComponent<LineRenderer>();
            lineDraw.transform.SetParent(transform, true);

            lineDraw.material.color = Color.red;
            lineDraw.startWidth = 0.1f;
            lineDraw.endWidth = 0.1f;
            lineDraw.positionCount = theLine.Length;
            lineDraw.useWorldSpace = true;

            lineDraw.SetPositions(theLine);

            CreateRoadMesh(lineDraw);

            startPoint = Vector3.zero; endPoint = Vector3.zero;
        }
    }

    void CreateRoadMesh(LineRenderer lr)
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
            vertices[2] = theLine[i] + normal + new Vector3(0, 0.05f, 0);
            vertices[3] = theLine[i] - normal + new Vector3(0, 0.05f, 0);

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

            GameObject roadSegment = new GameObject("Segment" + listOfPositionLists.Count.ToString(), typeof(MeshFilter), typeof(MeshRenderer));
            roadSegment.GetComponent<MeshFilter>().mesh = mesh;
            roadSegment.GetComponent<MeshRenderer>().material = material;
            roadSegment.transform.SetParent(lr.transform, true);
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
}
