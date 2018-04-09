using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.AI;

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
	public Text countDown;
	public GameObject panel;
	public GameObject panelText;

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

		countDown = canvas.transform.Find("Text (3)").GetComponent<Text>();

		//Hide loading 
		panel = GameObject.Find("Panel");
		panelText = GameObject.Find("Text (3)");
		panel.SetActive(false);
		panelText.SetActive(false);

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
				panel.SetActive(true);
				panelText.SetActive(true);
				Debug.Log("Round End");
				currentRound++;
				startRound = false;


				//Save all game data for 
				//GameOver();
			}
		}
		else if ( currentRound != 1)
		{
			Debug.Log("Timer");
			resetTimer = resetTimer-Time.deltaTime;
			countDown.text = "Next Round In \n" + resetTimer;

			if(resetTimer <= 0.0)
			{
				reset();
				panel.SetActive(false);
				panelText.SetActive(false);
				resetTimer = 5.0f;
				startRound = true;

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
		currentTime = maxTime;
		preyCaught = 0;
		predatorEvaded = 0;
		resetObjects();

	}

	public void resetObjects()
	{
		int waypointIndex =  UnityEngine.Random.Range(0, respawnPoints.Length);
		for( int i = 0; i <predList.Length; i++)
		{
			waypointIndex =  UnityEngine.Random.Range(0, respawnPoints.Length);
			predList[i].transform.position = respawnPoints[waypointIndex].transform.position;
			preyList[i].GetComponent<DataManager>().reset();
		}

		//Prey reset
		waypointIndex =  UnityEngine.Random.Range(0, respawnPoints.Length);
		for( int i = 0; i <preyList.Length; i++)
		{
			//make sure everyhting is reset
			
			waypointIndex =  UnityEngine.Random.Range(0, respawnPoints.Length);
			preyList[i].transform.position = respawnPoints[waypointIndex].transform.position;
			preyList[i].gameObject.GetComponent<UnityStandardAssets.Characters.ThirdPerson.basicPreyAI>().reset();
		}


	}

}
