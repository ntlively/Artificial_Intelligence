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

		public enum State{
			PATROL,
			FLEE
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


		// Variables for FLEE
		public float fleeSpeed = 1.0f;
		private int fleepointINDEX = 0;
		public GameObject[] fleepoints;
		private float fleeAngle = 0.0f;

		void Awake(){
			prey = GameObject.Find("Prey");
			agent = prey.GetComponent<UnityEngine.AI.NavMeshAgent>();
			character = prey.GetComponent<ThirdPersonCharacter>();
			visionScript = prey.GetComponent<Vision>();
			hearingScript = prey.GetComponent<Hearing>();
		}

		// Use this for initialization
		void Start () {
			//agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
			//character = GetComponent<ThirdPersonCharacter>();

			agent.updatePosition = true;
			agent.updateRotation = false;

			state = basicPreyAI.State.PATROL;
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
					case State.FLEE:
						Flee();
						break;
				}
				if (agent.isOnOffMeshLink)
				{
					character.Move(agent.desiredVelocity,false,true);
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

		void Flee()
		{
			agent.speed = fleeSpeed;

			if(Vector3.Distance(this.transform.position,fleepoints[fleepointINDEX].transform.position)> 5)
			{
				agent.SetDestination(fleepoints[fleepointINDEX].transform.position);
				character.Move(agent.desiredVelocity,false,false); //velocity, crouch, jump
			}
			else if (Vector3.Distance(this.transform.position,fleepoints[fleepointINDEX].transform.position)<=5)
			{
				fleepointINDEX += 1;
				if(fleepointINDEX>=fleepoints.Length)
				{
					fleepointINDEX = 0;
				}
			}
			else
			{
				character.Move(Vector3.zero,false,false);
			}
		}

		void OnTriggerEnter (Collider coll)
		{
			if(coll.tag == "Predator")
			{
				state = basicPreyAI.State.FLEE;
				fleeAngle = Vector3.Angle(this.transform.position - coll.gameObject.transform.position, this.transform.forward);
				for(int i = 0; i < fleepoints.Length; i++)
				{
					if((fleeAngle > (Vector3.Angle(fleepoints[i].transform.position - this.transform.position,this.transform.forward))) &&
						Vector3.Distance(this.transform.position,fleepoints[i].transform.position) > 10)
					{
						fleepointINDEX = i;
					}
				}
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