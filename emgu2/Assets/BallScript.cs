using UnityEngine;
using System.Collections;

public class BallScript : MonoBehaviour {

	private static float startingVelocity = 100;


	private Transform transform;
	private Rigidbody rb;
	private System.Random rnd = new System.Random();
	private bool lastShotNegative =false;

	// Use this for initialization
	void Start () {
		transform = GetComponent<Transform>();
		rb = GetComponent<Rigidbody>();
		rb.velocity = new Vector3(0, -20, -startingVelocity);
	}
	
	// Update is called once per frame
	void Update () {

		if (transform.position.y < 10) {
			Invoke("refreshBall",2);
		}
	
	}

	void OnTriggerEnter(Collider collider)
	{
		if (collider.transform.name.Equals("Bot")){
			int br = rnd.Next(-15,15); 
			if(lastShotNegative && br<0)
				br = -br;
			if(!lastShotNegative && br>0)
				br = -br;


			if(br <0){
				lastShotNegative = true;
			}
			else
				lastShotNegative = false;



			rb.velocity = new Vector3(br, 50, startingVelocity);
		}
		if (collider.transform.name.Equals("Player")){
			rb.velocity = new Vector3(0, 50, -startingVelocity);
		}
	}

	private void refreshBall(){
		transform.position = new Vector3(0,42,-79);
		rb.velocity = new Vector3(0, -30, -startingVelocity);
	}
}
