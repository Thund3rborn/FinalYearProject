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

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnMouseDown()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask))
        {
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.GetComponent<Transform>().localScale = new Vector3(2.0f, 0.1f, 2.0f);
            road.transform.position = raycastHit.point;
        }

        //GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //road.GetComponent<Transform>().localScale = new Vector3(2.0f, 0.1f, 2.0f);
        //road.transform.position = new Vector3(0,0,0);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
