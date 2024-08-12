using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//Controls a squadron of AI ships
public class SquadController : MonoBehaviour
{
    //VARIABLES
    //Configuration for the squadron
    public GameObject shipType;
    [Range(1,10)]
    public int squadSize;
    //Stores references to the squad ships
    private List<GameObject> squad;
    //Their route is assigned by the spawn point
    [HideInInspector]
    public List<Vector3> route;
    public int curPoint;
    //List of potential hazards to avoid
    public List<Transform> hazards;
    public List<Transform> enemies;
    public float detectionRange = 8;
    public int avoidance;

    // Use this for initialization
    void Start ()
    {
        //Spawn the squad then start the algorithms
        SpawnSquad();
        StartCoroutine("BoidAlgorithm");
        StartCoroutine("UpdateEnemies");
        StartCoroutine("UpdateHazards");
    }

    //Spawn our squad
    void SpawnSquad()
    {
        //Set the width of our squad to how many ships we have times their width so they can't start clipping.
        transform.localScale.Set(shipType.transform.localScale.x * squadSize * 2, transform.localScale.y, transform.localScale.z);
        //Step through on the x, but randomly assign the y and z to look more organic.
        float posStart = -transform.localScale.x / 2;
        float step = transform.localScale.x / squadSize;
        squad = new List<GameObject>();
        for (int i = 0; i < squadSize; i++)
        {
            //As we spawn the ships pass them into our list of ships for later handling
            float yPos = UnityEngine.Random.Range(transform.position.y - transform.localScale.y / 2, transform.position.y + transform.localScale.y / 2);
            float zPos = UnityEngine.Random.Range(transform.position.z - transform.localScale.z / 2, transform.position.z + transform.localScale.z / 2);
            Vector3 pos = new Vector3(posStart + (step * i), yPos, zPos);
            squad.Add((GameObject)Instantiate(shipType, pos, transform.rotation));
            squad[i].GetComponent<ShipBaseClass>().targPoint = route[0];
            squad[i].GetComponent<ShipBaseClass>().myCont = this;
        }
    }

    //Main coroutine for managing the squad
    IEnumerator BoidAlgorithm()
    {
        //check if we're near enough to the current point first, if we are, check if it's the last one.
        CheckPoints();
        //For each squad member
        for (int i = 0; i < squad.Count; i++)
        {
            //Grab a reference to the controller for our ships
            ShipBaseClass curShip = squad[i].GetComponent<ShipBaseClass>();
            int speed = curShip.speed;

            //Run the boid rules to create a combined vector which determines our movement
            Vector3 v1 = Rule1(squad[i].transform); //Group together
            Vector3 v2 = Rule2(squad[i].transform); //Avoid colliding
            Vector3 v3 = Rule3(squad[i].transform); //Match heading with squad
            Vector3 v4 = Rule4(squad[i].transform) * speed; //Follow route, factoring in speed because this is priority
            Vector3 v5 = Rule5(squad[i].transform); //Evade hazards (and really nearby enemies)

            //Assign enemy targets if they're ahead of you.
            curShip.enemies = enemies;
            //tell the squad member to adjust their heading.
            curShip.AdjustHeading(v1 + v2 + v3 + v4 + v5); //Look that direction
        }
        yield return null;
        StartCoroutine("BoidAlgorithm");
    }

    //The ship communicates its death to the controller, remove the ship from the squad and reset all coroutines
    public void RemoveShip(GameObject ship)
    {
        StopAllCoroutines();
        squad.Remove(ship);
        //Need to reset all the coroutines otherwise I'll get exceptions
        if(squad.Count == 0)
        {
            Destroy(this.gameObject);
        }
        StartCoroutine("BoidAlgorithm");
        StartCoroutine("UpdateEnemies");
        StartCoroutine("UpdateHazards");
    }

    //Handles pathing throught the route
    void CheckPoints()
    {
        //Set the squad controller in the middle of the pack.
        if (squad.Count > 0) //bug fix, sometimes this method will be calling when the last ship is destroyed
        {
            Vector3 myPos = Vector3.zero;
            for (int i = 0; i < squad.Count; i++)
            {
                myPos += squad[i].transform.position;
            }
        
            myPos /= squad.Count;
            transform.position = myPos;
        }

        //Check if I'm near the next node.
        if (Vector3.Distance(transform.position, route[curPoint]) < 2)
        {
            //if it's the last one, clear the squad, else increment and return
            if (curPoint + 1 >= route.Count)
            {
                Clear();
                return;
            }
            else
            {
                curPoint++;
                return;
            }
        }
    }

    //We've hit the end of the route, destroy the squadron and the handler
    private void Clear()
    {
        //Destroy the whole squad and the controller
        StopAllCoroutines();
        foreach(GameObject s in squad)
        {
            Destroy(s);
        }
        Destroy(this.gameObject);
    }

    //BOID RULES
    //bias towards percieved centre mass of the group
    Vector3 Rule1(Transform b)
    {
        Vector3 ret = Vector3.zero;
        Vector3 centreMass = Vector3.zero;
        for (int i = 0; i < squad.Count; i++)
        {
            //Don't consider yourself
            if (b != squad[i])
            {
                //add the position of the current element
                centreMass += squad[i].transform.position;
            }
        }
        //divide by the count to get an average
        centreMass /= squad.Count;
        //offset by your own position and scale down
        ret = (centreMass - b.position) / 10;
        //Debug.Log(ret);
        return ret;
    }

