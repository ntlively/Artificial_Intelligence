using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MemoryNode {
    
	public Vector3 preyPosition;
	public Vector3 predPositon;
	
	
	public MemoryNode ( Vector3 predPos, Vector3 preyPos)
	{
		preyPosition = preyPos;
		predPositon = predPos;
	}
	
	// Use this for initialization
	/*void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}*/
}


//Used for storing data from the envirorment
[System.Serializable]
public class WeightPoint {
    
	public Vector3 position;
	public Vector3 navPosition;
	public float wallDist;
	public int numWalls;

	public float visitTime;
	private float startTime;
	private float visitedTimer;

	public float preySpotted;
	public float preyCaught;
	public float predatorSpotted;

	
	
	public WeightPoint ( float dist, int walls, Vector3 pos, Vector3 navPos)
	{
		position = pos;
		navPosition = navPos;
		wallDist = dist;
		numWalls = walls;
		preySpotted = 0.0f;
		preyCaught = 0.0f;
		predatorSpotted = 0.0f;

		visitTime = 0.0f;
	}
	
	// Use this for initialization
	/*void Start () {
		
	}*/
	
	//Update is called once per frame
	public void setTime ()
	{
		visitTime = Time.time;
	}
}