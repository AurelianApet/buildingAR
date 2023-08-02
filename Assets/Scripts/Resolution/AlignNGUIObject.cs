using UnityEngine;
using System.Collections;

public class AlignNGUIObject : MonoBehaviour {
	private float fRate;
	private float fDesignRate = 16.0f / 9.0f;
	private float fScaleRate = 0.0f;
	// Use this for initialization
	void Awake()
	{
		
	}
	void Start () {
		fRate = (float)Screen.width / (float)Screen.height;
		fScaleRate = fDesignRate / fRate;

		Vector3 pos = this.transform.localPosition;
		this.transform.localPosition = new Vector3 (pos.x, pos.y * fScaleRate, pos.z);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
