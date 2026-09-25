using UnityEngine;
using UnityEngine.UI;

namespace MeteorStorm
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("--- HUD Principal ---")]
        [SerializeField] private Text livesText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text highScoreText;

        [Header("--- Painel de Game Over ---")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text gameOverFinalScoreText;
        [SerializeField] private Text gameOverHighScoreText;
        [SerializeField] private Text gameOverTimeText;
        [SerializeField] private Button restartButton;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            AutoFindReferences();
        }

        private void Start()
        {
            AutoFindReferences();

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            }
        }

        public void AutoFindReferences()
        {
            if (livesText == null) livesText = FindTextInObject(gameObject, "LivesText");
            if (timerText == null) timerText = FindTextInObject(gameObject, "TimerText");
            if (scoreText == null) scoreText = FindTextInObject(gameObject, "ScoreText");
            if (highScoreText == null) highScoreText = FindTextInObject(gameObject, "HighScoreText");

            if (gameOverPanel == null)
            {
                Transform panel = transform.Find("GameOverPanel");
                if (panel == null)
                {
                    Transform[] allChildren = GetComponentsInChildren<Transform>(true);
                    foreach (var c in allChildren)
                    {
                        if (c.name.Equals("GameOverPanel", System.StringComparison.OrdinalIgnoreCase))
                        {
                            panel = c;
                            break;
                        }
                    }
                }
                if (panel != null)
                    gameOverPanel = panel.gameObject;
            }

            if (gameOverPanel != null)
            {
                if (gameOverFinalScoreText == null) gameOverFinalScoreText = FindTextInObject(gameOverPanel, "FinalScoreText");
                if (gameOverHighScoreText == null) gameOverHighScoreText = FindTextInObject(gameOverPanel, "HighScoreText");
                if (gameOverTimeText == null) gameOverTimeText = FindTextInObject(gameOverPanel, "SurvivedTimeText");
                if (restartButton == null) restartButton = gameOverPanel.GetComponentInChildren<Button>(true);
            }

            // Garantir que todos os textos tenham uma fonte funcional
            Font font = AssetLoader.GetDefaultFont();
            Text[] allTexts = GetComponentsInChildren<Text>(true);
            foreach (var t in allTexts)
            {
                if (t != null && t.font == null)
                {
                    t.font = font;
                }
            }
        }

        private Text FindTextInObject(GameObject root, string name)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform c in children)
            {
                if (c.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                {
                    Text t = c.GetComponent<Text>();
                    if (t != null)
                    {
                        if (t.font == null) t.font = AssetLoader.GetDefaultFont();
                        return t;
                    }
                }
            }
            return null;
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = $"SCORE: {score:D6}";
        }

        public void UpdateHighScore(int highScore)
        {
            if (highScoreText != null)
                highScoreText.text = $"HIGH SCORE: {highScore:D6}";
        }

        public void UpdateLives(int lives)
        {
            if (livesText != null)
            {
                string hearts = "";
                for (int i = 0; i < lives; i++)
                {
                    hearts += "♥ ";
                }
                livesText.text = hearts.TrimEnd();
            }
        }

        public void UpdateTimer(float seconds)
        {
            if (timerText != null)
            {
                int mins = Mathf.FloorToInt(seconds / 60f);
                int secs = Mathf.FloorToInt(seconds % 60f);
                timerText.text = $"{mins:00}:{secs:00}";
            }
        }

        public void ShowGameOverPanel(int finalScore, int highScore, float survivedSeconds)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                if (gameOverFinalScoreText != null)
                    gameOverFinalScoreText.text = $"Pontuação Final: {finalScore}";

                if (gameOverHighScoreText != null)
                    gameOverHighScoreText.text = $"Maior Pontuação: {highScore}";

                int mins = Mathf.FloorToInt(survivedSeconds / 60f);
                int secs = Mathf.FloorToInt(survivedSeconds % 60f);
                if (gameOverTimeText != null)
                    gameOverTimeText.text = $"Tempo Sobrevivido: {mins:00}:{secs:00}";
            }
        }

        public void HideGameOverPanel()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        private void OnRestartButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }
    }
}
