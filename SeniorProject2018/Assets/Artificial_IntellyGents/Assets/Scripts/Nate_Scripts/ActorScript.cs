﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// BUG REPORT



namespace UnityStandardAssets.Characters.ThirdPerson
{
	public class ActorScript : MonoBehaviour {

		public GameObject 					actor;
		public UnityEngine.AI.NavMeshAgent 	agent;
		public ThirdPersonCharacter 		character;

		public DataManager 					manager;
		public DirectorScript 				director;
		public PatrolGuide 					patroller;

		public Vision 						visionScript;
		public Hearing 						hearingScript;


		// Variables for CHASE
		public Transform target;
		public Vector3 chasePos;
		public Vector3 chaseDir;
		public float predictionMod = 1.0f;
		public float predictionTime = 0.0f;
		private float predictionTimer = 0.0f;

		public float visionFudge = 0.0f;
		public float hearingFudge = 0.0f;

		// Variables for SEARCH
		public float sampleTime = 1.0f;
		private float sampleTimer;

		void Awake(){

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
		void Start () {
			agent.updatePosition = true;
			agent.updateRotation = false;

			manager.state = DataManager.State.THINK;
			manager.alive = true;

			Physics.IgnoreLayerCollision(0,9);

			//start finite state machine (FSM)
			StartCoroutine("Predator");
			
		}

		IEnumerator Predator()
		{
			while(manager.alive)
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
					case DataManager.State.THINK:
						Think();
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
		// keep this version
		void Patrol()
		{
			//Have the character move to a random way point based on errors.
			agent.speed = manager.patrolSpeed;
			sampleTimer += Time.deltaTime;

			if(Vector3.Distance(this.transform.position, patroller.nextWaypoint )>= 1.5 && sampleTimer < sampleTime)
			{
				agent.SetDestination(patroller.nextWaypoint);
				character.Move(agent.desiredVelocity, false, false);
			}
			//If the player is close to way point, set the next way point.
			else if (Vector3.Distance(this.transform.position, patroller.nextWaypoint) <= 1.5 || sampleTimer >= sampleTime)
			{
				patroller.nextHuntPosition();
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

		}
		//keep this version
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
					manager.state = DataManager.State.THINK;
				}
			}
			else
			{
				manager.state = DataManager.State.THINK;
				/*predictionTimer += Time.deltaTime;

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
					manager.state = DataManager.State.THINK;
					//Debug.Log("I LOST HIM");
					predictionTimer = 0.0f;
				}*/
			}

			patroller.setVisited(this.transform.position);
			
		}
		// keep this version
		void Sneak()
		{
			agent.speed = manager.sneakSpeed;
			agent.SetDestination(target.position);
			character.Move(agent.desiredVelocity,false,false);

			float dstToTarget = Vector3.Distance (transform.position, target.position);
			if(dstToTarget<0.8f)
			{
				target.gameObject.GetComponent<DataManager>().alive = false;
			}

			if(!target.gameObject.GetComponent<DataManager>().alive)
			{
				manager.state = DataManager.State.THINK;
			}

			if(visionScript.visibleTargets.Count>0)
			{
				Debug.Log("Lost vision, think again");
				manager.state = DataManager.State.THINK;
			}
			visionFunction();
			hearingFunction();
			patroller.setVisited(this.transform.position);
		}

