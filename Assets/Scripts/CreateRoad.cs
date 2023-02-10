using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Net;
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

    private Vector3 startPoint, endPoint;
    private List<List<Vector3>> listOfPositionLists = new List<List<Vector3>>();


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CreateRoads();
    }



    void CreateRoads()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

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

            GenerateMesh();

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

    private void GenerateMesh()
    {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        Vector3 direction = (endPoint - startPoint).normalized;
        Vector3 normal = Vector3.Cross(direction, Vector3.up).normalized * roadWidth;

        vertices[0] = startPoint - normal + new Vector3(0, 0.1f, 0);
        vertices[1] = startPoint + normal + new Vector3(0, 0.1f, 0);
        vertices[2] = endPoint + normal + new Vector3(0, 0.1f, 0);
        vertices[3] = endPoint - normal + new Vector3(0, 0.1f, 0);

        uv[0] = new Vector2(0f, 0f);
        uv[1] = new Vector2(0f, 1f);
        uv[2] = new Vector2(1f, 1f);
        uv[3] = new Vector2(1f, 0f);

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
}
