using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

//This class controls the actions the predator will take while in a state.
	public class PatrolGuide : MonoBehaviour {

	//Variables 
		//Patroling 
		//Area influences, 
			//Holds percentage amounts for areas the predator knows about
			public List<WeightPoint> weightedList = new List<WeightPoint>();
			public Vector3 nextWaypoint = new Vector3 (0.0f, 0.0f, 0.0f); 
			public Vector3 prevWaypoint = new Vector3 (0.0f, 0.0f, 0.0f); 
			public List<WeightPoint> reachablePoints = new List<WeightPoint>();
			public bool show = false;

	//Awake
	void Awake()
	{
		//Map of the ground floor.
		Vector3 fillPoint = new Vector3 (-19.3f, 0.0f, 19.0f);
		int row = 0;

		//Start from the left portion of the map and iterate through
		//creating weighted points that are on the navmesh.
		for(int i = 0; i < 1600; i++)
		{

			fillPoint [0] = fillPoint[0] + 1.0f;
			row++;

			NavMeshHit hit;
			//check if point is on navmesh
			if(NavMesh.SamplePosition(fillPoint, out hit , 0.2f, NavMesh.AllAreas))
			{
				//create object
				WeightPoint temp = new WeightPoint(0.5f, fillPoint);
				//add to list
				weightedList.Add(temp);
			}
			//Add in percentage 
			if(row == 40)
			{
			fillPoint[0] = -21.0f;
			fillPoint[2] = fillPoint[2]-1.0f;
			row = 0;

			}

		}

		//Map on the second level of the map.
		Vector3 fillPoint2 = new Vector3 (-17.5f, 4.5f, -2.5f);
		row = 0;
		for(int a = 0; a < 255; a++)
		{
			fillPoint2 [0] = fillPoint2[0] + 1.0f;
			row++;

			NavMeshHit hit;
			//Check if point is on navmesh
			if(NavMesh.SamplePosition(fillPoint2, out hit , 0.09f, NavMesh.AllAreas))
			{
				//create object
				WeightPoint temp = new WeightPoint(0.5f, fillPoint2);
				//add to list
				weightedList.Add(temp);
			}

			//Add in percentage 
			if(row == 15)
			{
				fillPoint2[0] = -17.5f;
				fillPoint2[2] = fillPoint2[2]-1.0f;
				row = 0;
			}
		}
		
		//Set the beginning waypoint for the agent.
		nextPatrolPosition ();
		
	}

	public virtual void OnDrawGizmos () 
	{

		if(show)
		{
			Vector3 fillPoints = new Vector3 (-21.0f, 1.0f, 20.0f);
			Vector3 reachPoint = new Vector3 (0.0f, 0.0f, 0.0f);
			int row = 0;	
		
			//Display entire influence map using cubes
			/*for(int k = 0; k < weightedList.Count; k++)
			{
				//If the point is on the second level display as blue
				/*if (weightedList[k].position[1] == 4.5f)
				{
					Gizmos.color = Color.blue;
					//Adjust cubes so that the top right corner of the cube is the center of the cube.	
					fillPoints[0] = weightedList[k].position[0];
					fillPoints[1] = weightedList[k].position[1];
					fillPoints[2] = weightedList[k].position[2];
				} 
				else
				{
					//Lower level of influence map.
					Gizmos.color = Color.green;
				}

				
				Gizmos.DrawCube(fillPoints, new Vector3(1.0f, 1.0f, 1.0f));
			}*/


			//Displays the current reachable waypoints from pervious waypoint.
			for(int i = 0; i < reachablePoints.Count; i++)
			{
				Gizmos.color = Color.white;
				reachPoint[0] = reachablePoints[i].position[0];
				reachPoint[1] = reachablePoints[i].position[1];
				reachPoint[2] = reachablePoints[i].position[2];
				Gizmos.DrawCube(reachPoint, new Vector3(1.0f, 1.0f, 1.0f));
			}
		}
			
		//Displays the selected waypoint the AI is headed to.
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(nextWaypoint, 0.5f);
		
	}
	
	//Patrol point
	public Vector3 nextPatrolPosition () 
	{
		//Filter reachable points
		reachablePoints = weightedList.Where( x => (Vector3.Distance(nextWaypoint, x.position) > 2 
									&& Vector3.Distance(nextWaypoint, x.position) < 5 )).ToList();

		//Vector3 position = new Vector3 (0.0f, 1.0f, 0.0f);
		//Choose a random number
		int index = Random.Range(0,reachablePoints.Count-1);

		//Next waypoint is adjusted to be the center of the cube
		nextWaypoint[0] = reachablePoints[index].position[0];
		nextWaypoint[1] = reachablePoints[index].position[1]+0.5f;
		nextWaypoint[2] = reachablePoints[index].position[2];	

		return nextWaypoint;
	}

	//Points the ai in a certain direction
	
	//Move the AI - Patrol

	//Pick a number between the high and low weights.
	//From the point, with distance 9.5, randomly select a new point
	//At some point update the weight at this point. 
	public List<WeightPoint> getInfluence()
	{
		return weightedList;
	}
}
