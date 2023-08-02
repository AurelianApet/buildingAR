using UnityEngine;
using System.Collections;

public class FollowSpot : MonoBehaviour {

	public int index = 0;
	public GameObject	target;
	private int loadFlag = 0;

	void Start(){
		index = int.Parse(this.name.Split ('_')[1]);
		target = GameObject.Find ("Spot_" + index);
		loadFlag = 0;
	}

	// Update is called once per frame
    //폰 자이로에 따라 스팟들의 위치를 변경시켜 주는 동작을 수행한다.
	void Update () {
		
		if (Global.isPhotoPopup == false) {
			Camera worldCam = NGUITools.FindCameraForLayer (target.layer);
			Camera guiCam = NGUITools.FindCameraForLayer (gameObject.layer);

			Vector3 vp_pos = worldCam.WorldToViewportPoint (target.transform.position);

			Vector3 pos = new Vector3 (-1000f, -1000f, -1000f);
			if (vp_pos.normalized.z > 0) {
				pos = guiCam.ViewportToWorldPoint (vp_pos);
				//Z는 0으로...
				pos.z = 0;
			}

			
			if (Global.spot == null)
				return;
			if (Global.spot.Length > index) {
				transform.position = pos; 
				if (loadFlag == 0) {
					transform.FindChild ("back1").GetComponent<UISprite> ().depth = index * 2;
					transform.FindChild ("back2").GetComponent<UISprite> ().depth = index * 2 - 1;

					transform.FindChild ("name").GetComponent<UILabel> ().depth = index * 2 + 1;
					transform.FindChild ("distance").GetComponent<UILabel> ().depth = index * 2 + 1;
				}

				transform.FindChild ("name").GetComponent<UILabel> ().text = Global.spot [index].ar_title;
				transform.FindChild ("distance").GetComponent<UILabel> ().text = Global.spot [index].distance.ToString ("N0") + "m";
				

				if(Global.ar_type == 1)
					transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y + index * 300f / Global.count - 100 * (Global.range / 200) - 50 * (2 - Global.range/200) + 200);
				else if(Global.ar_type == 2)
					transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y + index * 300f / Global.count - 100 * (Global.range / 200) - 50 * (2 - Global.range/200) - 50);

				loadFlag = 1;
			} else {
				Debug.Log ("Out of Index Error111 : " + index);
			}
		}

	}

    //화면에 보이는 스팟들을 처리해준다
	void FixedUpdate(){
		if (Global.isPhotoPopup == false  && Global.isSettingPopup == false) {
			if (Vector3.Distance (GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("cross").transform.localPosition, transform.localPosition) <= 60f) {
				if (Global.spotFlags != null) {
					if (Global.spotFlags.Length > index) {
						Global.spotFlags [index] = 1;
					} else {
						Debug.Log ("out of index error222 : " + index);
					}
				}
			} else {
				if (Global.spotFlags != null) {
					if (Global.spotFlags.Length > index) {
						Global.spotFlags [index] = 0;
					} else {
						Debug.Log ("out of index error333 : " + index);
					}
				}
			}
		}
	}

	IEnumerator setPosition(){
		yield return new WaitForSeconds(0.1f);
	}

}
