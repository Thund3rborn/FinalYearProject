using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

public class CreateRoad : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Button button;
    private Vector3 firstPos, secondPos;
    GameObject firstCoordinate;
    GameObject secondCoordinate;
    GameObject[] lines;

    //private Material roadMaterial;
    private float clickCount;

    // Start is called before the first frame update
    void Start()
    {
        clickCount= 0;
        //roadMaterial = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask) && clickCount == 0)
        {
            firstPos = raycastHit.point;

            clickCount = 1;
        }
        else if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out raycastHit, float.MaxValue, layerMask) && clickCount == 1)
        {
            secondPos = raycastHit.point;

            //For creating line renderer object
            LineRenderer theLine = new GameObject("Line").AddComponent<LineRenderer>();
            theLine.startColor = Color.white;
            theLine.endColor = Color.black;
            theLine.startWidth = 0.1f;
            theLine.endWidth = 0.1f;
            theLine.positionCount = 2;
            theLine.useWorldSpace = true;

            theLine.SetPosition(0, firstPos);
            theLine.SetPosition(1, secondPos);

            clickCount = 0;


        }
    }

    //private void OnMouseDown()
    //{
    //    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
    //    //Create first road position point
    //    if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask) && clickCount == 0)
    //    {
            
    //        GameObject firstPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //        firstPoint.GetComponent<Transform>().localScale = new Vector3(2.0f, 0.1f, 2.0f);
    //        firstPoint.transform.position = raycastHit.point;
    //        firstPoint.GetComponent<Renderer>().material.color = Color.cyan;

    //        clickCount = clickCount + 1;

    //        GameObject theRoad = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //        theRoad.GetComponent<Transform>().localScale = new Vector3(2.0f, 0.1f, 2.0f);
    //        theRoad.transform.position = raycastHit.point;
    //        theRoad.GetComponent<Renderer>().material.color = Color.gray;


    //    }
        ////Create the road starting at the first position point and this final point
        //if (Physics.Raycast(ray, out raycastHit, float.MaxValue, layerMask) && clickCount >= 0)
        //{
        //    GameObject theRoad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //    theRoad.GetComponent<Transform>().localScale = new Vector3(2.0f, 0.1f, 2.0f);
        //    theRoad.transform.position = raycastHit.point;
        //    theRoad.GetComponent<Renderer>().material.color = Color.gray;


        //    clickCount = 0;
        //}

        //GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //road.GetComponent<Transform>().localScale = new Vector3(2.0f, 0.1f, 2.0f);
        //road.transform.position = new Vector3(0,0,0);

    //}

}
