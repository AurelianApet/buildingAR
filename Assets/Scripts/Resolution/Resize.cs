using UnityEngine;
using System.Collections;

public class Resize : MonoBehaviour {

	private float fRate;
	private float fDesignRate = 16.0f / 9.0f;	
	private float fScale = 0.0f;
	// Use this for initialization
	void Awake()
	{

	}
	void Start () {
		fRate = (float)Screen.height / (float)Screen.width;
		fScale = fRate/fDesignRate;
		Vector3 scale = this.transform.localScale;
		this.transform.localScale = new Vector3 (scale.x, scale.y*fScale, scale.z);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
