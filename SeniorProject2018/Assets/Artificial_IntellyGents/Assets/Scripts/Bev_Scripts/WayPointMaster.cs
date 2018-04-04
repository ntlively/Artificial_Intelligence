using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WayPointMaster : MonoBehaviour {

	List<WayPointClass> waypoints;
	List<WayPointClass> notVisited;

	void Awake ()
	{
		//Get all way points for the player.
		waypoints = new List<WayPointClass>();
		GameObject[] tempPoints = GameObject.FindGameObjectsWithTag("Waypoint");

		//Debug.LogError("Faild to find waypoints"+ tempPoints.Length);
		for (int i = 0; i < tempPoints.Length; i++)
		{
			WayPointClass temp = tempPoints[i].GetComponent<WayPointClass>();
			waypoints.Add(temp); 
		}

	}

	public WayPointClass NewWayPoint()
	{
		return waypoints.ElementAt(0);
	}

	public WayPointClass NextWayPoint(WayPointClass previousWaypoint)
	{
		//Set waypoint visit
		int currentIndex = waypoints.IndexOf(previousWaypoint);
		waypoints[currentIndex].resetVisit();

		//Return way point
		WayPointClass nextWaypoint;
		
		//Get only visited nodes
		notVisited = waypoints.Where(x => x.visited == false).ToList();

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
		//Debug.LogError("Waypoint Position:"+ nextWaypoint.transform.position);

		return nextWaypoint;
	}
 
	//All way points
	/*List<WayPointClass> waypoints;
	//

	// Use this for initialization
	void Start () 
	{
		//Get all way points for the player.
		GameObject[] tempPoints = GameObject.FindGameObjectsWithTag("Waypoint");	
		for (int i = 0; i < tempPoints.Length; i++)
		{
			WayPointClass nextWayPoint = tempPoints[i].GetComponent<WayPointClass>();
			waypoints.Add(nextWayPoint); 
		}
			
	}

	//Give new waypoint
	public WayPointClass NextWayPoint(WayPointClass previousWaypoint)
	{
		//Set waypoint visit
		waypoints[previousWaypoint].resetVisit();

		//Return way point
		WayPointClass nextWaypoint;
		
		//Get only visited nodes
		List<WayPointClass> notVisited = waypoints.Where( waypoints.getVisit() == false).ToList();

		//Waypoints
		int waypointIndex =  UnityEngine.Random.Range(0, notVisited.Length);

		if(notVisited.Length == 0)
		{
			//Reset all waypoints 
			for(int i = 0; i < waypoints.Length; i++)
			{
				waypoints[i].resetVisit();
			}
		}

		//Get an index
		nextWaypoint = notVisited[waypointIndex];

		return nextWaypoint;
	}
	
	// Update is called once per frame
	//void Update () {
		
	//}*/
}
