using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

public class ClosetManage : MonoBehaviour
{
    public GameObject eyeScrollView, hatScrollView; // hat, eye 스크롤 뷰 오브젝트
    public Button eyeScrollViewBtn, hatScrollViewBtn; // hat, eye 스크롤 뷰 선택 버튼
    public GameObject[] eyeToggle, hatToggle; // hat, eye 카테고리의 커마 에셋 토글들
    public GameObject character;

    public class CustomInfoResponse
    {
        public int eyeCustom;
        public int hatCustom;
    }



    void Start()
    {
        // 스크롤뷰 선택 버튼 클릭시
        eyeScrollViewBtn.onClick.AddListener(ClickEyeCategory);
        hatScrollViewBtn.onClick.AddListener(ClickHatCategory);

        // 토글 클릭시 함수 호출
        for (int i = 0; i < eyeToggle.Length; i++)
        {
            int index = i;
            eyeToggle[i].GetComponent<Toggle>().onValueChanged.AddListener(delegate { ToggleEye(index); });
        }
        for (int i = 0; i < hatToggle.Length; i++)
        {
            int index = i;
            hatToggle[i].GetComponent<Toggle>().onValueChanged.AddListener(delegate { ToggleHat(index); });
        }
        StartCoroutine(GetCustomInfo(PlayerInfo.playerInfo.nickname));

    }

    private IEnumerator GetCustomInfo(string nickname)
    {
        string apiGatewayUrl = "https://q4xm6p11e1.execute-api.ap-northeast-2.amazonaws.com/test1/ch-custom"; 
        apiGatewayUrl += $"?nickname={nickname}";
        Debug.Log(apiGatewayUrl);

        // Create a request object
        var request = new UnityWebRequest(apiGatewayUrl, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();

        // Send the request and wait for a response
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Load successful!");

            // Parse the JSON response
            string jsonString = request.downloadHandler.text;
            var response = JsonUtility.FromJson<CustomInfoResponse>(jsonString);

            // Apply the custom info to the player
            PlayerInfo.playerInfo.eyeCustom = response.eyeCustom;
            PlayerInfo.playerInfo.hatCustom = response.hatCustom;
            character.GetComponent<PlayerScript>().SetCharacterCustom();
        }
        else
        {
            Debug.Log($"Load failed: {request.error}");
        }
        request.Dispose(); // 메모리 누수 방지를 위해 추가

    }

    private IEnumerator UpdateCustomInfo(string nickname, string attribute, int value)
    {
        string apiGatewayUrl = "https://q4xm6p11e1.execute-api.ap-northeast-2.amazonaws.com/test1/ch-custom"; 

        // Create a request object
        var request = new UnityWebRequest(apiGatewayUrl, "PUT");
        request.downloadHandler = new DownloadHandlerBuffer();

        // Create a JSON object for the request body
        string jsonBody = $"{{\"nickname\":\"{nickname}\",\"attribute\":\"{attribute}\",\"value\":{value}}}";
        Debug.LogFormat("jsonBody : {0}", jsonBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and wait for a response
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Update successful!");
        }
        else
        {
            Debug.Log($"Update failed: {request.error}");
        }
        request.Dispose(); // 메모리 누수 방지를 위해 추가

    }

    private void ToggleEye(int n)
    {
        if (eyeToggle[n].GetComponent<Toggle>().isOn)
        {
            // 다른 토글들 OFF
            for (int i = 0; i < eyeToggle.Length; i++) if (i != n) eyeToggle[i].GetComponent<Toggle>().isOn = false;
            // 정보 저장
            PlayerInfo.playerInfo.eyeCustom = n;
            // 선택한 토글 이미지 적용
            character.GetComponent<PlayerScript>().SetCharacterCustom();
        }
        else
        {
            // null 이미지로 설정
            PlayerInfo.playerInfo.eyeCustom = 11;
            // 이미지 적용
            character.GetComponent<PlayerScript>().SetCharacterCustom();

        }
        StartCoroutine(UpdateCustomInfo(PlayerInfo.playerInfo.nickname, "eyeCustom", PlayerInfo.playerInfo.eyeCustom));

    }
    private void ToggleHat(int n)
    {
        if (hatToggle[n].GetComponent<Toggle>().isOn)
        {
            // 다른 토글들 OFF
            for (int i = 0; i < hatToggle.Length; i++) if (i != n) hatToggle[i].GetComponent<Toggle>().isOn = false;
            // 정보 저장
            PlayerInfo.playerInfo.hatCustom = n;
            // 선택한 토글 이미지 적용
            character.GetComponent<PlayerScript>().SetCharacterCustom();
        }
        else
        {
            // null 이미지로 설정
            PlayerInfo.playerInfo.hatCustom = 8;
            // 이미지 적용
            character.GetComponent<PlayerScript>().SetCharacterCustom();
        }
        StartCoroutine(UpdateCustomInfo(PlayerInfo.playerInfo.nickname, "hatCustom", PlayerInfo.playerInfo.hatCustom));

    }
    private void ClickEyeCategory()
    {
        eyeScrollView.SetActive(true);
        hatScrollView.SetActive(false);
    }
    private void ClickHatCategory()
    {
        eyeScrollView.SetActive(false);
        hatScrollView.SetActive(true);
    }
}