using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RouteManager : MonoBehaviour
{
    //VARIABLES
    public bool showRoute;
    public Color nodeColour;
    public Color pathColour;
    public List<Vector3> nodes;
    
    //Go through the nodes on the route, draw a sphere at each point and a line between them
    void OnDrawGizmos()
    {
        //Checkbox to handle showing the gizmos
        if(showRoute)
        {
            for(int i = 0; i < nodes.Count; i++)
            {
                //Draw a sphere at the node position
                Gizmos.color = nodeColour;
                Gizmos.DrawWireSphere(nodes[i], 0.5f);

                //Make sure we have a next node, draw a line between the current and the next
                if(i+1 < nodes.Count)
                {
                    Gizmos.color = pathColour;
                    Gizmos.DrawLine(nodes[i], nodes[i + 1]);
                }
            }
        }
    }
}
