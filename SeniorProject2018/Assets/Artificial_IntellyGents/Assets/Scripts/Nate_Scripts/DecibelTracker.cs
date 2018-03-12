using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// LOG
// EDITED DECIBEL TRACKER TO WORK OFF THIRD PERSON CHARACTER SO THAT IT CAN WORK FOR GENERIC TYPE.  NEED TO UPDATE ACTOR SCRIPTS AND SUCH TO EDIT THIRDPERSON STATE ACCORDINGLY

namespace UnityStandardAssets.Characters.ThirdPerson{

	public class DecibelTracker : MonoBehaviour {

		// Use this for initialization

		public DataManager manager;
		//Sound set up 
		public float currentDecibel = 0.5f;

		//Constant decibel levels for each Predator State
		public const float PatrolDecibel = 4.0f;
		public const float RunningDecibel = 6.0f;
		public const float IdleDecibel = 2.0f;
		public const float SneakDecibel = 0.5f;


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
				//Set current decibel level
				if(manager.state == DataManager.State.PATROL)
				{
					currentDecibel = PatrolDecibel;
				}
				else if(manager.state == DataManager.State.CHASE)
				{
					currentDecibel = RunningDecibel;
				}
				else if(manager.state == DataManager.State.SNEAK)
				{
					currentDecibel = SneakDecibel;
				}
				else if(manager.state == DataManager.State.WAIT || manager.state == DataManager.State.TALK || manager.state == DataManager.State.THINK )
				{
					currentDecibel = IdleDecibel;
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
		
		// Update is called once per frame
		/*void Update () {
			
		}*/
	}
}