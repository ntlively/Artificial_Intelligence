using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WayPointClass : MonoBehaviour {

	[SerializeField]
	protected float debugDrawRadius = 0.5F;
	
	//Visited 
	public bool visited;
	public bool preyEvidence;
	public float evidenceTimer;

	//List<WayPointClass> waypoints;

	void Start ()
	{
		//Set the waypoint to unvisited
		visited = false;

		//Set timer 
		preyEvidence = false;
		evidenceTimer = 20.0f;

		//Get all way points for the player.
		/*waypoints = new List<WayPointClass>();
		GameObject[] tempPoints = GameObject.FindGameObjectsWithTag("Waypoint");

		//Debug.LogError("Faild to find waypoints"+ tempPoints.Length);
		for (int i = 0; i < tempPoints.Length; i++)
		{
			WayPointClass temp = tempPoints[i].GetComponent<WayPointClass>();
			waypoints.Add(temp); 
		}*/

	}

	//Updates everytime
	void Update ()
	{
		if(preyEvidence)
		{
			evidenceTimer -= Time.deltaTime;
			if (evidenceTimer <= 0 )
			{
				this.resetPreyEvidence(false);
				this.resetPreyTime();
			}
		}
	}

	//Displays waypoints
	public virtual void OnDrawGizmos () 
	{
		if(this.tag == "Waypoint")
		{
			//Visited by the predator
			if(visited == true)
			{
				Gizmos.color = Color.red;
			}
			else 
			{
				Gizmos.color = Color.yellow;
			}
		}
		else if (this.tag == "Finish")
		{
			Gizmos.color = Color.blue;
		}

		Gizmos.DrawWireSphere(transform.position,debugDrawRadius);

		if(preyEvidence)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position,1.0f);
		}
	}

	/*public WayPointClass NewWayPoint()
	{
		return waypoints.ElementAt(0);
	}* /


	//Give new waypoint
	public WayPointClass NextWayPoint(WayPointClass previousWaypoint)
	{
		//Set waypoint visit
		int currentIndex = waypoints.IndexOf(previousWaypoint);
		waypoints[currentIndex].resetVisit();

		//Return way point
		WayPointClass nextWaypoint;
		
		//Get only visited nodes
		List<WayPointClass> notVisited = waypoints.Where(x => x.visited == false).ToList();

		//Waypoints
		int waypointIndex =  UnityEngine.Random.Range(0, notVisited.Count);

		if(notVisited.Count == 0)
		{
			//Reset all waypoints 
			for(int i = 0; i < waypoints.Count; i++)
			{
				waypoints[i].resetVisit();
			}

			//Reset for new visits
			notVisited = waypoints.Where(x => x.visited == false).ToList();
			waypointIndex =  UnityEngine.Random.Range(0, notVisited.Count);
		}

		//Get an index
		nextWaypoint = notVisited[waypointIndex];

		return nextWaypoint;
	}*/


// -----------------------------------------------------------------------------------------
	//Gets and sets
//------------------------------------------------------------------------------------------
	public bool getVisit(){ return visited; }

	public bool getPreyEvidence(){ return preyEvidence; }

	public void resetVisit()
	{
		if(visited)
		{	
			visited = false;
		}
		else
		{
			visited = true;
		}
	}

	public void resetPreyEvidence(bool cur){ preyEvidence = cur;}

	public void resetPreyTime()
	{
		//Reset the timer 
		evidenceTimer = 20.0f;
	}




}
