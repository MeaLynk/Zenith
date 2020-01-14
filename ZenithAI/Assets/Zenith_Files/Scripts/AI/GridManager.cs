using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Grid manager class handles all the grid properties
public class GridManager : MonoBehaviour
{
    // s_Instance is used to cache the instance found in the scene so we don't have to look it up every time.
    private static GridManager s_Instance = null;

    // This defines a static instance property that attempts to find the manager object in the scene and
    // returns it to the caller.
    public static GridManager instance
    {
        get
        {
            if (s_Instance == null)
            {
                // This is where the magic happens.
                //  FindObjectOfType(...) returns the first GridManager object in the scene.
                s_Instance = FindObjectOfType(typeof(GridManager)) as GridManager;
                if (s_Instance == null)
                    Debug.Log("Could not locate an GridManager object. \n You have to have exactly one GridManager in the scene.");
            }
            return s_Instance;
        }
    }

    // Ensure that the instance is destroyed when the game is stopped in the editor.
    void OnApplicationQuit()
    {
        s_Instance = null;
    }

    #region Fields
    public int numOfRows;
    public int numOfColumns;
    public float gridCellSize;
    public bool showGrid = true;
    public bool showObstacleBlocks = true;

    private Vector3 origin = new Vector3();
    private GameObject[] obstacleList;
    private List<Bounds> obstacleBoundList;
    public Node[,] nodes { get; set; }
    #endregion

    //Origin of the grid manager
    public Vector3 Origin
    {
        get { return origin; }
    }

    //Initialise the grid manager
    void Awake()
    {
        obstacleBoundList = new List<Bounds>();
        origin = transform.position;
        //Get the list of obstacles objects tagged as "Obstacle"
        obstacleList = GameObject.FindGameObjectsWithTag("Obstacle");
        CalculateObstacles();
    }

    /// <summary>
    /// Calculate which cells in the grids are mark as obstacles
    /// </summary>
    void CalculateObstacles()
    {
        //Initialise the nodes
        nodes = new Node[numOfRows, numOfColumns];

        int index = 0;
        for (int i = 0; i < numOfRows; i++)
        {
            for (int j = 0; j < numOfColumns; j++)
            {
                Vector3 cellPos = GetGridCellCenter(index);
                Node node = new Node(cellPos);
                nodes[i, j] = node;

                index++;
            }
        }

        // Run through the bObstacle list and set the bObstacle position.
        if (obstacleList != null && obstacleList.Length > 0)
        {
            foreach (GameObject data in obstacleList)
            {
                // Find the maximum bounding box for each obstacle in scene.
                Bounds maxBound = GetMaxBounds(data);
                obstacleBoundList.Add(maxBound);
            }

            // Go through each node. Check if its center position is within one of the obstacles. NOTE: This can be very slow this way with large lists.
            for (int row = 0; row < numOfRows; row++)
            {
                for (int col = 0; col < numOfColumns; col++)
                {
                    foreach (Bounds b in obstacleBoundList)
                    {
                        float cellExtentSize = gridCellSize / 2;
                        Vector3 cellCenter = nodes[row, col].position;

                        Vector3[] cellPositions = new Vector3[] { cellCenter,

                                new Vector3(cellCenter.x - cellExtentSize, cellCenter.y, cellCenter.z - cellExtentSize), new Vector3(cellCenter.x + cellExtentSize, cellCenter.y, cellCenter.z - cellExtentSize),
                                new Vector3(cellCenter.x - cellExtentSize, cellCenter.y, cellCenter.z + cellExtentSize), new Vector3(cellCenter.x + cellExtentSize, cellCenter.y, cellCenter.z + cellExtentSize)
                            };

                        for (int i = 0; i < cellPositions.Length; i++)
                        {
                            bool containsNode = b.Contains(cellPositions[i]);
                            if (containsNode)
                            {
                                nodes[row, col].MarkAsObstacle();
                                break; // break out of position loop, if any position is in obsactle then mark cell as obstacle
                            }
                        }

                    }
                }
            }



            /*foreach (GameObject data in obstacleList)
            {
                int indexCell = GetGridIndex(data.transform.position);
                int col = GetColumn(indexCell);
                int row = GetRow(indexCell);
                if (row >= 0 && row < numOfRows && col >= 0 && col < numOfColumns)
                {
                    //Also make the node as blocked status
                    nodes[row, col].MarkAsObstacle();
                }
            }*/
        }
    }

