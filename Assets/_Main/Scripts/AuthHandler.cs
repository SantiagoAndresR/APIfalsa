using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class AuthHandler : MonoBehaviour
{
    public TMP_InputField UsernameInput;
    public TMP_InputField PasswordInput;

    public string apiUrl = "https://sid-restapi.onrender.com/api";

    private void Start()
    {
        UsernameInput = GameObject.Find("UsernameInput").GettComponent<TMP_ImputField>();
        PasswordInput = GameObject.Find("PasswordInput").GettComponent<TMP_ImputField>();
    }

    public void Register()
    {
        AuthData authData = new AuthData();
        authData.username = UsernameInput.text;
        authData.password = PasswordInput.text;

        string json = JsonUtility.ToJson(authData);

        StartCoroutine(SendRegister(json));
    }

    IEnumerator SendRegister(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(apiUrl + "/usuarios", json);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "POST";
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.log("Network Error :" + request.error);
        }
        else
        {
            Debug.log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);

                Debug.log("Se registro el usuario con id: " + data.usuario._id);
            }
            else
            {
                Debug.log(request.error);
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
            Debug.log("Network Error :" + request.error);
        }
        else
        {
            Debug.log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);

                Debug.log("Se inicio sesion del usuario: " + data.usuario.username);
                Debug.log(data.token);
            }
            else
            {
                Debug.log(request.error);
            }
        }
    }

    public void Login()
    {
        AuthData authData = new AuthData();
        authData.username = UsernameInput.text;
        authData.password = PasswordInput.text;

        string json = JsonUtility.ToJson(authData);

        StartCoroutine(SendLogin(json));
    }


}


[System.Serializable]
public class AuthData
{
    public string username;
    public string password;
    public UserData usuario;
    public string token;
}

[System.Serializable]
public class UserData
{
    public string _id;
    public string username;
    public bool estado;
}

