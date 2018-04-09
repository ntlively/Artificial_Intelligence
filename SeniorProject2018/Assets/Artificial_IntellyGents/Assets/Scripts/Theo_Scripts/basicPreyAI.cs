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
		public Vector3 chasePos;

		public float sampleTime = 1.0f;
		private float sampleTimer;

		public float fleeTime = 5.0f;
		private float fleeTimer = 0.0f;


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
			hearingFunction();
			agent.speed = manager.fleeSpeed;
			sampleTimer += Time.deltaTime;

			if(chaser != null)
			{
				chasePos = chaser.position;
				
				if(Vector3.Distance(this.transform.position,sn.nextWaypoint)>= 1.5 && sampleTimer < (sampleTime*0.75))
				{
					agent.SetDestination(sn.nextWaypoint);
					character.Move(agent.desiredVelocity,false,false); //velocity, crouch, jump
				}
				else if (Vector3.Distance(this.transform.position,sn.nextWaypoint)<= 1.5 || sampleTimer >= (sampleTime*0.75))
				{
					sn.nextFleePosition(chasePos);
					sampleTimer = 0.0f;
				}
				else
				{
					character.Move(Vector3.zero,false,false);
				}
			}
			else
			{
				fleeTimer += Time.deltaTime;

				if(fleeTimer < fleeTime && Vector3.Distance(this.transform.position,sn.nextWaypoint)>= 1.5 && sampleTimer < (sampleTime*0.75))
				{
					agent.SetDestination(sn.nextWaypoint);
					character.Move(agent.desiredVelocity,false,false); //velocity, crouch, jump
				}
				else if (fleeTimer < fleeTime && Vector3.Distance(this.transform.position,sn.nextWaypoint)<= 1.5 || sampleTimer >= (sampleTime*0.75))
				{
					sn.nextFleePosition(chasePos);
					sampleTimer = 0.0f;
				}
				else if(fleeTimer >= fleeTime)
				{
					manager.state = DataManager.State.SEARCH;
					manager.globalGame.GetComponent<GlobalGame>().predatorEvadedUpdate();
					//Debug.Log("I HIDE NOW");
					fleeTimer = 0.0f;
				}

			}
			

			sn.setVisited(this.transform.position);

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
			hearingFunction();
			agent.speed = manager.sneakSpeed;
			sampleTimer += Time.deltaTime;

			if(chaser != null)
			{
				chasePos = chaser.position;
				
				if(Vector3.Distance(this.transform.position,sn.nextWaypoint)>= 1.5 && sampleTimer < (sampleTime*0.75))
				{
					agent.SetDestination(sn.nextWaypoint);
					character.Move(agent.desiredVelocity,true,false); //velocity, crouch, jump
				}
				else if (Vector3.Distance(this.transform.position,sn.nextWaypoint)<= 1.5 || sampleTimer >= (sampleTime*0.75))
				{
					sn.nextFleePosition(chasePos);
					sampleTimer = 0.0f;
				}
				else
				{
					character.Move(Vector3.zero,true,false);
				}
			}
			else
			{
				fleeTimer += Time.deltaTime;

				if(fleeTimer < fleeTime && Vector3.Distance(this.transform.position,sn.nextWaypoint)>= 1.5 && sampleTimer < (sampleTime*0.75))
				{
					agent.SetDestination(sn.nextWaypoint);
					character.Move(agent.desiredVelocity,false,false); //velocity, crouch, jump
				}
				else if (fleeTimer < fleeTime && Vector3.Distance(this.transform.position,sn.nextWaypoint)<= 1.5 || sampleTimer >= (sampleTime*0.75))
				{
					sn.nextFleePosition(chasePos);
					sampleTimer = 0.0f;
				}
				else if(fleeTimer >= fleeTime)
				{
					manager.state = DataManager.State.SEARCH;
					Debug.Log("I HIDE NOW");
					fleeTimer = 0.0f;
				}

			}
			

			sn.setVisited(this.transform.position);
		}

		void Think()
		{

		}

		
		void visionFunction()
		{
			chaser = null;

			if (visionScript.visibleTargets.Count >0)
			{
				foreach (Vision.VisionInfo visibleTarget in visionScript.visibleTargets) 
				{
					if(visibleTarget.target.CompareTag("Predator")){
						//Debug.Log("WE GOT ONE");
						chaser = visibleTarget.target;
						manager.state = DataManager.State.FLEE;
						sn.predatorSpotted(chaser.transform.position);
					}
				}
			}
		}

		void hearingFunction()
		{
			chaser = null;

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
			sn.preyCaught(this.transform.position);
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
			//this.transform.tag = "Dead";
			//this.GetComponent<Rigidbody>().isKinematic = true;
			//agent.SetDestination(this.transform.position);
			agent.updatePosition = false;
			this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
			this.transform.GetChild(5).gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
			agent.enabled = false;
			// this.hearingScript.enabled = false;
			// this.visionScript.enabled = false;
			manager.state = DataManager.State.DEAD;
		}

		public void reset()
		{
			//sn.preyCaught(this.transform.position);
			//Vector3 hitDirection = (this.transform.position - catcherPos).normalized;
			manager.alive = true;
			this.transform.GetChild(0).gameObject.SetActive(true);
			this.transform.GetChild(1).gameObject.SetActive(true);
			this.transform.GetChild(2).gameObject.SetActive(true);
			this.transform.GetChild(3).gameObject.SetActive(true);
			this.transform.GetChild(4).gameObject.SetActive(true);
			this.transform.GetChild(5).gameObject.SetActive(false);
			//this.transform.GetChild(5).GetComponent<Rigidbody>().AddForce(hitDirection,ForceMode.Impulse);
			//this.transform.GetChild(5).tag = "Prey";
			//this.transform.tag = "Dead";
			//this.GetComponent<Rigidbody>().isKinematic = true;
			agent.enabled = true;
			agent.updatePosition = true;
			agent.SetDestination(this.transform.position);
			this.GetComponent<Rigidbody>().constraints =RigidbodyConstraints.None;
			this.transform.GetChild(5).gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
			
			// this.hearingScript.enabled = false;
			// this.visionScript.enabled = false;
			sn.nextWaypoint = this.transform.position;
			manager.state = DataManager.State.SEARCH;
		}

		public bool isAlive()
		{
			return manager.alive;
		}
	}	
}