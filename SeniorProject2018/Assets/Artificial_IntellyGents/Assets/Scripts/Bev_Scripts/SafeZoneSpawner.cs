using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneSpawner : MonoBehaviour {

	public float safeZoneTimer;
	public float zoneSize;
	public bool preyActive; // used to track if a prey is current hiding in the safe zone

	public GameObject[] waypoints;
	public int currentLocation = 0;
	public Transform currPosition;


	// Use this for initialization
	void Awake () 
	{
		waypoints = GameObject.FindGameObjectsWithTag("Finish");
		currentLocation = UnityEngine.Random.Range(0, waypoints.Length);
		zoneSize = 5.0f;
		safeZoneTimer = 2.0f;	
	}

	// Update is called once per frame
	void Start () 
	{
		changeLocation();
	}
	
	// Update is called once per frame
	void Update () 
	{
		safeZoneTimer = safeZoneTimer-Time.deltaTime;
		if(safeZoneTimer <= 0.0)
		{
			safeZoneTimer = 2.0f;
			changeLocation();
		}
	}

	//Move spawn location
	void changeLocation()
	{
		currentLocation = UnityEngine.Random.Range(0, waypoints.Length);
		currPosition = waypoints[currentLocation].transform;

	}

    public virtual void OnDrawGizmos () 
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(currPosition.position, zoneSize);

	}

}
