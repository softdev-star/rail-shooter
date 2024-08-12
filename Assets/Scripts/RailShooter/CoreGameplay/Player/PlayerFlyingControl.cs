using UnityEngine;
using System.Collections;

public class PlayerFlyingControl : MonoBehaviour
{
    //VARIABLES
    public int speed;
    public int turnSpeed;
    public int xBoundsRight;
    public int xBoundsLeft;
    public int yBoundsTop;
    public int yBoundsBottom;
    private Vector3 tarPos;
    public Transform cam;

    float xPos = 0;
    float yPos = 0;
    float zPos = 5;
    public float lookDist;
    public float minLookDist;

    void Awake()
    {
        //Set all the starting variables and references.
        cam = Camera.main.transform;
        //Start us in the middle of the screen, make the boundaries just in from the edges of the screen.
        xPos = Screen.width / 2;
        yPos = Screen.height / 2;
        xBoundsRight = Screen.width / 10 * 9;
        yBoundsTop = Screen.height / 8 * 7;
        xBoundsLeft = Screen.width / 10;
        yBoundsBottom = Screen.height / 8;
    }

    // Update is called once per frame
    void Update ()
    {
        //Incase we don't update movement, set it to what it is right now.
        Vector3 movePos = transform.position;

        //Fire a ray from the screen, if it hits something aim at that otherwise revert to our default depth.
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        RaycastHit hit;
        int layerMask = 1 << 8;
        //Store that point as a vector 3 to look at and to help with temporary crosshair.
        //If our raycast hits, look at that point, otherwise look at the screen point in our assigned depth
        if (Physics.Raycast(ray, out hit, 100.0f, layerMask))
        {
            if (hit.point.z > transform.position.z + minLookDist)
            {
                tarPos = hit.point;
            }
            else
            {
                tarPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z + lookDist));
            }
        }
        else
        {
            tarPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z + lookDist));
        }
        //Use a slerp to smooth rotation out for the sake of game feel.
        Quaternion targetRotation = Quaternion.LookRotation(tarPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f);

        //Movement works by keeping track of a position on the screen then positioning the player relative to that position in the world
        //This means the position will remain consistent as the camera pans around and in different resolutions.
        
        //Controls
        if (Input.GetAxis("Horizontal") != 0)
        {
            xPos += Input.GetAxis("Horizontal") * Screen.width/10;
        }
        if (Input.GetAxis("Vertical") != 0)
        {
            yPos += Input.GetAxis("Vertical") * Screen.height / 8;
        }

        //Restraints
        if (xPos < xBoundsLeft)
        {
            xPos = xBoundsLeft;
        }
        if (xPos > xBoundsRight)
        {
            xPos = xBoundsRight;
        }
        if (yPos < yBoundsBottom)
        {
            yPos = yBoundsBottom;
        }
        if (yPos > yBoundsTop)
        {
            yPos = yBoundsTop;
        }

        //Assign the calculated position, use a lerp to make it feel nice and give some control over speed.
        movePos = Camera.main.ScreenToWorldPoint(new Vector3(xPos, yPos, zPos));
        transform.position = Vector3.Lerp(transform.position, movePos, Time.smoothDeltaTime * speed);
    }

    void OnDrawGizmos()
    {
        //Temporary crosshair
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(tarPos, 0.5f);
    }
}
