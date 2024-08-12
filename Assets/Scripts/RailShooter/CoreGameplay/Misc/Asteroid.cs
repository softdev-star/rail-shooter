using UnityEngine;
using System.Collections;

public class Asteroid : Hazard
{
    //Asteroid subclass of hazard. Spin, when destroyed break into smaller pieces
    //VARIABLES
    public GameObject[] pieces;
    private Rigidbody rb;
    public Vector3 spinDir;
    public int spinForce;
    public bool randomSpin;

	// Use this for initialization
	void Start ()
    {
        //Start spinning
        rb = GetComponent<Rigidbody>();
        //Spinning might be random on some, could be fixed, use a bool to decide which behaviour to use
        if(randomSpin)
        {
            spinDir = new Vector3(Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2)).normalized;
            spinForce = Random.Range(10, 100);
        }
        rb.AddRelativeTorque(spinDir * spinForce * Time.smoothDeltaTime);
    }

    //Asteroid specific destruction behaviour
    public override void Destruction()
    {
        //Only spawn smaller pieces if they're there to spawn
        if (pieces.Length > 0)
        {
            //Spawn a random amount of pieces
            int num = Random.Range(0, pieces.Length);
            for (int i = 0; i <= num; i++)
            {
                Instantiate(pieces[Random.Range(0, pieces.Length)], transform.position, Quaternion.identity);
            }
        }
    }
}
