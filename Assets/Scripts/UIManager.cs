using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    public void ToggleBuildMode_Straight()
    {
        if (!CreateRoad.straightBuildingMode && !CreateRoad.curvedBuildingMode)
        {
            CreateRoad.straightBuildingMode = !CreateRoad.straightBuildingMode;
            Debug.Log("STRAIGHT building mode set to ON");
        }
        else if (CreateRoad.straightBuildingMode)
        {
            CreateRoad.straightBuildingMode = !CreateRoad.straightBuildingMode;
            Debug.Log("STRAIGHT building mode set to OFF");
        }
    }

    public void ToggleBuildMode_Curved()
    {
        if (!CreateRoad.curvedBuildingMode && !CreateRoad.straightBuildingMode)
        {
            CreateRoad.curvedBuildingMode = !CreateRoad.curvedBuildingMode;
            Debug.Log("CURVED building mode set to ON");
        }
        else if (CreateRoad.curvedBuildingMode)
        {
            CreateRoad.curvedBuildingMode = !CreateRoad.curvedBuildingMode;
            Debug.Log("CURVED building mode set to OFF");
        }
    }
}