		// use bev's finite state talk function
		void Talk()
		{
			/*Debug.Log("Now Talking...");
			agent.speed = manager.patrolSpeed;
			agent.SetDestination(target.position);
			character.Move(agent.desiredVelocity,false,false);*/

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
						/*if(manager.shout)
						{
							manager.shout = false;
						}*/
						
						manager.talkTimer = manager.talkTimer-Time.deltaTime;
						if(manager.talkTimer <= 0.0)
						{
							manager.updateTimer = 30.0f;
							manager.needUpdate = false;
							manager.talkTimer = 5.0f;
							manager.state = DataManager.State.PATROL;
							manager.shout = false;
							//exchange information
							//patroller.weightedList = manager.globalGame.GetComponent<Blackboard>().updateInfluence(patroller.getInfluence(), target.GetComponent<PatrolGuide>().getInfluence());
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


			/*if(visionScript.visibleTargets.Count>0)
			{
				manager.state = DataManager.State.PATROL;
			}*/
		}
		
		void Think()
		{
			if(manager.netTracking.Count == 0)
			{
				checkKnowledge(false);
			}
			else
			{
				checkKnowledge(true);
			}	

		}
		// neural net state changes
		void visionFunction()
		{
			//target = null;

			if (visionScript.visibleTargets.Count >0)
				{
					foreach (Vision.VisionInfo visibleTarget in visionScript.visibleTargets) 
					{
						if(visibleTarget.target.CompareTag("Player") || visibleTarget.target.CompareTag("Prey")){
							target = visibleTarget.target;
							manager.state = DataManager.State.THINK;
							chasePos = target.position;
							patroller.preySpotted(target.transform.position);
						}
						else if(visibleTarget.target.gameObject.CompareTag("Predator") &&(visibleTarget.target.gameObject.GetComponent<DataManager>().shout||manager.needUpdate) && manager.state != DataManager.State.CHASE){
							target = visibleTarget.target;
							manager.state = DataManager.State.TALK;
							manager.shout = true;
							visionFudge = 1000.0f;
							hearingFudge = 0.0f;
						}
					}
				}
		}
		// neural net state changes
		void hearingFunction()
		{
			//target = null;

			if (hearingScript.hearableTargets.Count >0)
				{
					foreach (Hearing.SoundInfo hearableTarget in hearingScript.hearableTargets) 
					{
						if(hearableTarget.target.CompareTag("Player") || hearableTarget.target.CompareTag("Prey")){
							target = hearableTarget.target;
							manager.state = DataManager.State.THINK;
							chasePos = target.position;
						}

						else if(hearableTarget.target.gameObject.CompareTag("Predator") && (hearableTarget.target.gameObject.GetComponent<DataManager>().shout||manager.needUpdate) && manager.state != DataManager.State.CHASE){

							target = hearableTarget.target;
							manager.state = DataManager.State.TALK;
							manager.shout = true;
							hearingFudge = 1000.0f;
						}
					}
				}
		}

		void checkKnowledge(bool tracking){

			List<double> sensors = new List<double>();
			Vision.VisionInfo bestVisibleTarget = new Vision.VisionInfo();
			float tempDist = Mathf.Infinity;
			// Vision Sensors
			foreach (Vision.VisionInfo visibleTarget in visionScript.visibleTargets)
			{
				if(visibleTarget.distance < tempDist)
				{
					bestVisibleTarget = visibleTarget;
					tempDist = visibleTarget.distance;
					visionFudge = visibleTarget.distance;
				}
			}

			Hearing.SoundInfo bestHearableTarget = new Hearing.SoundInfo();
			float tempDeci = Mathf.NegativeInfinity;
			// Hearing Sensors
			foreach (Hearing.SoundInfo hearableTarget in hearingScript.hearableTargets)
			{
				if(hearableTarget.decibel > tempDeci)
				{
					bestHearableTarget = hearableTarget;
					tempDeci = hearableTarget.decibel;
					hearingFudge = hearableTarget.decibel;
				}
			}


			if(tempDist == Mathf.Infinity)
			{
				bestVisibleTarget.distance = 0.0f;

			}
			else
			{
				visionFunction();
				bestVisibleTarget.distance = visionFudge;
			}
			//
			if(tempDeci == Mathf.NegativeInfinity)
			{
				bestHearableTarget.decibel = 0.0f;

			}
			else
			{
				hearingFunction();
				bestHearableTarget.decibel = hearingFudge;	
			}

			double distance = (double)bestVisibleTarget.distance;
			double decibel = (double)bestHearableTarget.decibel;
			sensors.Add(distance); //closest
			sensors.Add(decibel); //loudest

			double [] results = director.neuralNet.Run(sensors);
			List<double> netChoice = new List<double>(results);

			int index = 0;
			int tempState = 0;
			double tempChance = 0.0;
			foreach (double stateChance in netChoice)
			{
				//Debug.Log("net state chance:"+index+" : "+stateChance);
				double trackingVal = 0.0;
				if(tracking)
				{
					trackingVal = manager.netTracking.Peek()[index];
				}
				else
				{
					trackingVal = 0.0;
				}
				double delta = trackingVal - stateChance;
				//Debug.Log("<<|net state chance|>>"+index+" <<|>> "+stateChance+" <<|>> "+delta);
				//Debug.Log("\n net state delta:"+delta);
				//Debug.Log("\n");
				if(delta > tempChance)
				{
					tempState = index;
					tempChance = stateChance;
				}
				index++;
			}
			//Debug.Log("|state swap|>"+tempState+"<|");
			//Debug.Log("\n\n\n");
			if(tracking)
			{
				manager.netTracking.Pop();
			}
			manager.netTracking.Push(netChoice);

			switch(tempState)
			{
				case 0:
					manager.state = DataManager.State.PATROL;
					//Debug.Log("NO MORE THINK, ONLY PATROL NOW");
					break;
				case 1:
					manager.state = DataManager.State.CHASE;
					//Debug.Log("NO MORE THINK, ONLY CHASE NOW");
					break;
				case 2:
					manager.state = DataManager.State.SNEAK;
					//Debug.Log("NO MORE THINK, ONLY SNEAK NOW");
					break;
				case 4:
					manager.state = DataManager.State.TALK;
					//Debug.Log("NO MORE THINK, ONLY TALK NOW");
					break;
				case 5:
					manager.state = DataManager.State.THINK;
					break;
			}
		}

	}	
}