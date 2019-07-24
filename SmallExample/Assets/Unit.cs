using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
//using System.Diagnostics;
public class Unit : MonoBehaviour
{
    public Transform target;
    public LayerMask unwalkableMask;
    List<Vector3> relative;
    List<Node> path = new List<Node>();
    public Vector3[] waypoints;
    //Vector3[] path;
    Grid _grid;
    Node gridNode;
    Grid Agrid;
    bool pathSuccess = false;
    float speed = 20;
    int targetIndex;
    int relX;
    int relY;
    int bigger;
    int size ;
    public void Start()
    {
        //transform.localEulerAngles = new Vector3(target.rotation.x, target.rotation.x, target.rotation.x);
        //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! UNIT START !!!!!!!!!!!!!!!!!!!!!!!!!!");
        //GameObject astar = GameObject.Find("A*");
        //_grid = astar.GetComponent(typeof(Grid)) as Grid;
        //transform.localEulerAngles = new Vector3(target.rotation.x, target.rotation.y, target.rotation.z);

        //_grid.CheckUnwalkable();
        //GridForUnit();
        //StartFindPath(transform.position, target.position);//only find, not move

        //Debug.Log("8888888888888888888888   START END   88888888888888888888888888888888888888888");
        //GameObject.Find("").SendMessage("");
        //_grid.NodeFromWorldPoint(transform.position);
    }

