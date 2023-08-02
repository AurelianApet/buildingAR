using UnityEngine;
using System.Collections;

public class SetHighLight : MonoBehaviour {
	public int selected_texture_id = -1;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //갤러리 뷰에서 선택된 이미지를 highlight 처리 진행
	public void ShowHighLights()
	{
		GameObject[] highLights = GameObject.FindGameObjectsWithTag ("Gallery");
		for (int i = 0; i < highLights.Length; i++) {
			if (i == Global.selectedPhotoId) {
				int k = 0;
				foreach (Transform child in highLights[i].transform) {
					if(k == 0)
						child.gameObject.SetActive (true);
					else if(k == 1)
						child.gameObject.SetActive (false);
					k++;
				}
			} else {
				int k = 0;
				foreach (Transform child in highLights[i].transform) {
					if(k == 0)
						child.gameObject.SetActive (false);
					else if(k == 1)
						child.gameObject.SetActive (true);
					k++;
				}
			}
		}
	}

    //갤러리 뷰에서 이미지 선택시 호출하는 함수
	public void OnSelectImage()
	{
		Global.selectedPhotoId = selected_texture_id;

		GameObject.Find ("UI Root/PhotoSlider/PhotoCaption").transform.GetComponent<UILabel> ().text = Global.gMediaInfo[Global.selectedPhotoId].address;

		StartCoroutine (GameObject.Find("backgroundManager").transform.GetComponent<ToolbarManager>().ChangeGalleryMainImage());
		ShowHighLights ();
	}
}
