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
			//List of percentage with top left corner of the score. 
			public List<WeightPoint> weightedList = new List<WeightPoint>();
			public Vector3 nextWaypoint = new Vector3 (0.0f, 0.0f, 0.0f); 
			private float nextWeight = 0.0f;
			public Vector3 prevWaypoint = new Vector3 (0.0f, 0.0f, 0.0f); 
			private float prevWeight = -1.0f;
			public List<WeightPoint> reachablePoints = new List<WeightPoint>();

			private LayerMask obstacleMask;

			private float shortestDist = Mathf.Infinity;
			private float longestDist = 0.0f;

			public float searchRadius = 5.0f;
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
			if (weightedList[k].position[1] == 4.5f)
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
		}


		//Displays the current reachable waypoints from pervious waypoint.
		for(int i = 0; i < reachablePoints.Count; i++)
		{
			if(reachablePoints[i].visited)
			{
				Gizmos.color = new Color(1.0f,0.0f,0.0f,0.25f);
			}
			else
			{
				Gizmos.color = new Color(1.0f,1.0f,1.0f,0.25f);
			}
			reachPoint[0] = reachablePoints[i].position[0];
			reachPoint[1] = reachablePoints[i].position[1];
			reachPoint[2] = reachablePoints[i].position[2];
			Gizmos.DrawCube(reachPoint, new Vector3(1.0f, 1.0f, 1.0f));
		}
		
		//Displays the selected waypoint the AI is headed to.
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(nextWaypoint, 0.5f);
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public float wallWeight(Vector3 testPoint)
	{
		float shortestDistance = Mathf.Infinity;
		for(int i = 0; i < 360; i +=10)
		{
			Vector3 angle = new Vector3(Mathf.Sin(i * Mathf.Deg2Rad),0,Mathf.Cos(i * Mathf.Deg2Rad));

			RaycastHit hit;
			Physics.Raycast(testPoint,angle,out hit,10);

			float dstToWall = Vector3.Distance(testPoint,hit.point);
			shortestDistance = Mathf.Min(shortestDistance, dstToWall);
		}

		return shortestDistance;
	}

	public int wallCount(Vector3 testPoint)
	{
		List<GameObject> hitList = new List<GameObject>();
		int wallCount = 0;

		for(int i = 0; i < 360; i += 10)
		{
			Vector3 angle = new Vector3(Mathf.Sin(i * Mathf.Deg2Rad),0,Mathf.Cos(i * Mathf.Deg2Rad));

			RaycastHit hit;
			if(Physics.Raycast(testPoint, angle, out hit, 2.0f, obstacleMask))
			{

				if(hitList.Count == 0)
				{
					wallCount++;
					hitList.Add(hit.transform.gameObject);
				}
				else
				{
					bool unique = true;
					for(int j = 0; j < hitList.Count; j++)
					{
						if(hit.transform.gameObject == hitList[j])
						{
							unique = false;
						}
					}
					if(unique)
					{
						wallCount++;
						hitList.Add(hit.transform.gameObject);
					}
				}
				
			}
		}

		return wallCount;
	}

	//Patrol point
	public Vector3 nextRandomPosition () 
	{
		//Filter reachable points
		reachablePoints = weightedList.Where( x => (Vector3.Distance(nextWaypoint, x.position) < searchRadius)).ToList();

     //(Vector3.Distance(nextWaypoint, point.position) > 10 && Vector3.Distance(nextWaypoint, point.position) < 13)


		//Choose a random number
		int index = Random.Range(0,reachablePoints.Count-1);

		prevWaypoint = nextWaypoint;
		
		//Next waypoint is adjusted to be the center of the cube
		nextWaypoint[0] = reachablePoints[index].navPosition[0];
		nextWaypoint[1] = reachablePoints[index].navPosition[1]+0.5f;
		nextWaypoint[2] = reachablePoints[index].navPosition[2];	

		return nextWaypoint;
	}

	//Find next hide position based on wall proximity, number of walls, prey spotted and prey caught
	public Vector3 nextHidePosition () 
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
				testWeight = (2*(1.0f - ((reachablePoints[i].wallDist - shortestDist)/(longestDist - shortestDist)))
					+ reachablePoints[i].numWalls - reachablePoints[i].preySpotted - reachablePoints[i].preyCaught);
				

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
			nextWaypoint[1] = reachablePoints[index].navPosition[1]+0.5f;
			nextWaypoint[2] = reachablePoints[index].navPosition[2];
		}
		else
		{
			nextWaypoint = prevWaypoint;
		}

		return nextWaypoint;
	}

	//Find next hide position based on wall proximity, number of walls, prey spotted and prey caught
	public Vector3 nextHuntPosition () 
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

			reachablePoints[i].updateTimer();

			if(!Physics.Raycast(nextWaypoint,dirToTarget,dstToTarget,obstacleMask) && reachablePoints[i].visited == false)
			{
				testWeight = (1.0f - ((reachablePoints[i].wallDist - shortestDist)/(longestDist - shortestDist))
					+ reachablePoints[i].numWalls + reachablePoints[i].preySpotted + reachablePoints[i].preyCaught);

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
			reachablePoints[index].startTimer();
			nextWaypoint[0] = reachablePoints[index].navPosition[0];
			nextWaypoint[1] = reachablePoints[index].navPosition[1]+0.5f;
			nextWaypoint[2] = reachablePoints[index].navPosition[2];
			reachablePoints = weightedList.Where( x => (Vector3.Distance(nextWaypoint, x.position) < 2.0)).ToList();
			for(int i = 0; i < reachablePoints.Count; i++)
			{
				reachablePoints[i].startTimer();
			}
		}
		else
		{
			nextWaypoint = prevWaypoint;
			// prevWaypoint = nextWaypoint;
			// prevWeight = nextWeight;
			// nextWeight = bestWeight;
		}

		return nextWaypoint;
	}
	//Points the ai in a certain direction
	
	//Move the AI - Patrol

	//Pick a number between the high and low weights.
	//From the point, with distance 9.5, randomly select a new point
	//At some point update the weight at this point. 

}
