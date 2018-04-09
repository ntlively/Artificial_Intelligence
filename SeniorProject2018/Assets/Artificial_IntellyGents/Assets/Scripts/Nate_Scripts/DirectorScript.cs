using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNet.NeuralNet;

// BUG REPORT
// 1) ON TRIGGER ENTER DOES NOT TRIGGER IF PLAYER SPAWNS INSIDE SIGHT RADIUS
// 2) NO CASE FOR LEAVING SPHERE COLLIDER TRIGGER ZONE
// 3) CHASE SPEED IN SCRIPT NOT SETTING PROPERLY
// 4) DON'T NEED SPHERE COLLIDER, BUT WHERE TO PUT VISION CHECK BESIDES ON TRIGGER ENTER



namespace UnityStandardAssets.Characters.ThirdPerson
{
	public class DirectorScript : MonoBehaviour {
		
		//variables
		public NeuralNetwork 				neuralNet;
		public GameObject 					actor;
		public Vision 						visionScript;
		public Hearing 						hearingScript;
		void Awake(){
			// init neural net
			neuralNet = new NeuralNetwork(0.9, new int[] { 2, 4, 6 });
			actor 			= this.gameObject;
			visionScript	= actor.GetComponent<Vision>();
			hearingScript	= actor.GetComponent<Hearing>();

			// train neural net
			List<double> ins = new List<double>();
			List<double> ots = new List<double>();

			// Switch to CHASE when see but don't hear
			for(int i=0;i<visionScript.viewRadius*10;i++)
			{
				ins.Clear();
				double visTemp = 0.0 + ((double)i)/10.0;
				ins.Add(visTemp);
				ins.Add((double)0.0);

				ots.Clear();
				ots.Add((double)0.0);
				ots.Add((double)0.99);
				ots.Add((double)0.0);
				ots.Add((double)0.0);
				ots.Add((double)0.0);
				ots.Add((double)0.0);

				for(int j = 0; j < 100; j++){
					neuralNet.Train(ins, ots);
				}
			}
			// Switch to SNEAK when hear but don't see
			for(int i=0;i<100;i++)
			{
				ins.Clear();
				double soundTemp = 10 + ((double)i)/10.0;
				ins.Add((double)0.0);
				ins.Add(soundTemp);

				ots.Clear();
				ots.Add((double)0.0);
				ots.Add((double)0.0);
				ots.Add((double)0.99);
				ots.Add((double)0.0);
				ots.Add((double)0.0);
				ots.Add((double)0.0);

				for(int j = 0; j < 100; j++){
					neuralNet.Train(ins, ots);
				}
			}
			// Switch to PATROL
			ins.Clear();
            ins.Add((double)0.0);
            ins.Add((double)0.0);

			ots.Clear();
			ots.Add((double)0.99);
			ots.Add((double)0.0);
			ots.Add((double)0.0);
			ots.Add((double)0.0);
			ots.Add((double)0.0);
			ots.Add((double)0.0);

            for(int i = 0; i < 100; i++){
        		neuralNet.Train(ins, ots);
			}



			// 			// Switch to TALK when hear but don't see a friendly
			// for(int i=0;i<100;i++)
			// {
			// 	ins.Clear();
			// 	double soundTemp = 995.0 + ((double)i)/10.0;
			// 	ins.Add((double)0.0);
			// 	ins.Add(soundTemp);

			// 	ots.Clear();
			// 	ots.Add((double)0.0);
			// 	ots.Add((double)0.0);
			// 	ots.Add((double)0.0);
			// 	ots.Add((double)0.0);
			// 	ots.Add((double)0.99);
			// 	ots.Add((double)0.0);

			// 	for(int j = 0; j < 100; j++){
			// 		neuralNet.Train(ins, ots);
			// 	}
			// }
			// // Switch to TALK when you see a friendly
			// for(int i=0;i<100;i++)
			// {
			// 	ins.Clear();
			// 	double visTemp = 995.0 + ((double)i)/10.0;
			// 	ins.Add(visTemp);
			// 	ins.Add((double)0.0);

			// 	ots.Clear();
			// 	ots.Add((double)0.0);
			// 	ots.Add((double)0.0);
			// 	ots.Add((double)0.0);
			// 	ots.Add((double)0.0);
			// 	ots.Add((double)0.99);
			// 	ots.Add((double)0.0);

			// 	for(int j = 0; j < 100; j++){
			// 		neuralNet.Train(ins, ots);
			// 	}
			// }
		//
		}
	}	
}