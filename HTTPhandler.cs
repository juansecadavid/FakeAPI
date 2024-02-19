using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HTTPhandler : MonoBehaviour
{
    private string MortyUrl = "https://rickandmortyapi.com/api";
    [SerializeField] private RawImage[] characterQuads;
    [SerializeField] private TextMeshProUGUI[] cardNames;
    [SerializeField] private TextMeshProUGUI userText;
    private int userIndex=0;
    [SerializeField] private TextMeshProUGUI deckBtnText;
    private string currentUser = "";
    private int[] currentDeckCards=new int[4]; 
    private string fakeApiUrl = "https://my-json-server.typicode.com/juansecadavid/FakeAPI";


    public void SendRequest()
    {
        for (int i = 0; i < 4; i++)
        {
            StartCoroutine(GetCharacter(currentDeckCards[i],i));
        }
    }

    public void GetUser()
    {
        StartCoroutine(GetUsers());
    }

    IEnumerator GetCharacter(int image, int index)
    {
        UnityWebRequest request = UnityWebRequest.Get(MortyUrl+$"/character/{image}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                CharacterImage data = JsonUtility.FromJson<CharacterImage>(request.downloadHandler.text);
                StartCoroutine(DownloadImage(data.image,index));
                cardNames[index].text = data.name;
            }
            else
            {
                Debug.Log($"{request.responseCode}|{request.error}");
            }
        }
    }
    IEnumerator GetUsers()
    {
        UnityWebRequest request = UnityWebRequest.Get(fakeApiUrl + "/db");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                JsonData data = JsonUtility.FromJson<JsonData>(request.downloadHandler.text);
                currentUser = data.users[userIndex].name;
                deckBtnText.text = $"Get {currentUser}'s deck";
                for (int i = 0; i < 4; i++)
                {
                    currentDeckCards[i] = data.users[userIndex].deck[i].card;
                }
                userText.text = $"User: {currentUser}";
                userIndex++;
                if (userIndex > 3)
                    userIndex = 0;
            }
            else
            {
                Debug.Log($"{request.responseCode}|{request.error}");
            }
        }
    }

    IEnumerator DownloadImage(string url, int pos)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
        }
        else characterQuads[pos].texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
    }
}
[System.Serializable]
public class JsonData
{
    public CharacterImage[] users;
}
[System.Serializable]
public class CharacterImage
{
    public string name;
    public string image;
    public CardsDeck[] deck;
}

[System.Serializable]
public class CardsDeck
{
    public int card;
}
