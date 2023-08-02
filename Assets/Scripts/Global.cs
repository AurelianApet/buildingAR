using System.Collections.Generic;

public static class Global {
	#if UNITY_ANDROID
	public static string hybridPackageName = "com.strastar.bds114";
	#elif UNITY_IOS
	public static string hybridPackageName = "com.r114.default";
    public static string hybridStorePath = "https://itunes.apple.com/kr/app/id382372012?mt=8";
	#endif
	public static bool bGPSVersion = true;   //true
	public static bool bGPSStatus = true;

	public static int gUpdateFlag = -1;
	public static int gServerUpdateFlag = -1;

	public static string mediaInfoFileName = "/media_info";
	public static string globalInfoFileName = "/global";
	public static string arAppInfoFileName = "/ARApp_Status";

	public class GPS_pos
	{
		public float longitude;
		public float latitude;

		public GPS_pos(float x, float y){
			longitude = x;
			latitude = y;
		}
	}
		
	public struct MediaInfoStruct{
		public int media_id;
		public string file_name;
		public string cookie;
		public string user_id;
		public string position_x;
		public string position_y;
		public string b_code;
		public string j_code;
		public string photo_date;
		public string media_gubun;
		public string gubun;
		public string gubun_code;
		public string address;
		public bool isSelected;
	}

	public class Spot
	{
		public string ar_id;
		public string ar_title;
		public string ar_address;
		public GPS_pos ar_location;
		public GPS_pos[] ar_polygon;
		public string ar_dtl_title;
		public string ar_dtl_subtitle;
		public string ar_dtl_desc;
		public string ar_dtl_url_detail;
		public string ar_dtl_url_like;
		public string ar_dtl_subdata;
		public float distance;
		public Spot(string id, string title, string address, GPS_pos location, GPS_pos[] polygon, string ar_dtl_title, string ar_dtl_subtitle, string ar_dtl_desc, string ar_dtl_url_detail, string ar_dtl_url_like, 
			string ar_dtl_subdata, float distance){
			this.ar_id = id;
			this.ar_title = title;
			this.ar_address = address;
			this.ar_location = new GPS_pos(location.longitude, location.latitude);
			this.ar_polygon = new GPS_pos[polygon.Length];
			for(int i =0; i < ar_polygon.Length; i++){
				ar_polygon[i] = new GPS_pos(polygon[i].longitude, polygon[i].latitude);
			}
			this.ar_dtl_title = ar_dtl_title;
			this.ar_dtl_subtitle = ar_dtl_subtitle;
			this.ar_dtl_desc = ar_dtl_desc;
			this.ar_dtl_url_detail = ar_dtl_url_detail;
			this.ar_dtl_url_like = ar_dtl_url_like;
			this.ar_dtl_subdata = ar_dtl_subdata;
			this.distance = distance;
		}
	}

	//Spot Info
	public static Spot[] spot;

	public static GPS_pos defaultPos = new GPS_pos(127.02764118321978f, 37.497972894557826f);

	public static GPS_pos gFirstMapPos = new GPS_pos(124.432026f, 38.379005f);
	public static GPS_pos gLastMapPos = new GPS_pos(129.496729f, 33.223479f);

	//current Position
	public static GPS_pos curPos = new GPS_pos(0.0f, 0.0f);

	//current address
	public static string curAddr = "";

	public static GPS_pos orgstart_pos = new GPS_pos(0.0f, 0.0f);
	public static GPS_pos orgend_pos = new GPS_pos(0.0f, 0.0f);

	//
	public static float unitVSpace = 1f;		
	public static float unitLng = (0.9f / 0.00001f) * unitVSpace;
	public static float unitLat = (1.1f / 0.00001f) * unitVSpace;

    //info type	1:building 2:land
    public static bool setting_changed = false;
	public static int ar_type = 1;

    public static string selected_build_types = "1,2,3,4,5,6,";
    public static string selected_land_types = "1,2,3,4,5,6,7,8,9,";

    //Spot Count
    public static int count = 0;
	//Range of Spot arrount current position
	public static int range = 100;


	public static int[] spotFlags;

    //server
    //public static string domain = "https://m.r114.com";//"https://betam.r114.co.kr";
    //test
    public static string domain = "http://175.126.232.30:8009";//"https://pocm.r114.co.kr:443";//"http://175.126.232.30:8009";

    public static string gAndroidSuffix = "&_frDV=go_android";
	public static string gIosSuffix = "&_frDV=go_ios";

	public static int selectedPhotoId = 0;

	public static string photoAddr = "";

	public static bool isDetailPopup = false;
	public static bool isPhotoPopup = false;
	public static bool isExitPopup = false;
	public static bool isSettingPopup = false;

	public static bool isLogin = false;
	public static bool isVerify = false;
	public static string user_id = "";
	public static string cookie_token = "";

	public static List<MediaInfoStruct> gMediaInfo;

	public static string strGlobalInfoFile;

	public static bool gHybridInstalled = false;

	public static bool LoadSpotFinishFlag = true;

	public static int capturedImageCount = 0;

	public static string infoWndPrefabName = "infoWndOpenDate";
	public static string guideWndPrefabName = "guideWndOpenDate";
}
