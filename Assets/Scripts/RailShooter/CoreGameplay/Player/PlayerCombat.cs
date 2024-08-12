using UnityEngine;
using System.Collections;

//Combat and damage handling for the player
public class PlayerCombat : MonoBehaviour, iHurtable
{
    //VARIABLES
    public float fireRate = 0;
    private float fireTimer;
    public int damage;

    //Managing health and actions
    public int health;
    [HideInInspector]
    public bool isAlive;
    Rigidbody R;

    // Use this for initialization
    void Start()
    {
        isAlive = true;
        R = GetComponent<Rigidbody>();
        R.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isAlive)
        {
            //shooting mechanics, primary
            if (fireTimer > 0)
            {
                fireTimer -= Time.smoothDeltaTime;
            }
            else if (Input.GetMouseButton(0))
            {
                FireLaser();
            }

            if(health <= 0)
            {
                isAlive = false;
                GetComponent<PlayerFlyingControl>().enabled = false;
                GetComponentInParent<PathCamera>().speed = 0;
                StartCoroutine("Destruction");
            }
        }
    }

    //Fire a laser
    void FireLaser()
    {
        fireTimer = fireRate;
        //Shoot laser
        RaycastHit hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
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
        Debug.DrawRay(transform.position, fwd * 100, Color.red);
    }

    public void Hit(int damage, Vector3 point)
    {
        health -= damage;
        //Spawn an impact particle at the point of impact
    }

    IEnumerator Destruction()
    {
        //Spin out of control
        //Had trouble doing this the same way as the AI so I had to modify it
        R.freezeRotation = false;
        R.AddRelativeForce(Vector3.forward * 200);
        R.AddRelativeTorque(Vector3.forward * 200);
        yield return new WaitForSeconds(2.0f);
        Destroy(transform.GetChild(0).gameObject); //If I destroy the player everything freaks, just destroy the ship
        //end game
        yield return null;
    }
}
