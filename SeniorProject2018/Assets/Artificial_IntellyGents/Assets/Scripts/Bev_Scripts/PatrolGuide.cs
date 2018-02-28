using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//This class controls the actions the predator will take while in a state.

public class PatrolGuide : MonoBehaviour {

	//Variables 
		//Patroling 
		//Area influences, 
			//Holds percentage amounts for areas the predator knows about
			//List of percentage with top left corner of the score. 
			public List<WeightPoint> weightedList = new List<WeightPoint>();
			public Vector3 nextWaypoint = new Vector3 (0.0f, 0.0f, 0.0f); 
			public Vector3 prevWaypoint = new Vector3 (0.0f, 0.0f, 0.0f); 
			public List<WeightPoint> reachablePoints = new List<WeightPoint>();


	//Awake
	void Awake()
	{
		Vector3 fillPoints = new Vector3 (-21.0f, 1.0f, 20.0f);
		int row = 0;
		//Set map 
		for(int i = 0; i < 1600; i++)
		{

			fillPoints [0] = fillPoints[0] + 1.0f;
			row++;

			//create object
			WeightPoint temp = new WeightPoint(6.25f, fillPoints);
			//add to list
			weightedList.Add(temp);

			//Add in percentage 
			if(row == 40)
			{
			  fillPoints[0] = -21.0f;
			  fillPoints[2] = fillPoints[2]-1.0f;
			  row = 0;

			}

		}
		//nextPatrolPosition ();


	}
	// Use this for initialization
	void Start () {

		//Check the points
		/*for(int j = 0; j < weightedList.Count; j++)
		{
			Debug.LogError("Weight:" + weightedList[j].weight + "Point" + weightedList[j].position);
		}*/

		
	}

	public virtual void OnDrawGizmos () 
	{

		Vector3 fillPoints = new Vector3 (-21.0f, 1.0f, 20.0f);
		int row = 0;	
	
		/*for(int k = 0; k < weightedList.Count; k++)
		{
			if(k%2 == 0)
			{
				Gizmos.color = Color.green;
			}
			else
			{
				Gizmos.color = Color.blue;
			}
			fillPoints[0] = weightedList[k].position[0]+0.5f;
			fillPoints[2] = weightedList[k].position[2]-0.5f;

			Gizmos.DrawWireCube(fillPoints, new Vector3(1.0f, 1.0f, 1.0f));
			//UnityEditor.Handles.Label(fillPoints, " "+k+" "); 

			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(nextWaypoint, 0.5f);
		}*/

		//if(reachablePoints[0] != null)
		//{
			for(int i = 0; i < reachablePoints.Count; i++)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawWireCube(reachablePoints[i].position, new Vector3(1.0f, 1.0f, 1.0f));
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(nextWaypoint, 0.5f);
			}
		//}

		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	//Patrol point
	public Vector3 nextPatrolPosition () 
	{
		//Filter reachable points
		reachablePoints = weightedList.Where( x => (Vector3.Distance(nextWaypoint, x.position) > 10 && Vector3.Distance(nextWaypoint, x.position) < 13)).ToList();

     //(Vector3.Distance(nextWaypoint, point.position) > 10 && Vector3.Distance(nextWaypoint, point.position) < 13)
		Vector3 position = new Vector3 (0.0f, 1.0f, 0.0f);
		//Choose a random number
		int index = Random.Range(0,reachablePoints.Count-1);
		//int index = Random.Range(0,5);
		//Create a random number 
		nextWaypoint[0] = reachablePoints[index].position[0]+0.5f;
		nextWaypoint[2] = reachablePoints[index].position[2]-0.5f;	

		return nextWaypoint;
	}

	//Points the ai in a certain direction
	
	//Move the AI - Patrol

	//Pick a number between the high and low weights.
	//From the point, with distance 9.5, randomly select a new point
	//At some point update the weight at this point. 

}
