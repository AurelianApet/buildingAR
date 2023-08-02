using UnityEngine;
using System.Collections;

public class ResizeNGUIScrollView : MonoBehaviour {

	private float fRate;
	// Use this for initialization
	void Awake()
	{
		
	}
	void Start () {
		fRate = (float)Screen.width / (float)Screen.height;
		//	Debug.Log ("AAAAAAAAAAA width = " + Screen.width + " height = " + Screen.height);
		//	Debug.Log ("AAAAAAAAAAA fRate = " + fRate + " fDesignRate = " + fDesignRate);

		Vector3 scale = this.transform.localScale;
		this.transform.localScale = new Vector3 (scale.x / fRate, scale.y, scale.z);

		UIPanel uiPanel = this.transform.GetComponent<UIPanel> ();
		Debug.Log ("AAAAAAAAAAA width = " + uiPanel.width + " height = " + uiPanel.height);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
