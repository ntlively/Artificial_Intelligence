using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	public class basicPreyAI : MonoBehaviour {

		// Variable Declarations
		public GameObject 					prey;
		public UnityEngine.AI.NavMeshAgent 	agent;
		public ThirdPersonCharacter 		character;

		public DataManager					manager;
		public DirectorScript				director;
		public PatrolGuide 					sn;

		public Vision visionScript;
		public Hearing hearingScript;

		// Variables for FLEE
		private Transform chaser;

		public float sampleTime = 1.0f;
		private float sampleTimer;


		void Awake(){
			prey = this.gameObject;
			agent = prey.GetComponent<UnityEngine.AI.NavMeshAgent>();
			character = prey.GetComponent<ThirdPersonCharacter>();
			manager 		= prey.GetComponent<DataManager>();
			visionScript = prey.GetComponent<Vision>();
			hearingScript = prey.GetComponent<Hearing>();
			director		= prey.GetComponent<DirectorScript>();

			sn = this.GetComponent<PatrolGuide>();

			sn.nextWaypoint = this.transform.position;

		}

		// Use this for initialization
		void Start () {

			agent.updatePosition = true;
			agent.updateRotation = false;

			manager.state = DataManager.State.SEARCH;
			manager.alive = true;


			//start finite state machine (FSM)
			StartCoroutine("Prey");
			
		}

		IEnumerator Prey()
		{
			while(manager.alive)
			{
				switch(manager.state)
				{
					case DataManager.State.SEARCH:
						Search();
						break;
					case DataManager.State.FLEE:
						Flee();
						break;
					case DataManager.State.HIDE:
						Hide();
						break;
					case DataManager.State.SNEAK:
						Sneak();
						break;
					case DataManager.State.THINK:
						Think();
						break;
				}
				yield return null;
			}
		}
		
		void Search()
		{
			agent.speed = manager.patrolSpeed;
			sampleTimer += Time.deltaTime;

			if(Vector3.Distance(this.transform.position,sn.nextWaypoint)>= 1.5 && sampleTimer < sampleTime)
			{
				agent.SetDestination(sn.nextWaypoint);
				character.Move(agent.desiredVelocity,false,false); //velocity, crouch, jump
			}
			else if (Vector3.Distance(this.transform.position,sn.nextWaypoint)<= 1.5 || sampleTimer >= sampleTime)
			{
				sn.nextHidePosition();
				sampleTimer = 0.0f;
			}
			else
			{
				character.Move(Vector3.zero, false, false);
			}
			
			if(sn.nextWaypoint == sn.prevWaypoint)
			{
				if(Vector3.Distance(this.transform.position,sn.nextWaypoint)<= 1.5)
						manager.state = DataManager.State.HIDE;
			}
			
			sn.setVisited(this.transform.position);

			visionFunction();
			hearingFunction();
		}

		void Flee()
		{
			agent.speed = manager.fleeSpeed;

			if(Vector3.Distance(this.transform.position,sn.nextWaypoint)>= 1.5 && sampleTimer < (sampleTime/2.0))
			{
				agent.SetDestination(sn.nextWaypoint);
				character.Move(agent.desiredVelocity,false,false); //velocity, crouch, jump
			}
			else if (Vector3.Distance(this.transform.position,sn.nextWaypoint)<= 1.5 || sampleTimer >= (sampleTime/2.0))
			{
				sn.nextFleePosition(chaser.transform.position);
			}
			else
			{
				character.Move(Vector3.zero,false,false);
			}

			sn.setVisited(this.transform.position);

			visionFunction();
			hearingFunction();
		}

		void Hide()
		{
			//Hide function
			transform.Rotate(0.0f,5.0f,0.0f);
			character.Move(Vector3.zero,true,false);

			visionFunction();
			hearingFunction();
		}

		void Sneak()
		{
			//Sneak function
		}

		void Think()
		{

		}

		
		void visionFunction()
		{
			if (visionScript.visibleTargets.Count >0)
			{
				foreach (Vision.VisionInfo visibleTarget in visionScript.visibleTargets) 
				{
					if(visibleTarget.target.CompareTag("Predator")){
						//Debug.Log("WE GOT ONE");
						chaser = visibleTarget.target;
						manager.state = DataManager.State.FLEE;
					}
				}
			}
		}

		void hearingFunction()
		{
			if (hearingScript.hearableTargets.Count >0)
				{
					foreach (Hearing.SoundInfo hearableTarget in hearingScript.hearableTargets) 
					{
						if(hearableTarget.target.CompareTag("Predator")){
							//Debug.Log("WE GOT ONE");
							chaser = hearableTarget.target;
							manager.state = DataManager.State.FLEE;
						}
					}
				}
		}

		public void caught(Vector3 catcherPos)
		{
			Vector3 hitDirection = (this.transform.position - catcherPos).normalized;
			manager.alive = false;
			this.transform.GetChild(0).gameObject.SetActive(false);
			this.transform.GetChild(1).gameObject.SetActive(false);
			this.transform.GetChild(2).gameObject.SetActive(false);
			this.transform.GetChild(3).gameObject.SetActive(false);
			this.transform.GetChild(4).gameObject.SetActive(false);
			this.transform.GetChild(5).gameObject.SetActive(true);
			this.transform.GetChild(5).GetComponent<Rigidbody>().AddForce(hitDirection,ForceMode.Impulse);
			this.transform.GetChild(5).tag = "Dead";
			//this.GetComponent<Rigidbody>().isKinematic = true;
			//agent.SetDestination(this.transform.position);
			agent.enabled = false;
			// this.hearingScript.enabled = false;
			// this.visionScript.enabled = false;
			manager.state = DataManager.State.DEAD;
		}

		public bool isAlive()
		{
			return manager.alive;
		}
	}	
}