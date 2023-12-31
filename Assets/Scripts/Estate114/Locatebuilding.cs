using UnityEngine;
using System.Collections;

using UnityEngine.Networking;
using LitJson;
using System;

public class Locatebuilding : MonoBehaviour {
	public GameObject goPlane;
	public GameObject goWall;
	public GameObject camUser;
	public GameObject spot;
	public GameObject nguispot;
	public GameObject background;

	public GameObject lblPhotoCount;
	public GameObject lblRange;
	public GameObject lblType;
    private int FirstStickerLoadFlag = 0;

	public int iPositionFlag = -1;			// 0: gps off, 1 : gps on(service out) , 2 : gps on & service in

	//public WWWHelper helper;

	public static int ar_type = 1;
	public static int range = 100;

	public GameObject loadingImage;

    //기기 자이로 센서 enable, 위치 서비스 enable
    //
	void Start () {
        #if UNITY_ANDROID
        if (null != SystemInfo.deviceModel && ("LGE Nexus 5X").Equals(SystemInfo.deviceModel.ToString()))
        {
            camUser.transform.FindChild("Main Camera").transform.FindChild("Background").transform.rotation = Quaternion.Euler(0, -90, -90);
        }
        #endif
        
        
        initGlobal ();

		lblPhotoCount.GetComponent<UILabel> ().text = "0";
		lblRange.GetComponent<UILabel> ().text = (Global.range - 100).ToString() + "~" + Global.range + "m";
		lblType.GetComponent<UILabel> ().text = "건물";

		loadingImage.SetActive (false);

        //StartCoroutine (LogThread ());

        //start the main routine
        #if UNITY_ANDROID
        if(Input.location.status == LocationServiceStatus.Stopped)
            Input.location.Start(10,0.1f);
        #elif UNITY_IPHONE
        Input.location.Start ();
        #endif

        if (Global.bGPSVersion)
            StartCoroutine("StartRoutine");
        else
        {
            Global.LoadSpotFinishFlag = false;
            getLocationList();
        }
    }

    //로딩 이미지 현시
	IEnumerator PlayLoadingImage()
	{
		int lot_y = 0;
		loadingImage.SetActive (true);
		while (Global.LoadSpotFinishFlag == false) {
			lot_y += 5;
			loadingImage.transform.FindChild ("Loading").transform.localRotation = Quaternion.Euler (0, 0, -lot_y);
			yield return new WaitForSeconds (0.03f);
		}

		loadingImage.SetActive (false);
	}

	void Update () {
	}

    //로그 현시 스레드
	IEnumerator LogThread()
	{
		while (true) {
            Debug.Log("Camera Roattion : " + camUser.transform.FindChild("Main Camera").transform.FindChild("Background").transform.localRotation.x +
                " , " + camUser.transform.FindChild("Main Camera").transform.FindChild("Background").transform.localRotation.y +
                " , " + camUser.transform.FindChild("Main Camera").transform.FindChild("Background").transform.localRotation.z);
            yield return new WaitForSeconds (3.0f);
		}
	}

    //현재 위치로 부터 지도 영역 표시
	void FixedUpdate(){
		if (Global.bGPSVersion) {
			if (Global.count > 0) {
				if (Input.location.status == LocationServiceStatus.Failed) {
					if (Global.defaultPos.latitude != Global.curPos.latitude && Global.defaultPos.longitude != Global.curPos.longitude) {
						Global.curPos = Global.defaultPos;
						setOrgStartEndPos (range);
						float posx = (Global.curPos.longitude - Global.orgstart_pos.longitude) * Global.unitLng * 10.0f;
						float posz = (Global.curPos.latitude - Global.orgstart_pos.latitude) * Global.unitLat * 10.0f;
						camUser.transform.parent.localPosition = new Vector3 (posx, 1.0f, posz);
					}
                    #if UNITY_IPHONE
                    else{
						setOrgStartEndPos (range);
						float posx = (Global.curPos.longitude - Global.orgstart_pos.longitude) * Global.unitLng * 10.0f;
						float posz = (Global.curPos.latitude - Global.orgstart_pos.latitude) * Global.unitLat * 10.0f;
						camUser.transform.parent.localPosition = new Vector3 (posx, 1.0f, posz);
						//camUser.transform.parent.transform.parent.localPosition = new Vector3 (posx, 1.0f, posz);
                    }
                    #endif
				} 
                else {
                    #if UNITY_ANDROID
					if (Input.location.lastData.latitude != Global.curPos.latitude && Input.location.lastData.longitude != Global.curPos.longitude) {
						setOrgStartEndPos (range);
						float posx = (Global.curPos.longitude - Global.orgstart_pos.longitude) * Global.unitLng * 10.0f;
						float posz = (Global.curPos.latitude - Global.orgstart_pos.latitude) * Global.unitLat * 10.0f;
						camUser.transform.parent.localPosition = new Vector3 (posx, 1.0f, posz);
					}
                    #elif UNITY_IPHONE
                    setOrgStartEndPos (range);
					float posx = (Input.location.lastData.longitude - Global.orgstart_pos.longitude) * Global.unitLng * 10.0f;
					float posz = (Input.location.lastData.latitude - Global.orgstart_pos.latitude) * Global.unitLat * 10.0f;
					camUser.transform.parent.localPosition = new Vector3 (posx, 1.0f, posz);
                    #endif
				}
			}
		}
				
		if ((Global.ar_type != ar_type || Global.range != range || Global.setting_changed == true) && (Global.LoadSpotFinishFlag == true)) {
			Global.LoadSpotFinishFlag = false;
			ar_type = Global.ar_type;
			range = Global.range;
            Global.setting_changed = false;

            //set the camera pos
            setOrgStartEndPos (range);
			float posx = (Global.curPos.longitude - Global.orgstart_pos.longitude) * Global.unitLng * 10.0f;
			float posz = (Global.curPos.latitude - Global.orgstart_pos.latitude) * Global.unitLat * 10.0f;
			camUser.transform.parent.localPosition = new Vector3 (posx, 1.0f, posz);

			getLocationList ();
		}
	}

