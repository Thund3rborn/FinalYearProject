using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float scale = 10f;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform theCamera;
    [SerializeField] private float cameraSpeed = 15f;
    private float cameraVelocityX;
    private float cameraVelocityY;
    private const float DRAG = 0.87f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        mainCamera.transform.position += new Vector3(0, 0, cameraVelocityX) * Time.deltaTime;
        mainCamera.transform.position += new Vector3(cameraVelocityY, 0, 0) * Time.deltaTime;
        mainCamera.transform.position += new Vector3(0, Input.mouseScrollDelta.y * scale * Time.deltaTime, 0);

        if(cameraVelocityX != 0 || cameraVelocityY != 0)
        {
            cameraVelocityX *= DRAG;   //for smooth movement
            cameraVelocityY *= DRAG;
        }

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
