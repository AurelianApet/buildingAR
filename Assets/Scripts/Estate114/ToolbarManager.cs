using UnityEngine;
using System.Collections;

using System.Runtime.InteropServices;
using System.IO;
using System;
using Vuforia;
using System.Data;
using System.Data.SqlClient;
using Mono.Data.Sqlite;
using System.Text;
using System.Runtime.Remoting.Contexts;
using LitJson;
using System.Collections.Generic;

public delegate void delegateVolume(string vol);

public class ToolbarManager : MonoBehaviour {
    //added for Google Analytics
    public GoogleAnalyticsV3 googleAnalytics;

	public GameObject loadingImage;
	public AudioClip sndBtnTouch;

	public GameObject btnPhoto;
	public GameObject btnCapture;
	public GameObject btnSetting;

	public GameObject lblRange;
	public GameObject lblType;

	public int ar_type = 1;
	public int ar_range = 100;
    public string selected_build_types = "";
    public string selected_land_types = "";
    public GameObject settingSelectPopup;


    public GameObject Vcamera;

	public GameObject txtImageCount;
	public GameObject txtImageBack;

	public GameObject m_texture2D;
	public List<GameObject> m_textures;

	public GameObject mainBackgroundWnd;
	public GameObject sliderWnd;
	public GameObject GalleryMainImage;
	public GameObject googlemapWnd;

	public GameObject deletePopupWnd;
	public GameObject loginPopupWnd;
	public GameObject verifyPopupWnd;
	public GameObject beforeLoginPopupWnd;
	public GameObject afterSavePopupWnd;
	public GameObject gyroPopuwnd;
	public GameObject uploadFailedWnd;

	public GameObject exitPopupWnd;
	public GameObject r114Wnd;
	public GameObject infoWnd;
	public GameObject guideWnd;

	//variables for Main Setting
	public GameObject SettingWnd;
	public GameObject[] typeBtns;
	public GameObject[] distanceBtns;
    public GameObject[] buildTypeBtns;
    public GameObject[] landTypeBtns;

	public UITexture galleryBtn;
	private bool galleryBtnChanged = false;
	private string gUploadUrl = "https://m.r114.com/?_c=ar&_p=h&_f=pic.ajax.inc";
	private string gLoginCheckUrl = "https://m.r114.com/?_c=ar&_m=loginState&_p=AJAX&";


	public GameObject scrollView;
	public GameObject gpsMessageBox;
	public GameObject internetMessageBox;

	public GameObject SelectedPhotoIdText;

	private string strLoginMessage = "1:Login";
	private string strVerifyMessage = "1:VerifyLogin";
	private string strSuccessMessage = "1:SuccessSaving";

	private bool isCapturing = true;
	private bool isOpeningGallery = true;
	private bool isClosingGallery = true;
	private bool isAddingMediaFinish = false;
	private bool isUploading = false;
	private bool changeGalleryImageFlag = true;

	//private bool movingGalleryFlag = false;
	private Vector2 mainImage_FirstPos = new Vector2(0,0);
	private Vector2 mainImage_LastPos = new Vector2(0,0);

	private Texture2D captureTexture;
	private Texture2D thumbTexture;
	private Texture2D captureBtn_texTmp;
	private DateTime GpsPopupTime;
	private DateTime TempTime;

	AudioFocusListener m_FocusListener;

	class AudioFocusListener : AndroidJavaProxy
	{
		public AudioFocusListener() : base("android.media.AudioManager$OnAudioFocusChangeListener") { }

		public bool m_HasAudioFocus = true;      

		public bool HasAudioFocus { get { return m_HasAudioFocus;} }

		public void onAudioFocusChange(int focus)
		{
			m_HasAudioFocus = (focus >= 0);
		}

		public string toString()
		{
			return "MyAwesomeAudioListener";
		}
	}

	#if UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern bool IsInstalledApp (string appUrl);
	#endif

	void Reset()
	{
		Debug.Log ("---------------------Reset-----------------");
	}

	void OnEnable()
	{
		Debug.Log ("---------------------Enable-----------------");
	}

	void OnDisable()
	{
		Debug.Log ("---------------------Disable-----------------");
	}

	void OnApplicationPause(bool status)
	{
		if (status == true) {
			if (guideWnd.activeInHierarchy == false && infoWnd.activeInHierarchy == false) {
				CancelInvoke ("RetrieveGPSData");
			}
		}
		else if (status == false) {
			if (guideWnd.activeInHierarchy == false && infoWnd.activeInHierarchy == false) {
				InvokeRepeating ("RetrieveGPSData", 0, 3);
			}
		}
	}

	void Awake()
	{
		#if UNITY_IPHONE
		Global.gHybridInstalled = IsInstalledApp("WebView://");

		if(Global.gHybridInstalled == true)
			Debug.Log("Awake:Hybrid state : true");

		#endif
	}

	// Use this for initialization
	void Start () {
        googleAnalytics.LogScreen("MainScene");
		//First call to main page for update
		WaitForUpdateAndStart();


	}

	public void WaitForUpdateAndStart()
	{
		if (SystemInfo.supportsGyroscope == false) {
			gyroPopuwnd.SetActive (true);
		} else {
			GpsPopupTime = Convert.ToDateTime ("2000-01-01 23:59:59");
			if ((float)Screen.height / (float)Screen.width > 2.0f) {//if (SystemInfo.deviceName.Contains ("s8")) {
				googlemapWnd.transform.FindChild ("Map/Compass").transform.localPosition = new Vector3 (18, 0, -28);
			}

			//guide wnd open code
			string guideprefString = "";
			if (PlayerPrefs.HasKey (Global.guideWndPrefabName))
				guideprefString = PlayerPrefs.GetString (Global.guideWndPrefabName);
			string nowTime = System.DateTime.Now.ToString ("yyyyMMdd");
			if (guideprefString != nowTime) {
				gpsMessageBox.SetActive (false);
				guideWnd.SetActive (true);
			} else {
				CheckForInfoWnd ();
			}
		}
	}

	public void CheckForInfoWnd()
	{
		//guide wnd open code
		gpsMessageBox.SetActive(false);
		string infoprefString = "";
		if (PlayerPrefs.HasKey (Global.infoWndPrefabName))
			infoprefString = PlayerPrefs.GetString (Global.infoWndPrefabName);
		string nowTime = System.DateTime.Now.ToString ("yyyyMMdd");
		if (infoprefString != nowTime) {
			infoWnd.SetActive (true);
		} else {
			InitFunction ();
		}
	}

