using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	public class basicPredatorAI : MonoBehaviour {

		// Variable Declarations
		public GameObject predator;
		public UnityEngine.AI.NavMeshAgent agent;
		public ThirdPersonCharacter character;
		public Vision visionScript;
		public Hearing hearingScript;
		public MapData waypointGraph;

		public enum State{
			PATROL,
			CHASE
		}

		public State state;
		private bool alive;
		//public OffMeshLinkMoveMethod method = OffMeshLinkMoveMethod.Parabola;
		//public float jumpHeight = 2.0f;
		//public float jumpDuration = 0.5f;

		// Variables for PATROL
		private int waypointINDEX = 0;
		public float patrolSpeed = 0.5f;


		// Variables for CHASE
		public float chaseSpeed = 1.0f;
		public Transform target;

		void Awake(){
			predator = GameObject.Find("Predator");
			agent = predator.GetComponent<UnityEngine.AI.NavMeshAgent>();
			character = predator.GetComponent<ThirdPersonCharacter>();
			visionScript = predator.GetComponent<Vision>();
			hearingScript = predator.GetComponent<Hearing>();
			waypointGraph = new MapData();
			waypointGraph.triangulate();
		}

		// Use this for initialization
		void Start () {
			//agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
			//character = GetComponent<ThirdPersonCharacter>();

			agent.updatePosition = true;
			agent.updateRotation = false;

			state = basicPredatorAI.State.PATROL;
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
					case State.PATROL:
						Patrol();
						break;
					case State.CHASE:
						Chase();
						break;
				}
				// if (agent.isOnOffMeshLink)
				// {
				// 	character.Move(Vector3.zero,false,true);
				// 	if (method == OffMeshLinkMoveMethod.NormalSpeed)
				// 		yield return StartCoroutine(NormalSpeed(agent));
				// 	else if (method == OffMeshLinkMoveMethod.Parabola)
				// 		yield return StartCoroutine(Parabola(agent, jumpHeight, jumpDuration));
				// 	agent.CompleteOffMeshLink();
				// }
				yield return null;
			}
		}
		
		void Patrol()
		{
			agent.speed = patrolSpeed;
			if(Vector3.Distance(this.transform.position,waypointGraph.navPoints[waypointINDEX].position)>= 2)
			{
				agent.SetDestination(waypointGraph.navPoints[waypointINDEX].position);
				character.Move(agent.desiredVelocity,false,false); //velocity, crouch, jump
			}
			else if (Vector3.Distance(this.transform.position,waypointGraph.navPoints[waypointINDEX].position)<=2)
			{
				waypointINDEX += 1;
				if(waypointINDEX>=waypointGraph.navPoints.Count)
				{
					waypointINDEX = 0;
				}
			}
			if (visionScript.visibleTargets.Count >0)
			{
				foreach (Transform visibleTarget in visionScript.visibleTargets) {
					if(visibleTarget.CompareTag("Prey")){
						//Debug.Log("WE GOT ONE");
						target = visibleTarget;
						state = basicPredatorAI.State.CHASE;
					}
				}
			}
			// else
			// {
			// 	character.Move(Vector3.zero,false,false);
			// }
		}

		void Chase()
		{
			agent.speed = chaseSpeed;
			if(target.GetComponent<UnityEngine.AI.NavMeshAgent>().isOnOffMeshLink)
			{
				agent.SetDestination(target.GetComponent<UnityEngine.AI.NavMeshAgent>().currentOffMeshLinkData.endPos);
			}
			else
			{
				agent.SetDestination(target.transform.position);
			}
			character.Move(agent.desiredVelocity,false,false);
		}

		// void OnTriggerEnter (Collider coll)
		// {
		// 	if(coll.tag == "Prey")
		// 	{
		// 		state = basicPredatorAI.State.CHASE;
		// 		target = coll.gameObject;
		// 	}
		// }

		// public enum OffMeshLinkMoveMethod
		// {
		// 	Teleport,
		// 	NormalSpeed,
		// 	Parabola
		// }

		// IEnumerator NormalSpeed(UnityEngine.AI.NavMeshAgent agent)
		// {
		// 	UnityEngine.AI.OffMeshLinkData data = agent.currentOffMeshLinkData;
		// 	Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
		// 	while (agent.transform.position != endPos)
		// 	{
		// 		agent.transform.position = Vector3.MoveTowards(agent.transform.position, endPos, agent.speed * Time.deltaTime);
		// 		yield return null;
		// 	}
		// }
		// IEnumerator Parabola (UnityEngine.AI.NavMeshAgent agent, float jumpHeight, float jumpDuration)
		// {
		// 	UnityEngine.AI.OffMeshLinkData data = agent.currentOffMeshLinkData;
		// 	Vector3 startPos = agent.transform.position;
		// 	Vector3 endPos = data.endPos + Vector3.up*agent.baseOffset;
		// 	float normalizedTime = 0.0f;
		// 	while (normalizedTime < 1.0f)
		// 	{
		// 		float yOffset = jumpHeight * 4.0f*(normalizedTime - normalizedTime*normalizedTime);
		// 		agent.transform.position = Vector3.Lerp (startPos, endPos, normalizedTime) + yOffset * Vector3.up;
		// 		normalizedTime += Time.deltaTime / jumpDuration;
		// 		yield return null;
		// 	}
		// }
		// Update is called once per frame
		// void Update () {
			
		// }
	}	
}