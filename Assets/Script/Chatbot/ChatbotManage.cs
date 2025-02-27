using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using WebSocketSharp;

[System.Serializable]
public class PredictedChat
{
    public string question;
    public string response;
}

public class ChatbotManage : MonoBehaviour
{
    [Header("InputField")]
    public TMP_InputField requestInputField;

    [Header("Button")]
    public Button sendBtn;

    [Header("Prefab")]
    public GameObject requestGroup;
    public GameObject responseGroup;
    public GameObject responseGroupImage;

    [Header("Content - PrefabSpawnPos")]
    public GameObject content;

    [Header("ScrollBar")]
    public Scrollbar scrollbar;

    // 예측값
    PredictedChat predictedData;

    void Start()
    {
        sendBtn.onClick.AddListener(Question);
    }

    void Question()
    {
        if (!requestInputField.text.IsNullOrEmpty())
        {
            string question = requestInputField.text;
            // 질문 말풍선 생성
            GameObject _requestGroup = Instantiate(requestGroup, new Vector2(0, 0), Quaternion.identity, content.transform);
            // 질문 적용
            _requestGroup.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = question;
            // 입력창 초기화
            requestInputField.text = "";
            // 스크롤을 최하단으로 설정
            Invoke("ScrollDown", 0.1f);

            // 질문 데이터 수집을 위해 S3에 질문을 저장
            _ = S3Manage.s3Manage.PostToS3(question, PlayerInfo.playerInfo.nickname);
            // EC2 인스턴스에서 실행된 Flask 웹 서버에 질문 업로드
            StartCoroutine(PostQuestionToEC2("15.164.130.22", question, PlayerInfo.playerInfo.nickname));
            // 모델이 생성한 응답 데이터를 S3에 저장 -> S3에서 응답 데이터 가져오기
            StartCoroutine(GetChatBotSentenceFromS3("https://chatting-serivce.s3.ap-northeast-2.amazonaws.com/" + PlayerInfo.playerInfo.nickname));

        }
    }
    void Response(string response)
    {
        // 답변 말풍선 생성
        GameObject _responseGroup = Instantiate(responseGroup, new Vector2(0, 0), Quaternion.identity, content.transform);
        // 답변 적용
        _responseGroup.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = response;
        // 스크롤을 최하단으로 설정
        Invoke("ScrollDown", 0.2f);
    }
    void ResponseImage(string imageName)
    {
        // 답변 말풍선 생성
        GameObject _responseGroup = Instantiate(responseGroupImage, new Vector2(0, 0), Quaternion.identity, content.transform);
        // 답변 적용
        _responseGroup.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(imageName);
        // 스크롤을 최하단으로 설정
        Invoke("ScrollDown", 0.2f);
        Response("지도의 해당 위치에 있습니다.");
    }
    void ScrollDown()
    {
        scrollbar.value = 0;

    }
    // AWS EC2 웹서버에 질문 업로드를 위한 함수
    IEnumerator PostQuestionToEC2(string URL, string question, string nickname)
    {
        // S3에 질문이 업로드 될 시간 1초 정도 대기
        yield return new WaitForSeconds(1f);

        WWWForm form = new WWWForm();
        form.AddField("question", question);
        form.AddField("nickname", nickname);

        UnityWebRequest www = UnityWebRequest.Post(URL, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Question Upload to EC2 Complete!");
        }
        www.Dispose();
    }
    // 챗봇 생성한 답변을 S3에 저장 -> S3에서 답변 가져오기
    IEnumerator GetChatBotSentenceFromS3(string URL)
    {
        // 모델이 돌아갈 시간 2.5초 정도 대기
        yield return new WaitForSeconds(2.5f);

        using UnityWebRequest www = UnityWebRequest.Get(URL);
        // 답변 로드 완료까지 대기
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = www.downloadHandler.text;

            // JSON 데이터를 Class 형태로 파싱
            predictedData = JsonUtility.FromJson<PredictedChat>(jsonResponse);

            // 답변 추출
            Debug.LogFormat("Response\n{0}", predictedData.response);
            
            if (predictedData.response.Contains("이미지"))
            {
                ResponseImage(predictedData.response);
            }
            else
            {
                Response(predictedData.response);
            }

        }
        else
        {
            Debug.Log("GET ChatBot Response Value failed. Error: " + www.error);
        }
    }
}
