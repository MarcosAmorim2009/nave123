using System.Collections;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MeteorStorm
{
    /// <summary>
    /// Controla a nave do jogador: movimentacao fluida e responsiva,
    /// limites rigidos da tela, disparo continuo de projeteis,
    /// sistema de dano com invencibilidade temporaria e feedback visual.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        [Header("--- Movimentacao ---")]
        [Tooltip("Velocidade de deslocamento da nave")]
        [SerializeField] private float moveSpeed = 10f;
        
        [Tooltip("Margem interna de compensacao dos limites de tela")]
        [SerializeField] private Vector2 screenPadding = new Vector2(0.6f, 0.6f);

        [Header("--- Sistema de Disparo ---")]
        [Tooltip("Intervalo minimo entre disparos consecutivos (em segundos)")]
        [SerializeField] private float fireRate = 0.15f;
        
        [Tooltip("Ponto de saida dos tiros em relacao a nave")]
        [SerializeField] private Vector3 fireOffset = new Vector3(0f, 0.6f, 0f);

        [Header("--- Sobrevivencia e Invencibilidade ---")]
        [Tooltip("Duracao da invencibilidade apos tomar dano (segundos)")]
        [SerializeField] private float invincibilityDuration = 2.0f;
        
        [Tooltip("Velocidade do efeito de piscar durante a invencibilidade")]
        [SerializeField] private float blinkInterval = 0.1f;

        // Referencias e Controle Interno
        private SpriteRenderer spriteRenderer;
        private Collider2D playerCollider;
        private Camera mainCamera;
        private float nextFireTime;
        private bool isInvincible;
        private bool isDead;

        private Vector2 minScreenBounds;
        private Vector2 maxScreenBounds;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerCollider = GetComponent<Collider2D>();
            mainCamera = Camera.main;

            if (spriteRenderer != null)
            {
                if (spriteRenderer.sprite == null)
                {
                    spriteRenderer.sprite = AssetLoader.LoadSprite("player_ship.png");
                }
                spriteRenderer.material = AssetLoader.GetUnlitMaterial();
                spriteRenderer.sortingOrder = 10;
            }
        }

        private void Start()
        {
            UpdateScreenBounds();
            ResetPlayerState();
        }

        private void Update()
        {
            if (isDead || GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                return;

            HandleMovement();
            HandleShooting();
        }

        public void UpdateScreenBounds()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera == null) return;

            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

            minScreenBounds = new Vector2(bottomLeft.x + screenPadding.x, bottomLeft.y + screenPadding.y);
            maxScreenBounds = new Vector2(topRight.x - screenPadding.x, topRight.y - screenPadding.y);
        }

        private void HandleMovement()
        {
            float horizontal = 0f;
            float vertical = 0f;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            }
#endif

            try
            {
                if (Mathf.Approximately(horizontal, 0f))
                    horizontal = Input.GetAxisRaw("Horizontal");
                if (Mathf.Approximately(vertical, 0f))
                    vertical = Input.GetAxisRaw("Vertical");
            }
            catch { }

            Vector3 direction = new Vector3(horizontal, vertical, 0f);
            if (direction.sqrMagnitude > 1f)
                direction.Normalize();

            transform.position += direction * (moveSpeed * Time.deltaTime);

            float clampedX = Mathf.Clamp(transform.position.x, minScreenBounds.x, maxScreenBounds.x);
            float clampedY = Mathf.Clamp(transform.position.y, minScreenBounds.y, maxScreenBounds.y);
            transform.position = new Vector3(clampedX, clampedY, transform.position.z);
        }

        private void HandleShooting()
        {
            bool firePressed = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
                firePressed = true;
#endif

            try
            {
                if (!firePressed && Input.GetKey(KeyCode.Space))
                    firePressed = true;
            }
            catch { }

            if (firePressed && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                ShootBullet();
            }
        }

        private void ShootBullet()
        {
            Vector3 spawnPos = transform.position + fireOffset;
            if (SpawnManager.Instance != null)
            {
                SpawnManager.Instance.SpawnBullet(spawnPos);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShoot();
            }
        }

        public void TakeDamage()
        {
            if (isInvincible || isDead) return;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayDamage();

            if (GameManager.Instance != null)
                GameManager.Instance.LoseLife();

            if (GameManager.Instance != null && GameManager.Instance.CurrentLives > 0)
            {
                StartCoroutine(InvincibilityRoutine());
            }
            else
            {
                Die();
            }
        }

        private IEnumerator InvincibilityRoutine()
        {
            isInvincible = true;
            float elapsed = 0f;
            Color originalColor = spriteRenderer.color;

            while (elapsed < invincibilityDuration)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
            }

            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
            isInvincible = false;
        }

        public void Die()
        {
            isDead = true;
            isInvincible = true;
            StopAllCoroutines();

            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            if (playerCollider != null)
                playerCollider.enabled = false;

            if (SpawnManager.Instance != null)
            {
                SpawnManager.Instance.SpawnShipExplosion(transform.position);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayExplosionLarge();
            }
        }

        public void ResetPlayerState()
        {
            isDead = false;
            isInvincible = false;
            StopAllCoroutines();

            transform.position = new Vector3(0f, -3.8f, 0f);

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.color = Color.white;
                spriteRenderer.material = AssetLoader.GetUnlitMaterial();
            }

            if (playerCollider != null)
                playerCollider.enabled = true;

            UpdateScreenBounds();
        }

        public bool IsInvincible => isInvincible;
        public bool IsDead => isDead;
    }
}