    public void SetGrid(Grid incomeGrid)
    {
        _grid = incomeGrid;
    }
    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        //StartCoroutine(FindPath(startPos, targetPos));
        FindPath(startPos, targetPos);
    }
    //    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! BEGIN FIND PATH !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
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
                Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! FIND PATH IN FUNCTION !!!!!!!!!!!!!!!!!!!!!!!!!!");
                //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! UNWALKABLE NUMBER !!!!!!!!!!!!!!!!!!!!!!!!!!");
                //Debug.Log(number);
                //relative=null;
                break;
            }
            foreach (Node neighbour in _grid.GetNeighbours(currentNode))
            {
                bool checkR = CheckRelative(relative, neighbour.worldPosition);
                if ((!neighbour.walkable) || checkR)
                {
                    if (!neighbour.walkable)
                    {
                        neighbour.hCost = GetDistance(neighbour, targetNode) * 100000;
                        //Debug.Log("///////////////////////// UNWALKABLE  ///////////////////////////////");
                        //Debug.Log(neighbour.worldPosition);
                        number++;
                    }
                    if (checkR)
                    {
                        neighbour.hCost = GetDistance(neighbour, targetNode) * 100000;
                        neighbour.walkable = false;
                        //Debug.Log("////////////////////////////////////////////////////////////////////// CHECKR  ///////////////////////////////");
                    }
                    //Debug.Log("///////////////////////// HIGH H  ///////////////////////////////");
                }
                else
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                //所有前期处理，H都设好
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                //neighbour.gCost = newMovementCostToNeighbour;
                if (closedSet.Contains(neighbour))
                {
                    continue;
                }
                if (newMovementCostToNeighbour < neighbour.gCost)
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.parent = currentNode;
                    //if (!openSet.Contains(neighbour))
                    //    openSet.Add(neighbour);
                }
                if (!openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.parent = currentNode;
                    openSet.Add(neighbour);
                }
            }
        }
        Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! BEFORE Y  !!!!!!!!!!!!!!!!!!!!!!!!!!");

        //only find, not move
        Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! BEFORE IF  !!!!!!!!!!!!!!!!!!!!!!!!!!");
        if (pathSuccess)//to change the path
        {
            Debug.Log("///////////////////////// FIND A PATH, RETRACE PATH  ///////////////////////////////");
            waypoints = RetracePath(startNode, targetNode);

            Debug.Log("///////////////////////// WAYPOINTS  ///////////////////////////////");
            Debug.Log(waypoints.Length);
            //StartCoroutine(FollowPath());
        }
        else
            Debug.Log("BBBBB");
        //yield return null;
    }

    public bool CheckSuccess()
    {
        return pathSuccess;
    }
    bool CheckRelative(List<Vector3> list, Vector3 vec1)
    {
        //Debug.Log("------------------check-relative-----------------------------------");
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
                check = true;
                continue;
            }

        }
        return check;
    }


    public Vector3[] ToRetracePath(Node startNode, Node endNode)
    {
        return RetracePath(startNode, endNode);
    }
    Vector3[] RetracePath(Node startNode, Node endNode)//also get the right path of list node
    {
        Debug.Log("///////////////////////// START RETRACE PATH  ///////////////////////////////");
        Node currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] theWaypoints = PathA(path);
        Array.Reverse(theWaypoints);
        path.Reverse();
        Debug.Log("///////////////////////// FINISH RETRACE PATH  ///////////////////////////////");
        Debug.Log("///////////////////////// LENGTH OF PATH  ///////////////////////////////");
        Debug.Log(path.Count);
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
    }
    IEnumerator FollowPath()
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
                    Debug.Log("------------------------------- FOLLOW PATH FINISH ----------------------------------------------");
                    //transform.rotation.x=target.rotation.x;
                    //transform.rotation.x = target.rotation.y;
                    //transform.rotation.x=target.rotation.z;
                    //transform.localEulerAngles = new Vector3(target.rotation.x, target.rotation.y, target.rotation.z);
                    //Debug.Log("-----------------------------------------------------------------------------");

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
    void GridForUnit()
    {
        Debug.Log("&&&&&&&&&&&&&&&&&&&&&&&& GRID FOR UNIT &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&");
        Node centerNode;
        GameObject bstar = GameObject.Find("A*");
        relative = new List<Vector3>();
        Agrid = bstar.GetComponent(typeof(Grid)) as Grid;
        centerNode = Agrid.NodeFromWorldPoint(transform.position);
        //get all the grid information for the unit

        relX = Mathf.CeilToInt(transform.localScale.x);
        relY = Mathf.CeilToInt(transform.localScale.z);

        if (relX < relY)
            bigger = Mathf.CeilToInt(relY / (_grid.nodeRadius * 2));
        else
            bigger = Mathf.CeilToInt(relX / (_grid.nodeRadius * 2));

        Debug.Log("&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&   RELY    &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&");
        Debug.Log(bigger);
        //MESH COLLIDER
        Vector3 halfExtents;
        halfExtents.x = Agrid.nodeRadius;
        halfExtents.y = Agrid.nodeRadius;
        halfExtents.z = Agrid.nodeRadius;

        if (Mathf.CeilToInt(bigger / 2) == Mathf.CeilToInt((bigger + 1) / 2))//odd
            bigger++;
        for (int i = 0; i < bigger; i++)
        {
            for (int j = 0; j < bigger; j++)
            {
                //Physics.CheckSphere(Agrid.grid[centerNode.gridX - Mathf.CeilToInt(bigger/2) + i, centerNode.gridY - Mathf.CeilToInt(bigger / 2) + j].worldPosition, Agrid.nodeRadius, unwalkableMask)
                Quaternion qua;
                qua = Quaternion.Euler(0, 0, 0);
                if (Physics.CheckBox(Agrid.grid[centerNode.gridX - Mathf.CeilToInt(bigger / 2) + i, centerNode.gridY - Mathf.CeilToInt(bigger / 2) + j].worldPosition, halfExtents, transform.rotation, unwalkableMask))
                {
                    relative.Add(Agrid.grid[centerNode.gridX - Mathf.CeilToInt(bigger / 2) + i, centerNode.gridY - Mathf.CeilToInt(bigger / 2) + j].worldPosition - transform.position);
                    //Debug.Log("PPPPPPPPPPPPPPPPPPPPPPPPPPPPP   IS RELATIVE   PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP");
                }
            }
        }
        Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! RELATIVE COUNT !!!!!!!!!!!!!!!!!!!!!!!!!!");
        Debug.Log(relative.Count);

    }

    //set waypoints for the obj

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