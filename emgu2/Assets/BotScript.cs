using UnityEngine;
using System.Collections;

public class BotScript : MonoBehaviour {

	public GameObject ball;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 ballPosition = ball.GetComponent<Transform> ().position;
		Vector3 oldPosition = GetComponent<Transform> ().position;
		GetComponent<Transform> ().position = new Vector3 (ballPosition.x, ballPosition.y,oldPosition.z);

	}
}
