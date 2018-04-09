using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//using UnityEngine.SceneManagement;
/*
	//Predator Timers
	public float waitTimer = 5.0f; 
	public float patrolTimer = 10.0f;
	public float talkTimer = 5.0f; 
	public float updateTimer = 5.0f; 
*/

namespace UnityStandardAssets.Characters.ThirdPerson{

	public class bev_basicAI : MonoBehaviour {

		//public NavMeshAgent agent;
		//public ThirdPersonCharacter character;

		public GameObject 					actor;
		public UnityEngine.AI.NavMeshAgent 	agent;
		public ThirdPersonCharacter 		character;

		public DataManager 					manager;
		public DirectorScript 				director;
		public PatrolGuide 					patroller;

		public Vision 						visionScript;
		public Hearing 						hearingScript;
		public Transform 					target;

		// Variables for CHASE
		public Vector3 chasePos;
		public Vector3 chaseDir;
		public float predictionMod = 1.0f;
		public float predictionTime = 5.0f;
		private float predictionTimer = 0.0f;

		public float sampleTime = 1.0f;
		public float sampleTimer;

		//Global game counter
		public bool preyCaught = true;
		private float randomCheck = 5.0f;
		private bool notSkip = true;

		void Awake()
		{

			actor 			= this.gameObject;
			agent 			= actor.GetComponent<UnityEngine.AI.NavMeshAgent>();
			character 		= actor.GetComponent<ThirdPersonCharacter>();
			manager 		= actor.GetComponent<DataManager>();
			visionScript	= actor.GetComponent<Vision>();
			hearingScript	= actor.GetComponent<Hearing>();
			director		= actor.GetComponent<DirectorScript>();
			patroller		= actor.GetComponent<PatrolGuide>();

			patroller.nextWaypoint = this.transform.position;
			patroller.prevWaypoint = this.transform.position;

			sampleTimer = 0.0f;

		}
		// Use this for initialization
		void Start ()
		{
			agent.updatePosition = true;
			agent.updateRotation = false;

			manager.state = DataManager.State.PATROL;
			manager.alive = true;

			Physics.IgnoreLayerCollision(0,9);

			//Start FSM Finite state machine
			StartCoroutine("FSM");
		}

		IEnumerator FSM()
		{
			while (manager.alive)
			{
				switch(manager.state)
				{
					case DataManager.State.PATROL:
						Patrol();
						break;
					case DataManager.State.CHASE:
						Chase();
						break;
					case DataManager.State.SNEAK:
						Sneak();
						break;
					case DataManager.State.TALK:
						Talk();
						break;
				}

				//update talk timer
				manager.updateTimer = manager.updateTimer-Time.deltaTime;
				if(manager.updateTimer <= 0)
				{
 					manager.needUpdate = true;
				}
						

				yield return null;
			}
		}

		void Patrol()
		{
			//Have the character move to a random way point based on errors.
			agent.speed = manager.patrolSpeed;
			sampleTimer += Time.deltaTime;
			randomCheck -= Time.deltaTime;

			if(Vector3.Distance(this.transform.position, patroller.nextWaypoint )>= 1.5)
			{
				agent.SetDestination(patroller.nextWaypoint);
				character.Move(agent.desiredVelocity, false, false);
			}
			else if(randomCheck < 0)
			{
				randomCheck = 5.0f;
				notSkip = true;
				patroller.nextRandomPosition();
			}
			//If the player is close to way point, set the next way point.
			else if ((Vector3.Distance(this.transform.position, patroller.nextWaypoint) <= 1.5) && notSkip )
			{
				patroller.nextHuntPosition();
				notSkip = false;
				sampleTimer = 0.0f;
			}
			//If there are no way points close by.
			else
			{
				character.Move(Vector3.zero, false, false);
			}

			patroller.setVisited(this.transform.position);

			visionFunction();
			hearingFunction();

			/*patrolTimer = patrolTimer-Time.deltaTime;
			if(patrolTimer <= 0.0)
			{
				patrolTimer = 100.0f;
				//state = bev_basicAI.State.WAIT;
			}* */
		}

