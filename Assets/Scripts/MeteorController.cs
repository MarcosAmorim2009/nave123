using System.Collections;
using UnityEngine;

namespace MeteorStorm
{
    public enum MeteorType
    {
        Large,
        Medium,
        Small
    }

    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class MeteorController : MonoBehaviour
    {
        [Header("--- Classificacao do Meteoro ---")]
        [SerializeField] private MeteorType meteorType = MeteorType.Medium;

        [Header("--- Atributos Base ---")]
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private float baseSpeed = 3.5f;
        [SerializeField] private int scoreValue = 10;
        [SerializeField] private float minRotationSpeed = -90f;
        [SerializeField] private float maxRotationSpeed = 90f;

        private int currentHealth;
        private float currentSpeed;
        private float rotationSpeed;
        private float bottomBoundary;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Camera mainCamera;
        private bool isDestroyed;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
            mainCamera = Camera.main;

            if (spriteRenderer != null)
            {
                spriteRenderer.material = AssetLoader.GetUnlitMaterial();
                spriteRenderer.sortingOrder = 3;
            }
        }

        private void OnEnable()
        {
            isDestroyed = false;
            currentHealth = maxHealth;
            rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
                spriteRenderer.material = AssetLoader.GetUnlitMaterial();
            }

            UpdateBottomLimit();
        }

        public void Setup(MeteorType type, float speedMultiplier)
        {
            meteorType = type;
            string spriteName = "meteor_medium.png";

            switch (type)
            {
                case MeteorType.Large:
                    maxHealth = 5;
                    baseSpeed = 2.0f;
                    scoreValue = 5;
                    transform.localScale = new Vector3(1.7f, 1.7f, 1f);
                    spriteName = "meteor_large.png";
                    break;

                case MeteorType.Medium:
                    maxHealth = 3;
                    baseSpeed = 3.3f;
                    scoreValue = 10;
                    transform.localScale = new Vector3(1.15f, 1.15f, 1f);
                    spriteName = "meteor_medium.png";
                    break;

                case MeteorType.Small:
                    maxHealth = 1;
                    baseSpeed = 5.2f;
                    scoreValue = 20;
                    transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                    spriteName = "meteor_small.png";
                    break;
            }

            if (spriteRenderer != null && (spriteRenderer.sprite == null || spriteRenderer.sprite.name != spriteName))
            {
                spriteRenderer.sprite = AssetLoader.LoadSprite(spriteName);
            }

            currentHealth = maxHealth;
            currentSpeed = baseSpeed * speedMultiplier;
        }

        private void Update()
        {
            if (isDestroyed) return;

            transform.Translate(Vector3.down * (currentSpeed * Time.deltaTime), Space.World);
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            if (transform.position.y < bottomBoundary)
            {
                RecycleMeteor();
            }
        }

        private void UpdateBottomLimit()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera != null)
            {
                bottomBoundary = mainCamera.ViewportToWorldPoint(new Vector3(0, -0.15f, 0)).y;
            }
            else
            {
                bottomBoundary = -7f;
            }
        }

        public void TakeDamage(int damage)
        {
            if (isDestroyed) return;

            currentHealth -= damage;
            StartCoroutine(FlashRoutine());

            if (currentHealth <= 0)
            {
                Explode();
            }
        }

        private IEnumerator FlashRoutine()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0.4f, 0.4f, 1f);
                yield return new WaitForSeconds(0.06f);
                if (spriteRenderer != null)
                    spriteRenderer.color = originalColor;
            }
        }

        private void Explode()
        {
            if (isDestroyed) return;
            isDestroyed = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayExplosionSmall();
            }

            if (SpawnManager.Instance != null)
            {
                SpawnManager.Instance.SpawnMeteorExplosion(transform.position, meteorType);
            }

            RecycleMeteor();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isDestroyed) return;

            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null && !player.IsDead && !player.IsInvincible)
            {
                player.TakeDamage();

                if (SpawnManager.Instance != null)
                {
                    SpawnManager.Instance.SpawnMeteorExplosion(transform.position, meteorType);
                }

                RecycleMeteor();
            }
        }

        public void RecycleMeteor()
        {
            gameObject.SetActive(false);
        }

        public MeteorType Type => meteorType;
    }
}
