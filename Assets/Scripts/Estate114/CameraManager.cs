using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour {
	//private AndroidJavaObject curActivity;
	//private AndroidJavaClass java_AngleClass;
	//public GameObject angleLabel;



	void Update () {
		//float target_direction = curActivity.Call<float> ("getTargetDirection");
		//angleLabel.GetComponent<UILabel> ().text = target_direction.ToString ();
	}

    public void Start () {
		//AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		//curActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
    }

	private WebCamTexture webcamTexture;
	public GameObject rawImage;
	void OnEnable()
	{
		
		webcamTexture = new WebCamTexture();
		webcamTexture.requestedWidth = 1920;
		webcamTexture.requestedHeight = 1080;
		rawImage.GetComponent<MeshRenderer>().material.mainTexture = webcamTexture;
		webcamTexture.Play();

	}
		


}
