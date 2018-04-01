﻿using System.Collections;
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
			//List of percentage with top left corner of the score. 
			public List<WeightPoint> weightedList = new List<WeightPoint>();
			public Vector3 nextWaypoint; 
			private float nextWeight = Mathf.NegativeInfinity;
			public Vector3 prevWaypoint; 
			private float prevWeight = -1.0f;
			public List<WeightPoint> reachablePoints = new List<WeightPoint>();
			public List<WeightPoint> visitedPoints = new List<WeightPoint>();

			private LayerMask obstacleMask;

			private float shortestDist = Mathf.Infinity;
			private float longestDist = Mathf.NegativeInfinity;

			[Header("Ranges")]
			[Range(0.5f,40.0f)]
			public float visitedRadius = 1.0f;
			[Range(0.5f,40.0f)]
			public float searchRadius = 5.0f;

			[Header("Prey Hide Weights")]
			public float hideWallDistance = 1.0f;
			public float hideWallNumber = 1.0f;
			public float hidePreySpotted = -1.0f;
			public float hidePreyCaught = -1.0f;

			[Header("Prey Flee Weights")]
			public float fleeWallDistance = 1.0f;
			public float fleeWallNumber = 1.0f;
			public float fleeGainedDistance = 1.0f;
			public float fleeLOS = 1.0f;
			public float fleeCaught = -1.0f;
			
			[Header("Predator Hunt Weights")]
			public float huntWallDistance = 1.0f;
			public float huntWallNumber = 1.0f;
			public float huntVisitTime = -1.0f;
			public float huntPreySpotted = 1.0f;
			public float huntPreyCaught = 1.0f;

			[Header("Grid Display")]
			[Range(0.0f,1.0f)]
			public float gridOpacity = 0.5f;
			public bool displayWallDist = true;
			public bool displayNumWalls = true;
			public bool displayVisited = true;
			//public MapData waypointGraph;
			//public LayerMask mapLayer = 11;


	//Awake
	void Awake()
	{
		obstacleMask = 1 << 10;
		
		//Map of the ground floor
		Vector3 fillPoints = new Vector3 (-19.3f, 1.0f, 19.0f);
		int row = 0;
		
		//Start from the left portion of the map and iterate through
		//creating weighted points that are on the navmesh.
		for(int i = 0; i < 1600; i++)
		{

			fillPoints [0] = fillPoints[0] + 1.0f;
			row++;

			NavMeshHit hit;
			//check if point is on navmesh
			if(NavMesh.SamplePosition(fillPoints, out hit, 1.42f, NavMesh.AllAreas))
			{
				//Generate weight for position
				float wallDistance = wallWeight(hit.position);
				shortestDist = Mathf.Min(shortestDist,wallDistance);
				longestDist = Mathf.Max(longestDist,wallDistance);
				int numberWalls = wallCount(hit.position);

				//create object
				WeightPoint temp = new WeightPoint(wallDistance, numberWalls, fillPoints, hit.position);
				temp.navPosition[1] += 0.5f;
				//add to list
				weightedList.Add(temp);
			}
			

			//Add in percentage 
			if(row == 40)
			{
			  fillPoints[0] = -21.0f;
			  fillPoints[2] = fillPoints[2]-1.0f;
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
			if(NavMesh.SamplePosition(fillPoint2, out hit , 1.0f, NavMesh.AllAreas))
			{
				//Generate weight for position
				float wallDistance = wallWeight(fillPoints);
				shortestDist = Mathf.Min(shortestDist,wallDistance);
				longestDist = Mathf.Max(longestDist,wallDistance);
				int numberWalls = wallCount(fillPoints);

				//create object
				WeightPoint temp = new WeightPoint(wallDistance, numberWalls, fillPoints, hit.position);
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
		//nextRandomPosition ();


	}
	// Use this for initialization
	void Start ()
	{

		
	}

	public virtual void OnDrawGizmos () 
	{

		Vector3 fillPoints = new Vector3 (-21.0f, 1.0f, 20.0f);
		Vector3 reachPoint = new Vector3 (0.0f, 0.0f, 0.0f);
		int row = 0;	
	
		//Display entire influence map using cubes
		for(int k = 0; k < weightedList.Count; k++)
		{
			//If the point is on the second level display as blue
			// if (weightedList[k].position[1] == 4.5f)
			// {
			// 	Gizmos.color = Color.blue;
			// 	//Adjust cubes so that the top right corner of the cube is the center of the cube.	
			// }
			// else
			// {
			// 	//Lower level of influence map.
			// 	Gizmos.color = Color.green;
			// }

			//Red value is visit time ratio
			float redValue;
			if(displayVisited)
				redValue = 1.0f - (Time.time - weightedList[k].visitTime)/Time.time;
			else
				redValue = 0.0f;

			//Green value is # of walls
			float greenValue;
			if(displayNumWalls)
				greenValue = weightedList[k].numWalls / 4.0f;
			else
				greenValue = 0.0f;

			//Blue value is wall dist ratio
			float blueValue;
			if(displayWallDist)
				blueValue  = 1.0f - (weightedList[k].wallDist - shortestDist)/(longestDist - shortestDist);
			else
				blueValue = 0.0f;

			//Full color
			Gizmos.color = new Color(redValue,greenValue,blueValue,gridOpacity);

			fillPoints = weightedList[k].position;

			Gizmos.DrawCube(fillPoints, new Vector3(1.0f, 1.0f, 1.0f));
			//Gizmos.DrawSphere(fillPoints, 0.5f);
		}


		//Displays the current reachable waypoints from pervious waypoint.
		// for(int i = 0; i < reachablePoints.Count; i++)
		// {
		// 	float value = (Time.time - reachablePoints[i].visitTime)/Time.time;
		// 	Gizmos.color = new Color(1.0f,value,value,0.25f);
		// 	reachPoint[0] = reachablePoints[i].position[0];
		// 	reachPoint[1] = reachablePoints[i].position[1];
		// 	reachPoint[2] = reachablePoints[i].position[2];
		// 	Gizmos.DrawCube(reachPoint, new Vector3(1.0f, 1.0f, 1.0f));
		// }
		
		//Displays the selected waypoint the AI is headed to.
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(nextWaypoint, 0.5f);
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public float wallWeight(Vector3 testPoint)
	{
		float dist = Mathf.Infinity;
		for(int i = 0; i < 360; i ++)
		{
			Vector3 angle = new Vector3(Mathf.Sin(i * Mathf.Deg2Rad),0,Mathf.Cos(i * Mathf.Deg2Rad));

			RaycastHit hit;
			Physics.Raycast(testPoint,angle,out hit,obstacleMask);

			float dstToWall = Vector3.Distance(testPoint,hit.point);
			dist = Mathf.Min(dist, dstToWall);
		}

		return dist;
	}

	public int wallCount(Vector3 testPoint)
	{
		int hitCount = 0;

		for(int i = 0; i < 360; i += 10)
		{
			Vector3 angle = new Vector3(Mathf.Sin(i * Mathf.Deg2Rad),0,Mathf.Cos(i * Mathf.Deg2Rad));

			if(Physics.Raycast(testPoint, angle, 1.0f, obstacleMask))
			{
				hitCount++;
			}
		}

		return hitCount/9;
	}

	//Patrol point
	public void nextRandomPosition () 
	{
		//Filter reachable points
		reachablePoints = weightedList.Where( x => (Vector3.Distance(nextWaypoint, x.position) < searchRadius)).ToList();

     //(Vector3.Distance(nextWaypoint, point.position) > 10 && Vector3.Distance(nextWaypoint, point.position) < 13)


		//Choose a random number
		int index = Random.Range(0,reachablePoints.Count-1);

		prevWaypoint = nextWaypoint;
		
		//Next waypoint is adjusted to be the center of the cube
		nextWaypoint[0] = reachablePoints[index].navPosition[0];
		nextWaypoint[1] = reachablePoints[index].navPosition[1];
		nextWaypoint[2] = reachablePoints[index].navPosition[2];	
	}

	//Find next hide position based on wall proximity, number of walls, prey spotted and prey caught
	public void nextHidePosition () 
	{
		//Filter reachable points
		reachablePoints = weightedList.Where( x => (Vector3.Distance(nextWaypoint, x.position) < searchRadius)).ToList();

		float bestWeight = nextWeight;
		float testWeight = 0;
		int index = -1;
		for(int i = 0; i < reachablePoints.Count; i++)
		{
			Vector3 dirToTarget = (reachablePoints[i].navPosition - nextWaypoint).normalized;
			float dstToTarget = Vector3.Distance(nextWaypoint, reachablePoints[i].navPosition);

			if(!Physics.Raycast(nextWaypoint,dirToTarget,dstToTarget,obstacleMask))
			{
				float wallDistRatio = (1.0f - ((reachablePoints[i].wallDist - shortestDist)/(longestDist - shortestDist)));

				testWeight = hideWallDistance * wallDistRatio
					+ hideWallNumber * reachablePoints[i].numWalls + hidePreySpotted * reachablePoints[i].preySpotted + hidePreyCaught * reachablePoints[i].preyCaught;
				

				if(testWeight > bestWeight)
				{
					bestWeight = testWeight;
					index = i;
				}
			}
		}
		prevWaypoint = nextWaypoint;
		prevWeight = nextWeight;
		nextWeight = bestWeight;

		if(index != -1)
		{
			nextWaypoint[0] = reachablePoints[index].navPosition[0];
			nextWaypoint[1] = reachablePoints[index].navPosition[1];
			nextWaypoint[2] = reachablePoints[index].navPosition[2];
		}
		else
		{
			nextWaypoint = prevWaypoint;
		}
	}

	//Find next hide position based on wall proximity, number of walls, prey spotted and prey caught
	public void nextHuntPosition () 
	{
		//Filter reachable points
		reachablePoints = weightedList.Where( x => (Vector3.Distance(nextWaypoint, x.position) < searchRadius)).ToList();

		float bestWeight = nextWeight;
		float testWeight = 0;
		int index = -1;

		float wallDistRatio = 0.0f;
		float timeRatio = 0.0f;
		for(int i = 0; i < reachablePoints.Count; i++)
		{
			Vector3 dirToTarget = (reachablePoints[i].navPosition - nextWaypoint).normalized;
			float dstToTarget = Vector3.Distance(nextWaypoint, reachablePoints[i].navPosition);

			if(!Physics.Raycast(nextWaypoint,dirToTarget,dstToTarget,obstacleMask))
			{
				wallDistRatio = (1.0f - ((reachablePoints[i].wallDist - shortestDist)/(longestDist - shortestDist)));
				timeRatio = (1.0f - ((Time.time - reachablePoints[i].visitTime)/(Time.time)));

				testWeight = huntWallDistance * wallDistRatio + huntWallNumber * reachablePoints[i].numWalls + huntVisitTime * timeRatio + huntPreySpotted * reachablePoints[i].preySpotted + huntPreyCaught * reachablePoints[i].preyCaught;

				if(testWeight > bestWeight)
				{
					bestWeight = testWeight;
					index = i;
				}
			}
		}

		//Debug.Log(index);

		if(index != -1)
		{
			prevWaypoint = nextWaypoint;
			nextWaypoint[0] = reachablePoints[index].navPosition[0];
			nextWaypoint[1] = reachablePoints[index].navPosition[1];
			nextWaypoint[2] = reachablePoints[index].navPosition[2];



			//wallDistRatio = (1.0f - ((reachablePoints[index].wallDist - shortestDist)/(longestDist - shortestDist)));
			//timeRatio = (1.0f - ((Time.time - reachablePoints[index].visitTime)/(Time.time)));

			//searchRadius = 5.0f + wallDistRatio * 5.0f;

			//Debug.Log("Position: " + nextWaypoint + " Weight: " + bestWeight + "\nWall Distance: " + reachablePoints[index].wallDist + " Wall Ratio: " + wallDistRatio + "\nNumber of Walls: " + reachablePoints[index].numWalls + "\nVisit Time: " + reachablePoints[index].visitTime + " Visit Ratio: " + timeRatio);
		}
		else
		{
			nextWaypoint = prevWaypoint;
			prevWaypoint = nextWaypoint;
			prevWeight = nextWeight;
			nextWeight = bestWeight;
		}
	}

	public void nextFleePosition(Vector3 chaserPos)
	{
		//Filter reachable points
		reachablePoints = weightedList.Where( x => (Vector3.Distance(nextWaypoint, x.position) < searchRadius)).ToList();

		float bestWeight = nextWeight;
		float testWeight = 0;
		int index = -1;

		float wallDistRatio = 0.0f;
		float timeRatio = 0.0f;
		float distGained = 0.0f;
		int losValue = 0;

		for(int i = 0; i < reachablePoints.Count; i++)
		{
			Vector3 dirToTarget = (reachablePoints[i].navPosition - nextWaypoint).normalized;
			float dstToTarget = Vector3.Distance(nextWaypoint, reachablePoints[i].navPosition);

			if(!Physics.Raycast(nextWaypoint,dirToTarget,dstToTarget,obstacleMask))
			{
				wallDistRatio = (1.0f - ((reachablePoints[i].wallDist - shortestDist)/(longestDist - shortestDist)));
				timeRatio = (1.0f - ((Time.time - reachablePoints[i].visitTime)/(Time.time)));

				distGained =Vector3.Distance(chaserPos,reachablePoints[i].navPosition);
				Vector3 viewDir = (reachablePoints[i].navPosition - chaserPos).normalized;
				if(Physics.Raycast(chaserPos,viewDir,distGained,obstacleMask))
					losValue = 1;

				testWeight = fleeWallDistance * wallDistRatio + fleeWallNumber * reachablePoints[i].numWalls + fleeGainedDistance * distGained + fleeLOS * losValue + fleeCaught * reachablePoints[i].preyCaught;

				if(testWeight > bestWeight)
				{
					bestWeight = testWeight;
					index = i;
				}
			}
		}

		if(index != -1)
		{
			prevWaypoint = nextWaypoint;
			nextWaypoint[0] = reachablePoints[index].navPosition[0];
			nextWaypoint[1] = reachablePoints[index].navPosition[1];
			nextWaypoint[2] = reachablePoints[index].navPosition[2];



			//wallDistRatio = (1.0f - ((reachablePoints[index].wallDist - shortestDist)/(longestDist - shortestDist)));
			//timeRatio = (1.0f - ((Time.time - reachablePoints[index].visitTime)/(Time.time)));

			//searchRadius = 5.0f + wallDistRatio * 5.0f;

			//Debug.Log("Position: " + nextWaypoint + " Weight: " + bestWeight + "\nWall Distance: " + reachablePoints[index].wallDist + " Wall Ratio: " + wallDistRatio + "\nNumber of Walls: " + reachablePoints[index].numWalls + "\nVisit Time: " + reachablePoints[index].visitTime + " Visit Ratio: " + timeRatio);
		}
		else
		{
			nextWaypoint = prevWaypoint;
			prevWaypoint = nextWaypoint;
			prevWeight = nextWeight;
			nextWeight = bestWeight;
		}

	}



	public void setVisited(Vector3 position)
	{
		visitedPoints = weightedList.Where( x => (Vector3.Distance(position, x.position) < visitedRadius)).ToList();

		for(int i = 0; i < visitedPoints.Count; i++)
		{
			Vector3 dirToTarget = (visitedPoints[i].navPosition - position).normalized;
			float dstToTarget = Vector3.Distance(position, visitedPoints[i].navPosition);

			if(!Physics.Raycast(position,dirToTarget,dstToTarget,obstacleMask))
			{
				visitedPoints[i].setTime();
			}
		}
	}

	public void setSearchRadius(Vector3 position)
	{

	}
	//Points the ai in a certain direction
	
	//Move the AI - Patrol

	//Pick a number between the high and low weights.
	//From the point, with distance 9.5, randomly select a new point
	//At some point update the weight at this point. 

}
