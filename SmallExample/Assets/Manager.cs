using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class Manager : MonoBehaviour
{
    public GameObject obj1;
    public GameObject obj2;
    public GameObject obj3;
    public GameObject obj4;

    public GameObject targ1;
    public GameObject targ2;
    public GameObject targ3;
    public GameObject targ4;
    public LayerMask unwalkableMask;
    public List<Vector3> relative;

    List<Node> path;
    float speed = 20;
    int targetIndex;
    int bigger;
    int relX;
    int relY;
    Vector3[] waypoints;

    private Unit unit_comp;
    Grid ManagerGrid;
    bool FollowFinish=true;
    bool pathSuccess = false;

    List<GameObject> ObjectList=new List<GameObject>();
    // Start is called before the first frame update
    public void Start()
    {
        Debug.Log("1111111111111111111111111111 MANAGER START 1111111111111111111111111111111");
        GameObject astar = GameObject.Find("A*");
        ManagerGrid = astar.GetComponent(typeof(Grid)) as Grid;//get grid value

        //Add all the objects to the list
        ObjectList.Add(obj1);
        ObjectList.Add(obj2);
        ObjectList.Add(obj3);
        ObjectList.Add(obj4);

        if (ObjectList.Count > 0)
        {
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! List count !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Debug.Log(ObjectList.Count);

            foreach (GameObject obj in ObjectList)
            {
                Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! SWITCH GAME OBJ !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                //想要只有在前一个path走完之后再走
                
                unit_comp = obj.GetComponent(typeof(Unit)) as Unit;//get the unit of this object
                ManagerGrid.CheckUnwalkable();
                GridForUnit(obj);
                StartFindPath(unit_comp.transform.position, unit_comp.target.position, obj,unit_comp);

                //if (!Movable(path))//if fail, unit set off, break
                //{
                //        Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! FAILTO FIND PATH !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                //        ObjectList.Remove(obj);
                //        ObjectList.Add(obj);
                //        unit_comp.enabled = false;
                //        continue;
                //}
                //else//if path success,move
                //{
                //        Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! SUCCESS ManagerFollowpath !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                //        //unit_comp.ManagerFollowpath();//有可能这句有问题！！！
                //        //yield return new WaitForFixedUpdate(unit_comp);
                //        while (obj.transform.position != unit_comp.target.position)
                //        {

                //        }
                //        //yield return null;
                //}
            }
        }

    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos, GameObject obj,Unit aunit)
    {
        StartCoroutine(FindPath(startPos, targetPos, obj,aunit));
        //FindPath(startPos, targetPos);
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos,GameObject obj,Unit aunit)
    {
        //Debug.Log("Start finding!---------------------------------------------------------------------------------");
        waypoints = new Vector3[0];
        Debug.Log(startPos);
        Debug.Log(targetPos);
        Node startNode = ManagerGrid.NodeFromWorldPoint(startPos);
        Node targetNode = ManagerGrid.NodeFromWorldPoint(targetPos);

        Heap<Node> openSet = new Heap<Node>(ManagerGrid.MaxSize);
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

                Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! FIND PATH !!!!!!!!!!!!!!!!!!!!!!!!!!");
                Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! UNWALKABLE NUMBER !!!!!!!!!!!!!!!!!!!!!!!!!!");
                Debug.Log(number);
                //relative=null;
                break;
            }

            foreach (Node neighbour in ManagerGrid.GetNeighbours(currentNode))
            {
                bool checkR = CheckRelative(relative, neighbour.worldPosition);

                if ((!neighbour.walkable) || checkR)
                {
                    if (!neighbour.walkable)
                    {
                        neighbour.hCost = GetDistance(neighbour, targetNode) * 100000;
                        Debug.Log("///////////////////////// UNWALKABLE  ///////////////////////////////");
                        Debug.Log(neighbour.worldPosition);
                        number++;
                    }
                    if (checkR)
                    {
                        neighbour.hCost = GetDistance(neighbour, targetNode) * 100000;
                        Debug.Log("////////////////////////////////////////////////////////////////////// CHECKR  ///////////////////////////////");
                    }
                    Debug.Log("///////////////////////// HIGH H  ///////////////////////////////");
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
        yield return null;
        if (pathSuccess)
        {
            //waypoints = RetracePath(startNode, targetNode);
            waypoints = RetracePath(startNode, targetNode);
            StartCoroutine(FollowPath(obj,aunit));
        }
        else
            Debug.Log("BBBBB");

    }


    bool CheckRelative(List<Vector3> list, Vector3 vec1)
    {
        Debug.Log("------------------check-relative-----------------------------------");
        bool check = false;
        Vector3 relatePosition;

        foreach (Vector3 vec in list)
        {
            relatePosition = vec1 + vec;
            Vector3 halfExtents;
            halfExtents.x = ManagerGrid.nodeRadius;
            halfExtents.y = ManagerGrid.nodeRadius;
            halfExtents.z = ManagerGrid.nodeRadius;

            Node current = ManagerGrid.NodeFromWorldPoint(relatePosition);
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
    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] theWaypoints = PathA(path);
        Array.Reverse(theWaypoints);
        return theWaypoints;
        //path.Reverse();
        //_grid.Apath = path;

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

    IEnumerator FollowPath(GameObject obj,Unit unit)
    {
        Debug.Log("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz");
        Vector3 currentWaypoint = waypoints[0];

        while (true)
        {
            if (obj.transform.position == currentWaypoint)
            {
                targetIndex++;
                if (targetIndex >= waypoints.Length)
                {
                    targetIndex = 0;
                    waypoints = new Vector3[0];
                    obj.transform.position = unit.target.position;
                    //transform.rotation.x=target.rotation.x;
                    //transform.rotation.x = target.rotation.y;
                    //transform.rotation.x=target.rotation.z;
                    //transform.localEulerAngles = new Vector3(target.rotation.x, target.rotation.y, target.rotation.z);
                    Debug.Log("-----------------------------------------------------------------------------");

                    yield break;
                }
                currentWaypoint = waypoints[targetIndex];
            }

            obj.transform.position = Vector3.MoveTowards(obj.transform.position, currentWaypoint, speed * Time.deltaTime);
            yield return null;

        }

    }

    void GridForUnit(GameObject obj)
    {
        Debug.Log("&&&&&&&&&&&&&&&&&&&&&&&& GRID FOR UNIT &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&");
        Node centerNode;      
        relative = new List<Vector3>();
        centerNode = ManagerGrid.NodeFromWorldPoint(obj.transform.position);
        //get all the grid information for the current unit

        relX = Mathf.CeilToInt(obj.transform.localScale.x);
        relY = Mathf.CeilToInt(obj.transform.localScale.z);

        if (relX < relY)
            bigger = Mathf.CeilToInt(relY / (ManagerGrid.nodeRadius * 2));
        else
            bigger = Mathf.CeilToInt(relX / (ManagerGrid.nodeRadius * 2));

        Debug.Log("&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&   RELY    &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&");
        Debug.Log(bigger);
        //MESH COLLIDER
        Vector3 halfExtents;
        halfExtents.x = ManagerGrid.nodeRadius;
        halfExtents.y = ManagerGrid.nodeRadius;
        halfExtents.z = ManagerGrid.nodeRadius;

        if (Mathf.CeilToInt(bigger / 2) == Mathf.CeilToInt((bigger + 1) / 2))//odd
            bigger++;
        for (int i = 0; i < bigger; i++)
        {
            for (int j = 0; j < bigger; j++)
            {
                //Physics.CheckSphere(Agrid.grid[centerNode.gridX - Mathf.CeilToInt(bigger/2) + i, centerNode.gridY - Mathf.CeilToInt(bigger / 2) + j].worldPosition, Agrid.nodeRadius, unwalkableMask)
                //Quaternion qua;
                //qua = Quaternion.Euler(0, 0, 0);
                if (Physics.CheckBox(ManagerGrid.grid[centerNode.gridX - Mathf.CeilToInt(bigger / 2) + i, centerNode.gridY - Mathf.CeilToInt(bigger / 2) + j].worldPosition, halfExtents, transform.rotation, unwalkableMask))
                {
                    relative.Add(ManagerGrid.grid[centerNode.gridX - Mathf.CeilToInt(bigger / 2) + i, centerNode.gridY - Mathf.CeilToInt(bigger / 2) + j].worldPosition - obj.transform.position);
                    //Debug.Log("PPPPPPPPPPPPPPPPPPPPPPPPPPPPP   IS RELATIVE   PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP");
                }
            }
        }
        Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! RELATIVE COUNT !!!!!!!!!!!!!!!!!!!!!!!!!!");
        Debug.Log(relative.Count);

    }

    bool Movable(List<Node> CheckList)
    {
        bool movable=true;
        foreach (Node node in CheckList)
        {
            if (!node.walkable)
                movable = false;
        }
        return movable;
    }


    void Update()
    {
    //    Vector3 vec_temp1;
    //    Vector3 vec_temp2;
    //    Vector3 vec_temp3;
        
    //    vec_temp1 = obj1.transform.position - targ1.transform.position;
    //    vec_temp2 = obj2.transform.position - targ2.transform.position;
    //    vec_temp3 = obj3.transform.position - targ3.transform.position;
        

    //    if (vec_temp1.magnitude <= 1)
    //    {
    //        unit_comp2.enabled = true;
    //        //obj2.SetActive(true);
    //        Debug.Log("enter2");
    //    }
    //    else Debug.Log("D1");


    //    if (vec_temp2.magnitude <= 1)
    //    {
    //        unit_comp3.enabled = true;
    //        Debug.Log("enter3");
    //    }
    //    else
    //    {
    //        Debug.Log("D2");
    //        Debug.Log(vec_temp2);
    //    }

    //        if (vec_temp3.magnitude <= 1)
    //    {
    //        unit_comp4.enabled = true;
    //        Debug.Log("enter4");
    //    }
    //        else Debug.Log("D3");
    }
}
