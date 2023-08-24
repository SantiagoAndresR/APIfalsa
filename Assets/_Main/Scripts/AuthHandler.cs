using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Linq;
using System;


public class AuthHandler : MonoBehaviour

{
    [SerializeField] 
    TMP_InputField UserNameInput, PassWordInput;
    
    public string apiUrl = "https://sid-restapi.onrender.com/api/";
    
    string token;
    string username;
    
    [SerializeField] 
    TMP_Text menssage;
    
    [SerializeField] 
    TMP_Text errormenssage;
    
    [Header("Observacion Lista de Puntaje")]
    [SerializeField] 
    TextMeshProUGUI[] Playerscore;

    [SerializeField] 
    TextMeshProUGUI[] Score;
    
    [Header("Actualizar Referencia de Puntaje")]
    [SerializeField] 
    TMP_InputField ScoreInput;
    
    [Header("Game Panel")]
    [SerializeField] GameObject LoginPanel;
    [SerializeField] GameObject GamePanel;
    
    [Header("Otras Referencias")]
    [SerializeField] 
    TextMeshProUGUI CurrentUserInput;


    void Start()
    {
        menssage.text = "";
        errormenssage.text = "";
        token = PlayerPrefs.GetString("Token");
        username = PlayerPrefs.GetString("Username");

        print("Token: " + token + "User: " + username);

        List<UserT> lista = new List<UserT>(); 
        List<UserT> listaOrdenada = lista.OrderByDescending(u => u.data.score).ToList<UserT>(); 

        if (string.IsNullOrEmpty(token))
        {
            LoginPanel.SetActive(true);
            GamePanel.SetActive(false);
            menssage.text = "No se ha registrado ningun token del usuario";
        }
        else
        {
            LoginPanel.SetActive(false);
            GamePanel.SetActive(true);
            Debug.Log(token);
            Debug.Log(CurrentUserInput);
            StartCoroutine(GetProfile()); 
        }
    }

    public void Register()
    {
        UserT authData = new UserT(); 
        authData.username = UserNameInput.text;
        authData.password = PassWordInput.text;

        string json = JsonUtility.ToJson(authData);

        StartCoroutine(SendRegister(json));
    }

    public void Login()
    {
        UserT authData = new UserT(); 
        authData.username = UserNameInput.text;
        authData.password = PassWordInput.text;

        string json = JsonUtility.ToJson(authData);

        StartCoroutine(SendLogin(json));
    }

    IEnumerator SendRegister(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(apiUrl + "usuarios", json);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "POST";
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Oh oh, algo ha salido mal: " + request.error);
            errormenssage.text = "Oh oh, algo ha salido mal: " + request.error;
        }

