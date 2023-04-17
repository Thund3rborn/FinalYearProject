using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gridgenerator : MonoBehaviour
{
    public int gridSize;
    public float cellSize;
    public Material lineMaterial;

    private LineRenderer[,] lines;

    void Start()
    {
        lines = new LineRenderer[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject lineObject = new GameObject("Line " + x + "," + y);
                lineObject.transform.parent = transform;
                lines[x, y] = lineObject.AddComponent<LineRenderer>();
                lines[x, y].material = lineMaterial;
                lines[x, y].positionCount = 2;
                lines[x, y].startWidth = 0.1f;
                lines[x, y].endWidth = 0.1f;

                Vector3 startPosition = new Vector3(x * cellSize, 0, y * cellSize);
                Vector3 endPosition = new Vector3((x + 1) * cellSize, 0, y * cellSize);

                if (x < gridSize - 1)
                {
                    lines[x, y].SetPositions(new Vector3[] { startPosition, endPosition });
                }

                if (y > 0 && lines[x, y - 1] != null)
                {
                    Vector3 previousEndPosition = lines[x, y - 1].GetPosition(1);
                    lines[x, y].SetPosition(0, previousEndPosition);
                }
            }
        }
    }
}
