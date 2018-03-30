using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using BayesianUtils;

// BUG REPORT



namespace UnityStandardAssets.Characters.ThirdPerson
{
	public class ActorScript : MonoBehaviour {

		// Variable Declarations
		//public GameObject predator;
		public UnityEngine.AI.NavMeshAgent agent;
		public ThirdPersonCharacter character;

		public DataManager manager;
		public DirectorScript director;
		public Vision visionScript;
		public Hearing hearingScript;

		//public BayesNet  testNet;
		// Variables for PATROL
		public List<GameObject> waypoints = new List<GameObject>();
		private int waypointINDEX = 0;
		public float patrolSpeed = 0.5f;


		// Variables for CHASE
		public float chaseSpeed = 0.1f;
		public Transform target;
		public GameObject actor;


		// Variables for THINK
		//NeuralNetwork neuralNet;

		void Awake(){

			actor 			= this.gameObject;
			agent 			= actor.GetComponent<UnityEngine.AI.NavMeshAgent>();
			character 		= actor.GetComponent<ThirdPersonCharacter>();
			manager 		= actor.GetComponent<DataManager>();
			visionScript	= actor.GetComponent<Vision>();
			hearingScript	= actor.GetComponent<Hearing>();
			director		= actor.GetComponent<DirectorScript>();
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
			agent.speed = patrolSpeed;
			if(Vector3.Distance(this.transform.position,waypoints[waypointINDEX].transform.position)>= 2)
			{
				agent.SetDestination(waypoints[waypointINDEX].transform.position);
				character.Move(agent.desiredVelocity,false,false); //velocity, crouch, jump
			}
			else if (Vector3.Distance(this.transform.position,waypoints[waypointINDEX].transform.position)<2)
			{
				waypointINDEX += 1;
				if(waypointINDEX >= waypoints.Count)
				{
					waypointINDEX = 0;
				}
			}
			if (visionScript.visibleTargets.Count >0)
			{
				int index = 0;
				foreach (Vision.VisionInfo visibleTarget in visionScript.visibleTargets) {
					if(visibleTarget.target.CompareTag("Player")){
						target = visibleTarget.target;
						manager.state = DataManager.State.THINK;
					}
					index++;
				}
			}
			
		}

		void Chase()
		{
			agent.speed = chaseSpeed;
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