	public void InitFunction()
	{
		googlemapWnd.transform.FindChild ("Camera").transform.gameObject.SetActive (true);

		captureTexture = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, true);
		thumbTexture = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, true);
		captureBtn_texTmp = new Texture2D (64, 64, TextureFormat.ARGB32, false);

		ReadGlobalFile ();

		#if UNITY_IPHONE
		Global.gHybridInstalled = IsInstalledApp("WebView://");

		if(Global.gHybridInstalled == true)
			Debug.Log("Awake:Hybrid state : true");
		#endif

		if (!PlayerPrefs.HasKey ("LastMeidaID")) {
			DeleteOldDataAndInfo ();
			Global.capturedImageCount = 0;
		}

		ReadFromMediaFile ();
		Global.capturedImageCount = Global.gMediaInfo.Count;

		ShowImageCount ();
		InvokeRepeating ("RetrieveGPSData", 0, 3);

		m_texture2D = (Resources.Load<GameObject> ("Prefabs/Texture"));

	}

	//Delete the old media files and media info text file
	public void DeleteOldDataAndInfo()
	{
		string dirPath = "";
		#if UNITY_IPHONE
		dirPath = Application.persistentDataPath + "/" + "Estate114";
		if (!Directory.Exists(dirPath))
		{
			Directory.CreateDirectory(dirPath);
		}
		#elif UNITY_ANDROID	
		dirPath = "mnt/sdcard/DCIM/" + "Estate114";	//"mnt/sdcard/DCIM/"
		if (!Directory.Exists(dirPath))
		{
			Directory.CreateDirectory(dirPath);
		}
		#else
		if( Application.isEditor == true ){ 
			dirPath = "mnt/sdcard/DCIM/";
			if (!Directory.Exists(dirPath))
			{
				Directory.CreateDirectory(dirPath);
			}
		} 
		#endif

		var fileInfo =Directory.GetFiles(dirPath);
		int file_count = fileInfo.Length;

		for(int i=0;i<file_count;i++)
		{
			File.Delete (fileInfo [i]);
		}

		#if UNITY_IPHONE
		dirPath = Application.persistentDataPath + "/" + "thumb";
		if (!Directory.Exists(dirPath))
		{
			Directory.CreateDirectory(dirPath);
		}
		#elif UNITY_ANDROID	
		dirPath = "mnt/sdcard/DCIM/" + "thumb";
		if (!Directory.Exists(dirPath))
		{
			Directory.CreateDirectory(dirPath);
		}
		#endif

		var thumbfileInfo =Directory.GetFiles(dirPath);
		int thumbfile_count = thumbfileInfo.Length;
		for(int i=0;i<thumbfile_count;i++)
		{
			File.Delete (thumbfileInfo [i]);
		}


		dirPath = "";
		#if UNITY_IPHONE
		dirPath = Application.persistentDataPath + "/" + "estate";
		#elif UNITY_ANDROID
		dirPath = "mnt/sdcard/estate";
		#endif

		if (!Directory.Exists (dirPath))
			Directory.CreateDirectory (dirPath);

		if(File.Exists(dirPath + Global.mediaInfoFileName))
			File.WriteAllText (dirPath + Global.mediaInfoFileName,"");
	}

    //보관된 파일로 부터 이미지들을 로드 하는 함수
	public void ReadFromMediaFile()
	{
		string dirPath = "";
		string content = "";

		#if UNITY_IPHONE
		dirPath = Application.persistentDataPath + "/" + "estate";
		#elif UNITY_ANDROID	
		dirPath = "mnt/sdcard/estate";
		#endif
		if (!Directory.Exists(dirPath))
		{
			Global.gMediaInfo = new List<Global.MediaInfoStruct> ();
			return;
		}
		else
		{
			if (!File.Exists (dirPath + "/" + Global.mediaInfoFileName)) {
				Global.gMediaInfo = new List<Global.MediaInfoStruct> ();
				return;
			}
			content = File.ReadAllText(dirPath + "/" + Global.mediaInfoFileName);
			Global.strGlobalInfoFile = content;
			string[] seperator = { "\r\n" };
			string[] array_tmp = content.Split (seperator, StringSplitOptions.None);

			//Global.gMediaInfo = new Global.MediaInfoStruct[array_tmp.Length-1];
			Global.gMediaInfo = new List<Global.MediaInfoStruct> ();

			//Debug.Log ("Image Count : " + Global.gMediaInfo.Count.ToString ());

			for(int i=0;i<array_tmp.Length-1;i++)
			{
				//file saved type : id, filename, cookie, user_id, position_x, position_y, b_code, j_code, photo_date, media_gubun, gubun, gubun_code, address
				string[] main_seperator = {","};

				string[] media_array = array_tmp[i].Split(main_seperator, StringSplitOptions.None);

				if (media_array.Length > 10) {				//정보가 있으면
					Global.MediaInfoStruct mediainfoTemp = new Global.MediaInfoStruct();
					mediainfoTemp.media_id = Convert.ToInt32 (media_array [0]);
					mediainfoTemp.file_name = media_array [1];
					mediainfoTemp.cookie = media_array [2];
					mediainfoTemp.user_id = media_array [3];
					mediainfoTemp.position_x = media_array [4];
					mediainfoTemp.position_y = media_array [5];
					mediainfoTemp.b_code = media_array [6];
					mediainfoTemp.j_code = media_array [7];
					mediainfoTemp.photo_date = media_array [8];
					mediainfoTemp.media_gubun = media_array [9];
					mediainfoTemp.gubun = media_array [10];
					mediainfoTemp.gubun_code = media_array [11];
					mediainfoTemp.address = media_array [12];

					Global.gMediaInfo.Add (mediainfoTemp);
				}
			}
		}
		
	}

    //아이디로 부터 정보를 보내주는 함수
	public string[] GetSelectedMediaData(int media_id)
	{
		string[] return_array = new string[12];
		for (int i = 0; i < Global.gMediaInfo.Count; i++) {
			if (media_id == Global.gMediaInfo [i].media_id) {
				return_array [0] = Global.gMediaInfo [i].file_name;	return_array [1] = Global.gMediaInfo [i].cookie;	return_array [2] = Global.gMediaInfo [i].user_id;	return_array [3] = Global.gMediaInfo [i].position_x;
				return_array [4] = Global.gMediaInfo [i].position_y;	return_array [5] = Global.gMediaInfo [i].b_code;	return_array [6] = Global.gMediaInfo [i].j_code;	return_array [7] = Global.gMediaInfo [i].photo_date;
				return_array [8] = Global.gMediaInfo [i].media_gubun;	return_array [9] = Global.gMediaInfo [i].gubun;	return_array [10] = Global.gMediaInfo [i].gubun_code;	return_array [11] = Global.gMediaInfo [i].address;
				return return_array;
			}
		}
			
		return new string[]{""};
	}

    //메디어 정보를 추가하는 함수
	public void AddMediaInfoToFile(int id,string file_name, string cookie, string user_id, string positionx, string positiony, string bcode, string jcode, string photodate, string mediagubun, string gubun, string gubuncode, string address)
	{
		Debug.Log ("Start to Add Media Info");
		Global.MediaInfoStruct mediainfoTemp = new Global.MediaInfoStruct();
		Debug.Log("----------id----------" + id);
		mediainfoTemp.media_id = id;
		Debug.Log("----------file_name----------" + file_name);
		mediainfoTemp.file_name = file_name;
		Debug.Log("----------cookie----------" + cookie);
		mediainfoTemp.cookie = cookie;
		Debug.Log("----------user_id----------" + user_id);
		mediainfoTemp.user_id = user_id;
		Debug.Log("----------positionx----------" + positionx);
		mediainfoTemp.position_x = positionx;
		Debug.Log("----------positiony----------" + positiony);
		mediainfoTemp.position_y = positiony;
		Debug.Log("----------bcode----------" + bcode);
		mediainfoTemp.b_code = bcode;
		Debug.Log("----------jcode----------" + jcode);
		mediainfoTemp.j_code = jcode;
		Debug.Log("----------photodate----------" + photodate);
		mediainfoTemp.photo_date = photodate;
		Debug.Log("----------mediagubun----------" + mediagubun);
		mediainfoTemp.media_gubun = mediagubun;
		Debug.Log("----------gubun----------" + gubun);
		mediainfoTemp.gubun = gubun;
		Debug.Log("----------gubuncode----------" + gubuncode);
		mediainfoTemp.gubun_code = gubuncode;
		Debug.Log("----------address----------" + address);
		mediainfoTemp.address = address;

		#if UNITY_IPHONE
		if(Global.gMediaInfo == null)
			Global.gMediaInfo = new List<Global.MediaInfoStruct> ();
		#endif
		Global.gMediaInfo.Add (mediainfoTemp);

		string content = id.ToString() + ",";
		content += file_name + ",";		content += cookie + ",";		content += user_id + ",";		content += positionx + ",";		content += positiony + ",";		content += bcode + ",";		content += jcode + ",";
		content += photodate + ",";		content += mediagubun + ",";		content += gubun + ",";		content += gubuncode + ",";		content += address + ",";
		content += "\r\n";

		Global.strGlobalInfoFile += content;

		string dirPath = "";
		#if UNITY_IPHONE
		dirPath = Application.persistentDataPath + "/" + "estate";
		#elif UNITY_ANDROID
		dirPath = "mnt/sdcard/estate";
		#endif

		if (!Directory.Exists (dirPath))
			Directory.CreateDirectory (dirPath);

		File.WriteAllText (dirPath + Global.mediaInfoFileName,Global.strGlobalInfoFile);
	}

    //메디오 정보에서 선택된 메디오의 내용을 지우는 함수
	public void DeleteMediaInfoFromFile()
	{
		string str_content = "";
		Global.gMediaInfo.RemoveAt (Global.selectedPhotoId);
		for (int i = 0; i < Global.gMediaInfo.Count; i++) {
			//if (i != Global.selectedPhotoId) {
				str_content += Global.gMediaInfo [i].media_id.ToString() + ",";
				str_content += Global.gMediaInfo [i].file_name + ",";	str_content += Global.gMediaInfo [i].cookie + ",";	str_content += Global.gMediaInfo [i].user_id + ",";	str_content += Global.gMediaInfo [i].position_x + ",";
				str_content += Global.gMediaInfo [i].position_y + ",";	str_content += Global.gMediaInfo [i].b_code + ",";	str_content += Global.gMediaInfo [i].j_code + ",";	str_content += Global.gMediaInfo [i].photo_date + ",";
				str_content += Global.gMediaInfo [i].media_gubun + ",";	str_content += Global.gMediaInfo [i].gubun + ",";	str_content += Global.gMediaInfo [i].gubun_code + ",";	str_content += Global.gMediaInfo [i].address + ",";
				str_content += "\r\n";
			//}
		}

		string dirPath = "";
		#if UNITY_IPHONE
		dirPath = Application.persistentDataPath + "/" + "estate";
		#elif UNITY_ANDROID
		dirPath = "mnt/sdcard/estate";
		#endif

		if (!Directory.Exists (dirPath))
			Directory.CreateDirectory (dirPath);

		File.WriteAllText (dirPath + Global.mediaInfoFileName,str_content);
	}

    // 현재 GPS 상태를 돌려주는 함수
	void RetrieveGPSData()
	{
		#if UNITY_IPHONE
		//if(Input.location.isEnabledByUser)
		{
			if (Input.location.status != LocationServiceStatus.Running) {
				Global.bGPSStatus = false;
			} else {
				if (Input.location.lastData.longitude == null || Input.location.lastData.latitude == null)
					Global.bGPSStatus = false;
				else {
					Debug.Log ("------------------GPS Value = " + Input.location.lastData.longitude + " , " + Input.location.lastData.latitude + "---");
					if (Input.location.lastData.latitude == 0.0f && Input.location.lastData.longitude == 0.0f) {
						Global.bGPSStatus = false;
					}
					else
						Global.bGPSStatus = true;
				}
			}
		}
		/*else
		{
			Global.bGPSStatus = false;	
		}*/
		#elif UNITY_ANDROID
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject curActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
		string gpsStatus = curActivity.Call<string> ("checkGPSSettingStatus", "");

		if(gpsStatus == "YES")
			Global.bGPSStatus = true;
		else if(gpsStatus == "NO")
			Global.bGPSStatus = false;
		
		jc.Dispose();
		curActivity.Dispose();
		#endif

		if(Input.location.status == LocationServiceStatus.Failed)
			Debug.Log ("------------------GPS Flag = Fail-------------");
		else if(Input.location.status == LocationServiceStatus.Initializing)
			Debug.Log ("------------------GPS Flag = Initializing-------------");
		else if(Input.location.status == LocationServiceStatus.Running)
			Debug.Log ("------------------GPS Flag = Running-------------");
		else if(Input.location.status == LocationServiceStatus.Stopped)
			Debug.Log ("------------------GPS Flag = Stopped-------------");
		Debug.Log ("------------------GPS Flag = " + ((Global.bGPSStatus == true) ? "true" : "false") + "-------------");
	}
	
	public void onBtnDeleteCancel()
	{
		deletePopupWnd.SetActive (false);
	}
	
    //이미지를 갤러리 뷰의 Texture에 로드하는 함수
	IEnumerator LoadImages()
	{
		string[] files;

		string path = "";

		#if UNITY_ANDROID
		path = "mnt/sdcard/DCIM/" + "thumb";
		#elif UNITY_IPHONE
		path = Application.persistentDataPath+"/thumb";
		#endif
		files = System.IO.Directory.GetFiles (path,"*.png");
		Debug.Log ("------------------1--------------");
		DateTime[] creationTimes = new DateTime[files.Length];
		for (int i = 0; i < files.Length; i++)
			creationTimes[i] = new FileInfo(files[i]).CreationTime;
		Array.Sort(creationTimes, files);

		creationTimes = null;

		Array.Reverse (files);

		if(m_textures != null)
			m_textures.Clear ();
		else
			m_textures = new List<GameObject> ();

		Debug.Log ("------------------2--------------");
		string prePath = Application.dataPath;
		#if UNITY_IPHONE
		prePath = @"file://";
		#elif UNITY_ANDROID	
		//prePath = @"file://" + Application.dataPath.Replace("/Assets","/");
		prePath = @"file:///";
		#else
		prePath = @"file://" + Application.dataPath.Replace("/Assets","/");
		#endif

		int dummy = 0;
		Debug.Log ("------------------Start--------------");


		for(int i=0; i<files.Length;i++){
			string tstring = files[i];
			string pathTemp = tstring;
			WWW www = new WWW (prePath + pathTemp);
			yield return www;
			Texture2D texTmp = new Texture2D (64, 64, TextureFormat.ARGB32, false);	
			www.LoadImageIntoTexture (texTmp);

			Debug.Log ("------------------aaaaa--------------");
			GameObject textureTmp;
			#if UNITY_IPHONE
			if(m_texture2D == null)
			{
				Debug.Log ("------------------bbbb--------------");
				m_texture2D = (Resources.Load<GameObject> ("Prefabs/Texture"));
			}
			#endif
			textureTmp = (GameObject)UnityEngine.Object.Instantiate(m_texture2D, new Vector3(-310f + 100f * dummy, 0f, 0f), Quaternion.identity);

			if (dummy == 0) {
				#if UNITY_IPHONE
				if(captureTexture == null)
					captureTexture = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, true);
				#endif
				string mainPath = pathTemp.Replace ("thumb", "Estate114");
				WWW main_www = new WWW (prePath + mainPath);
				yield return main_www;
				main_www.LoadImageIntoTexture (captureTexture);
				Debug.Log ("------------------cccc--------------");
				GalleryMainImage.transform.GetComponent<UITexture> ().mainTexture = captureTexture;
			}
			
			textureTmp.transform.GetComponent<SetHighLight> ().selected_texture_id = dummy;
			Debug.Log ("------------------dddd--------------");
			textureTmp.transform.GetComponent<UITexture> ().mainTexture = texTmp;
			textureTmp.transform.parent = GameObject.Find ("UI Root/PhotoSlider/ScrollView/UIGrid").gameObject.transform;
			textureTmp.transform.GetComponent<Transform> ().localPosition = new Vector3 (-310f + 100f * dummy, 0f, 0f);
			textureTmp.transform.GetComponent<Transform>().localScale = new Vector3 (1.0f, 1.0f, 1.0f);

			textureTmp.SetActive(true);
	Debug.Log ("------------------dummy--------------" + dummy);	
			m_textures.Add (textureTmp);
			dummy++;

			texTmp = null;
			Destroy (texTmp);
		}
		

		Global.selectedPhotoId = 0;
		m_textures [0].transform.GetComponent<SetHighLight> ().ShowHighLights ();

		Vector3 pos = scrollView.transform.localPosition;
		pos.x = 0;
		Vector2 vec2DTemp = scrollView.transform.GetComponent<UIPanel> ().clipOffset;
		vec2DTemp.x = 0;
		scrollView.transform.localPosition = pos;
		scrollView.transform.GetComponent<UIPanel> ().clipOffset = vec2DTemp;

		if (files.Length == 0) {
			GameObject.Find ("UI Root/PhotoSlider/PhotoCaption").transform.GetComponent<UILabel> ().text = "";
		} else {
			GameObject.Find ("UI Root/PhotoSlider/PhotoCaption").transform.GetComponent<UILabel> ().text = Global.gMediaInfo[Global.selectedPhotoId].address;
		}
		Debug.Log ("------------------4--------------");
		isOpeningGallery = true;
	}

    //갤러리에 표시했던 Texture 객체들을 다 지우는 함수
	public void DeleteObjectsInGallery()
	{
		GameObject parentObject = GameObject.Find ("UI Root").transform.FindChild("PhotoSlider").transform.FindChild("ScrollView").transform.FindChild("UIGrid").transform.gameObject;
		foreach (Transform child in parentObject.transform) {
			GameObject.DestroyObject (child.gameObject);
		}
	}

	public void onbtnPhotoGallery()
	{
        googleAnalytics.LogScreen("PhotoGalleryBtn");

        if (txtImageCount.transform.GetComponent<UILabel> ().text != "0" && Global.isPhotoPopup == false) {
			Global.isPhotoPopup = true;
			mainBackgroundWnd.SetActive (false);
			googlemapWnd.transform.FindChild ("Camera").transform.gameObject.SetActive (false);
			Vcamera.SetActive (false);

			sliderWnd.SetActive (true);

			//loading image action
			isOpeningGallery = false;
			StartCoroutine (PlayLoadingImage ());

			StartCoroutine ("LoadImages");
		}
	}
		
	IEnumerator CloseGalleryWnd()
	{
		sliderWnd.SetActive(false);
		DeleteObjectsInGallery ();
		mainBackgroundWnd.SetActive (true);
		googlemapWnd.transform.FindChild ("Camera").transform.gameObject.SetActive (true);
		Vcamera.SetActive (true);
		yield return new WaitForSeconds (0.1f);
		Global.isPhotoPopup = false;
		isClosingGallery = true;
	}

	public void onbtnCloseGallery()
	{
		isClosingGallery = false;
		StartCoroutine (PlayLoadingImage ());
		StartCoroutine ("CloseGalleryWnd");
	}

    //로딩 이미지 현시
	IEnumerator PlayLoadingImage()
	{
		int lot_y = 0;
		loadingImage.SetActive (true);
		while (isCapturing == false || isOpeningGallery == false || isClosingGallery == false || isUploading == true || changeGalleryImageFlag == false) {
			lot_y += 5;
			loadingImage.transform.FindChild ("Loading").transform.localRotation = Quaternion.Euler (0, 0, -lot_y);
			yield return new WaitForSeconds (0.03f);
		}
			
		loadingImage.SetActive (false);
	}

	public void WriteLoginState(string strContent)
	{
		string dirPath = "";
		#if UNITY_IPHONE
		dirPath = Application.persistentDataPath + "/" + "estate";
		#endif

		if (!Directory.Exists (dirPath))
			Directory.CreateDirectory (dirPath);

		File.WriteAllText (dirPath + Global.globalInfoFileName,strContent);
	}

	public static void WriteGlobalFile(string strStatus)
	{
		string dirPath = "";
		#if UNITY_IPHONE
		dirPath = Application.persistentDataPath + "/" + "estate";
		#elif UNITY_ANDROID
		dirPath = "mnt/sdcard/estate";
		#endif

		if (!Directory.Exists (dirPath))
			Directory.CreateDirectory (dirPath);

		File.WriteAllText (dirPath + Global.arAppInfoFileName,strStatus);
	}

	public void ReadGlobalFile()
	{
		string dirPath = "";
		string content = "";

		#if UNITY_IPHONE
		//dirPath = Application.persistentDataPath + "/" + "estate";
		string passedData = "";
		passedData = PlayerPrefs.GetString("param");
		Debug.Log("Player Prefs > param :" + passedData);
		string[] seperaters = {"=","&"};
		if(passedData != "")
		{
			string[] array_temp = passedData.Split(seperaters, StringSplitOptions.None);
			if(array_temp.Length > 2)
			{
				Global.user_id = array_temp[1];
				Global.cookie_token = array_temp[3];
			}
		}
		#elif UNITY_ANDROID	
		dirPath = "mnt/sdcard/estate";

		if (!Directory.Exists(dirPath))
		{
			return;
		}
		else
		{
			if (!File.Exists (dirPath + Global.globalInfoFileName))
				return;
			content = File.ReadAllText(dirPath + "/" + Global.globalInfoFileName);
			string[] seperator = { "$$" };
			string[] array_tmp = content.Split (seperator, StringSplitOptions.None);
			if (array_tmp.Length == 3) {				//have status, userid, token
				Global.user_id = array_tmp [1].Split (new string[]{":"}, StringSplitOptions.None) [1];
				Global.cookie_token = array_tmp [2].Split (new string[]{":"}, StringSplitOptions.None) [1];
			}
		}
		#endif
	}

	public void ShowImageCount(int type = 0)
	{
		if (galleryBtnChanged == false) {
			int file_count = Global.capturedImageCount;

			if (file_count.ToString ().Length < 4) {
				txtImageBack.transform.GetComponent<UISprite> ().spriteName = "bg" + file_count.ToString ().Length;
				txtImageBack.transform.GetComponent<UISprite> ().width = 34 + (file_count.ToString ().Length - 1) * 10;
				txtImageBack.transform.GetComponent<UISprite> ().height = 36;
			} else {
				txtImageBack.transform.GetComponent<UISprite> ().width = 36 + (file_count.ToString ().Length - 1) * 30;
				txtImageBack.transform.GetComponent<UISprite> ().height = 36 + (file_count.ToString ().Length - 1) * 2;
			}

			txtImageCount.transform.GetComponent<UILabel> ().width = 35 + (file_count.ToString ().Length - 1) * 30;
			Vector3 newPos = txtImageCount.transform.localPosition;
			newPos.x = 34 + (file_count.ToString ().Length - 1) * 5;
			txtImageCount.transform.localPosition = newPos;

			txtImageCount.transform.GetComponent<UILabel> ().width = 35 + (file_count.ToString ().Length - 1) * 10;

			txtImageCount.transform.GetComponent<UILabel> ().text = file_count.ToString ();
			if(type == 0)
				StartCoroutine (LoadTextureToGalleryBtn(file_count));
		}
	}
		

	IEnumerator LoadTextureToGalleryBtn(int file_count)
	{
		if (file_count == 0) {
			galleryBtn.mainTexture = Resources.Load<Texture2D>("Design/photogallery/btnGallery");
		} else {
			string[] files;
			string path = "";
			#if UNITY_ANDROID
			path = "mnt/sdcard/DCIM/" + "thumb";
			#elif UNITY_IPHONE
			path = Application.persistentDataPath + "/thumb";
			#endif
			files = System.IO.Directory.GetFiles (path, "*.png");

			DateTime[] creationTimes = new DateTime[files.Length];
			for (int i = 0; i < files.Length; i++)
				creationTimes[i] = new FileInfo(files[i]).CreationTime;
			Array.Sort(creationTimes, files);
			
			creationTimes = null;

			Array.Reverse (files);
			
			string prePath;
			#if UNITY_IPHONE
			prePath = @"file://";
			#elif UNITY_ANDROID	
			prePath = @"file:///";
			#else
			prePath = @"file://" + Application.dataPath.Replace("/Assets","/");
			#endif

			string pathTemp = files [0];
			WWW www = new WWW (prePath + pathTemp);
			yield return www;

			Texture2D texTmp = new Texture2D (64, 64, TextureFormat.ARGB32, false);
			www.LoadImageIntoTexture (texTmp);
			galleryBtn.mainTexture = texTmp;

			texTmp = null;
			Destroy (texTmp);
		}
		galleryBtnChanged = true;
		isCapturing = true;

		GC.Collect ();
	}

	public void ExitOkWnd()
	{
		WriteGlobalFile ("0:Close");
		Application.Quit ();
	}

	public void ExitCancelWnd()
	{
		exitPopupWnd.SetActive (false);
		Global.isExitPopup = false;
	}

	// Update is called once per frame
    //상세 정보 팝업 현시시 스티커들의 depth 처리 진행
	private void Update () {

		if (guideWnd.activeInHierarchy == false && infoWnd.activeInHierarchy == false) {
			if (Application.platform == RuntimePlatform.Android) {
				if (Input.GetKey (KeyCode.Escape)) {
					GameObject detailWnd = GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").gameObject;
					if (detailWnd == null || detailWnd.activeInHierarchy == false) {
						if (Global.isPhotoPopup) {
							Debug.Log ("Back button clicked on Photo Gallery");
							isClosingGallery = false;
							StartCoroutine (PlayLoadingImage ());
							StartCoroutine ("CloseGalleryWnd");
						} else {
							if (Global.isExitPopup == false) {
								exitPopupWnd.SetActive (true);
								Global.isExitPopup = true;
								Debug.Log ("Back button once clicked");
							} else {
							}
						}
					}
				}
			}

			if (gpsMessageBox.activeInHierarchy == false) {
				TempTime = System.DateTime.Now;
			}

			if (guideWnd.activeInHierarchy == false && infoWnd.activeInHierarchy == false) {
				//Show the GPS Message Box
				if (Global.bGPSStatus == true) {
					gpsMessageBox.SetActive (false);
				} else if (gpsMessageBox.activeInHierarchy == false) {
					//GpsPopupTime = System.DateTime.Now.
					TimeSpan dateDiff = TempTime - GpsPopupTime;
					if (dateDiff.Seconds >= 5)
						gpsMessageBox.SetActive (true);
				}
				
				//Show the Internet Message Box
				if (Application.internetReachability == NetworkReachability.NotReachable)
					internetMessageBox.SetActive (true);
				else
					internetMessageBox.SetActive (false);

				if (Global.isPhotoPopup == true) {
					if (Global.gMediaInfo.Count == 0)
						SelectedPhotoIdText.gameObject.GetComponent<UILabel> ().text = "0/0";
					else
						SelectedPhotoIdText.gameObject.GetComponent<UILabel> ().text = (Global.selectedPhotoId + 1).ToString () + "/" + Global.gMediaInfo.Count.ToString ();
				}

				//set the depth of the background and other useful buttons.
				if (Global.spot != null) {
					int temp = Global.spot.Length * 3 + 8;
						
					GameObject.Find ("UI Root").transform.FindChild ("background").GetComponent<UISprite> ().depth = temp + 1;
					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("sp114").transform.GetComponent<UISprite> ().depth = temp + 2;
					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("address").transform.GetComponent<UILabel> ().depth = temp + 2;
					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("btnClose").transform.FindChild ("Background").GetComponent<UISprite> ().depth = temp + 2;
					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("btnPhoto").transform.FindChild ("CountBack").GetComponent<UISprite> ().depth = temp + 4;
					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("btnPhoto").transform.FindChild ("Texture").GetComponent<UITexture> ().depth = temp + 3;
					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("btnPhoto").transform.FindChild ("lblPhotoCount").GetComponent<UILabel> ().depth = temp + 5;

					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("btnCapture").transform.FindChild ("Background").GetComponent<UISprite> ().depth = temp + 2;

					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("btnSetting").transform.FindChild ("Background").GetComponent<UISprite> ().depth = temp + 2;
					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("btnSetting").transform.FindChild ("lblRange").GetComponent<UILabel> ().depth = temp + 3;
					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("btnSetting").transform.FindChild ("lblType").GetComponent<UILabel> ().depth = temp + 3;

				}

				if (Global.isPhotoPopup == true) {
					GameObject.Find ("UI Root").transform.FindChild ("background").GetComponent<UISprite> ().depth = 1;
					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("sp114").transform.GetComponent<UISprite> ().depth = 2;
					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("address").transform.GetComponent<UILabel> ().depth = 2;
					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("btnClose").transform.FindChild ("Background").GetComponent<UISprite> ().depth = 2;
					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("btnPhoto").transform.FindChild ("CountBack").GetComponent<UISprite> ().depth = 4;
					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("btnPhoto").transform.FindChild ("Texture").GetComponent<UITexture> ().depth = 3;
					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("btnPhoto").transform.FindChild ("lblPhotoCount").GetComponent<UILabel> ().depth = 5;

					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("btnCapture").transform.FindChild ("Background").GetComponent<UISprite> ().depth = 2;

					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("btnSetting").transform.FindChild ("Background").GetComponent<UISprite> ().depth = 2;
					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("btnSetting").transform.FindChild ("lblRange").GetComponent<UILabel> ().depth = 3;
					GameObject.Find ("UI Root").transform.FindChild ("background").transform.FindChild ("btnSetting").transform.FindChild ("lblType").GetComponent<UILabel> ().depth = 3;

				}
			}
		}
	}

	public void onBtnClose(){
		exitPopupWnd.SetActive (true);
		Global.isExitPopup = true;
		Debug.Log ("Back button once clicked");
	}

    //설정창 현시시 기존 정보들을 로드 하는 함수
	public void onBtnSetting(){
        googleAnalytics.LogScreen("SettingBtn");

        googlemapWnd.transform.FindChild ("Camera").transform.gameObject.SetActive (false);

		ar_type = Global.ar_type;
		ar_range = Global.range;
        selected_land_types = Global.selected_land_types;
        selected_build_types = Global.selected_build_types;

		for (int i = 0; i < typeBtns.Length; i++) {
			typeBtns [i].SetActive (false);
		}
		typeBtns [Global.ar_type - 1].SetActive (true);
        string[] seperator = { "," };

        if (Global.ar_type == 1) // build
        {
            SettingWnd.transform.FindChild("SettingMain/BuildSetting").gameObject.SetActive(true);
            SettingWnd.transform.FindChild("SettingMain/LandSetting").gameObject.SetActive(false);
            if (Global.selected_build_types != "")
            {
                for (int i = 0; i < buildTypeBtns.Length; i++)
                {
                    buildTypeBtns[i].SetActive(false);
                }

                string[] selected_texts = Global.selected_build_types.Split(seperator, StringSplitOptions.None);
                for(int i=0;i<selected_texts.Length - 1;i++)
                {
                    int type = System.Convert.ToInt32(selected_texts[i]) - 1;
                    buildTypeBtns[type].SetActive(true);
                }
            }
        }else                   // land
        {
            SettingWnd.transform.FindChild("SettingMain/BuildSetting").gameObject.SetActive(false);
            SettingWnd.transform.FindChild("SettingMain/LandSetting").gameObject.SetActive(true);
            if (Global.selected_land_types != "")
            {
                for (int i = 0; i < landTypeBtns.Length; i++)
                {
                    landTypeBtns[i].SetActive(false);
                }

                string[] selected_texts = Global.selected_land_types.Split(seperator, StringSplitOptions.None);
                for (int i = 0; i < selected_texts.Length - 1; i++)
                {
                    int type = System.Convert.ToInt32(selected_texts[i]) - 1;
                    landTypeBtns[type].SetActive(true);
                }
            }
        }

		for (int i = 0; i < distanceBtns.Length; i++) {
			distanceBtns [i].SetActive (false);
		}
		distanceBtns [Global.range/100].SetActive (true);
		SettingWnd.SetActive (true);
		Global.isSettingPopup = true;
	}

	public void onBtnType1()
	{
		typeBtns [0].SetActive (true);
		typeBtns [1].SetActive (false);
		ar_type = 1;

        SettingWnd.transform.FindChild("SettingMain/BuildSetting").gameObject.SetActive(true);
        SettingWnd.transform.FindChild("SettingMain/LandSetting").gameObject.SetActive(false);

        for (int i = 0; i < landTypeBtns.Length; i++)
        {
            landTypeBtns[i].SetActive(true);
        }

        SettingWnd.transform.FindChild("TotalBtn/Text").GetComponent<UILabel>().text = "전체 선택해제";

        Global.selected_land_types = "1,2,3,4,5,6,7,8,9,";
        selected_land_types = Global.selected_land_types;
    }

	public void onBtnType2()
	{
		typeBtns [1].SetActive (true);
		typeBtns [0].SetActive (false);
		ar_type = 2;

        SettingWnd.transform.FindChild("SettingMain/BuildSetting").gameObject.SetActive(false);
        SettingWnd.transform.FindChild("SettingMain/LandSetting").gameObject.SetActive(true);

        for (int i = 0; i < buildTypeBtns.Length; i++)
        {
            buildTypeBtns[i].SetActive(true);
        }

        SettingWnd.transform.FindChild("TotalBtn/Text").GetComponent<UILabel>().text = "전체 선택해제";

        Global.selected_build_types = "1,2,3,4,5,6,";
        selected_build_types = Global.selected_build_types;
    }

    public void onBuildBtnType(int type)
    {
        if (buildTypeBtns[type].activeInHierarchy == true)
        {
            string[] seperator = { "," };
            string[] array_tmp = selected_build_types.Split(seperator, StringSplitOptions.None);
            if (array_tmp.Length > 2)
            {
                buildTypeBtns[type].SetActive(false);
                selected_build_types = selected_build_types.Replace((type + 1) + ",", "");
            }

            SettingWnd.transform.FindChild("TotalBtn/Text").GetComponent<UILabel>().text = "전체 선택";
        }
        else
        {
            buildTypeBtns[type].SetActive(true);
            selected_build_types += (type+1) + ",";

            bool check_flag = false;
            for(int i=0; i<buildTypeBtns.Length;i++)
            {
                if(buildTypeBtns[i].activeInHierarchy == false)
                {
                    check_flag = true;
                    break;
                }
            }

            if(check_flag == false)
                SettingWnd.transform.FindChild("TotalBtn/Text").GetComponent<UILabel>().text = "전체 선택해제";
        }
    }

    public void onLandBtnType(int type)
    {
        if (landTypeBtns[type].activeInHierarchy == true)
        {
            string[] seperator = { "," };
            string[] array_tmp = selected_land_types.Split(seperator, StringSplitOptions.None);
            if (array_tmp.Length > 2)
            {
                landTypeBtns[type].SetActive(false);
                selected_land_types = selected_land_types.Replace((type + 1) + ",", "");
            }
            SettingWnd.transform.FindChild("TotalBtn/Text").GetComponent<UILabel>().text = "전체 선택";
        }
        else
        {
            landTypeBtns[type].SetActive(true);
            selected_land_types += (type+1) + ",";

            bool check_flag = false;
            for (int i = 0; i < landTypeBtns.Length; i++)
            {
                if (landTypeBtns[i].activeInHierarchy == false)
                {
                    check_flag = true;
                    break;
                }
            }

            if (check_flag == false)
                SettingWnd.transform.FindChild("TotalBtn/Text").GetComponent<UILabel>().text = "전체 선택해제";
        }
    }

    public void onBtnRange1()
	{
		distanceBtns [0].SetActive (true);	distanceBtns [1].SetActive (false);	distanceBtns [2].SetActive (false);
		ar_range = 50;
	}

	public void onBtnRange2()
	{
		distanceBtns [0].SetActive (false);	distanceBtns [1].SetActive (true);	distanceBtns [2].SetActive (false);
		ar_range = 100;
	}

	public void onBtnRange3()
	{
		distanceBtns [0].SetActive (false);	distanceBtns [1].SetActive (false);	distanceBtns [2].SetActive (true);
		ar_range = 200;
	}

    public void onSettingBtnTotal()
    {
        if (typeBtns[0].activeInHierarchy == true)       //build case
        {
            if (SettingWnd.transform.FindChild("TotalBtn/Text").GetComponent<UILabel>().text == "전체 선택해제")
            {
                SettingWnd.transform.FindChild("TotalBtn/Text").GetComponent<UILabel>().text = "전체 선택";
                for (int i = 0; i < buildTypeBtns.Length; i++)
                {
                    buildTypeBtns[i].SetActive(false);
                }
                selected_build_types = "";
            } else
            {
                SettingWnd.transform.FindChild("TotalBtn/Text").GetComponent<UILabel>().text = "전체 선택해제";
                selected_build_types = "";
                for (int i = 0; i < buildTypeBtns.Length; i++)
                {
                    buildTypeBtns[i].SetActive(true);
                    selected_build_types += (i+1).ToString() + ",";
                }
            }
        }
        else                                            //land case
        {
            if (SettingWnd.transform.FindChild("TotalBtn/Text").GetComponent<UILabel>().text == "전체 선택해제")
            {
                SettingWnd.transform.FindChild("TotalBtn/Text").GetComponent<UILabel>().text = "전체 선택";
                for (int i = 0; i < landTypeBtns.Length; i++)
                {
                    landTypeBtns[i].SetActive(false);
                }
                selected_land_types = "";
            }
            else
            {
                SettingWnd.transform.FindChild("TotalBtn/Text").GetComponent<UILabel>().text = "전체 선택해제";
                selected_land_types = "";
                for (int i = 0; i < landTypeBtns.Length; i++)
                {
                    landTypeBtns[i].SetActive(true);
                    selected_land_types += (i + 1).ToString() + ",";
                }
            }
        }
    }

    public void onCloseSettingPopup()
    {
        settingSelectPopup.SetActive(false);
    }

	public void onOkSetting()
	{
        if (ar_type == 1)
        {
            if(selected_build_types == "")
            {
                settingSelectPopup.SetActive(true);
                return;
            }
        }
        else if (ar_type == 2)
        {
            if(selected_land_types == "")
            {
                settingSelectPopup.SetActive(true);
                return;
            }
        }

        Global.ar_type = ar_type;
		Global.range = ar_range;
        Global.setting_changed = true;

        Debug.Log("---------------a-a--------a---------");
        Global.selected_build_types = selected_build_types;
        Global.selected_land_types = selected_land_types;

        if (Global.ar_type == 1)
            Global.selected_land_types = "1,2,3,4,5,6,7,8,9,";
        else
            Global.selected_build_types = "1,2,3,4,5,6,";
        SettingWnd.SetActive (false);
        Debug.Log("---------------b-b--------b---------");
        lblRange.GetComponent<UILabel> ().text = "0~" + Global.range.ToString() + "m";
		if (Global.ar_type == 1) {
			lblType.GetComponent<UILabel> ().text = "건물";
		} else if (Global.ar_type == 2) {
			lblType.GetComponent<UILabel> ().text = "토지";
		}

		googlemapWnd.transform.FindChild ("Camera").transform.gameObject.SetActive (true);
		Global.isSettingPopup = false;

		switch (Global.range) {
		case 50:
			googlemapWnd.transform.FindChild ("Map").transform.GetComponent<OnlineMaps> ().zoom = 18;
			break;
		case 100:
			googlemapWnd.transform.FindChild ("Map").transform.GetComponent<OnlineMaps> ().zoom = 18;
			break;
		case 200:
			googlemapWnd.transform.FindChild ("Map").transform.GetComponent<OnlineMaps> ().zoom = 17;
			break;
		}
        Debug.Log("---------------c-c--------c---------");
        googlemapWnd.transform.FindChild ("Map").transform.GetComponent<OnlineMaps> ().OnChangeZoom ();

        if(GameObject.Find("SettingRangeText"))
            GameObject.Find("SettingRangeText").GetComponent<TextMesh>().text = "0 ~ " + Global.range + "m";
        else
        {
            if(googlemapWnd.transform.FindChild("Map/Direction (Clone)/TextBack/SettingRangeText"))
                googlemapWnd.transform.FindChild("Map/Direction (Clone)/TextBack/SettingRangeText").GetComponent<TextMesh>().text = "0 ~ " + Global.range + "m";
            else if(googlemapWnd.transform.FindChild("Map/Direction/TextBack/SettingRangeText"))
                googlemapWnd.transform.FindChild("Map/Direction/TextBack/SettingRangeText").GetComponent<TextMesh>().text = "0 ~ " + Global.range + "m";
        }

        Debug.Log("---------------d-d--------d---------");
    }

	public void onCancelSetting()
	{
		SettingWnd.SetActive (false);
		googlemapWnd.transform.FindChild ("Camera").transform.gameObject.SetActive (true);

		Global.isSettingPopup = false;
	}

	public void onbtnRemove()
	{
		deletePopupWnd.SetActive (true);
	}

	public void onBtnDelete()
	{
		isClosingGallery = false;
		StartCoroutine (PlayLoadingImage ());
		string dirPath = "";
		#if UNITY_IPHONE
		dirPath = Application.persistentDataPath + "/" + "Estate114";
		if (!Directory.Exists(dirPath))
		{
			Directory.CreateDirectory(dirPath);
		}
		#elif UNITY_ANDROID	
		dirPath = "mnt/sdcard/DCIM/" + "Estate114";	//"mnt/sdcard/DCIM/"
		if (!Directory.Exists(dirPath))
		{
			Directory.CreateDirectory(dirPath);
		}
		#else
		if( Application.isEditor == true ){ 
			dirPath = "mnt/sdcard/DCIM/";
			if (!Directory.Exists(dirPath))
			{
				Directory.CreateDirectory(dirPath);
			}
		} 
		#endif
		var fileInfo =Directory.GetFiles(dirPath,"*.png");
		int file_count = fileInfo.Length;


		DateTime[] creationTimes = new DateTime[fileInfo.Length];
		for (int i = 0; i < fileInfo.Length; i++)
			creationTimes[i] = new FileInfo(fileInfo[i]).CreationTime;
		Array.Sort(creationTimes, fileInfo);


		creationTimes = null;
		Array.Reverse (fileInfo);


		File.Delete (fileInfo[Global.selectedPhotoId]);

		File.Delete (fileInfo[Global.selectedPhotoId].Replace("Estate114","thumb"));

		Global.capturedImageCount--;

		DeleteMediaInfoFromFile ();

		galleryBtnChanged = false;

		onBtnDeleteCancel ();

		StartCoroutine(ReLoadGallery ());
	}

    //선택된 이미지의 경로를 얻는 함수
	public string GetSelectedMainImagePath()
	{
		string[] files;
		string path = "";
		#if UNITY_ANDROID
		path = "mnt/sdcard/DCIM/" + "Estate114";
		#elif UNITY_IPHONE
		path = Application.persistentDataPath+"/Estate114";
		#endif
		files = System.IO.Directory.GetFiles (path,"*.png");

		string prePath = Application.dataPath;
		#if UNITY_IPHONE
		prePath = @"file://";
		#elif UNITY_ANDROID	
		//prePath = @"file://" + Application.dataPath.Replace("/Assets","/");
		prePath = @"file:///";
		#else
		prePath = @"file://" + Application.dataPath.Replace("/Assets","/");
		#endif

		DateTime[] creationTimes = new DateTime[files.Length];
		for (int i = 0; i < files.Length; i++)
			creationTimes[i] = new FileInfo(files[i]).CreationTime;
		Array.Sort(creationTimes, files);

		creationTimes = null;
		Array.Reverse (files);

		string mainPath = files[Global.selectedPhotoId];

		return prePath + mainPath;
	}

    //갤러리 다시 로드 하는 함수
	IEnumerator ReLoadGallery()
	{
		for(int i=Global.selectedPhotoId; i<m_textures.Count - 1;i++){
			m_textures [i].transform.GetComponent<UITexture> ().mainTexture = m_textures [i + 1].transform.GetComponent<UITexture> ().mainTexture;
		}

		if (Global.selectedPhotoId == (m_textures.Count - 1))
			Global.selectedPhotoId--;
		GameObject.DestroyObject (m_textures[m_textures.Count - 1]);
		m_textures.RemoveAt (m_textures.Count - 1);

		ShowImageCount ();

		if (Global.gMediaInfo.Count == 0) {
			yield return new WaitForSeconds (0.1f);
			StartCoroutine ("CloseGalleryWnd");
		} else {
			m_textures [0].transform.GetComponent<SetHighLight> ().ShowHighLights ();

			StartCoroutine (ChangeGalleryMainImage ());
		}
	}

    // 로그인 버튼 클릭시 호출되는 함수
	public void OnBtnOkLogin()
	{
		//hide the login popup window.
		OnBtnCancelLogin();

		//check if the app is installed on this machine and go to login page on the hybrid app
		WriteGlobalFile(strLoginMessage);

		#if UNITY_ANDROID
		launchApp(Global.hybridPackageName, 1);
		#elif UNITY_IPHONE
		if(Global.gHybridInstalled == false)
			Application.OpenURL(Global.hybridStorePath);
		else
			Application.OpenURL("WebView://" + strLoginMessage);
		

		#endif
	}

    //로그인 취소 함수
	public void OnBtnCancelLogin()
	{
		loginPopupWnd.SetActive (false);
	}

	public void OnBtnOkVerify()
	{
		//hide the Verify popup window.
		OnBtnCancelVerify();

		//go to verify on the hybrid app
		WriteGlobalFile(strVerifyMessage);
		#if UNITY_ANDROID
		launchApp(Global.hybridPackageName);
		#elif UNITY_IPHONE
		if(Global.gHybridInstalled == false)
			Application.OpenURL(Global.hybridStorePath);
		else
			Application.OpenURL("WebView://" + strVerifyMessage);
		#endif
	}

	public void OnBtnCancelVerify()
	{
		verifyPopupWnd.SetActive (false);
	}

	public void OnBeforeSaveOk()
	{
		OnBeforeSaveCancel ();
		PlayerPrefs.SetInt ("VerifyCheck", 1);
		StartCoroutine ("UploadImagesToServer");
	}

	public void OnBeforeSaveCancel()
	{
		beforeLoginPopupWnd.SetActive (false);
	}

	public void OnAfterSaveOk()
	{
		OnAfterSaveCancel ();
		//go to my page in Hybrid app
		WriteGlobalFile(strSuccessMessage);
		#if UNITY_ANDROID
		launchApp(Global.hybridPackageName);
		#elif UNITY_IPHONE
		if(Global.gHybridInstalled == false)
			Application.OpenURL(Global.hybridStorePath);
		else
			Application.OpenURL("WebView://" + strSuccessMessage);
		#endif
	}

	public void OnAfterSaveCancel()
	{
		afterSavePopupWnd.SetActive (false);
	}

	public void OnGyroOkButton()
	{
		WriteGlobalFile ("0:Close");
		Application.Quit ();
	}

	public void OnGyroCancelButton()
	{
		gyroPopuwnd.SetActive (false);
	}

	public void OnFailUploadMedia()
	{
		uploadFailedWnd.SetActive (false);
	}

    //서버에 캡쳐한 이미지를 업로드하는 함수
	IEnumerator UploadImagesToServer()			//upload selected image or all images to server
	{
		isUploading = true;
		StartCoroutine (PlayLoadingImage ());

		string dirPath = "";
		#if UNITY_IPHONE
		dirPath = Application.persistentDataPath + "/" + "Estate114";
		if (!Directory.Exists(dirPath))
		{
			Directory.CreateDirectory(dirPath);
		}
		#elif UNITY_ANDROID	
		dirPath = "mnt/sdcard/DCIM/" + "Estate114";	//"mnt/sdcard/DCIM/"
		if (!Directory.Exists(dirPath))
		{
			Directory.CreateDirectory(dirPath);
		}
		#else
		if( Application.isEditor == true ){ 
			dirPath = "mnt/sdcard/DCIM/";
			if (!Directory.Exists(dirPath))
			{
				Directory.CreateDirectory(dirPath);
			}
		} 
		#endif
		var fileInfo =Directory.GetFiles(dirPath);
		string[] files;
		files = System.IO.Directory.GetFiles (dirPath, "*.png");
		int file_count = files.Length;

		DateTime[] creationTimes = new DateTime[files.Length];
		for (int i = 0; i < files.Length; i++)
			creationTimes[i] = new FileInfo(files[i]).CreationTime;
		Array.Sort(creationTimes, files);

		creationTimes = null;

		string prePath = "";
		#if UNITY_IPHONE
		prePath = @"file://" + Application.persistentDataPath;
		#elif UNITY_ANDROID
		//prePath = @"file://" + Application.dataPath.Replace("/Assets","/");
		prePath = @"file:///";
		#endif

		WWW localFile = new WWW (prePath + fileInfo [file_count - 1 - Global.selectedPhotoId]);
		yield return localFile;

		if (localFile.isDone)
			Debug.Log ("Loaded file successfully");
		else {
			Debug.Log ("Open file error: " + localFile.error);
			yield break; // stop the coroutine here
		}

		WWWForm postForm = new WWWForm ();
		postForm.headers.Add ("Host", "m.r114.com");
		postForm.headers.Add ("Origin", "null");
		postForm.headers.Add ("Connecton", "keep-alive");
		postForm.headers.Add ("Cache-Control", "max-age=0");
		postForm.headers.Add ("Upgrade-Insecure-Requests", "1");
		postForm.headers.Add ("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
		postForm.headers.Add ("Accept", "image/jpeg, application/x-ms-application, image/gif, application/xaml+xml, image/pjpeg, application/x-ms-xbap, application/x-shockwave-flash, */*");
		postForm.headers.Add ("Accept-Encoding", "gzip, deflate");
		postForm.headers.Add ("Accept-Language", "ko-KR,ko;q=0.8,en-US;q=0.6,en;q=0.4");
		postForm.AddBinaryData ("upfile", localFile.bytes, Global.gMediaInfo[Global.selectedPhotoId].file_name, "image/png");

		postForm.AddField ("device_code", Global.cookie_token);

		//set the userid field
		postForm.AddField ("userid", Global.user_id);

		//set the position-x field
		postForm.AddField ("posionx", Global.gMediaInfo[Global.selectedPhotoId].position_x);

		//set the position-y field
		postForm.AddField ("posiony", Global.gMediaInfo[Global.selectedPhotoId].position_y);

		//set the b_code field
		postForm.AddField ("b_code", Global.gMediaInfo[Global.selectedPhotoId].b_code);

		//set the j_code field
		postForm.AddField ("j_code", Global.gMediaInfo[Global.selectedPhotoId].j_code == null ? "" : Global.gMediaInfo[Global.selectedPhotoId].j_code);

		//set the photo_date field
		postForm.AddField ("photo_date", Global.gMediaInfo[Global.selectedPhotoId].photo_date);

		//set the media_gubun field
		postForm.AddField ("media_gubun", Global.gMediaInfo[Global.selectedPhotoId].media_gubun);

		//set the gubun field
		postForm.AddField ("gubun", Global.gMediaInfo[Global.selectedPhotoId].gubun);			//건물:1 , 토지:2

		//set the position-y field
		postForm.AddField ("gubun_code", Global.gMediaInfo[Global.selectedPhotoId].gubun_code == null ? "" : Global.gMediaInfo[Global.selectedPhotoId].gubun_code);

		Debug.Log ("----------------------- Load all the parameters successfully -----------------");
		#if UNITY_ANDROID
		WWW upload = new WWW (gUploadUrl + Global.gAndroidSuffix, postForm);
		#elif UNITY_IPHONE
		WWW upload = new WWW (gUploadUrl + Global.gIosSuffix, postForm);
		#endif
		yield return upload;

		if (upload.error == null) {
			Debug.Log ("upload done :" + upload.text);
			JsonData json = JsonMapper.ToObject (upload.text);
			if (json["resultNo"].ToString().Contains("0")) {
				isUploading = false;
				afterSavePopupWnd.SetActive (true);
			}
			else {
				isUploading = false;
				uploadFailedWnd.SetActive (true);
				GameObject.Find ("ErrorMsg").transform.GetComponent<UILabel> ().text = upload.text;
			}
		} else {
			isUploading = false;
			uploadFailedWnd.SetActive (true);
			GameObject.Find ("ErrorMsg").transform.GetComponent<UILabel> ().text = upload.error;
			Debug.Log ("Error during upload: " + upload.error);
		}

		//code for error on somewhere
		isUploading = false;
	}

	IEnumerator LoginCheck()
	{
		//read user_id and token from global file and check if login and verify
		ReadGlobalFile ();

		//Global.user_id = "test1112";
		//Global.cookie_token = "cd22efe264fdb17abbf212aa7a1ba057";
		//check if login and verify
		if (Global.user_id == "" || Global.cookie_token == "")
			Global.isLogin = false;
		else {
			string strLoginCheck = "";
			strLoginCheck = gLoginCheckUrl + "id=" + Global.user_id + "&loginEnc=" + Global.cookie_token;

			#if UNITY_ANDROID
			strLoginCheck += Global.gAndroidSuffix;
			#elif UNITY_IPHONE
			strLoginCheck += Global.gIosSuffix;
			#endif

			Debug.Log ("login check url : " + strLoginCheck);
			WWW upload = new WWW (strLoginCheck);
			yield return upload;
			if (upload.error == null) {
				Debug.Log ("Login Check :" + upload.text);

				if (upload.text.Contains ("\"loginSt\":\"1\"") && upload.text.Contains ("\"joinSt\":\"join\"")) {
					Global.isLogin = true;	
				}

				if (upload.text.Contains ("\"agree\":\"1\"")) {
					Global.isVerify = true;
				}
			} else {
				Debug.Log ("Error during upload: " + upload.error);
				Global.isLogin = false;
			}
		}

		Debug.Log ("Login State : " + (Global.isLogin==true ? "true" : "false"));
		if (Global.isLogin == false) {						//run the hybrid app
			loginPopupWnd.SetActive (true);
		} else if (Global.isLogin == true) {				//upload the selected images to server
			Debug.Log ("Do the save action");

			if (Global.isVerify == false) {
				verifyPopupWnd.SetActive (true);
			} else {
				if (PlayerPrefs.HasKey ("VerifyCheck") && PlayerPrefs.GetInt ("VerifyCheck") == 1) {
					StartCoroutine ("UploadImagesToServer");
				} else
					beforeLoginPopupWnd.SetActive (true);
			}
		}
	}

	public void onbtnSave()
	{
		//test for uplaod
		StartCoroutine (LoginCheck ());
	}
	
    //부동산 하이브리드 앱이 설치되였는지 체크하는 함수
	public static bool isR114Installed()
	{
		#if UNITY_ANDROID
		bool fail = true;
		string bundleId = Global.hybridPackageName;
		AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");

		AndroidJavaObject launchIntent = null;

		try
		{
			launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage",bundleId);
			launchIntent.Call<AndroidJavaObject>("putExtra", "Log_Flag", "Logout");
		}
		catch (System.Exception e)
		{
			Debug.Log("----------Launch R114WebView Error : " + e.ToString() + "-----------");
			fail = false;
		}

		if(up != null)
			up.Dispose();
		if(ca != null)
			ca.Dispose();
		if(packageManager != null)
			packageManager.Dispose();
		if(launchIntent != null)
			launchIntent.Dispose ();
		return fail;
		#elif UNITY_IPHONE
		return true;
		#endif
	}

    //다른 앱을 실행 시키는 함수
	public static void launchApp(string packageName, int flag = 0)
	{
		#if UNITY_ANDROID
		bool fail = false;
		string bundleId = packageName; // your target bundle id
		AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");

		AndroidJavaObject launchIntent = null;

		try
		{
			launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage",bundleId);

			if(flag == 1)
				launchIntent.Call<AndroidJavaObject>("putExtra", "Log_Flag", "Logout");
			else if(flag == 0)
				launchIntent.Call<AndroidJavaObject>("putExtra", "Log_Flag", "Detail");
		}
		catch (System.Exception e)
		{
			Debug.Log("----------Launch R114WebView Error : " + e.ToString() + "-----------");
			fail = true;
		}

		if (fail)
		{ //open app in store
			Application.OpenURL("https://play.google.com/store/apps/details?id=" + packageName);
		}
		else //open the app
		{
			ca.Call("startActivity",launchIntent);
		}

		if(up != null)
			up.Dispose();
		if(ca != null)
			ca.Dispose();
		if(packageManager != null)
			packageManager.Dispose();
		if(launchIntent != null)
			launchIntent.Dispose ();
		#endif
	}

    //화면 캡쳐 하는 함수
	public void onbtnCapture(){
        googleAnalytics.LogScreen("CaptureBtn");

        if (Global.isPhotoPopup == false) {
			if (isCapturing == true) {
				isCapturing = false;		//set to false for next capture
				StartCoroutine(PlayLoadingImage());

				#if UNITY_ANDROID
				AndroidJavaClass jc = new AndroidJavaClass ("com.unity3d.player.UnityPlayer"); 
				AndroidJavaObject jcontext = jc.GetStatic<AndroidJavaObject> ("currentActivity");

				//set the phone volume by using jar file
				AndroidJavaClass cls_jni = new AndroidJavaClass ("com.coar.audiocontrol.AudioManager");

				string str_return = cls_jni.CallStatic<string> ("SetVolume", jcontext, 5);
				Debug.Log("Android Sound : " + str_return);

				jc.Dispose();
				jcontext.Dispose();
				cls_jni.Dispose();
				#elif UNITY_IPHONE
				this.GetComponent<AudioSource> ().PlayOneShot (sndBtnTouch);
				#endif

				galleryBtnChanged = false;

				StartCoroutine (ScreenShot ()); 
			}
		}
	}

    //카메라 해상도와 카메라 백그라운드 해상도를 맞춰주는 함수
	private Texture2D ScaleTexture(Texture2D source,int targetWidth,int targetHeight) {
		Texture2D result=new Texture2D(targetWidth,targetHeight,source.format,true);
		Color[] rpixels=result.GetPixels(0);
		float incX=((float)1/source.width)*((float)source.width/targetWidth);
		float incY=((float)1/source.height)*((float)source.height/targetHeight);
		for(int px=0; px<rpixels.Length; px++) {
			rpixels[px] = source.GetPixelBilinear(incX*((float)px%targetWidth),
				incY*((float)Mathf.Floor(px/targetWidth)));
		}
		result.SetPixels(rpixels,0);
		result.Apply();
		return result;
	}

    //화면을 캡쳐하여 부동산 앱 홀더에 보관하는 함수
	IEnumerator ScreenShot()
	{
		yield return new WaitForEndOfFrame ();
		Camera vcamera = Vcamera.GetComponent<Camera>();
		RenderTexture curRT = RenderTexture.active;
		RenderTexture.active = vcamera.targetTexture;
		vcamera.Render ();

		//real screenshot
		#if UNITY_IPHONE
		if(captureTexture == null)
			captureTexture = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, true);
		if(thumbTexture == null)
			thumbTexture = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, true);
		#endif
		captureTexture.ReadPixels (new Rect (0, 0, Screen.width, Screen.height), 0, 0, true);
		captureTexture.Apply ();

		//screenshot for thumbnail
		thumbTexture.ReadPixels (new Rect (0, 0, Screen.width, Screen.height), 0, 0, true);
		thumbTexture.Apply ();
		Texture2D thumbNail = ScaleTexture (thumbTexture, 100, 178);


		RenderTexture.active = curRT;

		byte[] imageByte = captureTexture.EncodeToPNG ();
		byte[] thumbByte = thumbNail.EncodeToPNG ();
		Debug.Log ("before capture:");

		RenderTexture.active = null;

		DestroyImmediate (curRT);

		isAddingMediaFinish = false;
		string destination = getCaptureName();

		while (isAddingMediaFinish == false) {
			yield return new WaitForSeconds (0.3f);
		}

		File.WriteAllBytes(destination, imageByte);

		string thumb_destination = destination.Replace ("Estate114", "thumb");
		File.WriteAllBytes(thumb_destination, thumbByte);

		Global.capturedImageCount++;
		ShowImageCount (1);

		galleryBtn.mainTexture = thumbNail;

		#if UNITY_ANDROID
		thumbNail = null;
		DestroyImmediate (thumbNail);
		#endif
		isCapturing = true;
		galleryBtnChanged = true;
	}

    //캡쳐한 이미지의 이름을 구하는 함수
	public string getCaptureName()
	{
		string dirPath = "";
		#if UNITY_IPHONE
		dirPath = Application.persistentDataPath + "/" + "thumb";
		if (!Directory.Exists(dirPath))
		{
			Directory.CreateDirectory(dirPath);
		}

		dirPath = Application.persistentDataPath + "/" + "Estate114";
		if (!Directory.Exists(dirPath))
		{
			Directory.CreateDirectory(dirPath);
		}
		#elif UNITY_ANDROID	
		dirPath = "mnt/sdcard/DCIM/" + "thumb";
		if (!Directory.Exists(dirPath))
		{
			Directory.CreateDirectory(dirPath);
		}

		dirPath = "mnt/sdcard/DCIM/" + "Estate114";	//"mnt/sdcard/DCIM/"
		if (!Directory.Exists(dirPath))
		{
			Directory.CreateDirectory(dirPath);
		}
		#else
		if( Application.isEditor == true ){ 
			dirPath = "mnt/sdcard/DCIM/";
			if (!Directory.Exists(dirPath))
			{
				Directory.CreateDirectory(dirPath);
			}
		} 
		#endif
		int lastMediaId = 1;
		if(!PlayerPrefs.HasKey("LastMeidaID"))
		{
			PlayerPrefs.SetInt("LastMeidaID",1);
		}
		else
		{
			lastMediaId = PlayerPrefs.GetInt("LastMeidaID");
			lastMediaId++;
			PlayerPrefs.SetInt("LastMeidaID",lastMediaId);
		}

		StartCoroutine (InputMediaInfo(lastMediaId));

		return string.Format("{0}/{1}.png", dirPath, lastMediaId.ToString());
	}

    //메디오 정보 변수에 내용을 넣는 함수
	IEnumerator InputMediaInfo(int MediaId)
	{
		string strCodeUrl = "";
		string bCode = "";

		strCodeUrl = "https://apis.daum.net/local/geo/coord2addr?apikey=b3593bd3ac45f5a643e030da6c861a6b&longitude=" +
		Global.curPos.longitude.ToString () + "&latitude=" + Global.curPos.latitude.ToString () + "&inputCoordSystem=WGS84&output=json";

		Debug.Log ("login check url" + strCodeUrl);

		WWW upload = new WWW (strCodeUrl);
		yield return upload;

		if (upload.error == null) {
			JsonData json = JsonMapper.ToObject (upload.text);
			if (json.Count > 2)
				bCode = json ["code"].ToString ();
			else
				bCode = "";
		} else
			bCode = "";

		AddMediaInfoToFile(MediaId,MediaId.ToString() + ".png",Global.cookie_token,Global.user_id,Global.curPos.longitude.ToString(), 
		Global.curPos.latitude.ToString(), bCode, "", System.DateTime.Now.ToString("yyyyMMddHHmmss"), 1.ToString(), Global.ar_type.ToString(),"",
		Global.photoAddr);

		isAddingMediaFinish = true;
	}

    //부동산 114 앱으로 이전하는 버튼 클릭시 호출되는 함수
	public void r114GoOkBtn()
	{
		r114Wnd.SetActive (false);
		#if UNITY_ANDROID
		launchApp (Global.hybridPackageName);
		#elif UNITY_IPHONE
		Application.OpenURL(Global.hybridStorePath);
		#endif
	}

	public void r114GoCancelBtn()
	{
		r114Wnd.SetActive (false);
	}

	public void onGallerybtnLeft()
	{
		changeGalleryImageRoutine (1);
	}

	public void onGallerybtnRight()
	{
		changeGalleryImageRoutine (2);
	}

    //갤러리의 기본이미지를 오른쪽에서 왼쪽, 왼쪽에서 오른쪽으로 현시하는 함수
	public void changeGalleryImageRoutine(int direction)
	{
		Vector3 pos = scrollView.transform.localPosition;
		Vector2 vec2DTemp = scrollView.transform.GetComponent<UIPanel> ().clipOffset;

		if (direction == 1) {
			if (0 < Global.selectedPhotoId) {
				Global.selectedPhotoId--;

				if (Global.selectedPhotoId > 3) {
					pos.x += 140;
					vec2DTemp.x -= 140;
				}

				if (Global.selectedPhotoId == 4) {
					pos.x = 0;
					vec2DTemp.x = 0;
				}
			}
		} else if (direction == 2) {
			if (m_textures.Count - 1 > Global.selectedPhotoId) {
				Global.selectedPhotoId++;
				if (!(Global.selectedPhotoId < 5)) {
					pos.x -= 140;
					vec2DTemp.x += 140;
				}
			}
		}

		GameObject.Find ("UI Root/PhotoSlider/PhotoCaption").transform.GetComponent<UILabel> ().text = Global.gMediaInfo[Global.selectedPhotoId].address;
		scrollView.transform.localPosition = pos;
		scrollView.transform.GetComponent<UIPanel> ().clipOffset = vec2DTemp;
		m_textures [0].transform.GetComponent<SetHighLight> ().ShowHighLights ();

		StartCoroutine ("ChangeGalleryMainImage");
	}

    //갤러리 뷰의 기본 이미지를 변경하는 함수
	public IEnumerator ChangeGalleryMainImage()
	{
		changeGalleryImageFlag = false;
		StartCoroutine (PlayLoadingImage ());
		WWW main_www = new WWW (GetSelectedMainImagePath());
		yield return main_www;
		#if UNITY_IPHONE
		if(captureBtn_texTmp == null)
			captureBtn_texTmp = new Texture2D (64, 64, TextureFormat.ARGB32, false);
		#endif
		main_www.LoadImageIntoTexture (captureBtn_texTmp);

		GalleryMainImage.transform.GetComponent<UITexture> ().mainTexture = captureBtn_texTmp;

		changeGalleryImageFlag = true;

		//code for call from ReLoadGallery
		isClosingGallery = true;
	}

	public void onGalleryMainImageClicked()
	{
		mainImage_FirstPos.x = Input.mousePosition.x;
		mainImage_FirstPos.y = Input.mousePosition.y;
	}

	public void onGalleryMainImageRelease()
	{
		mainImage_LastPos.x = Input.mousePosition.x;
		mainImage_LastPos.y = Input.mousePosition.y;

		if (mainImage_LastPos.x < mainImage_FirstPos.x - 30)
			onGallerybtnRight ();
		else if(mainImage_LastPos.x > mainImage_FirstPos.x + 30)
			onGallerybtnLeft ();
	}

	public void onGuideWndBtnOk()
	{
		guideWnd.SetActive (false);
		CheckForInfoWnd();
	}

	public void onGuideWndBtnCancel()
	{
		string nowTime = System.DateTime.Now.ToString ("yyyyMMdd");
		PlayerPrefs.SetString (Global.guideWndPrefabName, nowTime);
		guideWnd.SetActive (false);
		CheckForInfoWnd();
	}

	public void onInfoWndBtnOk()
	{
		infoWnd.SetActive (false);
		InitFunction ();
	}

	public void onInfoWndBtnCancel()
	{
		string nowTime = System.DateTime.Now.ToString ("yyyyMMdd");
		PlayerPrefs.SetString (Global.infoWndPrefabName, nowTime);
		infoWnd.SetActive (false);
		InitFunction ();
	}

	public void onGPSWndBtnOk()
	{
		gpsMessageBox.SetActive (false);
		GpsPopupTime = System.DateTime.Now;

		#if UNITY_IPHONE
		Application.OpenURL("app-settings:");
		Application.Quit();
		#elif UNITY_ANDROID
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject curActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
		curActivity.Call ("openSettingWindow", "");

		jc.Dispose();
		curActivity.Dispose();
		#endif
	}

	public void onGPSWndBtnCancel()
	{
		gpsMessageBox.SetActive (false);
		GpsPopupTime = System.DateTime.Now;
	}
}
