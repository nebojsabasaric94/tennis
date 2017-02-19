using UnityEngine;
using System.Collections;

public class MenuPlayerScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		MovePlayer ();
	}


	void MovePlayer() {

		Vector3 pos = GetComponent<Transform>().position;
		
		if (GetComponent<Transform> ().position.x > 120) {
			GetComponent<Transform> ().position = new Vector3(120,pos.y,pos.z);
		}
		if (GetComponent<Transform> ().position.x <0) {
			GetComponent<Transform> ().position = new Vector3(0,pos.y,pos.z);
		}

		if (GetComponent<Transform> ().position.y > 60) {
			GetComponent<Transform> ().position = new Vector3(pos.x,60,pos.z);
		}
		if (GetComponent<Transform> ().position.y < 6) {
			GetComponent<Transform> ().position = new Vector3(pos.x,6,pos.z);
		}

	}
}
