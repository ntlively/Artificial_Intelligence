using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


		void Awake(){

			actor 			= this.gameObject;
			agent 			= actor.GetComponent<UnityEngine.AI.NavMeshAgent>();
			character 		= actor.GetComponent<ThirdPersonCharacter>();
			manager 		= actor.GetComponent<DataManager>();
			visionScript	= actor.GetComponent<Vision>();
			hearingScript	= actor.GetComponent<Hearing>();
			director		= actor.GetComponent<DirectorScript>();
			patroller		= actor.GetComponent<PatrolGuide>();
		}



		// Use this for initialization
		void Start () {
			agent.updatePosition = true;
			agent.updateRotation = false;

			manager.state = DataManager.State.THINK;
			manager.alive = true;

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
					case DataManager.State.THINK:
						Think();
						break;
				}
				yield return null;
			}
		}
		
		void Patrol()
		{
			//Have the character move to a random way point based on errors.
			agent.speed = manager.patrolSpeed;

			if(Vector3.Distance(this.transform.position, patroller.nextWaypoint )>= 2)
			{
				agent.SetDestination(patroller.nextWaypoint);
				character.Move(agent.desiredVelocity, false, false);
			}
			//If the player is close to way point, set the next way point.
			else if (Vector3.Distance(this.transform.position, patroller.nextWaypoint) <= 2)
			{
				patroller.nextPatrolPosition(); 
				agent.SetDestination(patroller.nextWaypoint);
			}
			//If there are no way points close by.
			else
			{
				character.Move(Vector3.zero, false, false);
			}
			
			visionFunction();
			//hearingFunction();

			patroller.patrolTimer = patroller.patrolTimer-Time.deltaTime;
			if(patroller.patrolTimer <= 0.0)
			{
				patroller.patrolTimer = 100.0f;
			}
		}

		void Chase()
		{
			agent.speed = manager.chaseSpeed;
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
		void visionFunction()
		{
			if (visionScript.visibleTargets.Count >0)
				{
					foreach (Vision.VisionInfo visibleTarget in visionScript.visibleTargets) 
					{
						if(visibleTarget.target.CompareTag("Player")){
							//Debug.Log("WE GOT ONE");
							target = visibleTarget.target;
							manager.state = DataManager.State.THINK;
						}

						if(visibleTarget.target.CompareTag("Predator") && manager.needUpdate){
							Debug.Log("Vision");
							target = visibleTarget.target;
							manager.state = DataManager.State.TALK;
							manager.shout = true;
						}
					}
				}
		}

		void checkKnowledge(bool tracking){
			Debug.Log("Check for delta now PLZ");

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
				}
			}

			Debug.Log(bestVisibleTarget.distance);
			Debug.Log(bestHearableTarget.decibel);
			double distance = bestVisibleTarget.distance;
			double decibel = bestHearableTarget.decibel;
			sensors.Add(distance); //closest
			sensors.Add(decibel); //loudest

			double [] results = director.neuralNet.Run(sensors);
			List<double> netChoice = new List<double>(results);

			int index = 0;
			int tempState = 0;
			double tempChance = 0.0;
			foreach (double stateChance in netChoice)
			{
				Debug.Log("net state chance:"+stateChance);
				double trackingVal = 0.0;
				if(tracking)
				{
					trackingVal = manager.netTracking.Peek()[index];
				}
				else
				{
					trackingVal = 0.0;
				}
				double delta = stateChance - trackingVal;
				Debug.Log("\n net state delta:"+delta);
				Debug.Log("\n");
				if(delta > tempChance)
				{
					tempState = index;
					tempChance = stateChance;
				}
				index++;
			}
			Debug.Log("\n\n\n");
			if(tracking)
			{
				manager.netTracking.Pop();
			}
			manager.netTracking.Push(netChoice);

			switch(tempState)
			{
				case 0:
					manager.state = DataManager.State.PATROL;
					Debug.Log("NO MORE THINK, ONLY PATROL NOW");
					break;
				case 1:
					manager.state = DataManager.State.CHASE;
					break;
				case 5:
					manager.state = DataManager.State.THINK;
					break;
			}
		}

	}	
}