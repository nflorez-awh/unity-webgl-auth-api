using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class AuthHandler : MonoBehaviour
{
    private string Token;
    private string Username;

    private const string ApiUrl = "https://sid-restapi.onrender.com";

    // ── Panel Menu ───────────────────────────────────────────
    [Header("Panel Menu")]
    [SerializeField] private GameObject panelMenu;

    // ── Panel Login ──────────────────────────────────────────
    [Header("Panel Login")]
    [SerializeField] private GameObject panelLogin;
    [SerializeField] private TMP_InputField loginUsernameInput;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private TMP_Text loginErrorText;

    // ── Panel Register ───────────────────────────────────────
    [Header("Panel Register")]
    [SerializeField] private GameObject panelRegister;
    [SerializeField] private TMP_InputField registerUsernameInput;
    [SerializeField] private TMP_InputField registerPasswordInput;
    [SerializeField] private TMP_Text registerErrorText;

    // ── Panel Main ───────────────────────────────────────────
    [Header("Panel Main")]
    [SerializeField] private GameObject panelMain;
    [SerializeField] private TMP_Text welcomeLabel;

    // ── Panel Scores ─────────────────────────────────────────
    [Header("Panel Scores")]
    [SerializeField] private GameObject panelScores;
    [SerializeField] private Transform scoresContainer;
    [SerializeField] private GameObject scoreRowPrefab;

    private void Start()
    {
        // Apagar todo al inicio
        if (panelMenu != null) panelMenu.SetActive(false);
        panelLogin.SetActive(false);
        panelRegister.SetActive(false);
        panelMain.SetActive(false);
        if (panelScores != null) panelScores.SetActive(false);

        SetError(loginErrorText, "");
        SetError(registerErrorText, "");

        Token = PlayerPrefs.GetString("Token", "");
        Username = PlayerPrefs.GetString("Username", "");

        if (!string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(Username))
        {
            StartCoroutine(VerifyToken());
        }
        else
        {
            ShowPanel(panelMenu);
        }
    }

    private IEnumerator VerifyToken()
    {
        UnityWebRequest www = UnityWebRequest.Get(ApiUrl + "/api/usuarios/" + Username);
        www.SetRequestHeader("x-token", Token);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Sesión restaurada: " + Username);
            EnterGame();
        }
        else
        {
            Debug.LogWarning("Token inválido, redirigiendo al login.");
            ClearSession();
            ShowPanel(panelLogin);
        }
    }

    public void RegisterButtonHandler()
    {
        SetError(registerErrorText, "");
        string user = registerUsernameInput.text.Trim();
        string pass = registerPasswordInput.text;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            SetError(registerErrorText, "Completa todos los campos.");
            return;
        }

        StartCoroutine(RegisterCoroutine(user, pass));
    }

    private IEnumerator RegisterCoroutine(string username, string password)
    {
        RegisterData data = new RegisterData { username = username, password = password };
        string json = JsonUtility.ToJson(data);

        UnityWebRequest www = UnityWebRequest.Post(ApiUrl + "/api/usuarios", json, "application/json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            loginUsernameInput.text = username;
            loginPasswordInput.text = password;
            SetError(loginErrorText, "✔ Cuenta creada. Inicia sesión.");
            GoToLoginButtonHandler();
        }
        else
        {
            ErrorResponse err = TryParseError(www.downloadHandler.text);
            SetError(registerErrorText, err != null ? err.msg : "Error al registrar. Intenta otro usuario.");
        }
    }

    public void LoginButtonHandler()
    {
        SetError(loginErrorText, "");
        string user = loginUsernameInput.text.Trim();
        string pass = loginPasswordInput.text;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            SetError(loginErrorText, "Ingresa usuario y contraseña.");
            return;
        }

        StartCoroutine(LoginCoroutine(user, pass));
    }

    private IEnumerator LoginCoroutine(string username, string password)
    {
        AuthData authData = new AuthData { username = username, password = password };
        string json = JsonUtility.ToJson(authData);

        UnityWebRequest www = UnityWebRequest.Post(ApiUrl + "/api/auth/login", json, "application/json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            AuthResponse response = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
            Token = response.token;
            Username = response.usuario.username;

            PlayerPrefs.SetString("Token", Token);
            PlayerPrefs.SetString("Username", Username);
            PlayerPrefs.Save();

            EnterGame();
        }
        else
        {
            ErrorResponse err = TryParseError(www.downloadHandler.text);
            SetError(loginErrorText, err != null ? err.msg : "Usuario o contraseña incorrectos.");
        }
    }

    public void LogoutButtonHandler()
    {
        ClearSession();
        if (welcomeLabel != null) welcomeLabel.text = "";
        ShowPanel(panelMenu);
    }

    private void ClearSession()
    {
        Token = "";
        Username = "";
        PlayerPrefs.DeleteKey("Token");
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.Save();
    }

    public void SubmitScore(int score)
    {
        if (string.IsNullOrEmpty(Token))
        {
            Debug.LogWarning("No hay sesión activa para guardar el score.");
            return;
        }
        StartCoroutine(UpdateScoreCoroutine(score));
    }

    private IEnumerator UpdateScoreCoroutine(int score)
    {
        ScoreData scoreData = new ScoreData { score = score };
        string json = JsonUtility.ToJson(scoreData);

        UnityWebRequest www = UnityWebRequest.Put(ApiUrl + "/api/usuarios/" + Username, json);
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("x-token", Token);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
            Debug.Log("Score actualizado: " + score);
        else
            Debug.LogError("Error al actualizar score: " + www.downloadHandler.text);
    }

    public void ShowScoresButtonHandler()
    {
        StartCoroutine(FetchScoresCoroutine());
    }

    private IEnumerator FetchScoresCoroutine()
    {
        UnityWebRequest www = UnityWebRequest.Get(ApiUrl + "/api/usuarios");
        www.SetRequestHeader("x-token", Token);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al obtener scores: " + www.downloadHandler.text);
            yield break;
        }

        UsersListResponse listResponse = JsonUtility.FromJson<UsersListResponse>(www.downloadHandler.text);

        if (listResponse == null || listResponse.usuarios == null)
        {
            Debug.LogError("Respuesta inesperada al obtener scores.");
            yield break;
        }

        List<UserScore> sorted = new List<UserScore>(listResponse.usuarios);
        sorted.Sort((a, b) => b.score.CompareTo(a.score));

        foreach (Transform child in scoresContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < sorted.Count; i++)
        {
            GameObject row = Instantiate(scoreRowPrefab, scoresContainer);
            TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();

            if (texts.Length >= 2)
            {
                texts[0].text = (i + 1) + ". " + sorted[i].username;
                texts[1].text = sorted[i].score.ToString();
            }
        }

        ShowPanel(panelScores);
    }

    public void HideScoresButtonHandler()
    {
        ShowPanel(panelMain);
    }

    public void GoToRegisterButtonHandler()
    {
        SetError(registerErrorText, "");
        ShowPanel(panelRegister);
    }

    public void GoToLoginButtonHandler()
    {
        SetError(loginErrorText, "");
        ShowPanel(panelLogin);
    }

    private void ShowPanel(GameObject target)
    {
        if (panelMenu != null) panelMenu.SetActive(false);
        panelLogin.SetActive(false);
        panelRegister.SetActive(false);
        panelMain.SetActive(false);
        if (panelScores != null) panelScores.SetActive(false);

        if (target != null) target.SetActive(true);
    }

    private void EnterGame()
    {
        if (welcomeLabel != null)
            welcomeLabel.text = Username;

        ShowPanel(panelMain);
    }

    private void SetError(TMP_Text label, string message)
    {
        if (label != null) label.text = message;
    }

    private ErrorResponse TryParseError(string json)
    {
        try { return JsonUtility.FromJson<ErrorResponse>(json); }
        catch { return null; }
    }
}

[System.Serializable] public class AuthData { public string username; public string password; }
[System.Serializable] public class RegisterData { public string username; public string password; }
[System.Serializable] public class ScoreData { public int score; }

[System.Serializable] public class User { public string _id; public string username; }
[System.Serializable] public class UserScore { public string _id; public string username; public int score; }

[System.Serializable] public class AuthResponse { public User usuario; public string token; }
[System.Serializable] public class UsersListResponse { public UserScore[] usuarios; }
[System.Serializable] public class ErrorResponse { public string msg; }