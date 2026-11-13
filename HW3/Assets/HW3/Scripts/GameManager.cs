using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Game state (win / lose), HUD, background music toggle and the
/// "Play another round?" screen (bonus) with two buttons.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool IsGameOver => Instance != null && Instance.gameOver;

    [Header("References")]
    public PlayerController player;
    public AudioSource music;           // assign any music clip; if empty, procedural music is used

    [Header("Rules")]
    public float fallKillY = -5f;

    [Header("UI")]
    public string gameTitle = "MAZE ESCAPE";

    static bool musicOn = true;         // remembered between rounds

    bool gameOver;
    bool won;
    string resultText = "";
    float startTime;
    float endTime;
    float panelShowTime;
    string hint = "";
    float hintUntil;

    GUIStyle titleStyle, textStyle, hudStyle, buttonStyle, hintStyle;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.GetComponent<PlayerController>();
        }
        if (music == null) music = GetComponent<AudioSource>();
    }

    void Start()
    {
        startTime = Time.time;
        if (music != null)
        {
            if (music.clip == null) music.clip = ProceduralMusic.Create();
            music.loop = true;
            music.mute = !musicOn;
            music.Play();
        }
        ShowHint("Find the golden key (push it or press E to carry it) and bring it to the green door!", 5f);
    }

    void Update()
    {
        if (GameInput.MusicTogglePressed()) ToggleMusic();

        if (!gameOver && player != null && player.transform.position.y < fallKillY)
            Lose("You fell off the maze!");

        if (gameOver && GameInput.RestartPressed()) Restart();
    }

    public void ToggleMusic()
    {
        musicOn = !musicOn;
        if (music != null) music.mute = !musicOn;
        ShowHint(musicOn ? "Music ON" : "Music OFF", 1.2f);
    }

    public void ShowHint(string text, float seconds = 3f)
    {
        hint = text;
        hintUntil = Time.time + seconds;
    }

    public void Win()
    {
        if (gameOver) return;
        gameOver = true;
        won = true;
        endTime = Time.time;
        resultText = $"You escaped the maze in {endTime - startTime:0.0} seconds!";
        panelShowTime = Time.time + 1.2f;   // let the door open first
        ThirdPersonCamera.LockCursor(false);
    }

    public void Lose(string reason)
    {
        if (gameOver) return;
        gameOver = true;
        won = false;
        endTime = Time.time;
        resultText = reason;
        panelShowTime = Time.time + 0.8f;   // let the player fall over first
        if (player != null) player.Die();
        ThirdPersonCamera.LockCursor(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        var scene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(scene.path))
        {
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                scene.path, new LoadSceneParameters(LoadSceneMode.Single));
            return;
        }
#endif
        SceneManager.LoadScene(scene.buildIndex >= 0 ? scene.buildIndex : 0);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void EnsureStyles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 52, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        textStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true, richText = true };
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 28, fontStyle = FontStyle.Bold };
        hintStyle = new GUIStyle(GUI.skin.box) { fontSize = 24, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        textStyle.normal.textColor = Color.white;
        hudStyle.normal.textColor = Color.white;
        hintStyle.normal.textColor = Color.white;
    }

    void OnGUI()
    {
        EnsureStyles();

        // scale everything to a 1080p reference
        float s = Screen.height / 1080f;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(s, s, 1f));
        float w = Screen.width / s;
        const float h = 1080f;

        // ---- HUD ----
        float elapsed = (gameOver ? endTime : Time.time) - startTime;
        GUI.Box(new Rect(20, 20, 460, 190), "");
        GUILayout.BeginArea(new Rect(35, 28, 430, 180));
        GUILayout.Label($"<b>{gameTitle}</b>   Time: {elapsed:0.0}s", hudStyle);
        GUILayout.Label("WASD move · Shift run · Space jump · E take/drop key\nMouse look · M music · Esc free cursor", hudStyle);
        if (GUILayout.Button(musicOn ? "Music: ON  (M)" : "Music: OFF  (M)", GUILayout.Width(220), GUILayout.Height(36)))
            ToggleMusic();
        GUILayout.EndArea();

        // ---- Hint ----
        if (!gameOver && Time.time < hintUntil && !string.IsNullOrEmpty(hint))
            GUI.Box(new Rect(w / 2f - 380, 30, 760, 60), hint, hintStyle);

        // ---- Game over panel (play again?) ----
        if (gameOver && Time.time >= panelShowTime)
        {
            Rect r = new Rect(w / 2f - 320, h / 2f - 190, 640, 380);
            GUI.Box(r, "");
            GUI.Box(r, "");
            GUI.Box(r, ""); // stacked boxes = darker background

            titleStyle.normal.textColor = won ? new Color(0.3f, 1f, 0.45f) : new Color(1f, 0.3f, 0.3f);

            GUILayout.BeginArea(new Rect(r.x + 30, r.y + 25, r.width - 60, r.height - 50));
            GUILayout.Label(won ? "YOU ESCAPED!" : "GAME OVER", titleStyle, GUILayout.Height(80));
            GUILayout.Label(resultText, textStyle);
            GUILayout.Space(10);
            GUILayout.Label("Play another round?", textStyle);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Play Again", buttonStyle, GUILayout.Height(64))) Restart();
            GUILayout.Space(20);
            if (GUILayout.Button("Quit", buttonStyle, GUILayout.Height(64))) Quit();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
