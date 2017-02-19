using UnityEngine;
using System.Collections;

public class MenuScript : MonoBehaviour {

	public GameObject darkenScr;
	public GameObject previewPlane;
	public GameObject cursor;

	// Use this for initialization
	void Start () {
		//Object.DontDestroyOnLoad (gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		if( Input.GetMouseButtonDown(0) )
		{
			Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
			RaycastHit hit;
			
			if( Physics.Raycast( ray, out hit, 100 ) )
			{
				Debug.Log( hit.transform.gameObject.name );

				if(hit.transform.gameObject.name.Equals("pMarkerPln")){
					if(previewPlane.activeSelf){
						GameObject.Find("PreviewPlane").GetComponent<MenuSkinDetection>().capture.Dispose();
						GameObject.Find("PreviewPlane").GetComponent<MenuSkinDetection>().enabled = false;
					}

					darkenScr.SetActive(true);
					Application.LoadLevel("marker");
				}
				if(hit.transform.gameObject.name.Equals("pMovePln")){
					if(previewPlane.activeSelf){
						GameObject.Find("PreviewPlane").GetComponent<MenuSkinDetection>().capture.Dispose();
						GameObject.Find("PreviewPlane").GetComponent<MenuSkinDetection>().enabled = false;
					}
					darkenScr.SetActive(true);
					Application.LoadLevel("move");
				}
				if(hit.transform.gameObject.name.Equals("pHandPln")){
					if(previewPlane.activeSelf){
						GameObject.Find("PreviewPlane").GetComponent<MenuSkinDetection>().capture.Dispose();
						GameObject.Find("PreviewPlane").GetComponent<MenuSkinDetection>().enabled = false;
					}
					darkenScr.SetActive(true);
					Application.LoadLevel("hand");
				}
				if(hit.transform.gameObject.name.Equals("pUseCursor")){
					darkenScr.SetActive(true);
					cursor.SetActive(true);
					//previewPlane.SetActive(true);
					Invoke("activatePlane",0.1f);
				}


			}
		}
	}

	private void activatePlane(){
		previewPlane.SetActive (true);
	}


	void level(){
		Application.LoadLevel("marker");
	}

}
