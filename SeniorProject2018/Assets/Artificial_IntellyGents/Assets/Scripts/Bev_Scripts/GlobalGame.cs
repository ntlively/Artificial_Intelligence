using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
	GameObject[] respawnPoints;
	GameObject[] predList;
	GameObject[] preyList;
	public float resetTimer = 5.0f; 
	public int preyCount;
	public int preyTracker;

	//predator
	public int preyCaught;
	public Text preyCaughtDisplay;

	//prey
	public int predatorEvaded;
	public Text predatorEvadedDisplay;


	// Use this for initialization
	void Start () 
	{
		startRound = true;
		currentTime = maxTime;
		currentRound = 1;
		minutes =  Mathf.Floor(currentTime / 60).ToString("00");
		seconds = Mathf.Floor(currentTime % 60).ToString("00");
		canvas =  GameObject.Find("Canvas");
		timer = canvas.transform.Find("Text").GetComponent<Text>();

		//Respawning
		respawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

		//Prey Captured end check 
		predList = GameObject.FindGameObjectsWithTag("Predator");
		preyList = GameObject.FindGameObjectsWithTag("Prey");
		preyCount = preyList.Where(c => c.activeSelf == true).ToArray().Length;
		Debug.Log(preyCount);
		preyTracker = preyCount;

		//Predator functions
		preyCaught = 0;
		preyCaughtDisplay = canvas.transform.Find("Text (1)").GetComponent<Text>();

		//Prey Functions
		predatorEvaded = 0;
		predatorEvadedDisplay = canvas.transform.Find("Text (2)").GetComponent<Text>();

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
			preyCaughtDisplay.text = "Prey Caught: \n"+ preyCaught.ToString();
			//Prey caught info
			predatorEvadedDisplay.text = "Predator Evaded: \n"+ predatorEvaded.ToString();

			if(currentTime < 0 || preyTracker == 0)
			{
				Debug.Log("Round End");
				reset();

				//Save all game data for 
				//GameOver();
			}
		}
		else if ( currentRound != 1)
		{
			Debug.Log("Timer");
			resetTimer = resetTimer-Time.deltaTime;
			if(resetTimer <= 0.0)
			{
				resetTimer = 5.0f;
				startRound = false;
			}
		}
	}

	public void preyCaughtUpdate()
	{
		preyCaught++;
		preyTracker--;
	}

	public void predatorEvadedUpdate()
	{
		predatorEvaded++;
	}

	//Reset Objects
	public void reset()
	{
		startRound = false;
		currentTime = maxTime;
		currentRound++;
		resetObjects();

	}

	public void resetObjects()
	{
		int waypointIndex =  UnityEngine.Random.Range(0, respawnPoints.Length);
		for( int i = 0; i <predList.Length; i++)
		{
			waypointIndex =  UnityEngine.Random.Range(0, respawnPoints.Length);
			predList[i].transform.position = respawnPoints[waypointIndex].transform.position;
		}

	}

}
