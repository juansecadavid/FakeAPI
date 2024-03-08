using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class TokenHandler : MonoBehaviour
{
    public static string url= "https://sid-restapi.onrender.com";
    public static string MyToken;
    public static string MyUsername;

    public GameObject PlayBtn;

    public TextMeshProUGUI errorText;
    private void Start()
    {
        if (PlayerPrefs.HasKey("Username") && PlayerPrefs.HasKey("Token"))
        {
            MyUsername = PlayerPrefs.GetString("Username");
            MyToken = PlayerPrefs.GetString("Token");
            SendToken();
        }
    }

    public void Play()
    {
        SceneManager.LoadScene("Scenes/ClickGame");
    }

    public void SendRegister()
    {
        AuthData data = new AuthData();
        data.username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text;
        data.password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text;
        StartCoroutine(Register(JsonUtility.ToJson(data)));
    }

    public void SendLogin()
    {
        AuthData data = new AuthData();
        data.username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text;
        data.password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text;
        StartCoroutine(Login(JsonUtility.ToJson(data)));
    }
    public void SendToken()
    {
        AuthData data = new AuthData();
        data.token = MyToken;
        StartCoroutine(Token(JsonUtility.ToJson(data)));
    }

    IEnumerator Register(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url+"/api/usuarios",json);
        request.method = "POST";
        request.SetRequestHeader("Content-Type","application/json");
        //Debug.Log("enviado");
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                Debug.Log("Registro Exitoso");
                //StartCoroutine(Login(json));
            }
            else
            {
                Debug.Log($"{request.responseCode}|{request.error}");
                StartCoroutine(ShowError(request.responseCode.ToString(), request.error));
            }
        }
    }

    IEnumerator Login(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url+"/api/auth/login",json);
        request.method = "POST";
        request.SetRequestHeader("Content-Type","application/json");
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
                MyToken = data.token;
                MyUsername = data.usuario.username;
                Debug.Log($"{data.token}");
                Debug.Log(data.usuario.username);
                PlayerPrefs.SetString("Username",data.usuario.username);
                PlayerPrefs.SetString("Token",data.token);
                PlayBtn.SetActive(true);
            }
            else
            {
                Debug.Log($"{request.responseCode}|{request.error}");
                StartCoroutine(ShowError(request.responseCode.ToString(), request.error));
            }
        }
    }

    IEnumerator Token(string json)
    {
        UnityWebRequest request = UnityWebRequest.Get(url+"/api/usuarios/"+MyUsername);
        request.SetRequestHeader("x-token",MyToken);
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
                Debug.Log($"Mi nombres es: {data.usuario.username}");
                GameObject playBtn = GameObject.Find("PlayBtn");
                PlayBtn.SetActive(true);
            }
            else
            {
                Debug.Log($"{request.responseCode}|{request.error}");
                StartCoroutine(ShowError(request.responseCode.ToString(), request.error));
            }
        }
    }

    IEnumerator ShowError(string responseCode, string error)
    {
        errorText.text = $"{responseCode}|{error}";
        yield return new WaitForSeconds(5);
        errorText.text = "";
    }
}

[System.Serializable]
public class AuthData
{
    public string username;
    public string password;
    public Usuario usuario;
    public Usuario[] usuarios;
    public string token;
    public string score;
}
[System.Serializable]
public class Usuario
{
    public string uid;
    public string username;
    public int score;
    public Data data;

}
[System.Serializable]
public class Data
{
    public int score;

}
