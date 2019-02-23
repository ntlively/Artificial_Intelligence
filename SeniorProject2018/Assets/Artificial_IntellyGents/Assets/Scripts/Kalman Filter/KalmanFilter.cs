using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class KalmanFilter : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject actor;
    public DisplayGrid displayGrid;
    public Vector3 target = new Vector3();

    void Awake() {
        actor = this.gameObject;
        displayGrid = actor.GetComponent<DisplayGrid>();
        displayGrid.setTargetLocation(target);
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //GetComponent<DisplayGrid>().setTargetLocation();

}
