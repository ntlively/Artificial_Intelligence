using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WayPointClass : MonoBehaviour {

	[SerializeField]
	protected float debugDrawRadius = 0.5F;
	
	//Visited 
	public bool visited;
	public bool preyEvidence;
	public float evidenceTimer;

	//List<WayPointClass> waypoints;

	void Start ()
	{
		//Set the waypoint to unvisited
		visited = false;

		//Set timer 
		preyEvidence = false;
		evidenceTimer = 20.0f;
	}

	//Updates everytime
	void Update ()
	{
		if(preyEvidence)
		{
			evidenceTimer -= Time.deltaTime;
			if (evidenceTimer <= 0 )
			{
				this.resetPreyEvidence(false);
				this.resetPreyTime();
			}
		}
	}

	//Displays waypoints
	public virtual void OnDrawGizmos () 
	{
		if(this.tag == "Respawn")
		{
		
			Gizmos.color = Color.blue;
			
		}
		

		Gizmos.DrawWireSphere(transform.position,debugDrawRadius);
	}

// -----------------------------------------------------------------------------------------
	//Gets and sets
//------------------------------------------------------------------------------------------
	public bool getVisit(){ return visited; }

	public bool getPreyEvidence(){ return preyEvidence; }

	public void resetVisit()
	{
		if(visited)
		{	
			visited = false;
		}
		else
		{
			visited = true;
		}
	}

	public void resetPreyEvidence(bool cur){ preyEvidence = cur;}

	public void resetPreyTime()
	{
		//Reset the timer 
		evidenceTimer = 20.0f;
	}




}
