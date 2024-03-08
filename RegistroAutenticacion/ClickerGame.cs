using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class ClickerGame : MonoBehaviour
{
    [SerializeField] private float tiempo=0;
    [SerializeField] private int score=0;
    private bool startGame=false;
    private bool OnPlay = false;
    public Button exitBtn;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;

    public TextMeshProUGUI usernameText;

    public TextMeshProUGUI[] users;
    public TextMeshProUGUI[] scores;

    public GameObject LeaderBoard;
    // Start is called before the first frame update
    void Start()
    {
        usernameText.text += TokenHandler.MyUsername;
        if (!PlayerPrefs.HasKey("score"))
        {
            PlayerPrefs.SetInt("score",score);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)&&!startGame)
        {
            startGame = true;
            StartCoroutine(GameStarted());
        }
        //tiempo += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space)&&OnPlay)
        {
            score++;
            scoreText.text = $"score: {score}";
        }
    }

    IEnumerator GameStarted()
    {
        OnPlay = true;
        for (int i = 0; i < 11; i++)
        {
            timeText.text = $"Time: {i}";
            yield return new WaitForSeconds(1);
        }
        exitBtn.gameObject.SetActive(true);
        OnPlay = false;
        AuthData data = new AuthData();
        Usuario usuario = new Usuario();
        data.usuario = usuario;
        if (score > PlayerPrefs.GetInt("score"))
        {
            PlayerPrefs.SetInt("score",score);
        }
        else
        {
            score = PlayerPrefs.GetInt("score");
        }
        usuario.username = TokenHandler.MyUsername;
        usuario.score = score;
        Debug.Log($"Voy a enviar: {usuario.score}");
        StartCoroutine(UpdateScore(JsonUtility.ToJson(usuario)));
    }

    public void Exit()
    {
        SceneManager.LoadScene("SegundaEntrega");
    }
    IEnumerator UpdateScore(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(TokenHandler.url+"/api/usuarios",json);
        request.method = "PATCH";
        request.SetRequestHeader("Content-Type","application/json");
        request.SetRequestHeader("x-token",TokenHandler.MyToken);
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                Debug.Log("Hice el PATCH");
                StartCoroutine(GetScore(json));
            }
            else
            {
                Debug.Log($"{request.responseCode}|{request.error}");
            }
        }
    }
    
    IEnumerator GetScore(string json)
    {
        UnityWebRequest request = UnityWebRequest.Get(TokenHandler.url+"/api/usuarios");
        request.SetRequestHeader("x-token",TokenHandler.MyToken);
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
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
                Debug.Log($"EL largo es: {data.usuarios.Length}");
                Usuario[] usuariosOrdenados =
                    data.usuarios.OrderByDescending(user => user.data.score).Take(5).ToArray();
                ShowLeaderBoard(usuariosOrdenados);
                
                Debug.Log($"Mi score es: {data.usuarios[0].score}");
            }
            else
            {
                Debug.Log($"{request.responseCode}|{request.error}");
            }
        }
    }

    private void ShowLeaderBoard(Usuario[] Orderedusers)
    {
        for (int i = 0; i < Orderedusers.Length; i++)
        {
            users[i].text = Orderedusers[i].username+":";
            scores[i].text = $"{Orderedusers[i].score}";
        }
        LeaderBoard.SetActive(true);
    }
}