    //bias to avoid collisions with squad members
    Vector3 Rule2(Transform b)
    {
        Vector3 ret = Vector3.zero;
        for (int i = 0; i < squad.Count; i++)
        {
            //Don't consider yourself
            if(b != squad[i])
            {
                //if another squad member is within 2 units of me I should avoid them
                if(Vector3.Distance(b.position, squad[i].transform.position) < 2)
                {
                    //reduce the bias by the distance between us, aiming away
                    ret = ret - (squad[i].transform.position - b.position).normalized * 0.2f;
                }
            }
        }
        //Debug.Log(ret);
        return ret;
    }

    //average the velocity of the rest of the squad
    Vector3 Rule3(Transform b)
    {
        Vector3 ret = Vector3.zero;
        for (int i = 0; i < squad.Count; i++)
        {
            //Don't consider yourself
            if (b != squad[i])
            {
                //Apply the squad's velocity to a total and then average it
                ret = squad[i].GetComponent<Rigidbody>().velocity.normalized;
            }
            ret = ret / squad.Count;
        }
        return ret;
    }

    //bias towards a place (the current point)
    Vector3 Rule4(Transform b)
    {
        Vector3 ret = (route[curPoint] - b.position).normalized / 10;
        //Debug.Log(ret);
        return ret;
    }

    //evasion bias
    Vector3 Rule5(Transform b)
    {
        int c = 0;
        Vector3 ret = Vector3.zero;
        //Check the list of hazards
        foreach (Transform h in hazards)
        {
            //Use a dot product to ensure it's ahead of us
            Vector3 toH = (h.position - b.position);
            float hDot = Vector3.Dot(b.TransformDirection(Vector3.forward), toH.normalized);
            if (hDot > 0)
            { 
                //If it is, factor in its position and count it
                //Vector3 
                ret -= toH.normalized * hDot * avoidance * (1/toH.sqrMagnitude);
                c++;
            }
        }
        //Average the value based on count
        if (c > 0)
        {
            ret /= c;
        }
        
        //Debug.Log(ret);
        return ret;
    }

    //Populate lists of enemies and hazards
    //Inefficient, temporary workaround for collision bug, doesn't actually appear to hit performance much.
    IEnumerator UpdateHazards()
    {
        //Get an array of all the hazards in the level, this could be moved to start if I know it won't update.
        GameObject[] allHazards = GameObject.FindGameObjectsWithTag("Hazard");
        //Check through the hazards in the level.
        foreach (GameObject h in allHazards)
        {
            //If it's within detection range and not already in our list, add it.
            if (Vector3.Distance(transform.position,h.transform.position) < detectionRange)
            {
                if (!hazards.Contains(h.transform))
                {
                    hazards.Add(h.transform);
                }
            }
            //If it leaves detection range and it's in our list, cull it.
            else if(hazards.Contains(h.transform))
            {
                hazards.Remove(h.transform);
            }
        }

        //Check enemies as well to avoid collisions
        foreach (Transform e in enemies)
        {
            //If it's within detection range and not already in our list, add it.
            if (Vector3.Distance(transform.position, e.position) < detectionRange)
            {
                if (!hazards.Contains(e.transform))
                {
                    hazards.Add(e.transform);
                }
            }
            //If it leaves detection range and it's in our list, cull it.
            else if (hazards.Contains(e.transform))
            {
                hazards.Remove(e.transform);
            }
        }
        //Delay, to offset the performance hit (doesn't need to be per frame)
        yield return new WaitForSeconds(0.2f);
        //Start again
        StartCoroutine("UpdateHazards");
    }

    //Inefficient, temporary workaround for collision bug, doesn't actually appear to hit performance much.
    IEnumerator UpdateEnemies()
    {
        //Get an array of all the enemies in the level, this could be moved to start if I know it won't update.
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Player");
        //Check through the enemies in the level.
        foreach (GameObject h in allEnemies)
        {
            //If it's within detection range and not already in our list, add it.
            if (Vector3.Distance(transform.position, h.transform.position) < detectionRange * 3)
            {
                if (!enemies.Contains(h.transform))
                {
                    enemies.Add(h.transform);
                }
            }
            //If it leaves detection range and it's in our list, cull it.
            else if (enemies.Contains(h.transform))
            {
                enemies.Remove(h.transform);
            }
        }
        //Delay, to offset the performance hit (doesn't need to be per frame)
        yield return new WaitForSeconds(0.2f);
        //Start again
        StartCoroutine("UpdateEnemies");
    }

    //BROKEN, suspect it's the handling of the squad manager positioning preventing proper collisions
    void OnTriggerEnter(Collider hit)
    {
        if(hit.CompareTag("Player"))// || hit.CompareTag("Player"))
        {
            Debug.Log("Enter");
            enemies.Add(hit.transform);
        }
    }

    void OnTriggerStay(Collider hit)
    {
        if (hit.CompareTag("Player"))// || hit.CompareTag("Player"))
        {
            Debug.Log("Stay");
        }
    }

    void OnTriggerExit(Collider hit)
    {
        if (hit.CompareTag("Player"))// || hit.CompareTag("Player"))
        {
            Debug.Log("Exit");
            enemies.Remove(hit.transform);
        }
    }

    //Gizmos for detection ranges
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0,1,0,0.1f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = new Color(0, 1, 1, 0.1f);
        Gizmos.DrawWireSphere(transform.position, detectionRange * 3);
    }
}
