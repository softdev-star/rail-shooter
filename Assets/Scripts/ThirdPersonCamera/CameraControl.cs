using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour 
{
    //VARIABLES
	//Gizmo system
	public bool drawXZone;
	public bool drawYZone;
	public bool drawZZone;

    //Have I locked the camera
	public bool cameraLocked;

    //Player is controlling this axis, don't change it
	public bool playerCont_Pitch = false;
	public bool playerCont_Yaw = false;

    //mouse control variables
    public float sensitivity;
    public float deadZone;

    //Base angles
    public float basePitch;

    //For clamping vertical angle
    public int yMinLimit = 10; 
    public int yMaxLimit = 10; 

    //For manipulating the axis
    private float yaw;
    private float pitch;

    //timers for player or default control of axis
    public float playerPitchTimer = 0;
	public float playerYawTimer = 0;
	public float playerContLockDuration;

    //What the camera will follow (the player)
	public Transform target;
    //Potentially add a secondary target (the player's target) for facing in locked mode?

    //pitch inverted?
    public int isInverted;

	//position variables for locked camera
	//X
	private float disToPlayerX;
	public float disBaseX;
	private float disTargX;
	public float disVarianceX;
	//Y
	private float disToPlayerY;
	public float disBaseY;
	private float disTargY;
	public float disVarianceY;
	//Z
	private float disToPlayerZ;
	public float disBaseZ;
	private float disTargZ;
	public float disVarianceZ;

	//Catchup value
	public float pace;
	public float whiskerPanStrength;

	//control bools
	public bool panX = true;
	public bool panY = true;
	public bool panZ = true;

	//Debuging
	public Transform dummy;
	public bool showWhiskers;

	//For calculating variable distance to player 
	float disBack = 0;

    //Check if the mouse has moved much
    private float mouseMovedY;
    private float mouseMovedX;

    // Use this for initialization
    void Start () 
	{
        //Set starting position
        pitch = basePitch;

        //Position relative to target
        disTargX = disBaseX;
		disTargY = disBaseY;
		disTargZ = disBaseZ;
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
        //Toggle Locked Camera on keypress
        if (Input.GetButtonDown("LockCamera"))
        {
            cameraLocked = !cameraLocked;
        }

		//Position the camera 
		if(cameraLocked)
		{
			//Camera locked to follow
			CameraPositioning_Locked ();
        }
		else
		{
			//Orbit positioning
			CameraPositioning_Orbit();
        }
    }

	//LOCKED CONTROLS SECTION
	void CameraPositioning_Locked()
	{
        //This currently locks straight down the world forward vector, eventually I would like to adjust this
        //to work based on the screen instead of world space and then work along the vector the player is facing
        //or towards a designated target instead

		//Set my target position to the base values
		disTargX = disBaseX;
		disTargY = disBaseY;
		disTargZ = disBaseZ;

		//CAMERA POSITIONING
		//Find a target X and Z position based on the distance to the target and it's distance from the expected position (rubber banding).
		//Only run if the player is outside of variance range. Use an outer limit to tighten the band further so the player can't escape it.
		float x = CamContYaw();
		CalcDistBack();
		float z = disBack;
		float y = CamContPitch ();
		
		//New position vector
		Vector3 placement = new Vector3 (x, y, z);
		
		//Direction to the target on the Z axis from proposed position.
		//Vector3 dirToZ = (placement - target.position).normalized;
		//Distance to the target on the Z axis from proposed position.
		//float distToZ = (target.position - placement).z;
        //Put the camera at the calculated coordinates
        transform.position = placement;
        transform.LookAt(target);
    }

	//ORBIT CONTROLS SECTION
	void CameraPositioning_Orbit()
	{
        //Known bug, still having trouble with the axis influencing each other and locking up
		//Check for mouse input first, 
		if(Input.GetMouseButton(1))
		{
			MouseControlChecks ();

            if (playerCont_Pitch)
            {
                pitch += Input.GetAxis("Mouse Y") * sensitivity * isInverted;
                //clamp pitch between its limits
                pitch = pitch < yMinLimit ? yMinLimit : pitch > yMaxLimit ? yMaxLimit : pitch;
            }
            if(playerCont_Yaw)
            {
                yaw += Input.GetAxis("Mouse X") * sensitivity;
                //Keep the yaw between 360 and -360
                yaw = yaw < -180 ? yaw + 180 : yaw > 180 ? yaw - 180 : yaw;
            }

            Debug.Log("p: " + pitch + " " + "y: " + yaw);
        }

        //Delegate control of pitch or yaw to player or camera based on if the player moved the mouse recently (handled above)
        if (playerCont_Pitch)
		{
            playerPitchTimer -= Time.smoothDeltaTime;
            if (playerPitchTimer <= 0)
            {
                playerCont_Pitch = false;
            }
        }
		else
		{
            //CamContPitch();
            pitch = Mathf.Lerp(pitch, basePitch, Time.smoothDeltaTime);
        }
		
		if(playerCont_Yaw)
		{
            playerYawTimer -= Time.smoothDeltaTime;
            if (playerYawTimer <= 0)
            {
                playerCont_Yaw = false;
            }
        }
		else
		{
            //CamContYaw();
        }

        //Figure out the distance back. Use 
		PivotDistBack ();

        //Positioning. Place at player position, rotate, then move back as far as necessary.
        transform.position = target.position;
        //values seem to be doubling for some reason, perhaps due to being rotated, moved back, then told to lookat?
        //BROKEN, spirals down, will likely be scrapped and replaced with a different approch.
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);
		transform.Translate (transform.TransformDirection(Vector3.back) * disTargZ);
        transform.LookAt(target);
    }
	
	void MouseControlChecks()
	{
		//MOUSE CONTROL CHECKS
		//check if the player is moving the mouse and control a lock on that axis
		// Y/Pitch
		mouseMovedY = Input.mousePosition.y - Screen.height/2;
		if(Mathf.Abs(mouseMovedY) > deadZone)
		{
			//Debug.Log (mouseMovedY.ToString());
			playerCont_Pitch = true;
			playerPitchTimer = playerContLockDuration;
		}
		// X/Yaw
		mouseMovedX = Input.mousePosition.x - Screen.width/2;
		if(Mathf.Abs(mouseMovedX) > deadZone)
		{
			//Debug.Log (mouseMovedX.ToString());
			playerCont_Yaw = true;
			playerYawTimer = playerContLockDuration;
		}
	}

    //For handling the Z distance for a locked camera
	void CalcDistBack()
	{
		RaycastHit hit;
		//Debug.DrawRay(target.position, transform.TransformDirection(Vector3.back)*disTargZ);
		if(Physics.Raycast(target.position, transform.TransformDirection(Vector3.back), out hit, disTargZ))
		{
			disBack = hit.point.z;
			//Debug.Log("Hit");
		}
		else
		{
			//Calculate distance back
			disToPlayerZ = target.position.z - transform.position.z;
			//z handling
			int multiplierZ = 1; //this is for scaling the pace.
			float z = transform.position.z;
			if(disToPlayerZ > (disVarianceZ+disTargZ))  //gone too far forward, set panning to active.
			{
				panZ = true;
			}
			else if(disToPlayerZ < -(disVarianceZ-disTargZ)) //gone too far back, set panning to active.
			{
				//Debug.Log("back inner limit");
				//Debug.Log((-(disVarianceZ-disTargZ)).ToString());
				//Debug.Log(disToPlayerZ.ToString());
				panZ = true;
			}
			if(panZ)
			{
				if(disToPlayerZ > (disVarianceZ+disTargZ)*2)  
				{
					multiplierZ = 2;
					//Debug.Log("Too far forward");
				}
				else if(disToPlayerZ < ((disVarianceZ-disTargZ)*2))  
				{
					multiplierZ = 2;
					//Debug.Log("back outer limit");
					//Debug.Log((-(disVarianceZ-disTargZ)*2).ToString());
					//Debug.Log(disToPlayerZ.ToString());
				}
				else
				{
					multiplierZ = 1;
				}
				
				//pan on the Z by the pace and a factor of distance
				z = transform.position.z + Time.smoothDeltaTime * pace * (disToPlayerZ - disTargZ) * multiplierZ; 
				//Debug.Log(z);
				//Debug.Log(disToPlayerZ);
				//Debug.Log(disTargZ);
				if(Mathf.Abs(disToPlayerZ-disTargZ) < 0.2f)// || Mathf.Abs(disToPlayerz+disTargz < 0.2f )
				{
					//Debug.Log("Close Enough");
					panZ = false;
				}
			}
			//Debug.Log(z);
			disBack = z; //temporary, will have raycasts and decision making involved.
		}
	}

    //Handles the z distance for the orbiting camera
	void PivotDistBack ()
	{
        //Check if the player is controlling yaw or the computer, don't need to bother with whiskers if the player is in control
        if (!playerCont_Yaw)
        {
            //Use whiskers. If the player is in control, pan in, otherwise go tell the yaw to rotate away from the triggered whisker.
            //int system to keep track of impacts. Number result dictates the action.
            Vector3 relativeRight = new Vector3(transform.TransformDirection(Vector3.right).x, 0, transform.TransformDirection(Vector3.right).z).normalized;
            Vector3 relativeBack = new Vector3(transform.TransformDirection(Vector3.back).x, 0, transform.TransformDirection(Vector3.back).z).normalized;
            //add back to right and then give it an appropriate y value, subtract for the other side
            Vector3 farLeftWhiskerDir = ((relativeBack + relativeRight) + (new Vector3(0, 0.3f, 0))).normalized;    //1
            Vector3 innerLeftWhiskerDir = ((relativeBack * 0.75f + relativeRight * 0.25f) + (new Vector3(0, 0.2f, 0))).normalized;  //2
                                                                                                                                    //Don't need straight back.
            Vector3 innerRightWhiskerDir = ((relativeBack * 0.75f - relativeRight * 0.25f) + (new Vector3(0, 0.2f, 0))).normalized; //4
            Vector3 farRightWhiskerDir = ((relativeBack - relativeRight) + (new Vector3(0, 0.3f, 0))).normalized;   //8
            int decisionFactor = 0;

            //Setup raycasts and add to the decision factor, may add hit variables later if I need to prioritise objects or anything.
            decisionFactor += Physics.Raycast(target.position, farLeftWhiskerDir, disBaseZ) ? 1 : 0;
            decisionFactor += Physics.Raycast(target.position, innerLeftWhiskerDir, disBaseZ) ? 2 : 0;
            decisionFactor += Physics.Raycast(target.position, innerRightWhiskerDir, disBaseZ) ? 4 : 0;
            decisionFactor += Physics.Raycast(target.position, farRightWhiskerDir, disBaseZ) ? 8 : 0;
            //Debug.Log (decisionFactor);

            WhiskerReaction(decisionFactor);

            //Debugs for the whiskers, hidden behind a bool I can add to the inspector.
            if (showWhiskers)
            {
                Debug.DrawRay(target.position, farLeftWhiskerDir * disBaseZ, Color.red);
                Debug.DrawRay(target.position, innerLeftWhiskerDir * disBaseZ, Color.green);
                Debug.DrawRay(target.position, innerRightWhiskerDir * disBaseZ, Color.green);
                Debug.DrawRay(target.position, farRightWhiskerDir * disBaseZ, Color.red);
            }
        }
        else //The player is controlling yaw so we don't want to override, we also don't want to obscure vision
        {
            //If something is inbetween the player and the camera, pan in as well
            RaycastHit hit;
            if (Physics.Raycast(target.transform.position, (transform.position - target.transform.position).normalized, out hit, disTargZ))
            {
                disTargZ = Mathf.Lerp(disTargZ, hit.distance, Time.smoothDeltaTime * pace);
            }
            else
            {
                disTargZ = Mathf.Lerp(disTargZ, disBaseZ, Time.smoothDeltaTime * pace);
            }
        }
    }

    //Calculates what the camera should do based on the 'whiskers'
	void WhiskerReaction(int result)
	{
		//Results:
		//0 = nothing hits, still check directly behind to pan in.
		//1 = far left only, yaw right slightly
		//2 = inner left only, yaw right 
		//3 = both left, yaw right 
		//4 = inner right only, yaw left 
		//5 = inner right and far left, yaw left slightly
		//6 = both inner, yaw right 
		//7 = both left and right inner, yaw right 
		//8 = far right only, yaw left slightly
		//9 = both far, do nothing.
		//10 = inner left and far right, yaw right slightly.
		//11 = both left, far right, yaw right slightly.
		//12 = both right, yaw left.
		//13 = both right, far left, yaw left slightly.
		//14 = not far left. yaw left.
		//15 = all of them. Fire a ray back, if it hits, zoom in instead.
		RaycastHit hit;
		switch(result)
		{
			case 0:
            if (Physics.Raycast(target.transform.position, (transform.position - target.transform.position).normalized, out hit, disTargZ))
            {
                disTargZ = Mathf.Lerp(disTargZ, hit.distance, Time.smoothDeltaTime * pace);
            }
            else
            {
                disTargZ = Mathf.Lerp(disTargZ, disBaseZ, Time.smoothDeltaTime * pace);
            }
            break;

			case 1:
			yaw += Time.smoothDeltaTime * sensitivity * whiskerPanStrength;
			break;

			case 2:
			yaw += Time.smoothDeltaTime * sensitivity * whiskerPanStrength * 2;
			break;

			case 3:
			yaw += Time.smoothDeltaTime * sensitivity * whiskerPanStrength * 2;
			break;

			case 4:
			yaw -= Time.smoothDeltaTime * sensitivity * whiskerPanStrength * 2;
			break;

			case 5:
			yaw -= Time.smoothDeltaTime * sensitivity * whiskerPanStrength;
			break;

			case 6:
			yaw += Time.smoothDeltaTime * sensitivity * whiskerPanStrength * 2;
			break;

			case 7:
			yaw += Time.smoothDeltaTime * sensitivity * whiskerPanStrength * 2;
			break;

			case 8:
			yaw -= Time.smoothDeltaTime * sensitivity * whiskerPanStrength;
			break;

			case 9:
			if(Physics.Raycast(target.transform.position, target.transform.TransformDirection(Vector3.back), out hit,disTargZ))
			{
				disTargZ = Mathf.Lerp(disTargZ, hit.point.z, Time.smoothDeltaTime * pace);
			}
			break;

			case 10:
			yaw += Time.smoothDeltaTime * sensitivity * whiskerPanStrength;
			break;

			case 11:
			yaw += Time.smoothDeltaTime * sensitivity * whiskerPanStrength;
			break;

			case 12:
			yaw -= Time.smoothDeltaTime * sensitivity * whiskerPanStrength * 2;
			break;

			case 13:
			yaw -= Time.smoothDeltaTime * sensitivity * whiskerPanStrength;
			break;

			case 14:
			yaw -= Time.smoothDeltaTime * sensitivity * whiskerPanStrength * 2;
			break;

			case 15:
            if (Physics.Raycast(target.transform.position, (transform.position - target.transform.position).normalized, out hit, disTargZ))
            {
                disTargZ = Mathf.Lerp(disTargZ, hit.distance, Time.smoothDeltaTime * pace);
            }
            else
            {
                disTargZ = Mathf.Lerp(disTargZ, disBaseZ, Time.smoothDeltaTime * pace);
            }
            break;

			default:
			break;
		}
	}

	float CamContYaw()
	{
		//X handling
		disToPlayerX = target.position.x - transform.position.x;
		int multiplierX = 1; //this is for scaling the pace.
		float x = transform.position.x;
		if(disToPlayerX > (disVarianceX+disTargX))   //gone too far right, set panning to active.
		{
			panX = true;
		}
		else if(disToPlayerX < -(disVarianceX-disTargX)) //gone too far left, set panning to active.
		{
			panX = true;
		}
		if(panX)
		{
			if(disToPlayerX > (disVarianceX+disTargX)*2)  
			{
				multiplierX = 2;
				//Debug.Log("Too far right");
			}
			else if(disToPlayerX < -(disVarianceX-disTargX)*2)  
			{
				multiplierX = 2;
				//Debug.Log("Too far left");
			}
			else
			{
				multiplierX = 1;
			}
			
			x = transform.position.x + Time.smoothDeltaTime * pace * (disToPlayerX - disTargX) * multiplierX; //pan on the X by the pace and a factor of distance
			if(Mathf.Abs(disToPlayerX-disTargX) < 0.2f)// || Mathf.Abs(disToPlayerX+disTargX < 0.2f )
			{
				//Debug.Log("Close Enough");
				panX = false;
			}
		}
		return x;
	}

	float CamContPitch()
	{
		//Y handling
		disToPlayerY = target.position.y - transform.position.y;
		int multiplierY = 1; //this is for scaling the pace.
		float y = transform.position.y;
		if(disToPlayerY > (disVarianceY+disTargY))  //gone too far forward, set panning to active.
		{
			panY = true;
			//Debug.Log("Too far forward");
		}
		else if(disToPlayerY < -(disVarianceY-disTargY)) //gone too far back, set panning to active.
		{
			panY = true;
			//Debug.Log("Too far back");
		}
		if(panY)
		{
			if(disToPlayerY > (disVarianceY+disTargY)*2)  
			{
				multiplierY = 2;
			}
			else if(disToPlayerY < -(disVarianceY-disTargY)*2)  
			{
				multiplierY = 2;
			}
			else
			{
				multiplierY = 1;
			}
			
			y = transform.position.y + Time.fixedDeltaTime * pace * (disToPlayerY + disTargY) * multiplierY; //pan on the Y by the pace and a factor of distance
			if(Mathf.Abs(disToPlayerY-disTargY) < 0.2f)// || Mathf.Abs(disToPlayerY+disTargY < 0.2f )
			{
				//Debug.Log("Close Enough");
				panY = false;
			}
		}

		return y;
	}

	//Gizmos, for editor readouts to check things.
	void OnDrawGizmos()
	{
		if(cameraLocked)
		{
			float xScale = 1;
			float yScale = 1;
			float zScale = 1;
			
			if(drawXZone)
			{
				xScale = disVarianceX*2;
			}
			if(drawYZone)
			{
				yScale = disVarianceY*2;
			}
			if(drawZZone)
			{
				zScale = disVarianceZ*2;
			}
			//Vector3 fwd = transform.TransformDirection (Vector3.forward);
			Vector3 targetPos = new Vector3 (disTargX + transform.position.x, -disTargY + transform.position.y, disTargZ + transform.position.z);
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(targetPos, new Vector3(xScale,yScale,zScale));
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(targetPos, new Vector3(xScale*2,yScale*2,zScale*2));
		}
	}
}
