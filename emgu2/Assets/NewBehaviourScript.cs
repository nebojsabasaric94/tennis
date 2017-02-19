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

public class NewBehaviourScript : MonoBehaviour {

	private Capture capture;
	Image<Bgr, Byte> currentFrame;
	public Texture2D importedTexture;
	private HaarCascade _face;
	public Point currentLocation;
	public float lastLocationX = -1;
	public Queue<float> lastLocations = new Queue<float> ();
	public GameObject player;
	private BackgroundSubstractorMOG2 mog2;
	private Rectangle lastRectDetection;

	private System.Drawing.Color avgSkinColor;
	private Hsv avgSkin = new Hsv(-200,-200,-200);
	private Image<Hsv,Byte> currentFrameInHsv;
	private AdaptiveSkinDetector skinDetektor;
	private List<double> skinHueList;
	private List<double> skinValueList;
	private double minRed = 255;
	private double maxBlue = 0;
	private BackgroundSubstractorMOG2 mog22;

	// Use this for initialization
	void Start () {
		_face = new HaarCascade("D:\\hand_cascade.xml"); 
		capture = new Capture ();
		Invoke("BtnClick", 2);
		mog2 = new BackgroundSubstractorMOG2 (0, 16f, false);
		lastRectDetection = new Rectangle ();
		avgSkinColor = System.Drawing.Color.Black;
		skinDetektor = new AdaptiveSkinDetector (1, AdaptiveSkinDetector.MorphingMethod.NONE);
		skinHueList = new List<double> ();
		skinValueList = new List<double>();
		mog22 = new BackgroundSubstractorMOG2 (0, 16f, false);

	}

	private int frameCount = 0;
	// Update is called once per frame
	void Update () {
		currentFrame = capture.QueryFrame().Resize(0.3, Emgu.CV.CvEnum.INTER.CV_INTER_AREA);
		currentFrameInHsv = currentFrame.Convert<Hsv,Byte> ();
		Image<Gray,Byte> hueMask = new Image<Gray,Byte>(currentFrame.Width, currentFrame.Height);

		skinDetektor.Process (currentFrame, hueMask);

		frameCount ++;
		if (frameCount < 50) {
			mog2.Update (currentFrame, 0.9);
			mog22.Update (currentFrame, 0.9);
		} else {
			mog2.Update (currentFrame, 0.05);
			mog22.Update(currentFrame, 0.00001);
		}

		for (int i=0; i<mog2.ForgroundMask.Width; i++) {
			for(int j=0; j<mog2.ForgroundMask.Height; j++){
				if(mog2.ForgroundMask[j,i].Intensity<5){
					//ako se pixel nalazi u predhodnom Detektovanom kvadratu, ne sme biti crn:	
				//	if( !((j<lastRectDetection.Bottom) && (j>lastRectDetection.Top) && (i>lastRectDetection.Left) && (i<lastRectDetection.Right)) )

					// ako je otprilike boja koze
					/*if(!( Math.Abs(currentFrame[j,i].Red - avgSkinColor.R)<70 && 
						  Math.Abs(currentFrame[j,i].Blue - avgSkinColor.B)<50 && 
						  Math.Abs(currentFrame[j,i].Green - avgSkinColor.G)<50 )// ||
					    //  !((j<lastRectDetection.Bottom) && (j>lastRectDetection.Top) && (i>lastRectDetection.Left) && (i<lastRectDetection.Right))
					   )*/
					//if((currentFrameInHsv[j,i].Hue<avgSkin.Hue+40) && (currentFrameInHsv[j,i].Satuation<avgSkin.Satuation+40) && (currentFrameInHsv[j,i].Value<avgSkin.Value+40))

					//if( !(Math.Abs(currentFrameInHsv[j,i].Hue) > 180 || Math.Abs(currentFrameInHsv[j,i].Hue) < 20) )

					//if( !isSkinHue(currentFrameInHsv[j,i].Hue,currentFrameInHsv[j,i].Value, currentFrame[j,i]) )
					  if ( !((j<lastRectDetection.Bottom) && (j>lastRectDetection.Top) && (i>lastRectDetection.Left) && (i<lastRectDetection.Right)) || mog22.ForgroundMask[j,i].Intensity<5)
						currentFrame[j,i] = new Bgr(System.Drawing.Color.Black);
				}
			}
		}


		//----------hand detection

		Image<Gray, byte> grayframe = currentFrame.Convert<Gray, byte>();
		
		MCvAvgComp[] faces = _face.Detect(grayframe, 1.1, 8,
		                                  HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
		                                  new System.Drawing.Size(24, 24),
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
			updateAvgSkinColor();

		} else {
			//lastRectDetection = new Rectangle();
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




	public void BtnClick(){

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

	private int updateCount = 0;
	private void updateAvgSkinColor(){
		updateCount++;
		if (true) {
			minRed = 255;
			maxBlue = 0;
			skinHueList.Clear ();
			for (int i=lastRectDetection.Left; i<lastRectDetection.Right-3; i+=3) { //po sirini
				for (int j=lastRectDetection.Top; j<lastRectDetection.Bottom-3; j+=3) { //po visini
					if (currentFrame [j, i].Red > 2 && currentFrame [j, i].Green > 2 && currentFrame [j, i].Blue > 2) {	// ako piksel nije crn
						//if (!skinHueList.Contains (currentFrameInHsv [j, i].Hue))
							skinHueList.Add (currentFrameInHsv [j, i].Hue);
							//skinValueList.Add(currentFrameInHsv[j,i].Value);
						
												

						// min red
						if (currentFrame [j, i].Red < minRed)
							minRed = currentFrame [j, i].Red;

						// max blue
						if (currentFrame [j, i].Blue > maxBlue)
							maxBlue = currentFrame [j, i].Blue;
					}
				}
			}
			Debug.Log (maxBlue);
		}
	}


	private bool isSkinHue(double hue, double value, Bgr bgr){
		for (int i=0; i<skinHueList.Count; i++) {
			if (Math.Abs(skinHueList[i] - hue) < 1) {
				return true;
			}
		}

		return false;
	}

}
