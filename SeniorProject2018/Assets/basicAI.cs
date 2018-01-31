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
			CHASE,
			SNEAK,
			WAIT,
			TALK,

		}

		public State state; //current state.
		private bool alive; //whether the AI lives.
		
		//Variable patrolling
		//public GameObject[] waypoints;
		//private WayPointClass currentWaypoint;
		private WayPointClass currentWaypoint;
		WayPointMaster sn;
		public float patrolSpeed = 0.5f;

		//Variables for Chasing
		public float chaseSpeed = 1.0f;
		public GameObject target;

		//Sound object
		public DecibelTracker noise;

		// Use this for initialization
		void Start ()
		{
			agent = GetComponent<NavMeshAgent>();
			character = GetComponent<ThirdPersonCharacter>();

			agent.updatePosition = true;
			agent.updateRotation = false;

			state = basicAI.State.PATROL;
			alive = true;

			sn = this.GetComponent<WayPointMaster>();
			noise = this.GetComponent<DecibelTracker>();

			//Patroling 
			//Tracks visited way points.
			currentWaypoint = sn.NewWayPoint();
			//Get a random way point
			//GameObject[] tempPoints = GameObject.FindGameObjectsWithTag("Waypoint");
			//currentWaypoint = tempPoints[0].GetComponent<WayPointClass>();
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
					case State.SNEAK:
						Sneak();
						break;
					case State.WAIT:
						Wait();
						break;
					case State.TALK:
						Talk();
						break;

				}

				yield return null;
			}
		}

		void Patrol()
		{
			//Have the character move to a random way point based on errors.
			agent.speed = patrolSpeed;

			//Set sound
			noise.setCurrentDecibel(this);
			this.GetComponent<SphereCollider>().radius = noise.currentDecibel;

			//If player is within the range of a random way point, go to it.
			if(Vector3.Distance(this.transform.position, currentWaypoint.transform.position )>= 2)
			{
				agent.SetDestination(currentWaypoint.transform.position);
				character.Move(agent.desiredVelocity, false, false);
			}
			//If the player is close to way point, set the next way point.
			else if (Vector3.Distance(this.transform.position, currentWaypoint.transform.position) <= 2)
			{
				currentWaypoint = sn.NextWayPoint(currentWaypoint); 
			}
			//If there are no way points close by.
			else
			{
				character.Move(Vector3.zero, false, false);
			}

		}

		void Chase()
		{
			//Set Speed
			agent.speed = chaseSpeed;
			//Sound change
			noise.setCurrentDecibel(this);
			this.GetComponent<SphereCollider>().radius = noise.currentDecibel;


			agent.SetDestination(target.transform.position);
			character.Move(agent.desiredVelocity, false,false);
		}

		void Sneak()
		{
			//Set speed
				//Predator will move slower
			//Set sound	
				//Predator will move quieter

			//Predator will move quietly if prey hasn't detected their presence.
			//Prey movement speed.
				//If starts to move faster
					//Change state to Chase
				//Else 
					//Continuing sneaking.
		}

		void Wait()
		{
			//State in which is trigger depending on
				// Patroling Time
				// Prey seen in area
				// etc...
		}

		void Talk ()
		{
			//Trigger when one or more predators enter vision.
			//AND Predator is currently not in chase state

			//Predators will exchange information using Blackboard
		}



		//State changing
			//Switch to chase if prey.

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