	void initGlobal(){
                FirstStickerLoadFlag = 0;
		Global.spot = null;
		Global.ar_type = 1;
		Global.count = 0;
		Global.range = 100;
		Global.spotFlags = null;

	}

    //현재 위치에 따라 스팟 정보 가져오는 동작 수행
    //위치변화가 일정한 값을 넘으면 정보를 다시 가져온다.
	IEnumerator StartRoutine() // gps function
	{
		{
			while(true)
			{
				if (Input.location.status != LocationServiceStatus.Running) {
					Global.curPos = Global.defaultPos;
					if (iPositionFlag != 0) {
						Global.LoadSpotFinishFlag = false;
						getLocationList ();
						iPositionFlag = 0;
						Debug.Log ("-----------------Start Load Default Spots-------------");
					}
					yield return new WaitForSeconds (3.0f);
				} else {
					if ((Global.curPos.longitude > Global.gFirstMapPos.longitude && Global.curPos.longitude < Global.gLastMapPos.longitude) &&
					   (Global.curPos.latitude < Global.gFirstMapPos.latitude && Global.curPos.latitude > Global.gLastMapPos.latitude)) {
						// Access granted and location value could be retrieved
						if (getDistance (Global.curPos, new Global.GPS_pos (Input.location.lastData.longitude, Input.location.lastData.latitude)) > 10.0d && Global.LoadSpotFinishFlag == true && Global.isDetailPopup == false) {
							Global.curPos.latitude = Input.location.lastData.latitude;
							Global.curPos.longitude = Input.location.lastData.longitude;
							Global.LoadSpotFinishFlag = false;
							getLocationList ();
							iPositionFlag = 2;

							yield return new WaitForSeconds (10.0f);
						}
                                                #if UNITY_IPHONE
                                                else {												//added by rci 2017.7.2
							if (FirstStickerLoadFlag == 0) {
								FirstStickerLoadFlag = 1;
								Global.curPos.latitude = Input.location.lastData.latitude;
								Global.curPos.longitude = Input.location.lastData.longitude;
								if (iPositionFlag != 2) {
									Global.LoadSpotFinishFlag = false;
									getLocationList ();
									iPositionFlag = 2;
								}

								yield return new WaitForSeconds (10.0f);
							}
						}
                                                #endif
					} else {
						Global.curPos = Global.defaultPos;
						if (iPositionFlag != 1) {
							Global.LoadSpotFinishFlag = false;
							getLocationList ();
							iPositionFlag = 1;
						}
					}
					yield return new WaitForSeconds (3.0f);
				}
			}
			Input.location.Stop();
		}
	}

    //스팟 정보를 가져와서 스팟을 뿌려 놓는 기능 수행
	void getLocationList(){
		StartCoroutine ("PlayLoadingImage");

		if (Global.count > 0) {//이미 생성된 스팟제거
			for(int i = Global.count - 1; i >= 0; i--){
				Destroy (GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + i).gameObject);
				Destroy (GameObject.Find ("Spot_" + i).gameObject);
			}
			Global.count = 0;
			Global.spot = null;
		}

