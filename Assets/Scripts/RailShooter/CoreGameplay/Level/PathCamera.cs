using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathCamera : MonoBehaviour
{
    //VARIABLES
    //Route and junctions
    [SerializeField]
    private List<Vector3> route;
    public List<PathJunctionController> junctions;
    public int speed;
    public int rotSpeed;
    public int curPoint;
    private int curJunction = 0;

	// Use this for initialization
	void Start ()
    {
        //Starting route comes from assigned routemanager
        route = GetComponent<RouteManager>().nodes;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //Look towards the target point, use a slerp to make the rotation smooth
        Quaternion targetRotation = Quaternion.LookRotation(route[curPoint] - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.smoothDeltaTime * rotSpeed);
        
        //Translate forward
        transform.Translate(Vector3.forward * speed * Time.smoothDeltaTime);

        //Step through the route as we reach each point
        if (Vector3.Distance(transform.position, route[curPoint]) < 1 && curPoint + 1 < route.Count)
        {
            //Debug.Log("Next Point");
            curPoint++;
        }
        //When we reach the end of a route, check if we have a next junction, 
        else if (curJunction < junctions.Count && curPoint + 1 >= route.Count)
        {
            //if we do, ask the junction what our new route is and restart our route counter
            //Debug.Log("Next Junction");
            route = junctions[curJunction].ReturnRoute();
            curPoint = 0;
            curJunction++;
            //Debug.Log("Returned");
        }
        //if we're out of junctions, thats the end of the level
        else if (curJunction > junctions.Count) 
        {
            //For now we'll just pause the editor
            //Debug.Log("End Level");
            UnityEditor.EditorApplication.isPaused = true;
        }
    }
}
