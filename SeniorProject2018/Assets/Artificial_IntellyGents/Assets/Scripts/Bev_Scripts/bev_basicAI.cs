using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//using UnityEngine.SceneManagement;

namespace UnityStandardAssets.Characters.ThirdPerson{

	public class bev_basicAI : MonoBehaviour {

		public NavMeshAgent agent;
		public ThirdPersonCharacter character;

		public Vision visionScript;
		public Hearing hearingScript;
		public GameObject actor;
		public DataManager manager;
		public DirectorScript director;
		
		//Variable patrolling
		private WayPointClass currentWaypoint;
		PatrolGuide patroler;
		public float patrolSpeed = 0.7f;

		//Variables for Chasing
		public float chaseSpeed = 1.2f;
		public Transform target;

		//Sound object
		public DecibelTracker noise;

		public float waitTimer = 5.0f; 
		public float patrolTimer = 10.0f;
		public float talkTimer = 10.0f; 

		public static Stack<MemoryNode> memory = new Stack<MemoryNode>();

		//Obstacle Avoidance
		public bool obstacle = false; // No obstacle
		//TODO: Use vision to detect Obstacles
		Vector3 forwardRay;
		Vector3 sideLeft;
		Vector3 sideRight;


		//Communcation
		public bool shout = false;
		public bool predatorHeard = false;
		public bool needUpdate = false;

		void Awake()
		{
			/*agent = GetComponent<NavMeshAgent>();
			character = GetComponent<ThirdPersonCharacter>();
			visionScript = this.GetComponent<Vision>();
			hearingScript = this.GetComponent<Hearing>();

			agent.updatePosition = true;
			agent.updateRotation = false;*/

			/*state = bev_basicAI.State.PATROL;
			alive = true;*/
			actor 			= this.gameObject;
			agent 			= actor.GetComponent<UnityEngine.AI.NavMeshAgent>();
			character 		= actor.GetComponent<ThirdPersonCharacter>();
			manager 		= actor.GetComponent<DataManager>();
			visionScript	= actor.GetComponent<Vision>();
			hearingScript	= actor.GetComponent<Hearing>();
			director		= actor.GetComponent<DirectorScript>();
			patroler 		= actor.GetComponent<PatrolGuide>();
			//noise = this.GetComponent<DecibelTracker>();

			forwardRay = transform.TransformDirection (Vector3.forward);
			sideLeft = transform.TransformDirection (Vector3.left);
			sideRight = transform.TransformDirection (Vector3.right);

		}
		// Use this for initialization
		void Start ()
		{
			agent.updatePosition = true;
			agent.updateRotation = false;

			manager.state = DataManager.State.THINK;
			manager.alive = true;
			//Start FSM Finite state machine
			StartCoroutine("Predator");
		}

		IEnumerator Predator()
		{
			while (manager.alive)
			{

				switch(manager.state)
				{
					case DataManager.State.PATROL:
						Patrol ();
						break;
					case DataManager.State.CHASE:
						Chase();
						break;
					case DataManager.State.SNEAK:
						Sneak();
						break;
					case DataManager.State.WAIT:
						Wait();
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
			agent.speed = patrolSpeed;

			forwardRay = transform.TransformDirection (Vector3.forward);
			sideLeft = transform.TransformDirection (new Vector3(-10,1,10));
			sideRight = transform.TransformDirection (new Vector3(10,1,10));
			Vector3 newPlayer = new Vector3(transform.position.x, 1.0f, transform.position.z);

			/*//Check for obstacle
			if(Physics.Raycast(newPlayer, forwardRay, 1f) ||
			   Physics.Raycast(newPlayer, sideLeft, 10f) ||
			   Physics.Raycast(newPlayer, sideRight, 10f))
			{
				obstacle = true;
				/*Debug.DrawRay(newPlayer, forwardRay, Color.red);
				Debug.DrawRay(newPlayer, sideLeft, Color.red);
				Debug.DrawRay(newPlayer, sideRight, Color.red);
			}
			else 
			{
				obstacle = false;
			}*/

			if(Vector3.Distance(this.transform.position, patroler.nextWaypoint )>= 1)
			{
				agent.SetDestination(patroler.nextWaypoint);
				character.Move(agent.desiredVelocity, false, false);
			}
			//If the player is close to way point, set the next way point.
			else if (Vector3.Distance(this.transform.position, patroler.nextWaypoint) <= 1)
			{
				patroler.nextHuntPosition(); 
				agent.SetDestination(patroler.nextWaypoint);
			}
			//If there are no way points close by.
			else
			{
				character.Move(Vector3.zero, false, false);
			}
			//character.Move(agent.desiredVelocity, false, false);
			//agent.SetDestination(new Vector3(0,0,0));

			//Obstacle Adjustment
			//TO DO: Account for large obstacles
			
			/*if(obstacle)
			{
				if(Physics.Raycast(newPlayer, sideLeft, 10f))
				{
					transform.Rotate(new Vector3(0f,40f,0f)*Time.deltaTime);
				}
				else if(Physics.Raycast(newPlayer, sideRight,10f))
				{
					transform.Rotate(new Vector3(0f,-40f,0f)*Time.deltaTime);
				}
			}*/

			
			visionFunction();
			//hearingFunction();

			patrolTimer = patrolTimer-Time.deltaTime;
			if(patrolTimer <= 0.0)
			{
				patrolTimer = 100.0f;
				//state = bev_basicAI.State.WAIT;
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
				manager.state = DataManager.State.THINK;
				// Patroling Time
				// Prey seen in area
				// etc...
			}

		}

		void Talk ()
		{
			//Trigger when one or more predators enter vision.
			//AND Predator is currently not in chase state
			
			//Stop the guy who was spotted

			//Predators will exchange information using Blackboard
	
			//Move to predator until 2 blocks away. 
			if(Vector3.Distance(this.transform.position, target.transform.position) > 3.0 && shout)
			{
				agent.speed = patrolSpeed;
				character.Move(agent.desiredVelocity, false, false);
				agent.SetDestination(target.transform.position);
			}
			else
			{
				character.Move(Vector3.zero, false, false);
				agent.SetDestination(this.transform.position);
				transform.LookAt(target);
				if(shout)
				{
					shout = false;
				}

				if(Vector3.Distance(this.transform.position, target.transform.position) <= 2.0)
				{
					talkTimer = talkTimer-Time.deltaTime;
					if(talkTimer <= 0.0)
					{
						talkTimer = 10.0f;
							manager.state = DataManager.State.THINK;
					}

				}
	
				
			}

	
		}

		void onDrawGizmo()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(this.transform.position, 0.5f);
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

						if(visibleTarget.target.CompareTag("Predator") && needUpdate){
							Debug.Log("Vision");
							target = visibleTarget.target;
							manager.state = DataManager.State.TALK;
							shout = true;
						}
					}
				}

		}

		//Checking for call out 
		/*void hearingFunction()
		{

			if (hearingScript.hearableTargets.Count >0)
				{
					foreach (Hearing.SoundInfo hearableTarget in hearingScript.hearableTargets) 
					{
						if(hearableTarget.target.CompareTag("Player")){
							//Debug.Log("WE GOT ONE");
							target = hearableTarget.target;
							state = bev_basicAI.State.CHASE;
						}

						if(hearableTarget.target.CompareTag("Predator") && hearableTarget.target.GetComponent<bev_basicAI>().shout){
							Debug.Log("Hearing");
							target = hearableTarget.target;
							state = bev_basicAI.State.TALK;
						}
					}
				}
		}*/


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