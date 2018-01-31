using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace UnityStandardAssets.Characters.ThirdPerson{

	public class DecibelTracker : MonoBehaviour {

		// Use this for initialization
		
		public basicAI character;

		//Sound set up 
		public float currentDecibel = 0.5f;

		//Constant decibel levels for each Predator State
		public const float PatrolDecibel = 4.0f;
		public const float RunningDecibel = 6.0f;
		public const float IdleDecibel = 2.0f;
		public const float SneakDecibel = 0.5f;


		//Set Decibel level depending on current state
		public void setCurrentDecibel(basicAI chara)
		{
			//Set current decibel level
			if(chara.state == basicAI.State.PATROL)
			{
				currentDecibel = PatrolDecibel;
			}
			else if(chara.state == basicAI.State.CHASE)
			{
			  currentDecibel = RunningDecibel;
			}

			//Set sphere collider size
			//return currentDecibel;

		}
		//Get current decidel level
		public float getCurrentDecibel()
		{
			return currentDecibel;
		}

		void Start () 
		{	
			
		}
		// Update is called once per frame
		/*void Update () {
			
		}*/
	}
}