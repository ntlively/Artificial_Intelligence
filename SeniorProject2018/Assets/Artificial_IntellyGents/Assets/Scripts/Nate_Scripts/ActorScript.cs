using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayesianUtils;
using NeuralNet.NeuralNet;
// BUG REPORT
// 1) ON TRIGGER ENTER DOES NOT TRIGGER IF PLAYER SPAWNS INSIDE SIGHT RADIUS
// 2) NO CASE FOR LEAVING SPHERE COLLIDER TRIGGER ZONE
// 3) CHASE SPEED IN SCRIPT NOT SETTING PROPERLY
// 4) DON'T NEED SPHERE COLLIDER, BUT WHERE TO PUT VISION CHECK BESIDES ON TRIGGER ENTER
// 5) COMPILE ERROR, NOT SURE WHY


namespace UnityStandardAssets.Characters.ThirdPerson
{
	public class ActorScript : MonoBehaviour {

		// Variable Declarations
		//public GameObject predator;
		public UnityEngine.AI.NavMeshAgent agent;
		public ThirdPersonCharacter character;

		public DataManager manager;
		public Vision visionScript;
		public Hearing hearingScript;

		// public enum State{
		// 	PATROL,
		// 	CHASE,
		// 	SNEAK,
		// 	WAIT,
		// 	TALK,
		// 	THINK
		// }

		// public State state;
		// public bool alive;

		public BayesNet  testNet;

		// Variables for PATROL
		public List<GameObject> waypoints = new List<GameObject>();
		private int waypointINDEX = 0;
		public float patrolSpeed = 0.5f;


		// Variables for CHASE
		public float chaseSpeed = 0.1f;
		public Transform target;
		public GameObject actor;
		void Awake(){

			// List<BayesNode> nodeList = new List<BayesNode>();
			// // // // // // //
			// Dictionary<string, float> predActionStates = new Dictionary<string, float>();
			// BayesNode predActions = new BayesNode(predActionStates);
			// nodeList.Add(predActions);
			// // // // // // //
			// List<BayesNode> children = new List<BayesNode>();
            // Dictionary<string, float> states = new Dictionary<string, float>();
			// children.Add(predActions);
			// BayesNode preyActions = new BayesNode(children,states);
			// nodeList.Add(preyActions);
			// // // // // // //
			// children = new List<BayesNode>();
            // states = new Dictionary<string, float>();
			// children.Add(predActions);
			// children.Add(preyActions);
			// BayesNode environment = new BayesNode(children,states);
			// nodeList.Add(environment);
			// // // // // // //			
			// testNet = new BayesNet(nodeList);
			// test.Add(new BayesNode());
			// test.Add(new BayesNode());
			// testNet = new BayesNet(test);

			//testNet.AddNode(testNet._nodes[0], new BayesNode());

			// Debug.Log(testNet._nodes[0]._children.Count);
			// Debug.Log(testNet._nodes.Count);

			actor = this.gameObject;
			agent = actor.GetComponent<UnityEngine.AI.NavMeshAgent>();
			//Debug.Log(agent);
			character = actor.GetComponent<ThirdPersonCharacter>();

			manager = actor.GetComponent<DataManager>();
			//Debug.Log(character);
			visionScript = actor.GetComponent<Vision>();
			//Debug.Log(visionScript);
			hearingScript = actor.GetComponent<Hearing>();

			//Debug.Log(testNet.name);

			
			NeuralNetwork nn = new NeuralNetwork(0.9, new int[] { 2, 4, 6 });

			// List<double> ins = new List<double>();
            // ins.Add(hearingScript.hearableTargets[0].decibel);
            // ins.Add(1);

			// List<double> ots = new List<double>();
			// ots.Add(0);
			// ots.Add(0);

            // for(int i = 0; i <100000; i++)
        	// 	nn.Train(ins, ots);
		}



		// Use this for initialization
		void Start () {
			
			// predator = GameObject.Find("Predator");
			// agent = predator.GetComponent<UnityEngine.AI.NavMeshAgent>();
			// //Debug.Log(agent);
			// character = predator.GetComponent<ThirdPersonCharacter>();
			// //Debug.Log(character);
			// visionScript = predator.GetComponent<Vision>();
			// //Debug.Log(visionScript);
			// hearingScript = predator.GetComponent<Hearing>();
			//Debug.Log(hearingScript);
			agent.updatePosition = true;
			agent.updateRotation = false;


			manager.state = DataManager.State.PATROL;
			manager.alive = true;

			//start finite state machine (FSM)
			StartCoroutine("Predator");
			
		}
		// bool started = false;
		// void LateUpdate(){
		// 	if(!started){
		// 		StartCoroutine("Predator");
		// 		started = true;
		// 	}
		// }

		IEnumerator Predator()
		{
			while(manager.alive)
			{
				switch(manager.state)
				{
					case DataManager.State.PATROL:
						Patrol();
						break;
					case DataManager.State.CHASE:
						Chase();
						break;
					case DataManager.State.THINK:
						Think();
						break;
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
				//Debug.Log("agent: " + agent.desiredVelocity);
				character.Move(agent.desiredVelocity,false,false); //velocity, crouch, jump
			}
			else if (Vector3.Distance(this.transform.position,waypoints[waypointINDEX].transform.position)<2)
			{
				waypointINDEX += 1;
				if(waypointINDEX >= waypoints.Count)
				{
					waypointINDEX = 0;
				}
			}
			if (visionScript.visibleTargets.Count >0)
			{
				int index = 0;
				foreach (Vision.VisionInfo visibleTarget in visionScript.visibleTargets) {
					if(visibleTarget.target.CompareTag("Player")){
						//Debug.Log("WE GOT ONE");
						target = visibleTarget.target;
						manager.SwapState(DataManager.State.CHASE,visibleTarget,new Hearing.SoundInfo(null,0.0f));
						//manager.state = DataManager.State.CHASE;
					}
					index++;
				}
			}
			// else
			// {
			// 	character.Move(Vector3.zero,false,false);
			// }
		}

		void Chase()
		{
			agent.speed = chaseSpeed;
			agent.SetDestination(target.position);
			character.Move(agent.desiredVelocity,false,false);

			float dstToTarget = Vector3.Distance (transform.position, target.position);
			if(dstToTarget<2)
			{
				target.gameObject.GetComponent<DataManager>().alive = false;
			}

			if(!target.gameObject.GetComponent<DataManager>().alive)
			{
				manager.state = DataManager.State.THINK;
			}
		}
		
		void Think()
		{
			//reinforce good choices from stack in data manager.  retrain neural net
			//agent.SetDestination(target.position);
			//character.Move(agent.desiredVelocity,false,false);
		}

	}	
}