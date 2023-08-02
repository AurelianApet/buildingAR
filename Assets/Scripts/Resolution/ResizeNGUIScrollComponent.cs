using UnityEngine;
using System.Collections;

public class ResizeNGUIScrollComponent : MonoBehaviour {

	private float fRate;
	private float fDesignRate = 9.0f / 16.0f;
	private float fScale = 0.0f;
	// Use this for initialization
	void Awake()
	{
		
	}
	void Start () {
		fRate = (float)Screen.width / (float)Screen.height;
		fScale = fRate / fDesignRate;
		
		Vector3 scale = this.transform.localScale;
		this.transform.localScale = new Vector3 (scale.x / fScale, scale.y, scale.z);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
