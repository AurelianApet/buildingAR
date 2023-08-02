using UnityEngine;
using System.Collections;

public class GyroRotate : MonoBehaviour {
	public Gyroscope gyro;
	private bool gyroSupported;
	private Quaternion rotFix;

	[SerializeField]

	void Start(){
		gyroSupported = SystemInfo.supportsGyroscope;

		GameObject camParent = new GameObject ("camParent");
		camParent.transform.position = transform.position;
		transform.parent = camParent.transform;

		if (gyroSupported) {
			gyro = Input.gyro;
			gyro.enabled = true;

			camParent.transform.rotation = Quaternion.Euler (90f, 180f, 0f);
			rotFix = new Quaternion (0, 0, 1, 0);
		} else {
			
		}
	}
	 
	void Update(){
		if (Global.spot != null) {
			if (gyroSupported && Global.spot.Length > 0) {
				transform.localRotation = gyro.attitude * rotFix;
			}
		}
	}

	void ResetGyroRotation(){
	}
}