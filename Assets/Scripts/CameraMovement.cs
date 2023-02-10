using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform theCamera;
    [SerializeField] private float cameraSpeed;
    private float cameraVelocityX;
    private float cameraVelocityY;

    // Start is called before the first frame update
    void Start()
    {
        cameraSpeed = 15;
    }

    // Update is called once per frame
    void Update()
    {
        mainCamera.transform.position += new Vector3(0, 0, cameraVelocityX) * Time.deltaTime;
        mainCamera.transform.position += new Vector3(cameraVelocityY, 0, 0) * Time.deltaTime;

        cameraVelocityX = 0; cameraVelocityY = 0;
        if (Input.GetKey(KeyCode.W))
        {
            cameraVelocityX = cameraSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            cameraVelocityX = -cameraSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            cameraVelocityY = -cameraSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            cameraVelocityY = cameraSpeed;
        }


    }
}
