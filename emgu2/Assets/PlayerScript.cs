using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		MovePlayer ();
	}


	void MovePlayer() {

		if (Input.GetKey(KeyCode.LeftArrow))
			GetComponent<Transform>().Translate(new Vector3(2,0,0));
		
		if (Input.GetKey(KeyCode.RightArrow))
			GetComponent<Transform>().Translate(new Vector3(-2,0,0));

		
		Vector3 pos = GetComponent<Transform>().position;
		
		if (GetComponent<Transform> ().position.x > 50) {
			GetComponent<Transform> ().position = new Vector3(50,pos.y,pos.z);
		}
		if (GetComponent<Transform> ().position.x < -50) {
			GetComponent<Transform> ().position = new Vector3(-50,pos.y,pos.z);
		}

	}
}