    /// <summary>
    /// Returns position of the grid cell in world coordinates
    /// </summary>
    public Vector3 GetGridCellCenter(int index)
    {
        Vector3 cellPosition = GetGridCellPosition(index);
        cellPosition.x += (gridCellSize / 2.0f);
        cellPosition.z += (gridCellSize / 2.0f);

        return cellPosition;
    }

    /// <summary>
    /// Returns position of the grid cell in a given index
    /// </summary>
    public Vector3 GetGridCellPosition(int index)
    {
        int row = GetRow(index);
        int col = GetColumn(index);
        float xPosInGrid = col * gridCellSize;
        float zPosInGrid = row * gridCellSize;

        return Origin + new Vector3(xPosInGrid, 0.0f, zPosInGrid);
    }

    /// <summary>
    /// Get the grid cell index in the Astar grids with the position given
    /// </summary>
    public int GetGridIndex(Vector3 pos)
    {
        if (!IsInBounds(pos))
        {
            return -1;
        }

        pos -= Origin;

        int col = (int)(pos.x / gridCellSize);
        int row = (int)(pos.z / gridCellSize);

        return (row * numOfColumns + col);
    }

    /// <summary>
    /// Get the row number of the grid cell in a given index
    /// </summary>
    public int GetRow(int index)
    {
        int row = index / numOfColumns;
        return row;
    }

    /// <summary>
    /// Get the column number of the grid cell in a given index
    /// </summary>
    public int GetColumn(int index)
    {
        int col = index % numOfColumns;
        return col;
    }

    public bool IsInObstacle(Vector3 pos)
    {
        bool bIsInObs = false;

        if (IsInBounds(pos))
        {
            int col, row;

            pos -= Origin;

            col = (int)(pos.x / gridCellSize);
            row = (int)(pos.z / gridCellSize);
            if ((row >= 0 && row < numOfRows) && (col >= 0 && col < numOfColumns))
            {
                Node nodeToCheck = nodes[row, col];
                if (nodeToCheck.bObstacle)
                {
                    bIsInObs = true;
                }
            }
        }
        return bIsInObs;
    }
    /// <summary>
    /// Check whether the current position is inside the grid or not
    /// </summary>
    public bool IsInBounds(Vector3 pos)
    {
        float width = numOfColumns * gridCellSize;
        float height = numOfRows * gridCellSize;

        return (pos.x >= Origin.x && pos.x <= (Origin.x + width) && pos.x <= (Origin.z + height) && pos.z >= Origin.z);
    }


    /// <summary>
    /// Get the neighour nodes in 4 different directions
    /// </summary>
    public void GetNeighbours(Node node, ArrayList neighbors)
    {
        Vector3 neighborPos = node.position;
        int neighborIndex = GetGridIndex(neighborPos);

        int row = GetRow(neighborIndex);
        int column = GetColumn(neighborIndex);

        //Bottom
        int leftNodeRow = row - 1;
        int leftNodeColumn = column;
        AssignNeighbour(leftNodeRow, leftNodeColumn, neighbors);

        //Top
        leftNodeRow = row + 1;
        leftNodeColumn = column;
        AssignNeighbour(leftNodeRow, leftNodeColumn, neighbors);

        //Right
        leftNodeRow = row;
        leftNodeColumn = column + 1;
        AssignNeighbour(leftNodeRow, leftNodeColumn, neighbors);

        //Left
        leftNodeRow = row;
        leftNodeColumn = column - 1;
        AssignNeighbour(leftNodeRow, leftNodeColumn, neighbors);

    }

