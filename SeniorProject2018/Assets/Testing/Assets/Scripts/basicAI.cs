using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// BUG REPORT
// 1) ON TRIGGER ENTER DOES NOT TRIGGER IF PLAYER SPAWNS INSIDE SIGHT RADIUS
// 2) NO CASE FOR LEAVING SPHERE COLLIDER TRIGGER ZONE
// 3) CHASE SPEED IN SCRIPT NOT SETTING PROPERLY
// 4) DON'T NEED SPHERE COLLIDER, BUT WHERE TO PUT VISION CHECK BESIDES ON TRIGGER ENTER



namespace UnityStandardAssets.Characters.ThirdPerson
{
	public class basicAI : MonoBehaviour {

		// Variable Declarations
		public UnityEngine.AI.NavMeshAgent agent;
		public ThirdPersonCharacter character;

		public enum State{
			PATROL,
			CHASE
		}

		public State state;
		private bool alive;

		// Variables for PATROL
		public GameObject[] waypoints;
		private int waypointINDEX = 0;
		public float patrolSpeed = 0.5f;


		// Variables for CHASE
		public float chaseSpeed = 0.1f;
		public GameObject target;


		// Use this for initialization
		void Start () {
			agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
			character = GetComponent<ThirdPersonCharacter>();

			agent.updatePosition = true;
			agent.updateRotation = false;

			state = basicAI.State.PATROL;
			alive = true;


			//start finite state machine (FSM)
			StartCoroutine("FSM");
			
		}

		IEnumerator FSM()
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
			agent.speed = patrolSpeed;
			if(Vector3.Distance(this.transform.position,waypoints[waypointINDEX].transform.position)>= 2)
			{
				agent.SetDestination(waypoints[waypointINDEX].transform.position);
				character.Move(agent.desiredVelocity,false,false); //velocity, crouch, jump
			}
			else if (Vector3.Distance(this.transform.position,waypoints[waypointINDEX].transform.position)<=2)
			{
				waypointINDEX += 1;
				if(waypointINDEX >= waypoints.Length)
				{
					waypointINDEX = 0;
				}
			}
			else
			{
				character.Move(Vector3.zero,false,false);
			}
		}

		void Chase()
		{
			agent.speed = chaseSpeed;
			agent.SetDestination(target.transform.position);
			character.Move(agent.desiredVelocity,false,false);
		}

		void OnTriggerEnter (Collider coll)
		{
			FieldOfView script = GameObject.Find("AIThirdPersonController").GetComponent<FieldOfView>();
			bool viewCheck = false;

			foreach (Transform visibleTarget in script.visibleTargets) {

				if(visibleTarget.CompareTag("Player")){
					print("TRUE");
					viewCheck = true;
				}
			}
			//print(coll.tag+":"+viewCheck);
			//coll.tag == "Player"  &&

			if(viewCheck)
			{
				state = basicAI.State.CHASE;
				target = coll.gameObject;
			}
		}
		// Update is called once per frame
		// void Update () {
			
		// }
	}	
}