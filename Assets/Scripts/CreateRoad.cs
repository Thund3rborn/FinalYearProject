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
    public MousePos mousePos;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Button button;

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

        if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask) && startPoint == Vector3.zero)
        {
            Debug.Log("firstPos");
            startPoint = raycastHit.point;
        }
        else if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out raycastHit, float.MaxValue, layerMask))
        {
            List<Vector3> positionsList = new List<Vector3>();

            endPoint = raycastHit.point; Debug.Log("create");
            positionsList.Add(endPoint);

            positionsList.Add(startPoint);
            listOfPositionLists.Add(positionsList);

            CreateStraightRoad();


            for (int i = 0; i < listOfPositionLists.Count; i++)
            {
                List<Vector3> list = listOfPositionLists[i];
                Debug.Log("List " + i + ": " + string.Join(", ", list));
            }

            startPoint = Vector3.zero;
            endPoint = Vector3.zero;
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
