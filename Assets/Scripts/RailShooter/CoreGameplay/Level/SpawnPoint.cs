using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour
{
    //VARIABLES
    //Collection of the squads we'll spawn
    public GameObject[] spawns;
    //Their assigned routes, indexes will correlate so expect doubling up
    public RouteManager[] routes;
    //Initial delay and delay between waves
    public int initDelay;
    public int spawnTimer;

    void Start()
    {
        //Start the spawn coroutine
        StartCoroutine("Spawn");
    }

    IEnumerator Spawn()
    {
        //Wait for our initial delay 
        yield return new WaitForSeconds(initDelay);
        //spawn the squad at the start of its route,
        for (int i = 0; i < spawns.Length; i++)
        {
            GameObject temp;
            temp = (GameObject)Instantiate(spawns[i], routes[i].nodes[0], transform.rotation);
            //assign the route,
            temp.GetComponent<SquadController>().route = routes[i].nodes;
            //then wait till next wave
            yield return new WaitForSeconds(spawnTimer);
        }
        yield return null;
    }
}
