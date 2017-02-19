using UnityEngine;
using System.Collections;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Runtime.InteropServices;
using System;
using System.Drawing;
using System.Collections.Generic;

public class MoveWithAnythigScript : MonoBehaviour {

	private Capture capture;
	Image<Bgr, Byte> currentFrame;
	Image<Bgr,Byte> firstFrame;
	public Texture2D importedTexture;
	public Point currentLocation;
	public float lastLocationX = -1;
	public GameObject player;
	public List<int> xLocationOfColoredList = new List<int>();



	Image<Bgr, Byte> Difference;
	int Threshold = 60;
	float newAvg=0, oldAvg=0;

	// Use this for initialization
	void Start () {
		capture = new Capture ();
		loadFirstFrame ();
	}
	
	// Update is called once per frame
	void Update () {

		currentFrame = capture.QueryFrame().Resize(0.3, Emgu.CV.CvEnum.INTER.CV_INTER_AREA);

			
			Difference = firstFrame.AbsDiff(currentFrame); // maska, razlika izmedju predhodnog i sadasnjeg frejma
			Difference = Difference.ThresholdBinary(new Bgr(Threshold, Threshold, Threshold), new Bgr(255,255,255));
			
			
			firstFrame = currentFrame.Copy();

			

		currentFrame = Difference.Copy();
		Image<Gray,byte> grayscale = currentFrame.Convert<Gray,byte> ();


		grayscale = grayscale.ThresholdBinary(new Gray(50), new Gray(255));
		currentFrame = grayscale.Convert<Bgr,Byte> ();


		// iscratavanje na teksturu koja je na plane-u
		for (int i=0; i<currentFrame.Width; i++) {
			for(int j=0; j<currentFrame.Height; j++){

				Bgr pixel = currentFrame[j,i];
				UnityEngine.Color color = new UnityEngine.Color((float)pixel.Red/255,(float)pixel.Green/255,(float)pixel.Blue/255);
				importedTexture.SetPixel(j,i,color);

			}
		}
		importedTexture.Apply();


		movePlayer ();
	}


	int countOfColoredPixels = 0;
	public void movePlayer(){
		for (int i=0; i<currentFrame.Width; i++) {
			for(int j=0; j<currentFrame.Height; j++){
				if((currentFrame[j,i].Blue>0) && (currentFrame[j,i].Red>0) && (currentFrame[j,i].Green>0))
					xLocationOfColoredList.Add(i);
			}
		}

		oldAvg = newAvg;
		newAvg = Avg(xLocationOfColoredList);
		countOfColoredPixels = xLocationOfColoredList.Count;
		Debug.Log (countOfColoredPixels);

		xLocationOfColoredList.Clear();

		if (countOfColoredPixels > 30)
						player.GetComponent<Transform> ().Translate (newAvg - oldAvg, 0, 0);
				else
						newAvg = oldAvg;

	}




	public void loadFirstFrame(){
		firstFrame = capture.QueryFrame().Resize(0.3, Emgu.CV.CvEnum.INTER.CV_INTER_AREA);
	}

	public static float Avg(List<int> listings)
	{
		int total = 0;
		foreach (int p in listings)
		{
			total += p;
		}
		float avg = 0.00f;
		if (listings.Count != 0)
		{
			avg = total / listings.Count;
		}
		return avg;
	}




}
