using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ShipBaseClass : MonoBehaviour, iHurtable
{
    //VARIABLES
    [HideInInspector]
    public List<Transform> enemies;
    public Vector3 targPoint;
    private Rigidbody rig;
    public int speed;
    public Transform enemy;
    [HideInInspector]
    public SquadController myCont;
    public int damage;

    public float fireRate = 0;
    private float fireTimer;

    [Range(0,1)]
    public float attackCone;

    //Managing health and behaviour
    public int health;
    [HideInInspector]
    public bool isAlive;

    void Awake()
    {
        rig = GetComponent<Rigidbody>();
        isAlive = true;
    }

    //Squad controller communicates the direction we should be looking
    public void AdjustHeading(Vector3 adjustment)
    {
        targPoint = transform.position + adjustment.normalized;
    }
	
	void FixedUpdate ()
    {
        if (isAlive)
        {
            //Look the way we're told to, use a slerp to make it smooth.
            Quaternion targetRotation = Quaternion.LookRotation(targPoint - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f);

            //for now, cancel out our velocity then propel forward by our speed value.
            //may replace with proper acceleration later
            rig.velocity = Vector3.zero;
            rig.AddRelativeForce(Vector3.forward * speed);

            enemy = SetTarget();

            if (enemy != null)
            {
                Fire();
            }

            //health is below 0, stop reacting and dive
            if (health < 0)
            {
                StartCoroutine("Destruction");
            }
        }

        //Just explode
        if (health < -10)
        {
            //Explode
            //Spawn particles
            Destroy(gameObject);
        }
    }

    IEnumerator Destruction()
    {
        //Activate Death Dive
        isAlive = false;
        //remove yourself from the squad controller's list
        myCont.RemoveShip(this.gameObject);
        //Spin out of control
        transform.Rotate(0, 0, 180 * Time.smoothDeltaTime);
        yield return new WaitForSeconds(2.0f);
        health -= 10; //to force explosion to trigger
        yield return null;
    }

    private Transform SetTarget()
    {
        //Check the list of enemies and return one if it's inside our cone, otherwise return null
        //We can return the first we find for now, may add more prioritisation decision making later
        foreach (Transform e in enemies)
        {
            //Use a dot product to ensure it's ahead of us
            Vector3 toE = (e.position - transform.position);
            float eDot = Vector3.Dot(transform.TransformDirection(Vector3.forward), toE.normalized);
            if (eDot > attackCone)
            {
                return e;
            }
        }
        return null;
    }

    void Fire()
    {
        if (fireTimer > 0)
        {
            fireTimer -= Time.smoothDeltaTime;
        }
        else
        {
            //Shoot laser
            RaycastHit hit;
            Vector3 fwd = (enemy.position - transform.position).normalized;
            if (Physics.Raycast(transform.position, fwd, out hit))
            {
                //Collect all the monobehaviours on the object incase it has more than one
                MonoBehaviour[] scripts = hit.transform.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour mb in scripts)
                {
                    //if one of the scripts implements the interface for being hit, then run that method
                    if (mb is iHurtable)
                    {
                        //Debug.Log(hit.transform.name);
                        fireTimer = fireRate;
                        Debug.DrawRay(transform.position, fwd * 100, Color.red);
                        //Look into interfaces again, apply 'hit'
                        iHurtable h = (iHurtable)mb;
                        h.Hit(damage, hit.point);
                    }
                }
            }
            Debug.DrawRay(transform.position, fwd * 100, new Color(1,0,0,0.1f));
        }
    }

    public void Hit(int damage, Vector3 point)
    {
        health -= damage;
        targPoint += (new Vector3(UnityEngine.Random.Range(-2,2), UnityEngine.Random.Range(-2, 2), 0));
        targPoint *= 2;

        //Spawn an impact particle at the point of impact
    }
}
