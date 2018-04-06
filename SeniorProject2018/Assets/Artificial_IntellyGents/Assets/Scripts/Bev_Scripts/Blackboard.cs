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
				WeightPoint temp = new WeightPoint(0.5f, fillPoint);
				//add to list
				blackboardWeights.Add(temp);
				tempWeightChange.Add(temp);
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

	public void updateInfluence (List<WeightPoint> pred1, List<WeightPoint> pred2)
	{
		//Take in the two predators 
		//Update influence map between the two
		for(int influ = 0; influ < pred1.Count; influ++ )
		{
			//lets do an average
			tempWeightChange[influ].weight = (pred1[influ].weight + pred2[influ].weight)/2; 

		}

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
		Debug.Log("UPDATE");
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

			Gizmos.color = new Color(1.0F * tempWeightChange[k].weight, 0.0F, 0.0F, 0.2F);
			Gizmos.DrawCube(tempWeightChange[k].position, new Vector3(1.0f, 1.0f, 1.0f));

			
		}
		
	}
}
