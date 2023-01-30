using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Net;
using Unity.VisualScripting;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

public class CreateRoad : MonoBehaviour
{
    UnityEngine.Mesh UE_mesh;
    UnityEngine.Object UE_obj;

    public MousePos mousePos;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Button button;
    private Vector3 startPoint, endPoint;

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


        if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask) && startPoint == Vector3.zero)
        {
            Debug.Log("firstPos");
            startPoint = raycastHit.point;
        }
        else if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out raycastHit, float.MaxValue, layerMask))
        {
            endPoint = raycastHit.point; Debug.Log("create");

            CreateStraightRoad();

            startPoint = Vector3.zero;


        }
    }
    
    private void CreateStraightRoad()
    {
        LineRenderer theLine = new GameObject("Line").AddComponent<LineRenderer>();
        theLine.startColor = Color.white;
        theLine.endColor = Color.black;
        theLine.startWidth = 0.1f;
        theLine.endWidth = 0.1f;
        theLine.positionCount = 2;
        theLine.useWorldSpace = true;

        theLine.SetPosition(0, startPoint);
        theLine.SetPosition(1, endPoint);
    }
}