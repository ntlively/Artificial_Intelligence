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
	public float weight;
	public bool visited;
	
	
	public WeightPoint ( float wei, Vector3 pos)
	{
		position = pos;
		weight = wei;
	}
	
	// Use this for initialization
	/*void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}*/
}

