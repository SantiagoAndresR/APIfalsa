using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;


public class AuthHandler : MonoBehaviour
{
    public string apiUrl = "https://sid-restapi.onrender.com/api/";

    TMP_InputField UsernameInput;
    TMP_InputField PasswordInput;

    private string Token;
    private string Username;


    private void Start()
    {
        Token = PlayerPrefs.GetString("token");

        if (string.IsNullOrEmpty(Token))
        {
            Debug.Log("No hay Token almacenado");
        }
        else
        {
            Username = PlayerPrefs.GetString("username");
            StartCoroutine(GetPerfil(Username));
        }


        UsernameInput = GameObject.Find("UsernameInput").GetComponent<TMP_InputField>(); 
        PasswordInput = GameObject.Find("PasswordInput").GetComponent<TMP_InputField>();  

    }

    public void Register()
    {
        AuthData authData = new AuthData();
        authData.username = UsernameInput.text;
        authData.password = PasswordInput.text;

        string json = JsonUtility.ToJson(authData);

        StartCoroutine(SendRegister(json));
    }

    public void Login()
    {
        AuthData authData = new AuthData();
        authData.username = UsernameInput.text;
        authData.password = PasswordInput.text;

        string json = JsonUtility.ToJson(authData);

        StartCoroutine(SendLogin(json));
    }

    IEnumerator GetPerfil(string username)
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl + "usuarios/" + username);
        request.SetRequestHeader("x-token", Token);
        yield return request.SendWebRequest();
        if (request.isNetworkError)
        {
            Debug.Log("Network Error :" + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
                Debug.Log("Se del usuario: " + data.usuario.username);
                Debug.Log("Su Score es: " + data.usuario.data.score);
                SceneManager.LoadScene("Game");
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }

    IEnumerator SendRegister(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(apiUrl + "/usuarios", json);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "POST";
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("Network Error :" + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);

                Debug.Log("Se registro el usuario con id: " + data.usuario._id);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }


    IEnumerator SendLogin(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(apiUrl + "auth/login", json);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "POST";
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("Network Error :" + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);

                Debug.Log("Se inicio sesion del usuario: " + data.usuario.username);

                PlayerPrefs.SetString("token", data.token);
                PlayerPrefs.SetString("username", data.usuario.username);
                SceneManager.LoadScene("Game");
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }



}


[System.Serializable]
public class AuthData
{
    public string username;
    public string password;
    public DataUser usuario;
    public string token;
    public UserT[] usuarios;
}


[System.Serializable]
public class DataUser
{
    public string _id;
    public string username;
    public bool estado;
    public UserT data;
}

[System.Serializable]
public class UserT
{
    public int score;
    public string data;
    public DataUser[] Friends;
}
