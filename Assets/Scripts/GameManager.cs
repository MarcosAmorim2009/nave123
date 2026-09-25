#pragma warning disable CS0618
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MeteorStorm
{
    /// <summary>
    /// Gerenciador mestre do jogo:
    /// controla o estado global (Jogando, Fim de Jogo), pontuacao,
    /// vidas do jogador, cronometro de sobrevivencia e reinicio da fase.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState
        {
            Playing,
            GameOver
        }

        [Header("--- Configuracoes Iniciais ---")]
        [Tooltip("Quantidade inicial de vidas")]
        [SerializeField] private int startingLives = 3;

        [Header("--- Referencias ---")]
        [SerializeField] private PlayerController player;

        // Estado Atual
        public GameState CurrentState { get; private set; }
        public int CurrentScore { get; private set; }
        public int HighScore { get; private set; }
        public int CurrentLives { get; private set; }
        public float ElapsedSurvivalTime { get; private set; }

        private const string HIGH_SCORE_KEY = "MeteorStorm_HighScore";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadHighScore();
        }

        private void Start()
        {
            if (player == null)
            {
                player = FindObjectOfType<PlayerController>();
            }

            StartGame();
        }

        private void Update()
        {
            if (CurrentState == GameState.Playing)
            {
                ElapsedSurvivalTime += Time.deltaTime;

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateTimer(ElapsedSurvivalTime);
                }
            }
            else if (CurrentState == GameState.GameOver)
            {
                CheckRestartInput();
            }
        }

        /// <summary>
        /// Detecta o pressionamento da tecla R para reiniciar quando em Game Over.
        /// </summary>
        private void CheckRestartInput()
        {
            bool restartPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                restartPressed = true;
#endif

            try
            {
                if (!restartPressed && Input.GetKeyDown(KeyCode.R))
                    restartPressed = true;
            }
            catch { }

            if (restartPressed)
            {
                RestartGame();
            }
        }

        /// <summary>
        /// Inicializa ou reinicia todos os valores de partida.
        /// </summary>
        public void StartGame()
        {
            CurrentState = GameState.Playing;
            CurrentScore = 0;
            CurrentLives = startingLives;
            ElapsedSurvivalTime = 0f;

            if (player != null)
            {
                player.ResetPlayerState();
            }

            if (SpawnManager.Instance != null)
            {
                SpawnManager.Instance.ClearAllActiveObjects();
                SpawnManager.Instance.StartSpawning();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateScore(CurrentScore);
                UIManager.Instance.UpdateHighScore(HighScore);
                UIManager.Instance.UpdateLives(CurrentLives);
                UIManager.Instance.UpdateTimer(0f);
                UIManager.Instance.HideGameOverPanel();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic();
            }
        }

        /// <summary>
        /// Adiciona pontos ao score global e atualiza o High Score caso superado.
        /// </summary>
        public void AddScore(int amount)
        {
            if (CurrentState != GameState.Playing) return;

            CurrentScore += amount;

            if (CurrentScore > HighScore)
            {
                HighScore = CurrentScore;
                SaveHighScore();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateScore(CurrentScore);
                UIManager.Instance.UpdateHighScore(HighScore);
            }
        }

        /// <summary>
        /// Reduz uma vida do jogador e dispara Game Over ao atingir 0.
        /// </summary>
        public void LoseLife()
        {
            if (CurrentState != GameState.Playing) return;

            CurrentLives = Mathf.Max(0, CurrentLives - 1);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateLives(CurrentLives);
            }

            if (CurrentLives <= 0)
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// Dispara o estado de fim de jogo, congela spawns e exibe o painel de Game Over.
        /// </summary>
        private void TriggerGameOver()
        {
            CurrentState = GameState.GameOver;

            if (SpawnManager.Instance != null)
            {
                SpawnManager.Instance.StopSpawning();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGameOver();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGameOverPanel(CurrentScore, HighScore, ElapsedSurvivalTime);
            }
        }

        /// <summary>
        /// Reinicia a fase imediatamente sem recarregar a cena para maxima performance.
        /// </summary>
        public void RestartGame()
        {
            StartGame();
        }

        private void LoadHighScore()
        {
            HighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        }

        private void SaveHighScore()
        {
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, HighScore);
            PlayerPrefs.Save();
        }
    }
}
