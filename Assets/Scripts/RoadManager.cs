using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RoadManager : MonoBehaviour
{
    List<List<Vector3>> listOfPositions = CreateRoad.listOfPositionLists;
    public static List<GameObject> roadSegments = new List<GameObject>();

    private void Start()
    {
        
    }

    private void Update()
    {
        for (int i = 0; i < listOfPositions.Count - 1; i++)
        {
            List<Vector3> currentList = listOfPositions[i];
            List<Vector3> nextList = listOfPositions[i + 1];

            // Check if either list is empty or has only one element
            if (currentList.Count == 0 || currentList.Count == 1 || nextList.Count == 0 || nextList.Count == 1)
            {
                continue; // Skip to the next pair of lists
            }

            if (currentList[currentList.Count - 1] == nextList[0])
            {
                // The last value in the current list and the first value in the next list are the same
                // Connect them together

                // Remove the duplicate point
                nextList.RemoveAt(0);

                // Combine the two lists
                currentList.AddRange(nextList);

                // Remove the old nextList from listOfPositions
                listOfPositions.RemoveAt(i + 1);

                // Decrement i so that we don't skip the next list
                i--;
            }
        }


    }

}
