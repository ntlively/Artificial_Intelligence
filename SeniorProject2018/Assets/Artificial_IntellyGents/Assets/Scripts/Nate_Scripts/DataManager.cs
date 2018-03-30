using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour {



	public bool alive;
	public State state;
	public enum State{
		PATROL,
		CHASE,
		SNEAK,
		WAIT,
		TALK,
		THINK
	}


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
	// stack for reward tracking
	public Stack<RewardTrackingInfo> rewardTracking;
	public Stack<List<double>> netTracking;


//~////////////////////////////////////////////////////////~//
	// Use this for initialization
	void Awake () {
		alive = true;
		state = DataManager.State.WAIT;
		rewardTracking = new Stack<RewardTrackingInfo>();
		netTracking = new  Stack<List<double>>();
	}

	// public void SwapState(DataManager.State _state,Vision.VisionInfo _visionData,Hearing.SoundInfo _hearingData)
	// {
	// 	state = _state;
	// 	rewardTracking.Push(new RewardTrackingInfo(state,_visionData,_hearingData));
	// }
}
