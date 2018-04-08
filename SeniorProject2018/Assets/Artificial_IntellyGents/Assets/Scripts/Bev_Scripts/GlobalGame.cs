﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalGame : MonoBehaviour {


	//UI links
	public GameObject canvas;

	//Rounds - 3
	public int maxRounds = 3;
	public int currentRound;

	//Timer
	public bool startRound;
	public float currentTime;
	public float maxTime = 60.0f;
	public string minutes;
	public string seconds;
	public Text timer;

	//Tracking game info
	public int preyCaught;
	public Text preyCaughtDisplay;
	public string preyCaughtString;

	// Use this for initialization
	void Start () 
	{
		startRound = true;
		currentTime = maxTime;
		currentRound = 1;
		minutes =  Mathf.Floor(currentTime / 60).ToString("00");
		seconds = Mathf.Floor(currentTime % 60).ToString("00");
		canvas =  GameObject.Find("Canvas");
		timer = canvas.transform.FindChild("Text").GetComponent<Text>();

		//Predator functions
		preyCaught = 0;
		preyCaughtDisplay = canvas.transform.FindChild("Text (1)").GetComponent<Text>();

		//Prey Functions
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(startRound)
		{

			//Timing info
			minutes =  Mathf.Floor(currentTime / 60).ToString("00");
			seconds = Mathf.Floor(currentTime % 60).ToString("00");
			timer.text = "Round "+ currentRound +"\n" + minutes +" : "+seconds;
			currentTime -= Time.deltaTime;

			//Prey caught info
			preyCaughtString = preyCaught.ToString();
			preyCaughtDisplay.text = "Prey Caught: \n"+ preyCaught.ToString();

			if(currentTime < 0)
			{
				startRound = false;
				currentTime = maxTime;
				currentRound++;
				Debug.Log("Round End");

				//Save all game data for 
				//GameOver();
			}

		}
	}

	public void preyCaughtUpdate()
	{
		preyCaught++;
	}

}
