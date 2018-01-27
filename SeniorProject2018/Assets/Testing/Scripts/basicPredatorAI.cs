using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	public class basicPredatorAI : MonoBehaviour {

		// Variable Declarations
		public UnityEngine.AI.NavMeshAgent agent;
		public ThirdPersonCharacter character;

		public enum State{
			PATROL,
			CHASE
		}

		public State state;
		private bool alive;
		public OffMeshLinkMoveMethod method = OffMeshLinkMoveMethod.Parabola;
		public float jumpHeight = 2.0f;
		public float jumpDuration = 0.5f;

		// Variables for PATROL
		public GameObject[] waypoints;
		private int waypointINDEX = 0;
		public float patrolSpeed = 0.5f;


		// Variables for CHASE
		public float chaseSpeed = 1.0f;
		public GameObject target;


		// Use this for initialization
		void Start () {
			agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
			character = GetComponent<ThirdPersonCharacter>();

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
				if (agent.isOnOffMeshLink)
				{
					character.Move(Vector3.zero,false,true);
					if (method == OffMeshLinkMoveMethod.NormalSpeed)
						yield return StartCoroutine(NormalSpeed(agent));
					else if (method == OffMeshLinkMoveMethod.Parabola)
						yield return StartCoroutine(Parabola(agent, jumpHeight, jumpDuration));
					agent.CompleteOffMeshLink();
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
			else if (Vector3.Distance(this.transform.position,waypoints[waypointINDEX].transform.position)<=2)
			{
				waypointINDEX += 1;
				if(waypointINDEX>=waypoints.Length)
				{
					waypointINDEX = 0;
				}
			}
			else
			{
				character.Move(Vector3.zero,false,false);
			}
		}

		void Chase()
		{
			agent.speed = chaseSpeed;
			agent.SetDestination(target.transform.position);
			character.Move(agent.desiredVelocity,false,false);
		}

		void OnTriggerEnter (Collider coll)
		{
			if(coll.tag == "Prey")
			{
				state = basicPredatorAI.State.CHASE;
				target = coll.gameObject;
			}
		}

		public enum OffMeshLinkMoveMethod
		{
			Teleport,
			NormalSpeed,
			Parabola
		}

		IEnumerator NormalSpeed(UnityEngine.AI.NavMeshAgent agent)
		{
			UnityEngine.AI.OffMeshLinkData data = agent.currentOffMeshLinkData;
			Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
			while (agent.transform.position != endPos)
			{
				agent.transform.position = Vector3.MoveTowards(agent.transform.position, endPos, agent.speed * Time.deltaTime);
				yield return null;
			}
		}
		IEnumerator Parabola (UnityEngine.AI.NavMeshAgent agent, float jumpHeight, float jumpDuration)
		{
			UnityEngine.AI.OffMeshLinkData data = agent.currentOffMeshLinkData;
			Vector3 startPos = agent.transform.position;
			Vector3 endPos = data.endPos + Vector3.up*agent.baseOffset;
			float normalizedTime = 0.0f;
			while (normalizedTime < 1.0f)
			{
				float yOffset = jumpHeight * 4.0f*(normalizedTime - normalizedTime*normalizedTime);
				agent.transform.position = Vector3.Lerp (startPos, endPos, normalizedTime) + yOffset * Vector3.up;
				normalizedTime += Time.deltaTime / jumpDuration;
				yield return null;
			}
		}
		// Update is called once per frame
		// void Update () {
			
		// }
	}	
}