using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace UnityStandardAssets.Characters.ThirdPerson{

	public class DecibelTracker : MonoBehaviour {

		// Use this for initialization
		
		public bev_basicAI character;
		public ThirdPersonCharacter agent;

		//Sound set up 
		public float currentDecibel = 0.5f;

		//Constant decibel levels for each Predator State
		public const float PatrolDecibel = 10.0f;
		public const float RunningDecibel = 20.0f;
		public const float IdleDecibel = 5.0f;
		public const float SneakDecibel = 0.5f;


		void Awake () 
		{	
			character = GetComponent<bev_basicAI>();
			agent = GetComponent<ThirdPersonCharacter>();

		}

		void Start()
		{
			StartCoroutine("setCurrentDecibel");
		}

		//Set Decibel level depending on current state
		IEnumerator setCurrentDecibel()
		{
			while (character.alive)
			{
				//Set current decibel level
				if(character.state == bev_basicAI.State.PATROL)
				{
					currentDecibel = PatrolDecibel;
				}
				else if(character.state == bev_basicAI.State.CHASE)
				{
					currentDecibel = RunningDecibel;
				}

				character.GetComponent<SphereCollider>().radius = currentDecibel;

				yield return null;
			}

		
		}
		//Get current decidel level
		public float getCurrentDecibel()
		{
			return currentDecibel;
		}
		
		// Update is called once per frame
		/*void Update () {
			
		}*/
	}
}