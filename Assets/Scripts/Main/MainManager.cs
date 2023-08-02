using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class MainManager : MonoBehaviour {

	private WebViewObject webView;

	public GameObject btnHome;
	public GameObject btnPrev;
	public GameObject btnNext;
	public GameObject btnRefresh;
	public GameObject btnHeart;
	public GameObject btnAR;

	public GameObject ARsplash;

	// Use this for initialization
	void Start () {
		ARsplash.SetActive (false);
		StartWebView();
	}
	
	// Update is called once per frame
	void Update () {
		if (Application.platform == RuntimePlatform.Android)
		{
			if (Input.GetKey(KeyCode.Escape))
			{
				if (btnPrev.transform.FindChild ("Background").GetComponent<UISprite> ().spriteName == "tb_prev") {
					webView.GoBack ();
					return;
				} else {
					Application.Quit ();
					return;
				}
			}
		}

		if (webView.CanGoBack ()) {
			btnPrev.transform.FindChild ("Background").GetComponent<UISprite> ().spriteName = "tb_prev";
		} else {
			btnPrev.transform.FindChild ("Background").GetComponent<UISprite> ().spriteName = "tb_prev_r";
		}

		if (webView.CanGoForward ()) {
			btnNext.transform.FindChild ("Background").GetComponent<UISprite> ().spriteName = "tb_next";
		} else {
			btnNext.transform.FindChild ("Background").GetComponent<UISprite> ().spriteName = "tb_next_r";
		}
	}

	public void StartWebView()
	{
		webView = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
		webView.Init((msg)=>{
			Debug.Log(string.Format("CallFromJS[{0}]", msg));
		}, false);
		webView.SetMargins (-10, 0, -10, Screen.height * 120 / 1280);
		webView.LoadURL("http://localhost/choco/gift_view.php"); // Global.domain
		webView.SetVisibility(true);

		webView.EvaluateJS(
			"window.addEventListener('onpageshow', function(){" +
			"Unity.call('url:' + window.location.href);" +
			"}, false);");
	}

	public void onbtnHome(){
		webView.LoadURL (Global.domain);
	}

	public void onbtnPrev(){
		if (webView.CanGoBack ()) {
			webView.GoBack ();
		}
	}

	public void onbtnNext(){
		if(webView.CanGoForward()){
			webView.GoForward ();
		}
	}

	public void onbtnRefresh(){
		webView.EvaluateJS ("location.reload()");
	}

	public void onbtnHeart(){
		webView.LoadURL (Global.domain + "/?c=side&m=myIntrest.area");
	}

	public void onbtnAR(){
		Destroy (webView);
		ARsplash.SetActive (true);
		UnityEngine.SceneManagement.SceneManager.LoadScene("Estate114");
	}
}
