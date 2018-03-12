using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	public class basicPredatorAI : MonoBehaviour {

		// Variable Declarations
		public GameObject predator;
		public UnityEngine.AI.NavMeshAgent agent;
		public ThirdPersonCharacter character;
		public Vision visionScript;
		public Hearing hearingScript;
		public MapData waypointGraph;

		public enum State{
			PATROL,
			CHASE,
			SNEAK,
			WAIT,
			TALK
		}

		public State state;
		private bool alive;
		//public OffMeshLinkMoveMethod method = OffMeshLinkMoveMethod.Parabola;
		//public float jumpHeight = 2.0f;
		//public float jumpDuration = 0.5f;
		private WayPointClass currentWaypoint;
		public PatrolGuide sn;

		// Variables for PATROL
		private int waypointINDEX = 0;
		public float patrolSpeed = 0.5f;


		// Variables for CHASE
		public float chaseSpeed = 1.0f;
		public Transform target;

		public DecibelTracker noise;

		public float waitTimer = 10.0f;
		public float patrolTimer = 30.0f;
		public float talkTimer = 10.0f;

		public static Stack<MemoryNode> memory = new Stack<MemoryNode>();

		void Awake(){
			predator = GameObject.Find("Predator");
			agent = GetComponent<NavMeshAgent>();
			character = predator.GetComponent<ThirdPersonCharacter>();
			visionScript = predator.GetComponent<Vision>();
			hearingScript = predator.GetComponent<Hearing>();
			waypointGraph = new MapData();
			waypointGraph.triangulate();

			sn = this.GetComponent<PatrolGuide>();
			noise = this.GetComponent<DecibelTracker>();
		}

		// Use this for initialization
		void Start () {
			//agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
			//character = GetComponent<ThirdPersonCharacter>();

			agent.updatePosition = true;
			agent.updateRotation = false;

			state = basicPredatorAI.State.PATROL;
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
			agent.speed = patrolSpeed;

			if(Vector3.Distance(this.transform.position,sn.nextWaypoint)>= 2)
			{
				agent.SetDestination(sn.nextWaypoint);
				character.Move(agent.desiredVelocity,false,false); //velocity, crouch, jump
			}
			else if (Vector3.Distance(this.transform.position,sn.nextWaypoint)<=2)
			{
				sn.nextPatrolPosition();
			}
			else
			{
				character.Move(Vector3.zero, false, false);
			}
			if (visionScript.visibleTargets.Count >0)
			{
				foreach (Transform visibleTarget in visionScript.visibleTargets) {
					if(visibleTarget.CompareTag("Prey")){
						//Debug.Log("WE GOT ONE");
						target = visibleTarget;
						state = basicPredatorAI.State.CHASE;
					}
				}
			}
			patrolTimer = patrolTimer-Time.deltaTime;
			if(patrolTimer <= 0.0)
			{
				patrolTimer = 100.0f;
				state = basicPredatorAI.State.WAIT;
			}
		}

		void Chase()
		{
			agent.speed = chaseSpeed;
			if(target.tag == "prey" &&target.GetComponent<NavMeshAgent>().isOnOffMeshLink)
			{
				agent.SetDestination(target.GetComponent<NavMeshAgent>().currentOffMeshLinkData.endPos);
			}
			else
			{
				agent.SetDestination(target.transform.position);
			}
			character.Move(agent.desiredVelocity,false,false);

			//sound change
		}

		void Sneak()
		{

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
				state = basicPredatorAI.State.PATROL;
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
	}	
}