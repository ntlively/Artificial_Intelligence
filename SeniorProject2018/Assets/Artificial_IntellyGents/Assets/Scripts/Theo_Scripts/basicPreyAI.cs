using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	public class basicPreyAI : MonoBehaviour {

		// Variable Declarations
		public GameObject prey;
		public UnityEngine.AI.NavMeshAgent agent;
		public ThirdPersonCharacter character;
		public Vision visionScript;
		public Hearing hearingScript;

		public enum State{
			SEARCH,
			HIDE,
			SNEAK,
			FLEE
		}

		public State state;
		private bool alive;

		private WayPointClass currentWaypoint;

		public PatrolGuide sn;

		// Variables for PATROL
		private int waypointINDEX = 0;
		public float patrolSpeed = 0.5f;


		// Variables for FLEE
		public float fleeSpeed = 1.0f;
		private float fleeAngle = 0.0f;
		private Transform chaser;

		public DecibelTracker noise;

		public static Stack<MemoryNode> memory = new Stack<MemoryNode>();

		void Awake(){
			prey = GameObject.Find("Prey");
			agent = prey.GetComponent<UnityEngine.AI.NavMeshAgent>();
			character = prey.GetComponent<ThirdPersonCharacter>();
			visionScript = prey.GetComponent<Vision>();
			hearingScript = prey.GetComponent<Hearing>();

			sn = this.GetComponent<PatrolGuide>();
			sn.nextWaypoint = this.transform.position;
			noise = this.GetComponent<DecibelTracker>();

			
			//sn.nextHidePosition();
		}

		// Use this for initialization
		void Start () {
			//agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
			//character = GetComponent<ThirdPersonCharacter>();

			agent.updatePosition = true;
			agent.updateRotation = false;

			state = basicPreyAI.State.SEARCH;
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
					case State.SEARCH:
						Search();
						break;
					case State.FLEE:
						Flee();
						break;
					case State.HIDE:
						Hide();
						break;
					case State.SNEAK:
						Sneak();
						break;
				}
				yield return null;
			}
		}
		
		void Search()
		{
			agent.speed = patrolSpeed;

			if(Vector3.Distance(this.transform.position,sn.nextWaypoint)>= 1)
			{
				agent.SetDestination(sn.nextWaypoint);
				character.Move(agent.desiredVelocity,false,false); //velocity, crouch, jump
			}
			else if (Vector3.Distance(this.transform.position,sn.nextWaypoint)<=1)
			{
				sn.nextHidePosition();
				if(sn.nextWaypoint == sn.prevWaypoint)
					state = basicPreyAI.State.HIDE;
			}
			else
			{
				character.Move(Vector3.zero, false, false);
			}
			/*if (visionScript.visibleTargets.Count >0)
			{
				foreach (Transform visibleTarget in visionScript.visibleTargets) {
					if(visibleTarget.CompareTag("Predator")){
						//Debug.Log("WE GOT ONE");
						chaser = visibleTarget;
						setFleeAngle(chaser);
						state = basicPreyAI.State.FLEE;
						sn.nextFleePosition(chaser.transform.position);
					}
				}
			}*/
		}

		void Flee()
		{
			agent.speed = fleeSpeed;

			if(Vector3.Distance(this.transform.position,sn.nextWaypoint)>= 1)
			{
				agent.SetDestination(sn.nextWaypoint);
				character.Move(agent.desiredVelocity,false,false); //velocity, crouch, jump
			}
			else if (Vector3.Distance(this.transform.position,sn.nextWaypoint)<=1)
			{
				sn.nextFleePosition(chaser.transform.position);
			}
			else
			{
				character.Move(Vector3.zero,false,false);
			}
		}

		void setFleeAngle(Transform chaser)
		{
			fleeAngle = Vector3.Angle(this.transform.position - chaser.position, this.transform.forward);
		}

		void Hide()
		{
			//Hide function
			transform.Rotate(0.0f,2.5f,0.0f);
			character.Move(Vector3.zero,true,false);

			/*if (visionScript.visibleTargets.Count >0)
			{
				foreach (Transform visibleTarget in visionScript.visibleTargets) {
					if(visibleTarget.CompareTag("Predator")){
						//Debug.Log("WE GOT ONE");
						chaser = visibleTarget;
						setFleeAngle(chaser);
						state = basicPreyAI.State.FLEE;
						sn.nextFleePosition(chaser.transform.position);
					}
				}
			}*/
		}

		void Sneak()
		{
			//Sneak function
		}

		public void caught(Vector3 catcherPos)
		{
			Vector3 hitDirection = (this.transform.position - catcherPos).normalized;
			alive = false;
			this.transform.GetChild(0).gameObject.SetActive(false);
			this.transform.GetChild(1).gameObject.SetActive(false);
			this.transform.GetChild(6).gameObject.SetActive(true);
			this.transform.GetChild(6).GetComponent<Rigidbody>().AddForce(hitDirection,ForceMode.Impulse);
			this.transform.GetChild(6).tag = "Dead";
			this.GetComponent<Rigidbody>().isKinematic = true;
			agent.SetDestination(this.transform.position);
		}

		public bool isAlive()
		{
			return alive;
		}
	}	
}