    /// <summary>
    /// Check the neighbour. If it's not obstacle, assigns the neighbour.
    /// </summary>
    /// <param name='row'>
    /// Row.
    /// </param>
    /// <param name='column'>
    /// Column.
    /// </param>
    /// <param name='neighbors'>
    /// Neighbors.
    /// </param>
    void AssignNeighbour(int row, int column, ArrayList neighbors)
    {
        if (row != -1 && column != -1 && row >= 0 && row < numOfRows && column >= 0 && column < numOfColumns)
        {
            Node nodeToAdd = nodes[row, column];
            if (!nodeToAdd.bObstacle)
            {
                neighbors.Add(nodeToAdd);
            }
        }
    }

    /// <summary>
    /// Show Debug Grids and obstacles inside the editor
    /// </summary>
    void OnDrawGizmos()
    {
        //Draw Grid
        Gizmos.color = Color.blue;
        if (showGrid)
        {
            DebugDrawGrid(transform.position, numOfRows, numOfColumns, gridCellSize, Color.blue);
        }

        //Grid Start Position
        Gizmos.DrawSphere(transform.position, 1.5f);

        //Draw Obstacle obstruction
        /*  if (showObstacleBlocks)
          {
              Vector3 cellSize = new Vector3(gridCellSize, 1.0f, gridCellSize);

              if (obstacleList != null && obstacleList.Length > 0)
              {
                  foreach (GameObject data in obstacleList)
                  {
                      Gizmos.DrawCube(GetGridCellCenter(GetGridIndex(data.transform.position)), cellSize);
                  }
              }
          }
          */
        if (showObstacleBlocks)
        {
            Gizmos.color = Color.red;
            if (obstacleBoundList != null && obstacleBoundList.Count > 0)
            {
                foreach (Bounds b in obstacleBoundList)
                {
                    Gizmos.DrawWireCube(b.center, b.extents * 2);
                }
            }
            if (nodes != null)
            {
                Vector3 cellSize = new Vector3(gridCellSize, 1.0f, gridCellSize);
                // Go through each node. Check if its center position is within one of the obstacles. NOTE: This can be very slow this way with large lists.
                for (int row = 0; row < numOfRows; row++)
                {
                    for (int col = 0; col < numOfColumns; col++)
                    {
                        if (nodes[row, col].bObstacle)
                        {
                            Gizmos.DrawCube(nodes[row, col].position, cellSize);
                        }
                    }
                }

            }

        }
    }

    /// <summary>
    /// Draw the debug grid lines in the rows and columns order
    /// </summary>
    public void DebugDrawGrid(Vector3 origin, int numRows, int numCols, float cellSize, Color color)
    {
        float width = (numCols * cellSize);
        float height = (numRows * cellSize);

        // Draw the horizontal grid lines
        for (int i = 0; i < numRows + 1; i++)
        {
            Vector3 startPos = origin + i * cellSize * new Vector3(0.0f, 0.0f, 1.0f);
            Vector3 endPos = startPos + width * new Vector3(1.0f, 0.0f, 0.0f);
            Debug.DrawLine(startPos, endPos, color);
        }

        // Draw the vertial grid lines
        for (int i = 0; i < numCols + 1; i++)
        {
            Vector3 startPos = origin + i * cellSize * new Vector3(1.0f, 0.0f, 0.0f);
            Vector3 endPos = startPos + height * new Vector3(0.0f, 0.0f, 1.0f);
            Debug.DrawLine(startPos, endPos, color);
        }
    }

    // Recursive method to get a hierarchy's bounding box
    //  Base Case - has no children, returns a single point (childCount == 0)
    //  Recursive Case - adds all the bounding points of it's childrens bounding boxes
    static Bounds GetBoundingBox(Transform go)
    {
        Bounds b = new Bounds(go.position, Vector3.zero);

        for (int i = 0; i < go.childCount; i++)
        {
            b.Encapsulate(GetBoundingBox(go.GetChild(i)));
        }

        return b;

    }

    Bounds GetMaxBounds(GameObject g)
    {
        var b = new Bounds(g.transform.position, Vector3.zero);
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }

}
