using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GreenDetection : MonoBehaviour
{
    public static int halfWay = 2;


    private WebCamTexture mCamera = null;
    private List<float> newList = new List<float>();
    float newAvg=halfWay ;
    float oldAvg = halfWay;
    private float xTranslate = 0;
    public Transform player;
    private Color pix;

    // Use this for initialization
    void Start()
    {
        mCamera = new WebCamTexture();
        int devCount = WebCamTexture.devices.Length;
        if (WebCamTexture.devices[devCount - 1].isFrontFacing)
            mCamera.deviceName = WebCamTexture.devices[devCount - 1].name;
        else
            mCamera.deviceName = WebCamTexture.devices[0].name;
        


      //  GetComponent<Renderer>().material.mainTexture = mCamera;
        mCamera.Play();

    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < mCamera.width; i+=2)
        {
            for (int j = 0; j < mCamera.height; j+=2)
            {
                pix = mCamera.GetPixel(i, j);
                if ((pix.g > pix.r+0.07) && (pix.g > pix.b+0.07))
                {
                    newList.Add(i);
                }
            }
        }

        oldAvg = newAvg;
        newAvg = Avg(newList);

        newList.Clear();
		xTranslate = (newAvg -oldAvg) / 3;
        player.Translate(new Vector3(xTranslate, 0, 0));
    }




    public static float Avg(List<float> listings)
    {
        float total = 0;
        foreach (float p in listings)
        {
            total += p;
        }
        float avg = 0.00f;
        if (listings.Count != 0)
        {
            avg = total / listings.Count;
        }
        if (avg == 0)
            return halfWay;
        return avg;
    }
	


}