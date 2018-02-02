using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
using UnityEngine.AI;

namespace UnityStandardAssets.Characters.ThirdPerson{

	public class preyAI : MonoBehaviour {

		public NavMeshAgent agent;
		public ThirdPersonCharacter character;

		public enum State{
			PATROL,
			CHASE
		}

		public State state; //current state.
		private bool alive; //whether the AI lives.
		
		//Variable patrolling
		public GameObject[] waypoints;
		private int currentWaypoint = 0;
		public float patrolSpeed = 0.5f;

		//Variables for Chasing
		public float chaseSpeed = 1.0f;
		public GameObject target;

		// Use this for initialization
		void Start ()
		{
			agent = GetComponent<NavMeshAgent>();
			character = GetComponent<ThirdPersonCharacter>();

			agent.updatePosition = true;
			agent.updateRotation = false;

			state = preyAI.State.PATROL;
			alive = true;

			//Get all way points for the player.
			waypoints = GameObject.FindGameObjectsWithTag("Finish");

			//Start FSM Finite state machine
			StartCoroutine("FSM");
		}

	IEnumerator FSM()
	{
		while (alive)
		{
			switch(state)
			{
				case State.PATROL:
					Patrol ();
					break;
				case State.CHASE:
					Chase();
					break;

			}

			yield return null;
		}
	}

	void Patrol()
	{
		//Have the character move to a random way point based on errors.
		agent.speed = patrolSpeed;

		//If player is within the range of a random way point, go to it.
		if(Vector3.Distance(this.transform.position, waypoints[currentWaypoint].transform.position )>= 2)
		{
			agent.SetDestination(waypoints[currentWaypoint].transform.position);
			character.Move(agent.desiredVelocity, false, false);
		}
		//If the player is close to way point, set the next way point.
		else if (Vector3.Distance(this.transform.position, waypoints[currentWaypoint].transform.position )<= 2)
		{
			currentWaypoint = UnityEngine.Random.Range(0, waypoints.Length);
		}
		//If there are no way points close by.
		else
		{
			character.Move(Vector3.zero, false, false);
		}

	}

	void Chase()
	{
		agent.speed = chaseSpeed;
		agent.SetDestination(target.transform.position);
		character.Move(agent.desiredVelocity, false,false);
	}

	void OnTriggerEnter(Collider coll)
	{
		if (coll.tag == "Enemy")
		{
			state = preyAI.State.CHASE;
			target = coll.gameObject;
		}


	} 
		
		// Update is called once per frame
		//void Update () {
			
		//}
	}
}