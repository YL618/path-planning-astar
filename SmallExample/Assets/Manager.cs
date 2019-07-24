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
    public GameObject obj5;
    public GameObject obj6;

    public GameObject targ1;
    public GameObject targ2;
    public GameObject targ3;
    public GameObject targ4;
    public GameObject targ5;
    public GameObject targ6;


    public LayerMask unwalkableMask;
    public List<Vector3> relative;

    List<Node> path;
    Vector3[] waypoints;

    private Unit unit_comp;
    Grid ManagerGrid;
    bool FollowFinish=true;
    bool pathSuccess = false;

    //List<GameObject> ObjectList=new List<GameObject>();
    Queue<GameObject> ObjectQueue=new Queue<GameObject>();
    Queue<GameObject> Sequence = new Queue<GameObject>();
    // Start is called before the first frame update
    public IEnumerator Start()
    {
        Debug.Log("1111111111111111111111111111 MANAGER START 1111111111111111111111111111111");
        GameObject astar = GameObject.Find("A*");
        ManagerGrid = astar.GetComponent(typeof(Grid)) as Grid;//get grid value

        //Add all the objects to the queue
        ObjectQueue.Enqueue(obj1);
        ObjectQueue.Enqueue(obj2);
        ObjectQueue.Enqueue(obj3);
        ObjectQueue.Enqueue(obj4);
        ObjectQueue.Enqueue(obj5);
        ObjectQueue.Enqueue(obj6);

        while (ObjectQueue.Count>0)
        {
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! Queue count !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Debug.Log(ObjectQueue.Count);

            GameObject objTemp = ObjectQueue.Dequeue();
            unit_comp = objTemp.GetComponent(typeof(Unit)) as Unit;//get the unit of this object
            unit_comp.enabled = true;
            
            unit_comp.SetGrid(ManagerGrid);
            ManagerGrid.CheckUnwalkable();
            unit_comp.ManagerGridForUnit();
            //setting all the unwalkable&walkable
            unit_comp.StartFindPath(objTemp.transform.position, unit_comp.target.position);

            List<Node> ObjPath;
            ObjPath = unit_comp.PathTranfer();

            if (!Movable(ObjPath))//if fail, unit set off, break
            {
                Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! FAILTO FIND PATH !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                ObjectQueue.Enqueue(objTemp);
                unit_comp.enabled = false;
                Clear(unit_comp);
                yield return null;
            }
            else//if path success,move
            {
                Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! SUCCESS ManagerFollowpath !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Sequence.Enqueue(objTemp);
                yield return this.StartCoroutine(unit_comp.FollowPath());
            }
            yield return null;

        }




    }
    //foreach (GameObject obj in ObjectList)
    //{
    //    Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! SWITCH GAME OBJ !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    //    //想要只有在前一个path走完之后再走

    //    unit_comp = obj.GetComponent(typeof(Unit)) as Unit;//get the unit of this object
    //    unit_comp.enabled = true;
    //    ManagerGrid.CheckUnwalkable();
    //    unit_comp.SetGrid(ManagerGrid);
    //    unit_comp.ManagerGridForUnit();
    //    //unit_comp.GridForUnit();
    //    unit_comp.StartFindPath(obj.transform.position, unit_comp.target.position);

    //        //unit_comp.enabled = true;//start unit do the pathfinding in unit, will always fiure out a path
    //                                 //ObjWaypoints = unit_comp.WayPoints();

    //        //since the system will go through the strat function before calling unit's start function
    //        //we have to move all the commands in unit's start function here!!!!!

    //        //unit_comp.SetGrid(ManagerGrid);

    //        //unit_comp.ManagerGridForUnit();
    //        //StartFindPath(obj.transform.position, unit_comp.target.position);//only find, not move

    //        //StartFindPath(obj.transform.position, unit_comp.target.position, unit_comp);

    //        //After that the path isn't empty anymore
    //        List<Node> ObjPath;
    //        ObjPath = unit_comp.PathTranfer();//get the path from unit

    //        if (!Movable(ObjPath))//if fail, unit set off, break
    //        {
    //            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! FAILTO FIND PATH !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    //            ObjectList.Remove(obj);
    //            ObjectList.Add(obj);
    //            unit_comp.enabled = false;
    //            continue;
    //        }
    //        else//if path success,move
    //        {
    //            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!! SUCCESS ManagerFollowpath !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    //            unit_comp.ManagerFollowpath();//有可能这句有问题！！！
    //            //yield return new WaitForFixedUpdate(unit_comp);

    //            //yield return null;
    //        }
    //}


    bool Movable(List<Node> CheckList)
    {
        bool movable = true;
        foreach (Node node in CheckList)
        {
            if (!node.walkable)
                movable = false;
        }
        return movable;
    }

    void Clear(Unit unit)
    {
        unit.relative.Clear();
        unit.path.Clear();
        unit.NodeForUnit.Clear();
        unit.waypoints= new Vector3[0];
    }





    void Update()
    {

    }
}
