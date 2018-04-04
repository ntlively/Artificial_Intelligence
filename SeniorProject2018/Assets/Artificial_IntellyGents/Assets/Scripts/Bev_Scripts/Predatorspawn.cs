using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Predatorspawn : MonoBehaviour {


	public GameObject predator;
	public int spawnAmount = 2; 
	public bool spawnCall = false;
	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(spawnCall == false)
		{
				Spawn();
		}
	}


	void Spawn ()
	{
		spawnCall = true;
		Vector3 position = new Vector3(0.0f, 1.3f, 0.0f);
        // Create an instance of the enemy prefab at the randomly selected spawn point's position and rotation.
		for(int i = 0; i < spawnAmount; i++)
		{

        	Instantiate (predator, position, predator.transform.rotation);
			position[2] -= 2.0f;
		}

	}
}
