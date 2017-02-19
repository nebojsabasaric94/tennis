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
using Emgu.CV.VideoSurveillance;

public class SkinDetection : MonoBehaviour {
	
	private Capture capture;
	Image<Bgr, Byte> currentFrame;
	public Texture2D importedTexture;
	private HaarCascade _face;
	public Point currentLocation;
	public float lastLocationX = -1;
	public Queue<float> lastLocations = new Queue<float> ();
	public GameObject player;
	private Boolean doMog = true;
	private Rectangle lastRectDetection;

	private Image<Hsv,Byte> currentFrameInHsv;
	private double minRed = 0;
	private double maxBlue = 0;
	private double maxGreen = 0;
	private bool detectionStarted = false;
	private BackgroundSubstractorMOG2 mog2;

	// Use this for initialization
	void Start () {
		_face = new HaarCascade("Assets/Dataset/hand_cascade.xml"); 
		capture = new Capture ();
		Invoke("startDetection", 2);
		mog2 = new BackgroundSubstractorMOG2 (0, 16f, false);
		lastRectDetection = new Rectangle ();

	}

	// Update is called once per frame
	void Update () {
		currentFrame = capture.QueryFrame().Resize(0.3, Emgu.CV.CvEnum.INTER.CV_INTER_AREA);
		currentFrameInHsv = currentFrame.Convert<Hsv,Byte> ();
		Image<Gray,Byte> hueMask = new Image<Gray,Byte>(currentFrame.Width, currentFrame.Height);
		

		
		if(doMog)
		mog2.Update (currentFrame, 0.05);
		
		for (int i=0; i<mog2.ForgroundMask.Width; i++) {
			for(int j=0; j<mog2.ForgroundMask.Height; j++){
				if(mog2.ForgroundMask[j,i].Intensity<5 && doMog){
						currentFrame[j,i] = new Bgr(System.Drawing.Color.Black);
				}
				if(!doMog)
					reduceRed(currentFrame[j,i],i,j);

				if(!doMog && !rgbTest(currentFrame[j,i], currentFrameInHsv[j,i]) ){
					currentFrame[j,i] = new Bgr(System.Drawing.Color.Black);
				}
			}
		}

		currentFrame.Erode (300);
		
		
		//----------hand detection
		if (detectionStarted) {

			Image<Gray, byte> grayframe = currentFrame.Convert<Gray, byte> ();
	
			MCvAvgComp[] faces = _face.Detect (grayframe, 1.1, 8,
		                                  HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
		                                  new System.Drawing.Size (24, 24),
		                                  System.Drawing.Size.Empty);
		
						/*foreach (MCvAvgComp face in faces)
		{
			currentFrame.Draw(face.rect, new Bgr(System.Drawing.Color.Blue), 2);
		}*/
			//---------------------------
		
		
			if (faces.Length > 0) {
				currentFrame.Draw (faces [0].rect, new Bgr (System.Drawing.Color.Red), 3);
				if (lastLocationX == -1)
					lastLocationX = currentLocation.X;
				lastLocationX = currentLocation.X;
				computeLastLocation ();	// funkcija koja ublazava drhtanje, radice i bez nje
				currentLocation = faces [0].rect.Location;
				lastRectDetection = faces [0].rect;
		
				if (doMog) {
					doMog = false;
					updateAvgSkinColor ();
				}

				} else {
				//lastRectDetection = new Rectangle();
				}
		}
		
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
	
	
	
	
	/**
	 * Funkcija za pomernje igraca na osnovu currentLocation i lastLocation, vrsi se transliranje za vrednost current - last
	 */ 
	public void movePlayer(){
		//player.GetComponent<Transform>().position = new Vector3(0, 0, currentLocation.X/2-50);
		player.GetComponent<Transform>().Translate(new Vector3(((currentLocation.X)-(lastLocationX)),0,0));
		
		
		Vector3 pos = player.GetComponent<Transform>().position;
		
		if (player.GetComponent<Transform> ().position.x > 50) {
			player.GetComponent<Transform> ().position = new Vector3(50,pos.y,pos.z);
		}
		if (player.GetComponent<Transform> ().position.x < -50) {
			player.GetComponent<Transform> ().position = new Vector3(-50,pos.y,pos.z);
		}
		
	}
	
	

	/**
	 * Funkcija omogucava da se trenutna lokacija uporedjuje sa prosekom predhodnih 5 lokacija,
	 * sto smanjuje drhtanje igraca.
	 */
	private void computeLastLocation(){
		lastLocations.Enqueue ((float) currentLocation.X);
		if (lastLocations.Count > 5)
			lastLocations.Dequeue ();
		float sum = 0;
		foreach(float f in lastLocations){
			sum+=f;
		}
		float avg = sum / lastLocations.Count;
		lastLocationX = avg;
	}


	private void startDetection(){
		detectionStarted = true;
	}


	
	/**
	 * Funkcija se poziva samo jednom, kada se prvi put detektuje ruka pomocu backgroundSubstruct-era
	 * Na osnovu 100 pixela detektovane ruke, izracunvaca minimalnu crvenu komponentu, maximalne zelene i plave komponente
	 *
	 */

	private void updateAvgSkinColor(){
		int counter = 0;
		double[] reds = new double[100];
		double[] blues = new double[100];
		double[] greens = new double[100];


		String ispis = "Hues ";
			for (int i=lastRectDetection.Left; i<lastRectDetection.Right-3; i+=3) { //po sirini
				for (int j=lastRectDetection.Top; j<lastRectDetection.Bottom-3; j+=3) { //po visini
					if (currentFrame [j, i].Red > 2 && currentFrame [j, i].Green > 2 && currentFrame [j, i].Blue > 2) {	// ako piksel nije crn
						if(counter<100){
							reds[counter] = currentFrame[j,i].Red;
							blues[counter] = currentFrame[j,i].Blue;
							greens[counter] = currentFrame[j,i].Green;
						ispis +=" "+ currentFrameInHsv[j,i].Hue;
						}
					counter++;
					}
				}
			}
			//Debug.Log (ispis);
			Debug.Log(ispis);
			String ispis2 = "Max: " + (max (blues));
			Debug.Log (ispis2);
		minRed = min (reds);
		maxBlue = max (blues);
		maxGreen = max (greens);

		
	}


	/**
	 * Funkcija racuna minimum niza od 100 double elemenata, koristi se za pronalazenje minimalne crvene komponente
	 * ne vraca minimum pravi nego malo iznad minimuma tj. (avg+min)/2
	 *
	 */

	private double min(double[] array100){
		double min = array100 [1];
		double sum = 0;
		double avg = 0;

		for (int i=0; i<100; i++) {
			sum+=array100[i];
			if(array100[i] < min)
				min = array100[i];
		}

		avg = sum / 100;

		return (min + avg)/2;
	}


	/**
	 *	Funkcija racuna maximum niza od 100 double elementa,
	 *	koristi se za pronalazenje maximalne zelene i plave komponente
	 */
	private double max(double[] array100){
		double max= array100 [1];
		double sum = 0;
		double avg = 0;
		
		for (int i=0; i<100; i++) {
			sum+=array100[i];
			if(array100[i] > max)
				max = array100[i];
		}
		
		avg = sum / 100;
		
		return max;
	}


	/**
	 *	Fukncija se poziva za svaki piksel posebno, daje odgovor da li ispunjen uslov da se pixel uzme u razmatranje
	 *	ako pixel zadovoljava minRed, maxBlue, maxGreen i hue vrednost za kozu vraca true.
	 * 
	 */
	private bool rgbTest(Bgr bgr, Hsv hsv){
		if ((bgr.Red > minRed) && ( bgr.Blue<maxBlue || bgr.Green<maxGreen) && (hsv.Hue>150 || hsv.Hue<30)  )
			return true;
		else
			return false;
	}




	/**
	 * 	Funkcija se poziva za svaki pixel slike posebno,
	 * 	smanjuje crvenu komponentu pixela u zavisnosti koliko je udaljen od predhodne detekcije
	 * 	(smanjivanjem crvene komponente, pixel teze moze da ispuni uslov za minRed)
	 * 
	 */ 
	private void reduceRed(Bgr bgr, int i, int j){
		if (((j < lastRectDetection.Bottom) && (j > lastRectDetection.Top) && (i > lastRectDetection.Left) && (i < lastRectDetection.Right)))
						return;
		else {
			bgr.Red = bgr.Red - Math.Abs((lastRectDetection.Left + lastRectDetection.Size.Width/2) - i )*1.3f;
			bgr.Red = bgr.Red - Math.Abs((lastRectDetection.Top + lastRectDetection.Size.Height/2) - j )*1.3f;

			currentFrame [j, i] = bgr;

		}

	}




	
}
