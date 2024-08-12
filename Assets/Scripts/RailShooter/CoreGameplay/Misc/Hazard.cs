using UnityEngine;
using System.Collections.Generic;

public class Hazard : MonoBehaviour, iHurtable
{
    //Hazard base class, all the standard stuff most hazards need to have and do 
    //VARIABLES
    //For the hit method
    public int health;
    public GameObject impactParticle;

    //For the route, should it need one
    [HideInInspector]
    public List<Vector3> route;
    public bool doesMove;
    public Color nodeColour;
    public Color pathColour;

    //All objects should have a method for handling thier destruction
    public virtual void Destruction()
    {

    }

    //Hazards are hurtable so they'll all have a basic hit method implementation
    public void Hit(int damage, Vector3 point)
    {
        health -= damage;

        //spawn impact particle at impact point
        if(health <= 0)
        {
            Destruction();
        }
    }

    //Draw the movement path if it's selected and it has one
    void OnDrawGizmosSelected()
    {
        if (doesMove)
        {
            //Draw the route they'll path along
            for (int i = 0; i < route.Count; i++)
            {
                //Draw a sphere at the node position
                Gizmos.color = nodeColour;
                Gizmos.DrawWireSphere(route[i], 0.5f);

                //Make sure we have a next node, draw a line between the current and the next
                if (i + 1 < route.Count)
                {
                    Gizmos.color = pathColour;
                    Gizmos.DrawLine(route[i], route[i + 1]);
                }
            }
        }
    }
}
