using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Start Screen")]
    public GameObject startScreen;
    public TMP_Text startTitle;
    public TMP_Text startInstructions;

    [Header("Win Screen")]
    public GameObject winScreen;
    public TMP_Text winText;

    [Header("Pause Screen")]
    public GameObject pauseScreen;
    public TMP_Text pauseText;

    [Header("Game References")]
    public BossAI bossAI;

    [Header("In-Game UI (vor Spielstart unsichtbar)")]
    [Tooltip("Die 4 Buttons, die erst nach der Anleitung sichtbar werden sollen")]
    public Button[] uiButtons = new Button[4];

    [Tooltip("Die 6 TextMeshPro Texte, die erst nach der Anleitung sichtbar werden sollen")]
    public TMP_Text[] infoTexts = new TMP_Text[8];

    [Tooltip("Canvas, das den Lebensbalken enthält")]
    public Canvas healthBarCanvas;

    [Header("Game Settings")]
    public bool gameStarted = false;
    public bool gameEnded = false;
    private bool gamePaused = false;
    private bool bossAlreadyDefeated = false;

    void Awake()
    {

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


        if (winScreen != null) winScreen.SetActive(false);
        if (pauseScreen != null) pauseScreen.SetActive(false);
        if (startScreen != null) startScreen.SetActive(true);

        SetGameplayUIActive(false);
    }

    void Start()
    {

        if (winScreen != null && winScreen.activeSelf)
            winScreen.SetActive(false);

        if (pauseScreen != null && pauseScreen.activeSelf)
            pauseScreen.SetActive(false);

        if (startScreen != null && !startScreen.activeSelf)
            startScreen.SetActive(true);


        Time.timeScale = 0f;

        Debug.Log("Spiel bereit. Drücke ENTER/SPACE zum Starten.");
    }

    void Update()
    {
        if (!gameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                StartGame();
            }
            return;
        }


        if (gamePaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                ResumeGame();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                QuitGame();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResumeGame();
            }
            return;
        }


        if (!gameEnded)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (bossAI == null && !bossAlreadyDefeated)
            {
                WinGame();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
        }
 
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                ContinuePlaying();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
        }


        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }



    public void StartGame()
    {
        if (gameStarted) return;
        gameStarted = true;


        if (startScreen != null)
        {
            startScreen.SetActive(false);
        }

   
        SetGameplayUIActive(true);


        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;

        Debug.Log("Spiel gestartet! In-Game UI sichtbar.");
    }

    void WinGame()
    {
        if (gameEnded) return;

        gameEnded = true;
        bossAlreadyDefeated = true;
        Debug.Log("WinGame aufgerufen");


        if (winScreen != null)
        {
            winScreen.SetActive(true);
            if (winText != null)
            {
                winText.text = "GEWONNEN!\nBoss besiegt!\n\n" +
                               "ENTER = Weiterspielen";
            }
        }


        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ContinuePlaying()
    {
        Debug.Log("Weiterspielen...");


        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }


        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        gameEnded = false;

        Debug.Log("Spiel läuft weiter, Maus gesperrt, WinScreen versteckt");
    }

    void PauseGame()
    {
        if (gamePaused) return;

        gamePaused = true;
        Time.timeScale = 0f; 


        if (pauseScreen != null)
        {
            pauseScreen.SetActive(true);
            if (pauseText != null)
            {
                pauseText.text = "PAUSE\n\n" +
                                 "ENTER     =   Weiter\n" +
                                 "Q         =   Beenden\n" +
                                 "ESC       =   Zurück";
            }
        }


        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Spiel pausiert");
    }

    public void ResumeGame()
    {
        if (!gamePaused) return;

        gamePaused = false;


        if (pauseScreen != null)
        {
            pauseScreen.SetActive(false);
        }


        Time.timeScale = 1f;

        if (gameStarted && !gameEnded)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Debug.Log("Spiel fortgesetzt");
    }

    public void RestartGame()
    {
        Debug.Log("Spiel neu starten...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Spiel wird beendet...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    private void SetGameplayUIActive(bool active)
    {

        if (uiButtons != null)
        {
            foreach (var b in uiButtons)
            {
                if (b != null)
                    b.gameObject.SetActive(active);
            }
        }


        if (infoTexts != null)
        {
            foreach (var t in infoTexts)
            {
                if (t != null)
                    t.gameObject.SetActive(active);
            }
        }

 
        if (healthBarCanvas != null)
        {
            healthBarCanvas.gameObject.SetActive(active);
        }
    }


    public void ShowStartScreen()
    {
        gameStarted = false;
        gameEnded = false;
        gamePaused = false;
        bossAlreadyDefeated = false;

        if (startScreen != null)
            startScreen.SetActive(true);

        SetGameplayUIActive(false);

        if (pauseScreen != null)
            pauseScreen.SetActive(false);

        if (winScreen != null)
            winScreen.SetActive(false);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Startscreen angezeigt.");
    }
}
