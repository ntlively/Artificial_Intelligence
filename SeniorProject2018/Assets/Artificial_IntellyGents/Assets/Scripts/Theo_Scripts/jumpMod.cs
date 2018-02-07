using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	public class jumpMod : MonoBehaviour {

		// Variable Declarations
		public GameObject predator;
		public UnityEngine.AI.NavMeshAgent agent;
		public ThirdPersonCharacter character;

		private bool alive;
		public OffMeshLinkMoveMethod method = OffMeshLinkMoveMethod.Parabola;
		public float jumpHeight = 2.0f;
		public float jumpDuration = 0.5f;

		void Awake(){
			predator = GameObject.Find("Predator");
			agent = predator.GetComponent<UnityEngine.AI.NavMeshAgent>();
			character = predator.GetComponent<ThirdPersonCharacter>();
		}

		// Use this for initialization
		void Start () {

			agent.updatePosition = true;
			agent.updateRotation = false;
			alive = true;

			//start finite state machine (JumpMod)
			StartCoroutine("JumpMod");
			
		}

		IEnumerator JumpMod()
		{
			while(alive)
			{
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
	}	
}