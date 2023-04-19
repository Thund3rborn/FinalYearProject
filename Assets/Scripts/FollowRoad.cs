using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowRoad : MonoBehaviour
{
    public float speed = 5f;  // speed of movement
    public float rotationspeed = 10f;

    private int currentListIndex = 0;  // current index of the list being followed
    private int listIndex = 0;  
    private int currentPointIndex = 0;  // current index of the point being followed

    private void Update()
    {
        // check if there are lists of points to follow
        if (CreateRoad.listOfPositionLists != null && CreateRoad.listOfPositionLists.Count > 0)
        {
            // get the current list of points
            List<Vector3> points = CreateRoad.listOfPositionLists[currentListIndex];

            // check if there are points in the list
            if (points != null && points.Count > 0)
            {
                // calculate the direction to the current point
                Vector3 direction = points[currentPointIndex] - transform.position;
                direction.Normalize();

                // calculate the rotation towards the direction
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

                // smoothly rotate towards the target rotation
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationspeed * Time.deltaTime);

                // move towards the current point
                transform.position += direction * speed * Time.deltaTime;

                // check if the current point has been reached
                if (Vector3.Distance(transform.position, points[currentPointIndex]) < 0.1f)
                {
                    // move to the next point in the list
                    currentPointIndex++;

                    // check if the last point has been reached
                    if (currentPointIndex >= points.Count)
                    {
                        // move to the next list in the list of lists
                        currentListIndex++;

                        // check if the last list has been reached
                        if (currentListIndex >= CreateRoad.listOfPositionLists.Count)
                        {
                            // loop back to the first list and point
                            currentListIndex = 0;
                            currentPointIndex = 0;
                        }
                        else
                        {
                            // move to the first point in the next list
                            currentPointIndex = 0;
                        }
                    }
                }
            }
            Quaternion currentRotation = gameObject.transform.rotation;
            Quaternion newRotation = Quaternion.Euler(new Vector3(-90, currentRotation.eulerAngles.y, currentRotation.eulerAngles.z));
            gameObject.transform.rotation = newRotation;
        }
    }
}
