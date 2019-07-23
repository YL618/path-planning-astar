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