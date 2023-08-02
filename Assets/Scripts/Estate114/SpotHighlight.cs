using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using LitJson;

public class SpotHighlight : MonoBehaviour {

	public GameObject polygon;
	public GameObject mapObject;
	public GameObject curPos;
    private int drawedPolygonIndex = -1;

	//public bool isPopup = false;


	public int polygon_count = 0;

	public string ar_url_detail = "";
	public string ar_url_like = "";


	//For polygon
	public List<Vector2> RawPoints;
	public Material Mat;

	private struct PolyPoint{

		public int NextP;
		public int PrevP;

		public int NextEar;
		public int PrevEar;

		public int NextRefl;
		public int PrevRefl;

		public bool isEar;

	}

	private List<Vector3> m_TriPointList;
	private int Pointcount;
	private PolyPoint[] PolyPointList;

	private Mesh 						m_Mesh;
	private MeshFilter 					m_MeshFilter;
	private MeshRenderer 				m_MeshRenderer;	
	private Vector2[] 					m_Uv;


	// Use this for initialization
	void Start () {
		OnlineMaps.instance.OnChangePosition += OnChangePosition;
		OnlineMaps.instance.OnChangeZoom += OnChangeZoom;
	}

	// Update is called once per frame
	void Update () {
	}

    //스팟 표시
	void FixedUpdate(){
		if(Global.count > 0 && Global.LoadSpotFinishFlag == true)
		{
			drawSpot ();//StartCoroutine (drawSpot ());
		}
	}

    //화면에 보이는 스팟 오브젝트 현시
	public void drawSpot(){
		if (Global.isPhotoPopup == false) {
			for (int i = 0; i < Global.count; i++) {
				GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + i).transform.gameObject.SetActive (true);
				GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + i).transform.FindChild("back1").transform.GetComponent<UISprite> ().depth = (Global.count - i) * 2;
				GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + i).transform.FindChild("back2").transform.GetComponent<UISprite> ().depth = (Global.count - i) * 2 - 1;

				GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + i).transform.FindChild ("name").GetComponent<UILabel> ().depth = (Global.count - i) * 2 + 1;
				GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + i).transform.FindChild ("distance").GetComponent<UILabel> ().depth = (Global.count - i) * 2 + 1;

				GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + i).transform.FindChild ("name").GetComponent<UILabel> ().color = Color.white;
				GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + i).transform.FindChild ("distance").GetComponent<UILabel> ().color = Color.white;
				GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + i).transform.localScale = new Vector3 (0.9f, 0.9f, 1.0f);
			}

			int index = -1;

			for (int i = Global.count - 1; i >= 0; i--) {
				if (Global.spotFlags [i] == 1) {
					index = i;
					break;
				}
			}

			if (index != -1) {
                Debug.Log("----------------------------------------");
                if (Global.isExitPopup == false) {
					GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + index).transform.FindChild ("back1").transform.GetComponent<UISprite> ().depth = Global.count * 3 + 9;
					GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + index).transform.FindChild ("back2").transform.GetComponent<UISprite> ().depth = Global.count * 3 + 10;
					GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + index).transform.FindChild ("name").GetComponent<UILabel> ().depth = Global.count * 3 + 11;
					GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + index).transform.FindChild ("distance").GetComponent<UILabel> ().depth = Global.count * 3 + 11;

					GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + index).transform.FindChild ("name").GetComponent<UILabel> ().color = Color.black;
					GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + index).transform.FindChild ("distance").GetComponent<UILabel> ().color = Color.red;
					GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + index).transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);

					if (!Global.isDetailPopup) {
						for (int k = 0; k < Global.count; k++) {
							if (k != index) {
								GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + k).transform.FindChild ("back1").transform.GetComponent<UISprite> ().depth = k * 2;
								GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + k).transform.FindChild ("back2").transform.GetComponent<UISprite> ().depth = k * 2 - 1;
								GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + k).transform.FindChild ("name").GetComponent<UILabel> ().depth = k * 2 + 1;
								GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + k).transform.FindChild ("distance").GetComponent<UILabel> ().depth = k * 2 + 1;
							}
						}
							

						Global.isDetailPopup = true;
						StopCoroutine ("showDetailPopup");
						StartCoroutine (showDetailPopup (index));

                        //added 2017.6.5
                        if (index != drawedPolygonIndex)
                        {
                            StartCoroutine(setPolygon(Global.spot[index]));
                            drawedPolygonIndex = index;
                        }
                    }
					Global.photoAddr = Global.spot [index].ar_dtl_title;
				}
			} else {
                //added 2017.6.5
                if (drawedPolygonIndex != -1)
                {
                    drawedPolygonIndex = -1;
                    Debug.Log("----------------1111111111--------------");
                    StartCoroutine("DisablePolygon");
                }

                Global.photoAddr = Global.curAddr;
			}
		} else {
			for (int i = 0; i < Global.count; i++) {
				GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + i).transform.gameObject.SetActive (false);
			}
		}
	}

    //지도에 표시된 polygon 지우기
    IEnumerator DisablePolygon()
    {
        yield return new WaitForSeconds(1.0f);
        if (drawedPolygonIndex == -1 && GameObject.Find("UI Root").transform.FindChild("DetailPopup").gameObject.activeInHierarchy == false)
        {
            Destroy(polygon.GetComponent<MeshRenderer>());
            Destroy(polygon.GetComponent<MeshFilter>());
        }
    }

    //화면 중심에 들어온 스팟 정보를 새로운 팝업으로 현시하는 함수
	IEnumerator showDetailPopup(int index){
		yield return new WaitForSeconds(1.5f);
		if (GameObject.Find ("UI Root").transform.FindChild ("nguispot_" + index).transform.localScale.x == 1.0f) {
			int depth = Global.count*3 + 12;
			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("Transparent").GetComponent<UISprite> ().depth = depth;
			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.GetComponent<UISprite> ().depth = depth + 1;

			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("lblName").GetComponent<UILabel> ().depth = depth + 2;
			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("lblAddress").GetComponent<UILabel> ().depth = depth + 2;
			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("lblDescription").GetComponent<UILabel> ().depth = depth + 2;
			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnClose").transform.FindChild ("Background").GetComponent<UISprite> ().depth = depth + 3;
			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnLease").transform.FindChild ("Background").GetComponent<UISprite> ().depth = depth + 4;
			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnLease").transform.FindChild ("Label").GetComponent<UILabel> ().depth = depth + 5;
			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnSale").transform.FindChild ("Background").GetComponent<UISprite> ().depth = depth + 4;
			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnSale").transform.FindChild ("Label").GetComponent<UILabel> ().depth = depth + 5;
			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnMonthlyRent").transform.FindChild ("Background").GetComponent<UISprite> ().depth = depth + 4;
			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnMonthlyRent").transform.FindChild ("Label").GetComponent<UILabel> ().depth = depth + 5;
			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnDetailInfo").transform.FindChild ("Background").GetComponent<UISprite> ().depth = depth + 4;
			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnDetailInfo").transform.FindChild ("Label").GetComponent<UILabel> ().depth = depth + 5;

			if (Global.ar_type == 1) {//건물
				GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnDetailInfo").transform.FindChild ("Label").GetComponent<UILabel> ().text = "건물정보";
			} else if (Global.ar_type == 2) {//토지
				GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnDetailInfo").transform.FindChild ("Label").GetComponent<UILabel> ().text = "토지정보";
			}

			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("lblName").GetComponent<UILabel> ().text = Global.spot[index].ar_dtl_title;
			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("lblAddress").GetComponent<UILabel> ().text = Global.spot[index].ar_dtl_subtitle;
			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("lblDescription").GetComponent<UILabel> ().text = Global.spot[index].ar_dtl_desc;

			string[] ar_subdata = Global.spot[index].ar_dtl_subdata.Trim().Split(' ');
			if (Global.spot [index].ar_dtl_subdata == "") {
				GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnSale").gameObject.SetActive (false);
				GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnLease").gameObject.SetActive (false);
				GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnMonthlyRent").gameObject.SetActive (false);
			}
			else
			{
				GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnSale").gameObject.SetActive (true);
				GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnLease").gameObject.SetActive (true);
				GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnMonthlyRent").gameObject.SetActive (true);

				GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnSale").transform.FindChild("Label").GetComponent<UILabel> ().text = "매매 " + ar_subdata[1];
				GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnLease").transform.FindChild("Label").GetComponent<UILabel> ().text = "전세 " + ar_subdata[3];		//매전
				GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").transform.FindChild ("btnMonthlyRent").transform.FindChild("Label").GetComponent<UILabel> ().text = "월세 " + ar_subdata[5];	//매월
			}

			ar_url_detail = Global.domain + Global.spot [index].ar_dtl_url_detail;
			ar_url_like = Global.domain + Global.spot [index].ar_dtl_url_like;

			#if UNITY_ANDROID
			ar_url_detail += Global.gAndroidSuffix;
			ar_url_like += Global.gAndroidSuffix;
			#elif UNITY_IPHONE
			ar_url_detail += Global.gIosSuffix;
			ar_url_like += Global.gIosSuffix;
			#endif

			GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").gameObject.SetActive (true);

            //deleted 2017.6.5
            //StartCoroutine(setPolygon(Global.spot[index]));
        } else {
			Global.isDetailPopup = false;
		}
	}

    //현재 지도 영역에 polygon 을 현시하는 함수
	IEnumerator setPolygon(Global.Spot spot){
		yield return new WaitForSeconds(0.1f);
		Texture2D temptext = Resources.Load ("Spot/trans") as Texture2D;
		OnlineMaps.instance.RemoveAllMarkers ();

		polygon_count = spot.ar_polygon.Length;
		for (int i = 0; i < polygon_count; i++) {
			OnlineMaps.instance.AddMarker (spot.ar_polygon [i].longitude, spot.ar_polygon [i].latitude, temptext).label = i.ToString();
		}
		//OnlineMaps.instance.SetPosition(spot.ar_location.longitude, spot.ar_location.latitude);
		if (Global.bGPSVersion == false)
			OnlineMaps.instance.SetPosition (Global.curPos.longitude, Global.curPos.latitude);
		else {
			if(Global.bGPSStatus == true)
				OnlineMaps.instance.SetPosition (Input.location.lastData.longitude, Input.location.lastData.latitude);
			else
				OnlineMaps.instance.SetPosition (Global.curPos.longitude, Global.curPos.latitude);
		}

		StopCoroutine("SetPosition");
		StartCoroutine ("SetPosition");

	}

	public void OnChangePosition()
	{
		// When the position changes you will see in the console new map coordinates.
		if(Global.isDetailPopup == true)// && Global.bGPSStatus == true)
			GetLocationMaps ();
	}

	public void OnChangeZoom()
	{
		// When the zoom changes you will see in the console new zoom.
		StopCoroutine("SetPosition");
		StartCoroutine ("SetPosition");
	}

	void GetLocationMaps()
	{
		Transform[] markers = mapObject.transform.Find ("Markers").transform.GetComponentsInChildren<Transform> (true);
		if (markers.Length - 1 == polygon_count) {
			Vector2[] vec = new Vector2[markers.Length-1];
			for (int i = 0; i < markers.Length-1; i++) {
				int index = int.Parse(markers [i + 1].GetComponent<OnlineMapsMarkerBillboard> ().marker.label);
				vec [index].x = markers [i + 1].position.x;
				vec [index].y = markers [i + 1].position.z;
			}
			if(Global.isDetailPopup == true)
				DrawPolygen (vec);
		} else {
			Destroy(polygon.GetComponent<MeshRenderer> ());
			Destroy(polygon.GetComponent<MeshFilter> ());
		}
	}

    //좌표 배렬로 부터 polygon 을 그려주는 함수
	public void DrawPolygen(Vector2[] vertices2D)
	{
		RawPoints.Clear();
		for (int i = 0; i < vertices2D.Length; i++)
			RawPoints.Add (vertices2D [i]);
		
		Pointcount = RawPoints.Count;
		PolyPointList = new PolyPoint[Pointcount+1];
		m_TriPointList = new List<Vector3>();

		FillLists();

		Triangulate();

		DrawPolygon();
	}

	IEnumerator SetPosition()
	{
		yield return new WaitForSeconds (0.2f);
			GetLocationMaps ();
	}

    //상세 정보 팝업 없애는 기능 수행
	public void closePopup(){
        GameObject.Find("backgroundManager").transform.GetComponent<ToolbarManager>().googleAnalytics.LogScreen("CloseDetailPopup");

        Global.isDetailPopup = false;
		GameObject.Find ("UI Root").transform.FindChild ("DetailPopup").gameObject.SetActive (false);
		Destroy(polygon.GetComponent<MeshRenderer> ());
		Destroy(polygon.GetComponent<MeshFilter> ());

		if (Global.bGPSVersion == false)
			OnlineMaps.instance.SetPosition (Global.curPos.longitude, Global.curPos.latitude);
		else {
			if(Global.bGPSStatus == true)
				OnlineMaps.instance.SetPosition (Input.location.lastData.longitude, Input.location.lastData.latitude);
			else
				OnlineMaps.instance.SetPosition (Global.curPos.longitude, Global.curPos.latitude);
		}
	}

    //상세 팝업에서 정보보기 버튼 클릭시 호출되는 함수
	public void onbtnDetailInfo(){
        GameObject.Find("backgroundManager").transform.GetComponent<ToolbarManager>().googleAnalytics.LogScreen("GoToDetailView");

        #if UNITY_ANDROID
        ToolbarManager.WriteGlobalFile("1:Detail:"+ar_url_detail);
		Debug.Log ("Detail Url : " + ar_url_detail);

		if(ToolbarManager.isR114Installed() == true)
			ToolbarManager.launchApp(Global.hybridPackageName);
		else
			GameObject.Find("backgroundManager").transform.GetComponent<ToolbarManager>().r114Wnd.SetActive(true);
		
		#elif UNITY_IPHONE
		if(Global.gHybridInstalled == false)
		{
			Application.OpenURL(ar_url_detail);
		}
		else
			Application.OpenURL("WebView://1:Detail:" + ar_url_detail);
		#endif
		closePopup ();
	}


	//For the polygon draw
	private void FillLists(){

		/*
		 * three doubly linked lists (points list,reflective points list, ears list) are
		 * maintained in the "PolyPointList" arry.
		 * points list is a cyclic list while other two arent.
		 * 0 index of the Point list is kept only for entering the lists
		 * -1 means undefined link
		 */
		PolyPoint p = new PolyPoint();

		PolyPointList[0] = p;
		PolyPointList[0].NextP = 1;
		PolyPointList[0].PrevP = -1;
		PolyPointList[0].NextEar = -1;
		PolyPointList[0].PrevEar = -1;
		PolyPointList[0].NextRefl = -1;
		PolyPointList[0].PrevRefl = -1;
		PolyPointList[0].isEar = false;

		int T_Reflective = -1;
		int T_Convex = -1;

		for(int i=1;i<=Pointcount;i++){

			PolyPointList[i]=p;

			if(i==1)
				PolyPointList[i].PrevP = Pointcount;
			else
				PolyPointList[i].PrevP = i-1;

			PolyPointList[i].NextP = (i%Pointcount)+1;

			if(isReflective(i)){

				PolyPointList[i].PrevRefl = T_Reflective;

				if(T_Reflective==-1){
					PolyPointList[0].NextRefl =i;
				}
				else
					PolyPointList[T_Reflective].NextRefl=i;

				T_Reflective = i;
				PolyPointList[i].NextRefl = -1;

				PolyPointList[i].PrevEar = -1;
				PolyPointList[i].NextEar = -1;

			}
			else{

				PolyPointList[i].PrevRefl = -1;
				PolyPointList[i].NextRefl = -1;
				PolyPointList[i].isEar = true;

				PolyPointList[i].PrevEar = T_Convex;

				if(T_Convex==-1){
					PolyPointList[0].NextEar = i;
				}
				else
					PolyPointList[T_Convex].NextEar=i;

				T_Convex = i;

				PolyPointList[i].NextEar = -1;
			}

		}


		int Con = PolyPointList[0].NextEar;

		while(Con!=-1){

			if(!isCleanEar(Con)){
				RemoveEar(Con);
			}
			Con = PolyPointList[Con].NextEar;

		}


	}


	/*
	 * "Ear Clipping" is used for
	 * Polygon triangulation
	 */
	private void Triangulate(){

		int i;

		while(Pointcount>3){

			/*
			 * The Two-Ears Theorem: "Except for triangles every 
			 * simple ploygon has at least two non-overlapping ears"
			 * so there i will always have a value
			 */
			i= PolyPointList[0].NextEar;

			int PrevP = PolyPointList[i].PrevP;
			int NextP = PolyPointList[i].NextP;

			m_TriPointList.Add(new Vector3(PrevP,i,NextP));

			RemoveEar(i);
			RemoveP(i);

			if(!isReflective(PrevP)){

				if(isCleanEar(PrevP)){ 

					if(!PolyPointList[PrevP].isEar){

						AddEar(PrevP);
					}

				}
				else{

					if(PolyPointList[PrevP].isEar){

						RemoveEar(PrevP);
					}  

				}

			}

			if(!isReflective(NextP)){

				if(isCleanEar(NextP)){ 

					if(!PolyPointList[NextP].isEar){

						AddEar(NextP);
					}

				}
				else{

					if(PolyPointList[NextP].isEar){

						RemoveEar(NextP);
					}  

				}

			}


		}

		int y = PolyPointList[0].NextP;
		int x = PolyPointList[y].PrevP;
		int z = PolyPointList[y].NextP;

		m_TriPointList.Add(new Vector3(x , y , z));

	}



	private void DrawPolygon(){
		if (polygon.GetComponent<MeshRenderer> () == null) {
			polygon.AddComponent (typeof(MeshRenderer));
			m_MeshFilter = polygon.AddComponent (typeof(MeshFilter)) as MeshFilter;//(MeshFilter)GetComponent(typeof(MeshFilter));
		}
		else
			m_MeshFilter = polygon.GetComponent<MeshFilter> ();

		polygon.GetComponent<MeshRenderer> () .material = Resources.Load ("Spot/polygon") as Material;
		m_Mesh = m_MeshFilter.mesh;

		int vertex_count = RawPoints.Count;
		int triangle_count = m_TriPointList.Count;

		/*
			 * Mesh vertices
			 */
		Vector3 [] vertices = new Vector3 [vertex_count]; 

		for(int i=0;i<vertex_count;i++){

			vertices[i] = RawPoints[i];
		}

		RawPoints.Clear();

		m_Mesh.vertices = vertices;

		/*
			 * Mesh trangles
			 */
		int [] tri = new int [triangle_count*3];

		for(int i=0,j=0;i<triangle_count;i++,j+=3){

			tri[j]=(int)(m_TriPointList[i].x-1);
			tri[j+1]=(int)(m_TriPointList[i].y-1);
			tri[j+2]=(int)(m_TriPointList[i].z-1);

		}

		m_Mesh.triangles = tri;

		/*
			 * Mesh noramals
			 */
		Vector3[] normals= new Vector3[vertex_count];

		for(int i=0;i<vertex_count;i++){
			normals[i] = -Vector3.forward;
		}

		m_Mesh.normals = normals;

		/*
			 * Mesh UVs
			 */
		m_Uv    = new Vector2[vertex_count];

		for(int i=0;i<m_Uv.Length;i++){
			m_Uv[i] = new Vector2(0, 0);
		}


		m_Mesh.uv = m_Uv;
	}



	/*
	 * Utility Methods
	 */

	private bool isCleanEar(int Ear){

		/*
		 * Barycentric Technique is used to test
		 * if the reflective vertices are in selected ears
		 */

		float dot00;
		float dot01;
		float dot02;
		float dot11;
		float dot12;

		float invDenom;
		float U;
		float V;

		Vector2 v0 = RawPoints[PolyPointList[Ear].PrevP-1]-RawPoints[Ear-1];
		Vector2 v1 = RawPoints[PolyPointList[Ear].NextP-1]-RawPoints[Ear-1];
		Vector2 v2;

		int i = PolyPointList[0].NextRefl;

		while(i!=-1){

			v2 = RawPoints[i-1]-RawPoints[Ear-1];

			dot00=Vector2.Dot(v0,v0);
			dot01=Vector2.Dot(v0,v1);
			dot02=Vector2.Dot(v0,v2);
			dot11=Vector2.Dot(v1,v1);
			dot12=Vector2.Dot(v1,v2);

			invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
			U = (dot11 * dot02 - dot01 * dot12) * invDenom;
			V = (dot00 * dot12 - dot01 * dot02) * invDenom;

			if((U > 0) && (V > 0) && (U + V < 1))
				return false;

			i = PolyPointList[i].NextRefl;
		}

		return true;
	}

	private bool isReflective(int P){

		/*
		 * vector cross product is used to determin the reflectiveness of vertices
		 * because "Sin" values of angles are always - if the angle > 180 
		 */

		Vector2 v0 = RawPoints[PolyPointList[P].PrevP-1]- RawPoints[P-1];
		Vector2 v1 = RawPoints[PolyPointList[P].NextP-1]- RawPoints[P-1];

		Vector3 A = Vector3.Cross(v0,v1);

		if(A.z<0)
			return true;

		return false;
	}

	private void RemoveEar(int Ear){

		int PrevEar = PolyPointList[Ear].PrevEar;
		int NextEar = PolyPointList[Ear].NextEar;

		PolyPointList[Ear].isEar = false;

		if(PrevEar==-1){
			PolyPointList[0].NextEar = NextEar;
		}
		else{
			PolyPointList[PrevEar].NextEar = NextEar;
		}

		if(NextEar!=-1){
			PolyPointList[NextEar].PrevEar = PrevEar;
		}
	}

	private void AddEar(int Ear){

		int NextEar=PolyPointList[0].NextEar;

		PolyPointList[0].NextEar = Ear;

		PolyPointList[Ear].PrevEar = -1;
		PolyPointList[Ear].NextEar = NextEar;

		PolyPointList[Ear].isEar = true;

		if(NextEar!=-1){

			PolyPointList[NextEar].PrevEar = Ear;

		}

	}

	private void RemoverReflective(int P){

		int PrevRefl = PolyPointList[P].PrevRefl;
		int NextRefl = PolyPointList[P].NextRefl;

		if(PrevRefl==-1){
			PolyPointList[0].NextRefl = NextRefl;
		}
		else{
			PolyPointList[PrevRefl].NextRefl = NextRefl;
		}

		if(NextRefl!=-1){
			PolyPointList[NextRefl].PrevRefl = PrevRefl;
		}

	}

	private void AddReflective(int P){

		int NextRefl=PolyPointList[0].NextRefl;

		PolyPointList[0].NextRefl = P;

		PolyPointList[P].PrevRefl = -1;
		PolyPointList[P].NextRefl = NextRefl;

		if(NextRefl!=-1){

			PolyPointList[NextRefl].PrevRefl = P;

		}

	}

	private void RemoveP(int P){

		int NextP = PolyPointList[P].NextP;
		int PrevP = PolyPointList[P].PrevP;

		PolyPointList[PrevP].NextP=NextP;
		PolyPointList[NextP].PrevP=PrevP;

		if(PolyPointList[0].NextP==P)
			PolyPointList[0].NextP=NextP;

		--Pointcount;
	}
}
