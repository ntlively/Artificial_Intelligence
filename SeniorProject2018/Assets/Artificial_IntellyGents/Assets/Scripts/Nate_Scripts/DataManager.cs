using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour {


	// booleans
	public bool alive;
	public bool shout = false;
	//public bool predatorHeard = false;
	public bool needUpdate = false;
	
	// floats
	public float patrolSpeed = 0.5f;
	public float chaseSpeed = 0.7f;
	public float sneakSpeed = 0.1f;
	public float fleeSpeed = 1.0f;

	//Timers
	public float waitTimer = 5.0f; 
	public float patrolTimer = 10.0f;
	public float talkTimer = 5.0f; 
	public float updateTimer = 5.0f; 
	
	// States
	public State state = DataManager.State.WAIT;

	public enum State{
		PATROL,
		CHASE,
		SNEAK,
		WAIT,
		TALK,
		THINK,
		SEARCH,
		HIDE,
		FLEE,
		DEAD
	}

	// Stacks
	public Stack<RewardTrackingInfo> rewardTracking;
	public Stack<List<double>> netTracking;

	public struct RewardTrackingInfo{
		public State state;
		public Vision.VisionInfo visionData;
		public Hearing.SoundInfo hearingData;

		public RewardTrackingInfo(State _state, Vision.VisionInfo _visionData,Hearing.SoundInfo _hearingData ){
			state = _state;
			visionData = _visionData;
			hearingData = _hearingData;
		}
	}

	// Use this for initialization
	void Awake () {
		alive = true;
		//state = DataManager.State.WAIT;
		rewardTracking = new Stack<RewardTrackingInfo>();
		netTracking = new  Stack<List<double>>();
	}
}
