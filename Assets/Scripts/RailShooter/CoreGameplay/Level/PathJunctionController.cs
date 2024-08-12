using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathJunctionController : MonoBehaviour
{
    //VARIABLES
    public bool showGizmo = false;
    public bool cameraJunction;
    public RouteManager[] routes;
    public int chosenPath;

    void OnDrawGizmos()
    {
        //Checkbox to toggle gizmos on/off
        if(showGizmo)
        {
            //Different colours for camera and AI junctions
            if(cameraJunction)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.magenta;
            }
            //Cube to differentiate from the route nodes, position at the start of the first route
            //(Pressumably all routes will start from here and you won't turn gizmos on without a route)
            transform.position = routes[0].nodes[0];
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }

    //Give the requesting class a new route from the junction
    public List<Vector3> ReturnRoute ()
    {
        //Make a distinction between camera and AI junctions in case I decide to have different decision making
        //For now just pick a random route from our selection
        chosenPath = Random.Range(0, routes.Length);
        List<Vector3> nodes = new List<Vector3>();
        
        //path junction is for a camera
        if (cameraJunction)
        {
            //Decision Making process... Random, position, ect
            nodes = routes[chosenPath].nodes;
        }
        else //junction is for AI
        {
            //Decision Making process... Random, position, ect
            nodes = routes[chosenPath].nodes;
        }

        //Return the list of nodes on the determined route
        return nodes;
    }
}
