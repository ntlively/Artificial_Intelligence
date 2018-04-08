using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// LOG
// EDITED DECIBEL TRACKER TO WORK OFF DATAMANAGER SO THAT IT CAN WORK FOR GENERIC TYPE

namespace UnityStandardAssets.Characters.ThirdPerson{

	public class DecibelTracker : MonoBehaviour {

		// Use this for initialization

		public DataManager manager;
		//Sound set up 
		public float currentDecibel = 1.0f;

		//Constant decibel levels for each Predator State
		public  float RunningDecibel = 7.0f;
		public  float PatrolDecibel = 5.0f;
		public  float IdleDecibel = 3.0f;
		public  float SneakDecibel = 1.0f;
		public  float HideDecibel = 1.0f;
		public  float DeadDecibel = 0.01f;


		void Awake () 
		{	

			manager = GetComponent<DataManager>();
			currentDecibel = 2.0f;

		}

		void Start()
		{
			StartCoroutine("setCurrentDecibel");
		}

		//Set Decibel level depending on current state
		IEnumerator setCurrentDecibel()
		{
			while (manager.alive)
			{
				switch(manager.state)
				{
					case DataManager.State.PATROL:
						currentDecibel = PatrolDecibel;
						break;
					case DataManager.State.CHASE:
						currentDecibel = RunningDecibel;
						break;
					case DataManager.State.SNEAK:
						currentDecibel = SneakDecibel;
						break;
					case DataManager.State.THINK:
						currentDecibel = IdleDecibel;
						break;
					case DataManager.State.TALK:
						currentDecibel = IdleDecibel;
						break;
					case DataManager.State.WAIT:
						currentDecibel = IdleDecibel;
						break;
					case DataManager.State.DEAD:
						currentDecibel = DeadDecibel;
						break;
					case DataManager.State.HIDE:
						currentDecibel = HideDecibel;
						break;
					case DataManager.State.FLEE:
						currentDecibel = RunningDecibel;
						break;
				}

				manager.gameObject.GetComponent<SphereCollider>().radius = currentDecibel;

				yield return null;
			}

		
		}
		//Get current decidel level
		public float getCurrentDecibel()
		{
			return currentDecibel;
		}
	}
}