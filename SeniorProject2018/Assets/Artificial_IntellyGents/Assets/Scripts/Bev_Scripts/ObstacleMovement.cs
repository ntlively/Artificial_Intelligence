using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ObstacleMovement : MonoBehaviour {

	public NavMeshAgent agent;
	public GameObject[] waypoints;
	private int currentWaypoint = 0;

	// Use this for initialization
	void Start () 
	{
		agent = GetComponent<NavMeshAgent>();
		waypoints = GameObject.FindGameObjectsWithTag("Finish");
	}
	
	// Update is called once per frame
	void Update () 
	{
		Move();
	}

	void Move()
	{
		//If player is within the range of a random way point, go to it.
		if(Vector3.Distance(this.transform.position, waypoints[currentWaypoint].transform.position )>= 1)
		{
			agent.SetDestination(waypoints[currentWaypoint].transform.position);
		}
		//If the player is close to way point, set the next way point.
		else if (Vector3.Distance(this.transform.position, waypoints[currentWaypoint].transform.position )<= 1)
		{
			currentWaypoint = UnityEngine.Random.Range(0, waypoints.Length);
		}


	}
}
