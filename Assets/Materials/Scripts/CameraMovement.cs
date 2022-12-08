using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraVelocity;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.W))
        {
            mainCamera.transform.position += new Vector3(0, 0, cameraVelocity) * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            mainCamera.transform.position -= new Vector3(0, 0, cameraVelocity) * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            mainCamera.transform.position -= new Vector3(cameraVelocity, 0, 0) * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            mainCamera.transform.position += new Vector3(cameraVelocity, 0, 0) * Time.deltaTime;
        }
    }
}
