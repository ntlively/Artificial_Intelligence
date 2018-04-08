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

		public float visionFudge = 0.0f;
		public float hearingFudge = 0.0f;

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
			hearingFunction();

			patroller.patrolTimer = patroller.patrolTimer-Time.deltaTime;
			if(patroller.patrolTimer <= 0.0)
			{
				patroller.patrolTimer = 100.0f;
			}
		}

		void Chase()
		{
			bool deadSwitch = false;
			agent.speed = manager.chaseSpeed;
			agent.SetDestination(target.position);
			character.Move(agent.desiredVelocity,false,false);

			float dstToTarget = Vector3.Distance (transform.position, target.position);
			if(dstToTarget<0.8f)
			{
				target.gameObject.GetComponent<DataManager>().alive = false;
				deadSwitch = true;

			}

			if(!target.gameObject.GetComponent<DataManager>().alive)
			{
				manager.state = DataManager.State.THINK;
			}
			
			if(deadSwitch){
				visionFunction();
				hearingFunction();
			}

		}

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
				manager.state = DataManager.State.THINK;
			}
		}

		void Talk ()
		{
			//Trigger when one or more predators enter vision.
			//AND Predator is currently not in chase state
			
			//Stop the guy who was spotted

			//Predators will exchange information using Blackboard
	
			//Move to predator until 2 blocks away. 
			if(target.gameObject.CompareTag("Player")){
				target = null;
				manager.state = DataManager.State.THINK;
			}else{

			Debug.Log("TALKING.........");
			if(Vector3.Distance(this.transform.position, target.transform.position) > 3.0 && manager.shout)
			{
				agent.speed = manager.patrolSpeed;
				character.Move(agent.desiredVelocity, false, false);
				agent.SetDestination(target.transform.position);
			}
			else
			{
				character.Move(Vector3.zero, false, false);
				agent.SetDestination(this.transform.position);
				transform.LookAt(target);
				if(manager.shout)
				{
					manager.shout = false;
				}

				if(Vector3.Distance(this.transform.position, target.transform.position) <= 2.0)
				{
					manager.talkTimer = manager.talkTimer-Time.deltaTime;
					if(manager.talkTimer <= 0.0)
					{
						manager.talkTimer = 3.0f;
						manager.state = DataManager.State.THINK;
					}

				}
	
				
			}
		}
	
		}
		
		void Think()
		{
			Debug.Log("<Think>");
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
						if(visibleTarget.target.gameObject.CompareTag("Player")){
							target = visibleTarget.target;
							manager.state = DataManager.State.THINK;
						}

						if(visibleTarget.target.gameObject.CompareTag("Predator") &&(visibleTarget.target.gameObject.GetComponent<DataManager>().shout||manager.needUpdate)){
							target = visibleTarget.target;
							Debug.Log("found a fellow predator");
							manager.state = DataManager.State.TALK;
							manager.shout = true;
							visionFudge = 1000.0f;
							hearingFudge = 0.0f;
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
						if(hearableTarget.target.gameObject.CompareTag("Player")){
							//Debug.Log("HEARD A BITCH");
							target = hearableTarget.target;
							manager.state = DataManager.State.THINK;
						
						}

						if(hearableTarget.target.gameObject.CompareTag("Predator") && (hearableTarget.target.gameObject.GetComponent<DataManager>().shout||manager.needUpdate)){
							Debug.Log("STOP SHOUTING AT ME BICC");
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
					Debug.Log("NO MORE THINK, ONLY PATROL NOW");
					break;
				case 1:
					manager.state = DataManager.State.CHASE;
					Debug.Log("NO MORE THINK, ONLY CHASE NOW");
					break;
				case 2:
					manager.state = DataManager.State.SNEAK;
					Debug.Log("NO MORE THINK, ONLY SNEAK NOW");
					break;
				case 4:
					manager.state = DataManager.State.TALK;
					Debug.Log("NO MORE THINK, ONLY TALK NOW");
					break;
				case 5:
					manager.state = DataManager.State.THINK;
					break;
			}
		}

	}	
}