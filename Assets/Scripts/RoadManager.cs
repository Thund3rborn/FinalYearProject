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

            //check if either list is empty or has only one element
            if (currentList.Count == 0 || currentList.Count == 1 || nextList.Count == 0 || nextList.Count == 1)
            {
                continue; //skip to the next pair of lists
            }

            if (currentList[currentList.Count - 1] == nextList[0])
            {
                //the last value in the current list and the first value in the next list are the same
                //connect them together

                //remove the duplicate point
                nextList.RemoveAt(0);

                //combine the two lists
                currentList.AddRange(nextList);

                //remove the old nextList from listOfPositions
                listOfPositions.RemoveAt(i + 1);

                //decrement i so that we don't skip the next list
                i--;
            }
        }
    }

}
