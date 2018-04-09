using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class Blackboard : MonoBehaviour {

	public List<WeightPoint> blackboardWeights = new List<WeightPoint>();
	public List<WeightPoint> predatorLocations = new List<WeightPoint>();	
	public List<WeightPoint> tempWeightChange = new List<WeightPoint>();	
	public GameObject[] predators; 
	public float updatePredPosTimer = 2.0f;
	public float learningWeight = 0.01f;

	private float shortestDist = Mathf.Infinity;
	private float longestDist = Mathf.NegativeInfinity;
	private LayerMask obstacleMask;


	//Set up board
	void Awake()
	{
		obstacleMask = 1 << 10;
		
		//Map of the ground floor
		Vector3 fillPoints = new Vector3 (-21.0f, 0.0f, 21.0f);
		int row = 0;
		
		//Start from the left portion of the map and iterate through
		//creating weighted points that are on the navmesh.
		for(int i = 0; i < 5; i++)
		{
			for(int j = 0; j < 1600; j++)
			{
				fillPoints [0] = fillPoints[0] + 1.0f;
				row++;

				NavMeshHit hit;
				//check if point is on navmesh
				if(NavMesh.SamplePosition(fillPoints, out hit, 0.71f, NavMesh.AllAreas))
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
					blackboardWeights.Add(temp);
					tempWeightChange.Add(temp);
				}
				
				if(row == 40)
				{
				fillPoints[0] = -21.0f;
				fillPoints[2] = fillPoints[2]-1.0f;
				row = 0;

				}
			}
			
			fillPoints[0] = -21.0f;
			fillPoints[1] = fillPoints[1] + 1.0f;
			fillPoints[2] = 21.0f;

		}
		
	}
	
	// Use this for initialization
	void Start () 
	{
		updateCurPosition();
	}
	
	// Update is called once per frame
	void Update () 
	{
		//updateCurPosition();
		updatePredPosTimer = updatePredPosTimer-Time.deltaTime;
		if(updatePredPosTimer <= 0.0)
		{
			updatePredPosTimer = 2.0f;
			updateCurPosition();
		
		}
	}

	void updateBlackBoard()
	{
		//Take in the two predators 
		//Update influence map between the two

	}

	public List<WeightPoint> updateInfluence (List<WeightPoint> pred1, List<WeightPoint> pred2)
	{
		//Take in the two predators 
		//Update influence map between the two
		for(int influ = 0; influ < pred1.Count; influ++ )
		{
			float newWeightPC =  (Mathf.Abs(pred1[influ].preyCaught - pred2[influ].preyCaught))*learningWeight;
			float newWeightPS =  (Mathf.Abs(pred1[influ].preySpotted - pred2[influ].preySpotted))*learningWeight;
			float newWeightVT =  (Mathf.Abs(pred1[influ].visitTime - pred2[influ].visitTime))*learningWeight;
			//lets do an average
			pred1[influ].preyCaught = pred1[influ].preyCaught + pred1[influ].preyCaught*newWeightPC;
			pred1[influ].preySpotted = pred1[influ].preySpotted + pred1[influ].preySpotted*newWeightPS;
			pred1[influ].visitTime =  pred1[influ].visitTime + pred1[influ].visitTime*newWeightVT;
			//tempWeightChange[influ].weight = (pred1[influ].weight + pred2[influ].weight)/2; 
			tempWeightChange[influ].preyCaught = pred1[influ].preyCaught+newWeightPC;
			tempWeightChange[influ].preySpotted = pred1[influ].preySpotted+newWeightPS;
			tempWeightChange[influ].visitTime = pred1[influ].visitTime+newWeightVT;
		}

		return pred1;

	}

	//Called by predator to update current location on blackboard
	void updateCurPosition()
	{
		//Change current predator positions to false
		for(int k = 0; k < predatorLocations.Count; k++ )
		{
			int index = blackboardWeights.IndexOf(predatorLocations[k]);
			blackboardWeights[index].active = false;
		}
		//predatorLocations = blackboardWeights.Where( x => && Vector3.Distance(position, x.position) < 1.0f )).ToList();
		//Debug.Log("UPDATE");
		predators = GameObject.FindGameObjectsWithTag("Predator");
		for (int i = 0; i < predators.Length; i++)
		{
			List<WeightPoint> predLocat = blackboardWeights.Where( x => Vector3.Distance(predators[i].transform.position, x.position) < 1.0f ).ToList();
			//change found set
			for(int j = 0; j < predLocat.Count; j++ )
			{
				int index = blackboardWeights.IndexOf(predLocat[j]);
				blackboardWeights[index].active = true;
			}	

			//add to predator location list
			for(int an = 0; an < predLocat.Count; an++)
			{
				predatorLocations.Add(predLocat[an]);
			}
		}
	

	}

	//Called to make decision based on blackboard informaition
	void queryBlackboard()
	{
		//Getting next chase position
		//Getting patrol area
		
	}


	public virtual void OnDrawGizmos () 
	{

		Vector3 fillPoints = new Vector3 (0.0f, 0.0f, 0.0f);
	
		//Display entire influence map using cubes
		for(int k = 0; k < blackboardWeights.Count; k++)
		{
			if (blackboardWeights[k].active)
			{
				Gizmos.color = Color.red;
			} 
			else
			{
				Gizmos.color = new Color(0.0F, 0.0F, 1.0F, 0.2F);
			}

			//Adjust cubes so that the top right corner of the cube is the center of the cube.	
			fillPoints[0] = blackboardWeights[k].position[0];
			fillPoints[1] = blackboardWeights[k].position[1];
			fillPoints[2] = blackboardWeights[k].position[2];
		
			//Gizmos.DrawCube(fillPoints, new Vector3(1.0f, 1.0f, 1.0f));

			Gizmos.color = new Color(1.0F * tempWeightChange[k].preySpotted, 0.0F, 0.0F, 1.0F);
			Gizmos.DrawCube(tempWeightChange[k].position, new Vector3(1.0f, 1.0f, 1.0f));

			
		}
		
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
}
