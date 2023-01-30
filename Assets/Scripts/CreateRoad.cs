using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
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
    private Vector3 firstPos, secondPos;
    public Vector3[] v_line_pos;
    public LineRenderer[] g_lines;

    //private Material roadMaterial;
    private float clickCount;

    // Start is called before the first frame update
    void Start()
    {
        clickCount= 0;
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
}
