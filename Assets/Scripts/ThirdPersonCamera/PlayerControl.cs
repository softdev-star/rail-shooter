using UnityEngine;
using System.Collections;

//Basic physics based third person controller
public class PlayerControl : MonoBehaviour 
{
    //VARIABLES
	Rigidbody rBody;
	public int acceleration;
	public int turnSpeed;
	public int topSpeed;
	public int jumpForce;
	public Transform camTrans;
	private Vector3 oldPos;

	// Use this for initialization
	void Start () 
	{
		rBody = GetComponent<Rigidbody> ();
		oldPos = transform.position += Vector3.forward;
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		MovementControls ();
	}

	void MovementControls()
	{
		//Get the camera's forward (cancel the Y) then the right using Cross
		//Set the player to face that way and move relative to it also
		Vector3 tF = camTrans.TransformDirection (Vector3.forward);
		Debug.DrawRay (transform.position, tF, Color.red);
		Vector3 camForward = new Vector3(tF.x, 0, tF.z).normalized;
		Vector3 camRight = Vector3.Cross(Vector3.up, camForward).normalized;

		//Debug.DrawRay (transform.position, camForward * 10);
		//Debug.DrawRay (transform.position, camRight * 10);

		//hacky way to find the direction to face. Get the direction from my previous position to the current one and face that way. 
		//(well, away from where I was).
		if(oldPos != transform.position)
		{
			Vector3 lookDir = (transform.position - oldPos).normalized;
			transform.LookAt (new Vector3(transform.position.x + lookDir.x, transform.position.y, transform.position.z + lookDir.z));
		}

		//Movement controls
		if(rBody.velocity.x < topSpeed && rBody.velocity.x > -topSpeed)
		{
			rBody.AddForce(camRight * Input.GetAxis("Horizontal") * acceleration * Time.smoothDeltaTime);
		}
		if(rBody.velocity.y < topSpeed && rBody.velocity.y > -topSpeed)
		{
			rBody.AddForce(camForward * Input.GetAxis("Vertical") * acceleration * Time.smoothDeltaTime);
		}
		if(Input.GetButtonDown("Jump") && Physics.Raycast(transform.position, Vector3.down, 1))
		{
			rBody.AddForce(Vector3.up * jumpForce);
		}

		//Record position for next frame
		oldPos = transform.position;
	}
}