        else
        {
            menssage.text = request.downloadHandler.text;
            if (request.responseCode == 200)
            {
                DataUser data = JsonUtility.FromJson<DataUser>(request.downloadHandler.text); 
                Debug.Log("El id del usuario es: " + data.usuario._id);
                menssage.text = "El id del usuario es: " + data.usuario._id;
            }
            else
            {
                errormenssage.text = request.error;
            }
        }
    }

    IEnumerator SendLogin(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(apiUrl + "auth/login", json);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "POST";
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            errormenssage.text = "Oh oh, algo ha salido mal: " + request.error;
        }

        else
        {
            menssage.text = request.downloadHandler.text;
            if (request.responseCode == 200)
            {
                DataUser data = JsonUtility.FromJson<DataUser>(request.downloadHandler.text); 

                token = data.token;
                username = data.usuario.username;

                PlayerPrefs.SetString("Token", token);
                PlayerPrefs.SetString("Username", username);

                Debug.Log("El usuario: " + data.usuario.username + " ha iniciado sesion y su token es: " + data.token);
                menssage.text = "El usuario: " + data.usuario.username + "ha iniciado sesion y su token es: " + data.token;


                LoginPanel.SetActive(false);
                GamePanel.SetActive(true);

                CurrentUserInput.text = data.usuario.username;
                ScoreInput.text = data.data.score.ToString();


                StartCoroutine(ResetScore());
            }
            else
            {

                errormenssage.text = "Oh oh, algo ha salido mal: " + request.error;
            }
        }
    }

    public void UpdateCurrentUserScore()
    {
        UserT user = new UserT(); 
        user.username = username;

        if (int.TryParse(ScoreInput.text, out _))
        {
            user.data.score = int.Parse(ScoreInput.text);
        }

        string NextData = JsonUtility.ToJson(user);
        Debug.Log(NextData);
        StartCoroutine(UpdateScore(NextData));
    }

    IEnumerator UpdateScore(string NextData)
    {
        UnityWebRequest request = UnityWebRequest.Put(apiUrl + "usuarios", NextData);

        request.method = "PATCH";
        request.SetRequestHeader("x-token", token);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {

            Debug.Log("Ha ocurrido un error: " + request.error);
            errormenssage.text = "Oh oh, algo ha salido mal: " + request.error;
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            menssage.text = request.downloadHandler.text;

            if (request.responseCode == 200)
            {

                DataUser jsonData = JsonUtility.FromJson<DataUser>(request.downloadHandler.text); 
                StartCoroutine(ResetScore());
                Debug.Log(jsonData.usuario.username + " ha sido actualizado con: " + jsonData.usuario.data.score + " puntos");
            }
            else
            {
                string mensaje = "Estado de la web: " + request.responseCode;
                mensaje += "\ncontent-type: " + request.GetResponseHeader(" content-type");
                mensaje += "\nOh oh, algo ha salido mal: " + request.error;
                Debug.Log(mensaje);
            }

        }
    }


    IEnumerator ResetScore() 
        {
            UnityWebRequest request = UnityWebRequest.Get(apiUrl + "usuarios");
            request.SetRequestHeader("x-token", token);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Ha ocurrido un error: " + request.error);
                errormenssage.text = "Oh oh, algo ha salido mal: " + request.error; 

            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                menssage.text = request.downloadHandler.text; 

                if (request.responseCode == 200)
                {
                    Userlist jsonList = JsonUtility.FromJson<Userlist>(request.downloadHandler.text);
                    Debug.Log(jsonList.usuarios.Count);

                    foreach (UserT userJson in jsonList.usuarios) 
                    {
                        Debug.Log(userJson.username);
                    }

                    List<UserT> lista = jsonList.usuarios; 
                    List<UserT> listaOrdenada = lista.OrderByDescending(u => u.data.score).ToList<UserT>(); 

                    int len = Playerscore.Length; 
                    for (int i = 0; i < len; i++)
                    {
                        Playerscore[i].text = listaOrdenada[i].username; 
                        Score[i].text = listaOrdenada[i].data.score.ToString(); 
                    }
                }
                else
                {
                    string mensaje = "Estado de la web: " + request.responseCode;
                    mensaje += "\ncontent-type: " + request.GetResponseHeader(" content-type");
                    mensaje += "\nOh oh, algo ha salido mal: " + request.error;
                    Debug.Log(mensaje);
                }
            }
        }


    IEnumerator GetProfile()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl + "usuarios/" + username);


        request.SetRequestHeader("x-token", token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Ha ocirrido un error: " + request.error);
            errormenssage.text = "Oh oh, algo ha salido mal: " + request.error;
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            menssage.text = request.downloadHandler.text;

            if (request.responseCode == 200)
            {

                DataUser jsonData = JsonUtility.FromJson<DataUser>(request.downloadHandler.text);  

                Debug.Log(jsonData.usuario.username + " es el usuario actual");

                CurrentUserInput.text = jsonData.usuario.username;
                ScoreInput.text = jsonData.usuario.data.score.ToString();

                StartCoroutine(ResetScore());
            }
            else
            {
                string mensaje = "Estado de la web: " + request.responseCode;
                mensaje += "\ncontent-type: " + request.GetResponseHeader(" content-type");
                mensaje += "\nOh oh, algo ha salido mal: " + request.error;

                Debug.Log(mensaje);
            }

        }
    }
    public void GoBack()
    {
        LoginPanel.SetActive(true);
        errormenssage.text = "";

        GamePanel.SetActive(false);
    }

}


[System.Serializable]
public class UserT 
{
    public string _id;
    public string username;
    public string password;

    public UserData data;

    public UserT() 
    {
        data = new UserData();
    }
    public UserT(string username, string password) 
    {
        this.username = username;
        this.password = password;
        data = new UserData();
    }
}

[System.Serializable]
public class UserData
{
    public int score;
}

[System.Serializable]
public class DataUser
{
    public UserT usuario; 
    public UserData data;
    public string token;
}

[System.Serializable]
public class Userlist
{
    public List<UserT> usuarios; 
}