        string selected_types = "";
        if (Global.ar_type == 1)
            selected_types = Global.selected_build_types;
        else
            selected_types = Global.selected_land_types;
        //string requestURL = domain + "/?c=ar&p=ajax&ncX="+Global.curPos.longitude+"&ncY="+Global.curPos.latitude+"&ncR="+Global.building_range+"&ncL="+Global.land_range;
        string requestURL = Global.domain + "/?c=ar&p=ajax&ncX="+Global.curPos.longitude+"&ncY="+Global.curPos.latitude+"&ncT="+Global.ar_type+"&ncTD="+selected_types
            +"&ncR="+Global.range;
		Debug.Log ("----------------Request Url : " + requestURL + "----------------");
		WWW www = new WWW (requestURL);
		StartCoroutine (GetSpotInfo (www));
	}

    //위치 정보 가져오는 요청 보내고 응답 받아오는 함수
	IEnumerator GetSpotInfo(WWW www)
	{
		yield return www;
        Debug.Log("----------------End Page Request-------------");
		OnParsingSpotInfo (www);
	}

    //서버로 부터 받은 위치 정보를 파싱하는 함수
	void OnParsingSpotInfo(WWW www){
		if (www.error != null)
		{
			Debug.Log ("[Error] " + www.error);
			Global.LoadSpotFinishFlag = true;
			return;
		}
		Debug.Log ("---------------Read Spots from web ------------------");
		JsonData json = JsonMapper.ToObject (www.text);
		Global.count = int.Parse(json ["count"].ToString());
		if (Global.count > 0) {
			Global.spot = new Global.Spot[Global.count];
		}
			
		Global.curAddr = json ["address"].ToString ();
		Global.photoAddr = Global.curAddr;
		background.transform.FindChild("address").GetComponent<UILabel> ().text = Global.curAddr;

		if (Global.count < 1) {
            Debug.Log("----------------Spot Count is less than 1-------------");
            Global.LoadSpotFinishFlag = true;
			return;
		}
			
		try{
			JsonData items = json ["data"];
			for (int i = 0; i < items.Count; i++) {
				string row = items [i].ToJson ();
				JsonData item = JsonMapper.ToObject(row);

				string strLocation = item["ar_location"].ToString();
				Global.GPS_pos location = new Global.GPS_pos(float.Parse(strLocation.Split(',')[0]), float.Parse(strLocation.Split(',')[1]));
				string[] strPolygon = item["ar_polygon"].ToString().Split(',');
				Global.GPS_pos[] polygon = new Global.GPS_pos[strPolygon.Length];
				for(int j=0; j < strPolygon.Length; j++){
					if (strPolygon [j].Split (' ') [0] != "") {
						polygon [j] = new Global.GPS_pos (float.Parse (strPolygon [j].Split (' ') [0]), float.Parse (strPolygon [j].Split (' ') [1]));
					} else {
						polygon [j] = new Global.GPS_pos (float.Parse (strPolygon [j].Split (' ') [1]), float.Parse (strPolygon [j].Split (' ') [2]));
					}
				}
				Global.spot[i] = new Global.Spot(item["ar_id"].ToString(), item["ar_title"].ToString(), item["ar_address"].ToString(), location, polygon, item["ar_dtl_title"].ToString(), item["ar_dtl_subtitle"].ToString(), 
					item["ar_dtl_desc"].ToString(), item["ar_dtl_url_detail"].ToString(), item["ar_dtl_url_like"].ToString(), item["ar_dtl_subdata"].ToString(), float.Parse(item["ar_dtl_distance"].ToString()));
			}

			Debug.Log ("----------------------------Load : " + Global.count + " --------------------------------");
			setBackgroundDepth ();
			setInit();
		}catch(System.Exception e)
		{
			Debug.Log ("-------------Showing Images Error : " + e.ToString () + "-------------");
		}
		Global.LoadSpotFinishFlag = true;
	}

    //두 위치 사이 거리 계산하는 함수
	double getDistance(Global.GPS_pos pos1, Global.GPS_pos pos2){
		double theta, dist;
		theta = pos1.longitude - pos2.longitude;

		dist = System.Math.Sin(deg2rad(pos1.latitude)) * System.Math.Sin(deg2rad(pos2.latitude)) + System.Math.Cos(deg2rad(pos1.latitude))
			* System.Math.Cos(deg2rad(pos2.latitude)) * System.Math.Cos(deg2rad(theta));
		dist = System.Math.Acos(dist);
		dist = rad2deg(dist);
		dist = dist * 60 * 1.1515;
		dist = dist * 1.609344;
		dist = dist * 1000.0;
		return dist;
	}

	double deg2rad(double deg)
	{
		return (double)(deg * System.Math.PI / (double)180d);
	}

	double rad2deg(double rad)
	{
		return (double)(rad * (double)180d / System.Math.PI);
	}

    //거리에 따라 스팟들을 정렬하는 함수
	void sortByDistance(){
		for (int i = 0; i < Global.count; i++) {
			for(int j = i+1; j < Global.count; j++){
				if (Global.spot [i].distance > Global. spot [j].distance) {
					Global.Spot temp = Global.spot [i];
					Global.spot [i] = Global.spot [j];
					Global.spot [j] = temp;
				}
			}
		}
	}

    //스팟들을 위치에 따라 depth 조절
	void setBackgroundDepth(){
		int temp = Global.count;
		background.GetComponent<UISprite> ().depth = Global.count + 1;
		background.transform.FindChild ("sp114").GetComponent<UISprite> ().depth = Global.count + 2;
		background.transform.FindChild ("address").GetComponent<UILabel> ().depth = Global.count + 2;
		background.transform.FindChild ("cross").GetComponent<UISprite> ().depth = Global.count + 2;
		background.transform.FindChild ("btnClose").transform.FindChild("Background").GetComponent<UISprite> ().depth = Global.count + 2;
		background.transform.FindChild ("btnPhoto").transform.FindChild("Background").GetComponent<UISprite> ().depth = Global.count + 2;
		background.transform.FindChild ("btnPhoto").transform.FindChild("lblPhotoCount").GetComponent<UILabel> ().depth = Global.count + 5;
		background.transform.FindChild ("btnCapture").transform.FindChild("Background").GetComponent<UISprite> ().depth = Global.count + 2;
		background.transform.FindChild ("btnSetting").transform.FindChild("Background").GetComponent<UISprite> ().depth = Global.count + 2;
		background.transform.FindChild ("btnSetting").transform.FindChild("lblRange").GetComponent<UILabel> ().depth = Global.count + 3;
		background.transform.FindChild ("btnSetting").transform.FindChild("lblType").GetComponent<UILabel> ().depth = Global.count + 3;

		Global.count = temp;
	}

    //3d 스팟 정보로 부터 2d 스팟을 초기화 진행
	void setInit(){
		setOrgStartEndPos (Global.range);

		Global.spotFlags = new int[Global.count];
		for (int i = 0; i < Global.spotFlags.Length; i++) {
			Global.spotFlags [i] = 0;
		}

		for (int i = 0; i < Global.count; i++) {
			SetSpot (Global.spot[i], i);
			GameObject temp = (GameObject)Instantiate(nguispot);	
			temp.name =  "nguispot_" + i;
		}
	}

    //지도 영역에 따르는 실지 위치 구하는 함수
	void setOrgStartEndPos(float distance){
		float offsetX = 0.8813820033002971f;
		float offsetY = 1.1119492667985354f;

		Global.orgstart_pos = new Global.GPS_pos (float.Parse((Global.curPos.longitude - (distance / offsetX + 1) * 0.00001).ToString()), float.Parse((Global.curPos.latitude - (distance / offsetY + 1) * 0.00001).ToString()));
		Global.orgend_pos = new Global.GPS_pos (float.Parse((Global.curPos.longitude + (distance / offsetX + 1) * 0.00001).ToString()), float.Parse((Global.curPos.latitude + (distance / offsetY + 1) * 0.00001).ToString()));

	}

    //3d 스팟을 정보로 부터 현시 하는 함수
	void SetSpot(Global.Spot param, int index){
		float centerGpsX, centerGpsY, cneterGpsZ;
		centerGpsX = param.ar_location.longitude;
		cneterGpsZ = param.ar_location.latitude;
		centerGpsY = 0.0f;

		float posx = (centerGpsX - Global.orgstart_pos.longitude) * Global.unitLng * 10.0f;	
		float posz = (cneterGpsZ - Global.orgstart_pos.latitude) * Global.unitLat * 10.0f;

		float posy = centerGpsY * Global.unitVSpace * 10.0f;

		spot.transform.localPosition = new Vector3 (posx, posy, posz);

		GameObject temp = (GameObject)Instantiate(spot, spot.transform.position, spot.transform.rotation);	
		temp.name =  "Spot_" + index;
		temp.transform.FindChild ("Name").GetComponent<TextMesh> ().text = Global.spot [index].ar_title;
		temp.transform.FindChild ("Distance").GetComponent<TextMesh> ().text = Global.spot [index].distance + "m";
	}
}