		void Chase()
		{
			agent.speed = manager.chaseSpeed;
			visionFunction();
			hearingFunction();

			if(target != null)
			{
				chaseDir = new Vector3(target.rotation[0],target.rotation[1],target.rotation[2]).normalized;
				//agent.SetDestination(target.position);
				chasePos = target.position + predictionMod * chaseDir;

				
				NavMeshHit hit;
				//check if point is on navmesh
				if(NavMesh.SamplePosition(chasePos, out hit, predictionMod, NavMesh.AllAreas))
				{
					chasePos = hit.position;
					chasePos[1] += 0.5f;
				}

				agent.SetDestination(chasePos);
				character.Move(agent.desiredVelocity,false,false);

				float dstToTarget = Vector3.Distance (transform.position, chasePos);
				if(dstToTarget < 1.5f)
				{
					target.gameObject.GetComponent<basicPreyAI>().caught(this.transform.position);
					patroller.preyCaught(target.transform.position);
					
					manager.globalGame.GetComponent<GlobalGame>().preyCaughtUpdate();
				}

				if(!target.gameObject.GetComponent<DataManager>().alive)
				{
					manager.state = DataManager.State.PATROL;
				}
			}
			else
			{
				predictionTimer += Time.deltaTime;

				agent.SetDestination(chasePos);
				character.Move(agent.desiredVelocity,false,false);

				if(predictionTimer < predictionTime && Vector3.Distance(this.transform.position,chasePos) <= 1.5f)
				{
					chasePos = chasePos + predictionMod * chaseDir;
					
					NavMeshHit hit;
					//check if point is on navmesh
					if(NavMesh.SamplePosition(chasePos, out hit, predictionMod, NavMesh.AllAreas))
					{
						chasePos = hit.position;
						chasePos[1] += 0.5f;
					}
				}
				else if(predictionTimer >= predictionTime)
				{
					manager.state = DataManager.State.PATROL;
					//Debug.Log("I LOST HIM");
					predictionTimer = 0.0f;
				}
			}

			patroller.setVisited(this.transform.position);
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

		void Talk ()
		{
			//Trigger when one or more predators enter vision.
			//AND Predator is currently not in chase state
			
			//Stop the guy who was spotted

			//Predators will exchange information using Blackboard
				//Move to predator until 2 blocks away. 
			visionFunction();
			hearingFunction();
			if(this.target != null)
			{
				if(Vector3.Distance(this.transform.position, this.target.transform.position) > 2.0f && manager.shout)
				{
					Debug.Log("Moving Talk");
					this.agent.speed = manager.patrolSpeed;
					this.character.Move(agent.desiredVelocity, false, false);
					this.agent.SetDestination(target.transform.position);
				}
				else
				{
					Debug.Log("Data Exchange");
					this.character.Move(Vector3.zero, false, false);
					this.agent.SetDestination(this.transform.position);
					this.transform.LookAt(target);

					if(Vector3.Distance(this.transform.position, this.target.transform.position) <= 2.0f)
					{
						if(manager.shout)
						{
							manager.shout = false;
						}
						manager.talkTimer = manager.talkTimer-Time.deltaTime;
						if(manager.talkTimer <= 0.0)
						{
							manager.updateTimer = 30.0f;
							manager.needUpdate = false;
							manager.talkTimer = 2.0f;
							manager.shout = false;
							manager.state = DataManager.State.PATROL;
							//exchange information
							patroller.weightedList = manager.globalGame.GetComponent<Blackboard>().updateInfluence(patroller.getInfluence(), target.GetComponent<PatrolGuide>().getInfluence());
							this.target = null;
						
						}
					}
					
				}
			}
			else
			{
				manager.updateTimer = 30.0f;
				manager.needUpdate = false;
				manager.talkTimer = 5.0f;
				manager.shout = false;
				manager.state = DataManager.State.PATROL;
			}
		}

		void onDrawGizmo()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(this.transform.position, 0.5f);
		}

		void visionFunction()
		{
			//target = null;

			if (visionScript.visibleTargets.Count >0)
				{
					foreach (Vision.VisionInfo visibleTarget in visionScript.visibleTargets) 
					{
						if(visibleTarget.target.CompareTag("Player") || visibleTarget.target.CompareTag("Prey")){
							target = visibleTarget.target;
							manager.state = DataManager.State.CHASE;
							chasePos = target.position;
							patroller.preySpotted(target.transform.position);
						}
						else if( (visibleTarget.target.gameObject.name == "FMS_Pred (1)" || visibleTarget.target.gameObject.name == "FMS_Pred")
								 &&(manager.needUpdate) 
								 && (manager.state != DataManager.State.CHASE ||visibleTarget.target.gameObject.GetComponent<DataManager>().state != DataManager.State.TALK)){
							target = visibleTarget.target;
							manager.state = DataManager.State.TALK;
							manager.shout = true;
						}
					}
				}



		}

		//Checking for call out 
		void hearingFunction()
		{

			//target = null;

			if (hearingScript.hearableTargets.Count >0)
				{
					foreach (Hearing.SoundInfo hearableTarget in hearingScript.hearableTargets) 
					{
						if(hearableTarget.target.CompareTag("Player") || hearableTarget.target.CompareTag("Prey")){
							target = hearableTarget.target;
							manager.state = DataManager.State.CHASE;
							chasePos = target.position;
						}

						if( (hearableTarget.target.gameObject.name == "FMS_Pred (1)" || hearableTarget.target.gameObject.name == "FMS_Pred" )
						&& (hearableTarget.target.gameObject.GetComponent<DataManager>().shout) 
						&& (manager.state != DataManager.State.CHASE || hearableTarget.target.gameObject.GetComponent<DataManager>().state != DataManager.State.TALK)){

							target = hearableTarget.target;
							manager.state = DataManager.State.TALK;
							manager.shout = true;
						}
					}
				}
		}
		
	}
}