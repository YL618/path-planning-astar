using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
//using System.Diagnostics;
public class Unit : MonoBehaviour
{
    public Transform target;
    public LayerMask unwalkableMask;//very improtant for the collision detection 
    public List<Node> path = new List<Node>();
    public HashSet<Node> NodeForUnit = new HashSet<Node>();

    Vector3[] waypoints;
    List<Vector3> relative;
    Grid _grid;
    Grid Agrid;
    bool pathSuccess = false;
    float speed = 200;
    int targetIndex;
    int relX;
    int relY;
    int bigger;

    public void SetGrid(Grid incomeGrid)//grid need to updated every time before moving the wall
    {
        _grid = incomeGrid;
    }

    public void CheckRelativeClear()
    {
        relative.Clear();
    }
    public void CheckWaypointsClear()
    {
        waypoints = new Vector3[0];
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        //StartCoroutine(FindPath(startPos, targetPos));
        FindPath(startPos, targetPos);
    }
    //    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    void FindPath(Vector3 startPos, Vector3 targetPos)//change the original IEnumerator to normal function, but IEnumerator may still work 
    {
        waypoints = new Vector3[0];
        //bool pathSuccess = false;
        Node startNode = _grid.NodeFromWorldPoint(startPos);
        Node targetNode = _grid.NodeFromWorldPoint(targetPos);
        Heap<Node> openSet = new Heap<Node>(_grid.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        int number = 0;
        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);
            if (currentNode == targetNode)
            {
                pathSuccess = true;
                break;
            }
            foreach (Node neighbour in _grid.GetNeighbours(currentNode))
            {
                bool checkR = CheckRelative(relative, neighbour.worldPosition);
                neighbour.hCost = GetDistance(neighbour, targetNode);

                    if (!neighbour.walkable)
                        {
                            neighbour.hCost = GetDistance(neighbour, targetNode) * 100000;
                            number++;
                        }
                if (checkR)
                {
                    neighbour.hCost = GetDistance(neighbour, targetNode) * 100000;
                    neighbour.walkable = false;
                }
                if (NodeForUnit.Contains(neighbour))
                    {
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.walkable = true;
                    }
   
                //Heuristic Part
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                if (closedSet.Contains(neighbour))
                {
                    continue;
                }
                if (newMovementCostToNeighbour < neighbour.gCost)
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.parent = currentNode;
                }
                if (!openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.parent = currentNode;
                    openSet.Add(neighbour);
                }
            }
        }

        //only find, not move
        if (pathSuccess)//to change the path
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        else
            Debug.Log("BBBBB");
        //yield return null;
    }

    public bool CheckSuccess()
    {
        return pathSuccess;
    }

    //This function is used for checking whether moving to the current node will cause any collision
    //pass the stored relative Vector position storeds in relative to CheckRelative and see wheter there will a collision
    bool CheckRelative(List<Vector3> list, Vector3 vec1) 
    {
        bool check = false;
        Vector3 relatePosition;

        foreach (Vector3 vec in list)
        {
            relatePosition = vec1 + vec;
            Vector3 halfExtents;
            halfExtents.x = Agrid.nodeRadius;
            halfExtents.y = Agrid.nodeRadius;
            halfExtents.z = Agrid.nodeRadius;

            Node current = _grid.NodeFromWorldPoint(relatePosition);
            Quaternion qua;
            qua = Quaternion.Euler(0, 0, 0);
            //Physics.CheckSphere(relatePosition, Agrid.nodeRadius, unwalkableMask)    
            if (Physics.CheckBox(current.worldPosition, halfExtents, transform.rotation, unwalkableMask))
            {
                if(!NodeForUnit.Contains(_grid.NodeFromWorldPoint(current.worldPosition)))
                {
                    check = true;
                    break;
                }    
            }

        }
        return check;
    }


    public Vector3[] ToRetracePath(Node startNode, Node endNode)
    {
        return RetracePath(startNode, endNode);
    }
    Vector3[] RetracePath(Node startNode, Node endNode)//get the right path of list node
    {
        Node currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] theWaypoints = PathA(path);
        Array.Reverse(theWaypoints);
        path.Reverse();
        return theWaypoints;
    }
    public List<Node> PathTranfer()//return the finding path to manager
    {
        return path;
    }
    Vector3[] PathA(List<Node> path)
    {
        List<Vector3> Awaypoints = new List<Vector3>();

        for (int i = 0; i < path.Count; i++)
        {
            Awaypoints.Add(path[i].worldPosition);
        }
        return Awaypoints.ToArray();
    }
    
    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
    public void ManagerFollowpath()
    {
        Debug.Log("zzzzzzzzzzzzzzzzzzzzzzzzzzzz MANAGER FOLLOWPATH zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz");
        StartCoroutine(FollowPath());
        //FollowPath();
    }
    public IEnumerator FollowPath()
    {
        Debug.Log("zzzzzzzzzzzzzzzzzzzzzzzzz  FOLLOW PATH  zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz");
        Vector3 currentWaypoint = waypoints[0];

        while (true)
        {
            if (transform.position == currentWaypoint)
            {
                targetIndex++;
                if (targetIndex >= waypoints.Length)
                {
                    targetIndex = 0;
                    waypoints = new Vector3[0];
                    transform.position = target.position;
                    yield break;
                }
                currentWaypoint = waypoints[targetIndex];
            }

            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
            yield return null;

        }

    }
    public void ManagerGridForUnit()
    {
        GridForUnit();
    }

    //store the relative position between each node the current wall occupies and the node for the wall's center
    //store them for the later use of checking the collision in CheckRelative function
    //also note that we need to set the node current wall occupys to walkable, otherwise the path will always contains unwalkable nodes
    void GridForUnit()
    {
        Node centerNode;
        GameObject bstar = GameObject.Find("A*");
        relative = new List<Vector3>();
        Agrid = bstar.GetComponent(typeof(Grid)) as Grid;
        centerNode = Agrid.NodeFromWorldPoint(transform.position);
        //get all the grid information for the unit

        //draw an area for the nodes the current wall occupies
        relX = Mathf.CeilToInt(transform.localScale.x);
        relY = Mathf.CeilToInt(transform.localScale.z);

        int Xcount= Mathf.CeilToInt(relX / (_grid.nodeRadius * 2))+2;
        int Ycount= Mathf.CeilToInt(relY / (_grid.nodeRadius * 2))+2;

        Vector3 halfExtents;
        halfExtents.x = Agrid.nodeRadius;
        halfExtents.y = Agrid.nodeRadius;
        halfExtents.z = Agrid.nodeRadius;


        for (int i = 0; i < Xcount; i++)
        {
            for (int j = 0; j < Ycount; j++)
            {
                if (Physics.CheckBox(Agrid.grid[centerNode.gridX - Mathf.CeilToInt(Xcount / 2) + i, centerNode.gridY - Mathf.CeilToInt(Ycount / 2) + j].worldPosition, halfExtents, transform.rotation, unwalkableMask))
                {
                    relative.Add(Agrid.grid[centerNode.gridX - Mathf.CeilToInt(Xcount / 2) + i, centerNode.gridY - Mathf.CeilToInt(Ycount / 2) + j].worldPosition - transform.position);
                    NodeForUnit.Add(Agrid.grid[centerNode.gridX - Mathf.CeilToInt(Xcount / 2) + i, centerNode.gridY - Mathf.CeilToInt(Ycount / 2) + j]);
                    Agrid.grid[centerNode.gridX - Mathf.CeilToInt(Xcount / 2) + i, centerNode.gridY - Mathf.CeilToInt(Ycount / 2) + j].walkable = true;
                }
            }
        }
    }

    public void OnDrawGizmos()
    {
        if (waypoints != null)
        {
            for (int i = targetIndex; i < waypoints.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(waypoints[i], Vector3.one);

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, waypoints[i]);
                }
                else
                {
                    Gizmos.DrawLine(waypoints[i - 1], waypoints[i]);
                }
            }
        }
    }
}