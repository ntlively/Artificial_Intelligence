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

		//public BayesNet  testNet;
		// Variables for PATROL
		public List<GameObject> waypoints = new List<GameObject>();
		private int waypointINDEX = 0;
		public float patrolSpeed = 0.5f;


		// Variables for CHASE
		public float chaseSpeed = 0.1f;
		public Transform target;
		public GameObject actor;


		// Variables for THINK
		NeuralNetwork neuralNet;

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

			actor 			= this.gameObject;
			agent 			= actor.GetComponent<UnityEngine.AI.NavMeshAgent>();
			character 		= actor.GetComponent<ThirdPersonCharacter>();
			manager 		= actor.GetComponent<DataManager>();
			visionScript	= actor.GetComponent<Vision>();
			hearingScript	= actor.GetComponent<Hearing>();

			neuralNet = new NeuralNetwork(0.9, new int[] { 2, 4, 6 });

			List<double> ins = new List<double>();
			List<double> ots = new List<double>();
			for(int i=0;i<10;i++)
			{
				ins.Clear();
				double visTemp = 5.0 + ((double)i)/10.0;
				ins.Add(visTemp);
				ins.Add((double)0.0);

				ots.Clear();
				ots.Add((double)0.0);
				ots.Add((double)0.9);
				ots.Add((double)0.0);
				ots.Add((double)0.0);
				ots.Add((double)0.0);
				ots.Add((double)0.0);

				for(int j = 0; j < 10000; j++){
					neuralNet.Train(ins, ots);
				}
			}

			ins.Clear();
            ins.Add((double)0.0);
            ins.Add((double)0.0);

			ots.Clear();
			ots.Add((double)0.9);
			ots.Add((double)0.0);
			ots.Add((double)0.0);
			ots.Add((double)0.0);
			ots.Add((double)0.0);
			ots.Add((double)0.0);

            for(int i = 0; i < 10000; i++){
        		neuralNet.Train(ins, ots);
			}

			// for(int l=0;l<3;l++){
			// List<double> ins = new List<double>();
            // ins.Add((double)6.0);
            // ins.Add((double)0.0);

			// List<double> ots = new List<double>();
			// ots.Add((double)0.0);
			// ots.Add((double)0.9);
			// ots.Add((double)0.0);
			// ots.Add((double)0.0);
			// ots.Add((double)0.0);
			// ots.Add((double)0.0);

            // for(int i = 0; i < 100000; i++){
        	// 	neuralNet.Train(ins, ots);
			// }
			// ins.Clear();
            // ins.Add((double)5.0);
            // ins.Add((double)0.0);

			// ots.Clear();
			// ots.Add((double)0.0);
			// ots.Add((double)0.9);
			// ots.Add((double)0.0);
			// ots.Add((double)0.0);
			// ots.Add((double)0.0);
			// ots.Add((double)0.0);

            // for(int i = 0; i < 100000; i++){
        	// 	neuralNet.Train(ins, ots);
			// }

			// ins.Clear();
            // ins.Add((double)0.0);
            // ins.Add((double)0.0);

			// ots.Clear();
			// ots.Add((double)0.9);
			// ots.Add((double)0.0);
			// ots.Add((double)0.0);
			// ots.Add((double)0.0);
			// ots.Add((double)0.0);
			// ots.Add((double)0.0);

            // for(int i = 0; i < 100000; i++){
        	// 	neuralNet.Train(ins, ots);
			// }

			// }
		}



		// Use this for initialization
		void Start () {
			agent.updatePosition = true;
			agent.updateRotation = false;

			manager.state = DataManager.State.THINK;
			manager.alive = true;

			//start finite state machine (FSM)
			StartCoroutine("Predator");
			
		}

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
						target = visibleTarget.target;
						//manager.SwapState(DataManager.State.CHASE,visibleTarget,new Hearing.SoundInfo(null,0.0f));
						manager.state = DataManager.State.THINK;
					}
					index++;
				}
			}
			
		}

		void Chase()
		{
			agent.speed = chaseSpeed;
			agent.SetDestination(target.position);
			character.Move(agent.desiredVelocity,false,false);

			float dstToTarget = Vector3.Distance (transform.position, target.position);
			if(dstToTarget<0.8f)
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
			
			agent.SetDestination(character.transform.position);
			character.Move(Vector3.zero,false,false);
			//if(manager.rewardTracking.Count == 0)
			if(manager.netTracking.Count == 0)
			{

				Debug.Log("State Choice, Run the Net!!!");

				List<double> sensors = new List<double>();


				Vision.VisionInfo bestVisibleTarget = new Vision.VisionInfo();
				float tempDist = Mathf.Infinity;
				// Vision Sensors
				foreach (Vision.VisionInfo visibleTarget in visionScript.visibleTargets)
				{
					if(visibleTarget.distance < tempDist)
					{
						bestVisibleTarget = visibleTarget;
						tempDist = visibleTarget.distance;
					}
				}

				Hearing.SoundInfo bestHearableTarget = new Hearing.SoundInfo();
				float tempDeci = Mathf.NegativeInfinity;
				// Hearing Sensors
				foreach (Hearing.SoundInfo hearableTarget in hearingScript.hearableTargets)
				{
					if(hearableTarget.decibel > tempDeci)
					{
						bestHearableTarget = hearableTarget;
						tempDeci = hearableTarget.decibel;
					}
				}

				Debug.Log(bestVisibleTarget.distance);
				Debug.Log(bestHearableTarget.decibel);
				double distance = bestVisibleTarget.distance;
				double decibel = bestHearableTarget.decibel;
				sensors.Add(distance); //closest
				sensors.Add(decibel); //loudest

				double [] results = neuralNet.Run(sensors);
				List<double> netChoice = new List<double>(results);

				int index = 0;
				int tempState = 0;
				double tempChance = 0.0;
				foreach (double stateChance in netChoice)
				{
					Debug.Log("net state chance:"+stateChance);
					if(stateChance > tempChance)
					{
						tempState = index;
						tempChance = stateChance;
					}
					index++;
				}
				Debug.Log("\n\n\n");
				manager.netTracking.Push(netChoice);
				//manager.rewardTracking.Push(new DataManager.RewardTrackingInfo(DataManager.State.WAIT, bestVisibleTarget,bestHearableTarget));

				switch(tempState)
				{
					case 0:
						manager.state = DataManager.State.PATROL;
						Debug.Log("NO MORE THINK, ONLY PATROL NOW");
						break;
					case 1:
						manager.state = DataManager.State.CHASE;
						break;
					case 5:
						manager.state = DataManager.State.THINK;
						break;
				}
				//yield return null;

			}
			else
			{
				Debug.Log("Check for delta now PLZ");
				//train???

				List<double> sensors = new List<double>();


				Vision.VisionInfo bestVisibleTarget = new Vision.VisionInfo();
				float tempDist = Mathf.Infinity;
				// Vision Sensors
				foreach (Vision.VisionInfo visibleTarget in visionScript.visibleTargets)
				{
					if(visibleTarget.distance < tempDist)
					{
						bestVisibleTarget = visibleTarget;
						tempDist = visibleTarget.distance;
					}
				}

				Hearing.SoundInfo bestHearableTarget = new Hearing.SoundInfo();
				float tempDeci = Mathf.NegativeInfinity;
				// Hearing Sensors
				foreach (Hearing.SoundInfo hearableTarget in hearingScript.hearableTargets)
				{
					if(hearableTarget.decibel > tempDeci)
					{
						bestHearableTarget = hearableTarget;
						tempDeci = hearableTarget.decibel;
					}
				}

				Debug.Log(bestVisibleTarget.distance);
				Debug.Log(bestHearableTarget.decibel);
				double distance = bestVisibleTarget.distance;
				double decibel = bestHearableTarget.decibel;
				sensors.Add(distance); //closest
				sensors.Add(decibel); //loudest

				double [] results = neuralNet.Run(sensors);
				List<double> netChoice = new List<double>(results);

				int index = 0;
				int tempState = 0;
				double tempChance = 0.0;
				foreach (double stateChance in netChoice)
				{
					Debug.Log("net state chance:"+stateChance);
					double delta = stateChance - manager.netTracking.Peek()[index];
					Debug.Log("\n net state delta:"+delta);
					Debug.Log("\n");
					if(delta > tempChance)
					{
						tempState = index;
						tempChance = stateChance;
					}
					index++;
				}
				Debug.Log("\n\n\n");
				manager.netTracking.Pop();
				manager.netTracking.Push(netChoice);

				switch(tempState)
				{
					case 0:
						manager.state = DataManager.State.PATROL;
						Debug.Log("NO MORE THINK, ONLY PATROL NOW");
						break;
					case 1:
						manager.state = DataManager.State.CHASE;
						break;
					case 5:
						manager.state = DataManager.State.THINK;
						break;
				}
			}	
			//Debug.Log("THINKING REALLY HARD HMMMMMM");
			//reinforce good choices from stack in data manager.  retrain neural net
			//agent.SetDestination(target.position);
			//character.Move(agent.desiredVelocity,false,false);
		}

	}	
}