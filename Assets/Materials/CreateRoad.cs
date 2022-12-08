using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

public class CreateRoad : MonoBehaviour
{
    //GameObject road;
    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnMouseDown()
    {
        //Instantiate(road);
        //GameObject road = new GameObject;
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.AddComponent<Rigidbody>();
        road.transform.position = /*new Vector3*/(/*Input.mousePosition.normalized.x, Input.mousePosition.normalized.y, */GameObject.FindWithTag("Plane").gameObject.transform.position);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //private Vector3 mouseposition()
    //{

    //    Viewport coordinates go from 0.0 in the lower left corner of the screen to 1.0 in the upper right of the screen.
    //    Vector3 mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

    //    Give both axis a different offset  so they don't overlap each other.
    //    mousePos.x -= 0.5f;
    //    mousePos.y += 1.5f;

    //    Debug.Log(mousePos);

    //    return mousePos;

    //}
}
