using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//using UnityEngine.SceneManagement;

namespace UnityStandardAssets.Characters.ThirdPerson{

	public class bev_basicAI : MonoBehaviour {

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
		public bool alive; //whether the AI lives.
		
		//Variable patrolling
		//public GameObject[] waypoints;
		//private WayPointClass currentWaypoint;
		private WayPointClass currentWaypoint;
		PatrolGuide sn;
		public float patrolSpeed = 0.7f;

		//Variables for Chasing
		public float chaseSpeed = 1.2f;
		public Transform target;

		//Sound object
		public DecibelTracker noise;

		public float waitTimer = 10.0f; 
		public float patrolTimer = 30.0f;
		public float talkTimer = 10.0f; 

		public static Stack<MemoryNode> memory = new Stack<MemoryNode>();

		public Vision visionScript;
		public Hearing hearingScript;
		public GameObject predator;

		void Awake()
		{
			agent = GetComponent<NavMeshAgent>();
			character = GetComponent<ThirdPersonCharacter>();

			agent.updatePosition = true;
			agent.updateRotation = false;

			state = bev_basicAI.State.PATROL;
			alive = true;

			sn = this.GetComponent<PatrolGuide>();
			noise = this.GetComponent<DecibelTracker>();

			predator = GameObject.Find("Predator");
			visionScript = predator.GetComponent<Vision>();
			hearingScript = predator.GetComponent<Hearing>();

		}
		// Use this for initialization
		void Start ()
		{
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

			//

			if(Vector3.Distance(this.transform.position, sn.nextWaypoint )>= 2)
			{
				agent.SetDestination(sn.nextWaypoint);
				character.Move(agent.desiredVelocity, false, false);
			}
			//If the player is close to way point, set the next way point.
			else if (Vector3.Distance(this.transform.position, sn.nextWaypoint) <= 2)
			{
				sn.nextRandomPosition(); 
			}
			//If there are no way points close by.
			else
			{
				character.Move(Vector3.zero, false, false);
			}

			if (visionScript.visibleTargets.Count >0)
			{
				foreach (Transform visibleTarget in visionScript.visibleTargets) {
					if(visibleTarget.CompareTag("Player")){
						//Debug.Log("WE GOT ONE");
						target = visibleTarget;
						state = bev_basicAI.State.CHASE;
					}
				}
			}


			patrolTimer = patrolTimer-Time.deltaTime;
			if(patrolTimer <= 0.0)
			{
				patrolTimer = 100.0f;
				state = bev_basicAI.State.WAIT;
			}
		}

		void Chase()
		{
			character.Move(Vector3.zero, false, false);
			//Set Speed
			agent.speed = chaseSpeed;

			//Sound change
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
			agent.SetDestination(this.transform.position);
			character.Move(Vector3.zero, false, false);
			waitTimer = waitTimer-Time.deltaTime;

			character.transform.Rotate(Vector3.up * Time.deltaTime);
			//State in which is trigger depending on
			if(waitTimer <= 0.0)
			{
				waitTimer = 10.0f;
				state = bev_basicAI.State.PATROL;
				// Patroling Time
				// Prey seen in area
				// etc...
			}

		}

		void Talk ()
		{
			//Trigger when one or more predators enter vision.
			//AND Predator is currently not in chase state

			//Predators will exchange information using Blackboard
			Debug.LogError("Talking");
		}


		/*void OnTriggerEnter(Collider coll)
		{
			if (coll.tag == "Player")
			{
				state = bev_basicAI.State.CHASE;
				target = coll.gameObject;
			}


		} */

		
	}
}