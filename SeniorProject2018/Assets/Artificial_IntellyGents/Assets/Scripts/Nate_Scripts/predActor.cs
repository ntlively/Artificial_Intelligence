using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// BUG REPORT
// 1) ON TRIGGER ENTER DOES NOT TRIGGER IF PLAYER SPAWNS INSIDE SIGHT RADIUS
// 2) NO CASE FOR LEAVING SPHERE COLLIDER TRIGGER ZONE
// 3) CHASE SPEED IN SCRIPT NOT SETTING PROPERLY
// 4) DON'T NEED SPHERE COLLIDER, BUT WHERE TO PUT VISION CHECK BESIDES ON TRIGGER ENTER
// 5) COMPILE ERROR, NOT SURE WHY


namespace UnityStandardAssets.Characters.ThirdPerson
{
	public class predActor : MonoBehaviour {

		// Variable Declarations
		public GameObject predator;
		public UnityEngine.AI.NavMeshAgent agent;
		public ThirdPersonCharacter character;
		public Vision visionScript;
		public Hearing hearingScript;

		public enum State{
			PATROL,
			CHASE
		}

		public State state;
		private bool alive;

		// Variables for PATROL
		public List<GameObject> waypoints = new List<GameObject>();
		private int waypointINDEX = 0;
		public float patrolSpeed = 0.5f;


		// Variables for CHASE
		public float chaseSpeed = 0.1f;
		public Transform target;

		void Awake(){
			predator = GameObject.Find("Predator");
			agent = predator.GetComponent<UnityEngine.AI.NavMeshAgent>();
			//Debug.Log(agent);
			character = predator.GetComponent<ThirdPersonCharacter>();
			//Debug.Log(character);
			visionScript = predator.GetComponent<Vision>();
			//Debug.Log(visionScript);
			hearingScript = predator.GetComponent<Hearing>();
		}



		// Use this for initialization
		void Start () {
			
			// predator = GameObject.Find("Predator");
			// agent = predator.GetComponent<UnityEngine.AI.NavMeshAgent>();
			// //Debug.Log(agent);
			// character = predator.GetComponent<ThirdPersonCharacter>();
			// //Debug.Log(character);
			// visionScript = predator.GetComponent<Vision>();
			// //Debug.Log(visionScript);
			// hearingScript = predator.GetComponent<Hearing>();
			//Debug.Log(hearingScript);
			agent.updatePosition = true;
			agent.updateRotation = false;


			state = predActor.State.PATROL;
			alive = true;

			//start finite state machine (FSM)
			StartCoroutine("Predator");
			
		}
		// bool started = false;
		// void LateUpdate(){
		// 	if(!started){
		// 		StartCoroutine("Predator");
		// 		started = true;
		// 	}
		// }

		IEnumerator Predator()
		{
			while(alive)
			{
				switch(state)
				{
					case State.PATROL:
						Patrol();
						break;
					case State.CHASE:
						Chase();
						break;
				}
				yield return null;
			}
		}
		
		void Patrol()
		{
//			Debug.Log(visionScript.visibleTargets.Count);
			agent.speed = patrolSpeed;
			if(Vector3.Distance(this.transform.position,waypoints[waypointINDEX].transform.position)>= 2)
			{
				agent.SetDestination(waypoints[waypointINDEX].transform.position);
				//Debug.Log("agent: " + agent.desiredVelocity);
				character.Move(agent.desiredVelocity,false,false); //velocity, crouch, jump
			}
			else if (Vector3.Distance(this.transform.position,waypoints[waypointINDEX].transform.position)<2)
			{
				waypointINDEX += 1;
				if(waypointINDEX >= waypoints.Count)
				{
					waypointINDEX = 0;
				}
			}
			if (visionScript.visibleTargets.Count >0)
			{
				foreach (Transform visibleTarget in visionScript.visibleTargets) {
					if(visibleTarget.CompareTag("Player")){
						//Debug.Log("WE GOT ONE");
						target = visibleTarget;
						state = predActor.State.CHASE;
					}
				}
			}
			// else
			// {
			// 	character.Move(Vector3.zero,false,false);
			// }
		}

		void Chase()
		{
			agent.speed = chaseSpeed;
			agent.SetDestination(target.position);
			character.Move(agent.desiredVelocity,false,false);
		}

	}	
}