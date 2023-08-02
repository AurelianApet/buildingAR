using UnityEngine;
using System.Collections;
using LitJson;
using System;

public class SplashManager : MonoBehaviour {
    public GameObject updateWnd;
    public GameObject serverupdateWnd;
    public GameObject splashWnd;

    private string serverCheckFlag = "";
    private string startDateString = "";
    private string endDateString = "";
    private string serverCheckingMessage = "";
    // Use this for initialization
    public void Start () {
        splashWnd.SetActive(false);
        updateWnd.SetActive(false);
        serverupdateWnd.SetActive(false);

        StartCoroutine(LoadMainScene());
	}

    //메인 화면 진입 스레드
    public IEnumerator LoadMainScene()
    {
        splashWnd.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Estate114");
    }

    //업데이트 페이지로 이동
    public void onUpdateWndBtnOk()
    {
#if UNITY_ANDROID
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.r114.rgo");
#elif UNITY_IPHONE
		//Application.OpenURL("itms-apps://itunes.com/app/com.r114.rgo");
		Application.OpenURL("https://itunes.apple.com/kr/app/id1223752790?mt=8");
#endif
    }

    //업데이트 취소
    public void onUpdateWndBtnCancel()
    {
        if (Global.gUpdateFlag == 1){
            updateWnd.SetActive(false);
            Global.gUpdateFlag = 0;
            StartCoroutine(LoadMainScene());
            
        } else if (Global.gUpdateFlag == 2){
            Application.Quit();
        }
    }

    public void onServerUpdatebtnOk()
    {
        Application.Quit();
    }

    //앱 업데이트 체크
    IEnumerator CheckIfUpdate()
    {
#if UNITY_ANDROID
        string requestURL = Global.domain + "/?c=App&m=Entry&func=init&platform=1&app_version=" + Application.version + "&app_name=AR&device_id=" + SystemInfo.deviceUniqueIdentifier;
#elif UNITY_IPHONE
		string requestURL = Global.domain + "/?c=App&m=Entry&func=init&platform=0&app_version=" + Application.version + "&app_name=AR&device_id=" + SystemInfo.deviceUniqueIdentifier;
#endif
        WWW www = new WWW(requestURL);

        yield return www;

        if (www.error == null)
        {
            if (!string.IsNullOrEmpty(www.text))
            {
                try
                {
                    JsonData json = JsonMapper.ToObject(www.text);

                    serverCheckFlag = json["data"]["system"]["server_checking"].ToString();
                    startDateString = json["data"]["system"]["server_checking_startdate"].ToString();
                    endDateString = json["data"]["system"]["server_checking_enddate"].ToString();
                    serverCheckingMessage = json["data"]["system"]["server_checking_message"].ToString();

                    if (serverCheckFlag == "Y")
                    {
                        Global.gServerUpdateFlag = 1;
                        serverupdateWnd.transform.FindChild("Description").transform.GetComponent<UILabel>().text = serverCheckingMessage.Replace("\\n", "\n");
                        serverupdateWnd.SetActive(true);
                    }
                    else
                    {
                        Global.gServerUpdateFlag = 0;
                    }

                    if (json["data"]["update"]["app_update_mandatory"].ToString() == "0")
                    { 
                        Global.gUpdateFlag = 0;
                    }
                    else if (json["data"]["update"]["app_update_mandatory"].ToString() == "1")
                    {
                        updateWnd.transform.FindChild("Description").transform.GetComponent<UILabel>().text = json["data"]["update"]["app_update_message"].ToString().Replace("\\n", "\n");
                        Global.gUpdateFlag = 1;
                    }
                    else if (json["data"]["update"]["app_update_mandatory"].ToString() == "2")
                    {
                        updateWnd.transform.FindChild("Description").transform.GetComponent<UILabel>().text = json["data"]["update"]["app_update_message"].ToString();
                        Global.gUpdateFlag = 2;
                    }
                    else
                    {
                        Global.gUpdateFlag = 0;
                    }
                }
                catch (Exception)
                {
                    Global.gUpdateFlag = -1;
                    Global.gServerUpdateFlag = -1;
                }

            }
            else
            {
                Global.gUpdateFlag = -1;
                Global.gServerUpdateFlag = -1;
            }
        }
        else
        {
            Global.gUpdateFlag = -1;
            Global.gServerUpdateFlag = -1;
        }

        if (Global.gServerUpdateFlag == 0)
        {
            CheckAppUpGrade();
        }

        if(Global.gServerUpdateFlag == -1 && Global.gUpdateFlag == -1)
        {
            serverupdateWnd.transform.FindChild("Description").transform.GetComponent<UILabel>().text = "인터넷 연결에 실패하였습니다.\n인터넷 연결을 확인해주세요.";
            serverupdateWnd.SetActive(true);
        }
    }

    public void CheckAppUpGrade()
    {
        if (Global.gUpdateFlag == 0)
            StartCoroutine(LoadMainScene());
        else
            updateWnd.SetActive(true);

    }

    void OnApplicationPause(bool status)
    {
    }

    // Update is called once per frame
    void Update () {
	
	}
}
