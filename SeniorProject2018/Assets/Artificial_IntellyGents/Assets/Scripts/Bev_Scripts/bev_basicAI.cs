using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//using UnityEngine.SceneManagement;

namespace UnityStandardAssets.Characters.ThirdPerson{

	public class bev_basicAI : MonoBehaviour {

		public NavMeshAgent agent;
		public ThirdPersonCharacter character;

		public enum State{
			PATROL,
			CHASE,
			SNEAK,
			WAIT,
			TALK,

		}

		public State state; //current state.
		public bool alive; //whether the AI lives.
		
		//Variable patrolling
		private WayPointClass currentWaypoint;
		PatrolGuide patroller;
		public float patrolSpeed = 0.7f;

		//Variables for Chasing
		public float chaseSpeed = 2.5f;
		public Transform target;

		//Sound object
		public DecibelTracker noise;

		public float waitTimer = 5.0f; 
		public float patrolTimer = 10.0f;
		public float talkTimer = 5.0f; 
		public float updateTimer = 5.0f; 

		public static Stack<MemoryNode> memory = new Stack<MemoryNode>();

		public Vision visionScript;
		public Hearing hearingScript;
		public GameObject predator;

		//Communcation
		public bool shout = false;
		public bool predatorHeard = false;
		public bool needUpdate = false;

		public float sampleTime = 1.0f;
		public float sampleTimer;


		// Variables for CHASE
		public Vector3 chasePos;
		public Vector3 chaseDir;
		public float predictionMod = 1.0f;
		public float predictionTime = 5.0f;
		private float predictionTimer = 0.0f;

		void Awake()
		{
			agent = GetComponent<NavMeshAgent>();
			character = GetComponent<ThirdPersonCharacter>();

			agent.updatePosition = true;
			agent.updateRotation = false;

			state = bev_basicAI.State.PATROL;
			alive = true;

			patroller = this.GetComponent<PatrolGuide>();
			noise = this.GetComponent<DecibelTracker>();

			//predator = //GameObject.Find("Predator");
			visionScript = this.GetComponent<Vision>();
			hearingScript = this.GetComponent<Hearing>();


			patroller.nextWaypoint = this.transform.position;
			patroller.prevWaypoint = this.transform.position;

			sampleTimer = 0.0f;

		}
		// Use this for initialization
		void Start ()
		{
			//Start FSM Finite state machine
			StartCoroutine("FSM");
		}

		IEnumerator FSM()
		{
			while (alive)
			{
				switch(state)
				{
					case State.PATROL:
						Patrol ();
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

				//update talk timer
				updateTimer = updateTimer-Time.deltaTime;
				if(updateTimer <= 0)
				{
 					needUpdate = true;
				}
						

				yield return null;
			}
		}

		void Patrol()
		{
			//Have the character move to a random way point based on errors.
			this.agent.speed = patrolSpeed;
			this.sampleTimer += Time.deltaTime;

			if(Vector3.Distance(this.transform.position, this.patroller.nextWaypoint )>= 1.5 && this.sampleTimer < this.sampleTime)
			{
				this.agent.SetDestination(this.patroller.nextWaypoint);
				this.character.Move(this.agent.desiredVelocity, false, false);
			}
			//If the player is close to way point, set the next way point.
			else if (Vector3.Distance(this.transform.position, patroller.nextWaypoint) <= 1.5 || this.sampleTimer >= this.sampleTime)
			{
				this.patroller.nextHuntPosition(); 
				this.sampleTimer = 0.0f;
				//Debug.Log("WAYPOINT:"+ patroller.nextWaypoint);
				//agent.SetDestination(patroller.nextWaypoint);
			}
			//If there are no way points close by.
			else
			{
				this.character.Move(Vector3.zero, false, false);
			}

			this.patroller.setVisited(this.transform.position);
			
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
			this.agent.speed = chaseSpeed;
			visionFunction();

			if(target != null)
			{
				chaseDir = new Vector3(target.rotation[0],target.rotation[1],target.rotation[2]).normalized;
				//agent.SetDestination(target.position);
				chasePos = this.target.position + predictionMod * chaseDir;

				
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
					this.target.gameObject.GetComponent<basicPreyAI>().caught(this.transform.position);

					//count prey caught

				}

				if(!target.gameObject.GetComponent<DataManager>().alive)
				{
					state = State.PATROL;
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
					state = State.PATROL;
				}
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
				state = bev_basicAI.State.PATROL;
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
			if(Vector3.Distance(this.transform.position, this.target.transform.position) > 3.0 && this.shout)
			{
				this.agent.speed = patrolSpeed;
				this.character.Move(agent.desiredVelocity, false, false);
				this.agent.SetDestination(target.transform.position);
			}
			else
			{
				Debug.Log("Data Exchange");
				this.character.Move(Vector3.zero, false, false);
				this.agent.SetDestination(this.transform.position);
				this.transform.LookAt(target);
				if(shout)
				{
					this.shout = false;
				}

				this.updateTimer = 5.0f;
				this.needUpdate = false;
				this.talkTimer = this.talkTimer-Time.deltaTime;
				if(this.talkTimer <= 0.0)
				{
					this.talkTimer = 10.0f;
					this.state = bev_basicAI.State.PATROL;
					//exchange information
					GameObject globalGame =  GameObject.Find("PredatorSpawn");
					globalGame.GetComponent<Blackboard>().updateInfluence(patroller.getInfluence(), target.GetComponent<PatrolGuide>().getInfluence());
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
					if(visibleTarget.target.CompareTag("Prey")){
						//Debug.Log("WE GOT ONE");
						target = visibleTarget.target;
						state = bev_basicAI.State.CHASE;
					}

					if(visibleTarget.target.CompareTag("Predator") && needUpdate)
					{
						Debug.Log("Vision");
						target = visibleTarget.target;
						state = bev_basicAI.State.TALK;
						shout = true;
					}
				}
			}

		}

		//Checking for call out 
		void hearingFunction()
		{

			if (hearingScript.hearableTargets.Count >0)
			{
				foreach (Hearing.SoundInfo hearableTarget in hearingScript.hearableTargets) 
				{
					if(hearableTarget.target.CompareTag("Prey")){
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

		}
		
	}
}