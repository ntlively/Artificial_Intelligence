using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class Blackboard : MonoBehaviour {

	public List<WeightPoint> blackboardWeights = new List<WeightPoint>();
	public List<WeightPoint> predatorLocations = new List<WeightPoint>();	
	public GameObject[] predators; 


	//Set up board
	void Awake()
	{

		Vector3 fillPoint = new Vector3 (-19.3f, 0.0f, 19.0f);
		int row = 0;
		for(int i = 0; i < 1600; i++)
		{

			fillPoint [0] = fillPoint[0] + 1.0f;
			row++;

			NavMeshHit hit;
			//check if point is on navmesh
			if(NavMesh.SamplePosition(fillPoint, out hit , 0.2f, NavMesh.AllAreas))
			{
				//create object
				WeightPoint temp = new WeightPoint(6.25f, fillPoint);
				//add to list
				blackboardWeights.Add(temp);
			}
			//Add in percentage 
			if(row == 40)
			{
			fillPoint[0] = -21.0f;
			fillPoint[2] = fillPoint[2]-1.0f;
			row = 0;

			}

		}
	}
	
	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	void updateBlackBoard()
	{
		//Take in the two predators 
		//Update influence map between the two

	}

	//Called by predator to update current location on blackboard
	void updateCurPosition(Vector3 position)
	{
		//Set closest points to false
		//predatorLocations = blackboardWeights.Where( x => && Vector3.Distance(position, x.position) < 1.0f )).ToList();
		


		predators = GameObject.FindGameObjectsWithTag("Predator");
		for (int i = 0; i < predators.Length; i++)
		{
			predatorLocations = blackboardWeights.Where( x => Vector3.Distance(predators[i].transform.position, x.position) < 1.0f ).ToList();
			//change found set
			/*for(int j = )
			{

			}*/
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

		Vector3 fillPoints = new Vector3 (-21.0f, 1.0f, 20.0f);
	
		//Display entire influence map using cubes
		for(int k = 0; k < blackboardWeights.Count; k++)
		{
			//If the point is on the second level display as blue
			if (blackboardWeights[k].active)
			{
				Gizmos.color = Color.red;
			} 
			else
			{
				//Lower level of influence map.
				Gizmos.color = Color.green;
			}

			//Adjust cubes so that the top right corner of the cube is the center of the cube.	
			fillPoints[0] = blackboardWeights[k].position[0];
			fillPoints[1] = blackboardWeights[k].position[1];
			fillPoints[2] = blackboardWeights[k].position[2];
		
			Gizmos.DrawCube(fillPoints, new Vector3(1.0f, 1.0f, 1.0f));
		}
		
	}
}
