using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	public class basicPreyAI : MonoBehaviour {

		// Variable Declarations
		public GameObject prey;
		public UnityEngine.AI.NavMeshAgent agent;
		public ThirdPersonCharacter character;
		public Vision visionScript;
		public Hearing hearingScript;
		//public NavMeshPainter meshPainter;
		public MapData waypointGraph;

		public enum State{
			SEARCH,
			HIDE,
			SNEAK,
			FLEE
		}

		public State state;
		private bool alive;
		//public OffMeshLinkMoveMethod method = OffMeshLinkMoveMethod.Parabola;
		//public float jumpHeight = 2.0f;
		//public float jumpDuration = 0.5f;

		// Variables for PATROL
		private int waypointINDEX = 0;
		public float patrolSpeed = 0.5f;


		// Variables for FLEE
		public float fleeSpeed = 1.0f;
		private float fleeAngle = 0.0f;
		private Transform chaser;

		void Awake(){
			prey = GameObject.Find("Prey");
			agent = prey.GetComponent<UnityEngine.AI.NavMeshAgent>();
			character = prey.GetComponent<ThirdPersonCharacter>();
			visionScript = prey.GetComponent<Vision>();
			hearingScript = prey.GetComponent<Hearing>();
			//meshPainter = prey.GetComponent<NavMeshPainter>();
			waypointGraph = new MapData();
			waypointGraph.triangulate();
		}

		// Use this for initialization
		void Start () {
			//agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
			//character = GetComponent<ThirdPersonCharacter>();

			agent.updatePosition = true;
			agent.updateRotation = false;

			state = basicPreyAI.State.SEARCH;
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
					case State.SEARCH:
						Patrol();
						break;
					case State.FLEE:
						Flee();
						break;
				}
				// if (agent.isOnOffMeshLink)
				// {
				// 	character.Move(agent.desiredVelocity,false,true);
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
					if(visibleTarget.CompareTag("Predator")){
						//Debug.Log("WE GOT ONE");
						chaser = visibleTarget;
						setFleeAngle(chaser);
						state = basicPreyAI.State.FLEE;
					}
				}
			}
			// else
			// {
			// 	character.Move(Vector3.zero,false,false);
			// }
		}

		void Flee()
		{
			agent.speed = fleeSpeed;

			if(Vector3.)


			if(Vector3.Distance(this.transform.position,waypointGraph.navPoints[waypointINDEX].position)> 5)
			{
				agent.SetDestination(waypointGraph.navPoints[waypointINDEX].position);
				character.Move(agent.desiredVelocity,false,false); //velocity, crouch, jump
			}
			else if (Vector3.Distance(this.transform.position,waypointGraph.navPoints[waypointINDEX].position)<=5)
			{
				waypointINDEX += 1;
				if(waypointINDEX>=waypointGraph.navPoints.Count)
				{
					waypointINDEX = 0;
				}
			}
			else
			{
				character.Move(Vector3.zero,false,false);
			}
		}

		void setFleeAngle(Transform chaser)
		{
			fleeAngle = Vector3.Angle(this.transform.position - chaser.position, this.transform.forward);
		}
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