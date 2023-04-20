using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPlacement : MonoBehaviour
{
    public GameObject carPrefab;
    public float levitationHeight = 1.0f;

    [SerializeField] private LayerMask layerMask;
    private GameObject currentCar;
    public static bool isPlacingCar = false;

    public void PlaceCar()
    {
        //check if the user has pressed the button to enable car placement mode
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            isPlacingCar = !isPlacingCar;
            if (isPlacingCar)
            {
                //instantiate the car prefab and set its position to the cursor position
                currentCar = Instantiate(carPrefab);
                currentCar.transform.position = GetTerrainCursorPosition();
            }
            else
            {
                //destroy the car prefab when placement mode is turned off
                Destroy(currentCar);
            }
        }

        //if the user is currently placing a car, update its position to follow the cursor
        if (isPlacingCar)
        {
            currentCar.transform.position = GetTerrainCursorPosition() + Vector3.up * levitationHeight;
        }

        if(Input.GetMouseButtonDown(0))
        {
            currentCar = Instantiate(carPrefab);
            currentCar.transform.position = GetTerrainCursorPosition();
            isPlacingCar = false;
        }
    }

    //helper method to get the cursor position on the terrain
    private Vector3 GetTerrainCursorPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask))
        {
            return raycastHit.point;
        }
        return Vector3.zero;
    }
}
