    using System.Collections.Generic;
using UnityEngine;
using Amazon.CognitoIdentityProvider.Model;
using System;
using System.Net;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentity;
using Amazon;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

public class CognitoSignUp : MonoBehaviour
{
    private AmazonCognitoIdentityProviderClient cognitoService; // cognitoService 객체 선언

    [Header("InputField")]
    public TMP_InputField nicknameInputField;
    public TMP_InputField passwordInputField;
    public TMP_InputField emailInputField;

    [Header("Button")]
    public Button signUpBtn;
    public Button backBtn;

    private void Start()
    {

        // Amazon Cognito 인증 정보 설정 (IdentityPool, Region)
        var credentials = new CognitoAWSCredentials("ap-northeast-2:49b91ce6-d3db-47f2-af63-2a9db71ca292", RegionEndpoint.APNortheast2);

        // Amazon Cognito 서비스의 객체를 인스턴스화
        cognitoService = new AmazonCognitoIdentityProviderClient(credentials, RegionEndpoint.APNortheast2);
        
        signUpBtn.onClick.AddListener(SignUp);
        backBtn.onClick.AddListener(ClickBackBtn);
    }

    public async void SignUp()
    {
        // 사용자 등록 요청 생성
        var signUpRequest = new SignUpRequest
        {
            ClientId = "1luokqrq9t4j8gag5kbnphunvu", // 클라이언트 ID (모든 사용자들 공통)
            Username = nicknameInputField.text.ToString(), // 사용자 이름 = 닉네임
            Password = passwordInputField.text.ToString(), // 비밀번호
            UserAttributes = new List<AttributeType> // aws에서 우리가 직접 설정한 필수 속성
            {
                new AttributeType { Name = "email", Value = emailInputField.text.ToString() },
                new AttributeType { Name = "nickname", Value = nicknameInputField.text.ToString() }
            }
        };


        // 예외 처리 및 성공 및 실패 처리
        try
        {
            var response = await cognitoService.SignUpAsync(signUpRequest);
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                Debug.Log("Sign-Up Successful.");
                var apiGatewayUrl = "https://q4xm6p11e1.execute-api.ap-northeast-2.amazonaws.com/test1/user-singup";
                var httpClient = new HttpClient();
                var requestBody = new
                {
                    data = new
                    {
                        PK = nicknameInputField.text.ToString(),

                    }
                };
                var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var apiGatewayResponse = await httpClient.PostAsync(apiGatewayUrl, requestContent);
                if (apiGatewayResponse.StatusCode == HttpStatusCode.OK)
                {
                    Debug.Log("Sign Up Successful");
                }
                else
                {
                    Debug.Log("Failed to invoke Lambda function. Response: " + apiGatewayResponse.StatusCode);
                }
                SceneManager.LoadScene("SignInScene");
            }
            else
            {
                Debug.Log("Sign Up Failed. Response: " + response.HttpStatusCode);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Sign Up Failed: " + e.Message);
        }
    }
    public void ClickBackBtn()
    {
        SceneManager.LoadScene("SignInScene");
    }
}