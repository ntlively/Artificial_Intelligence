using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace UnityStandardAssets.Characters.ThirdPerson{

	public class basicAI : MonoBehaviour {

		public NavMeshAgent agent;
		public ThirdPersonCharacter character;

		public enum State{
			PATROL,
			CHASE
		}

		public State state; //current state.
		private bool alive; //whether the AI lives.
		
		//Variable patrolling
		//public GameObject[] waypoints;
		private WayPointClass currentWaypoint;
		public float patrolSpeed = 0.5f;

		//Variables for Chasing
		public float chaseSpeed = 1.0f;
		public GameObject target;

		// Use this for initialization
		void Start ()
		{
			agent = GetComponent<NavMeshAgent>();
			character = GetComponent<ThirdPersonCharacter>();

			agent.updatePosition = true;
			agent.updateRotation = false;

			state = basicAI.State.PATROL;
			alive = true;

			//Get a random way point
			GameObject[] tempPoints = GameObject.FindGameObjectsWithTag("Waypoint");
			currentWaypoint = tempPoints[0].GetComponent<WayPointClass>();
			//waypoints = GameObject.FindGameObjectsWithTag("Waypoint");

			//Start FSM Finite state machine
			StartCoroutine("FSM");
		}

		IEnumerator FSM()
		{
			while (alive)
			{
				switch(state)
				{
					case State.PATROL:
						Patrol ();
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
			//Have the character move to a random way point based on errors.
			agent.speed = patrolSpeed;

			//If player is within the range of a random way point, go to it.
			if(Vector3.Distance(this.transform.position, currentWaypoint.transform.position )>= 2)
			{
				agent.SetDestination(currentWaypoint.transform.position);
				character.Move(agent.desiredVelocity, false, false);
			}
			//If the player is close to way point, set the next way point.
			else if (Vector3.Distance(this.transform.position, currentWaypoint.transform.position) <= 2)
			{
				currentWaypoint = currentWaypoint.NextWayPoint(currentWaypoint); 
			}
			//If there are no way points close by.
			else
			{
				character.Move(Vector3.zero, false, false);
			}

		}

		void Chase()
		{
			agent.speed = chaseSpeed;
			agent.SetDestination(target.transform.position);
			character.Move(agent.desiredVelocity, false,false);
		}

		void OnTriggerEnter(Collider coll)
		{
			if (coll.tag == "Player")
			{
				state = basicAI.State.CHASE;
				target = coll.gameObject;
			}


		}
		
		// Update is called once per frame
		//void Update () {
			
		//}
	}
}