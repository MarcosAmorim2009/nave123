using UnityEngine;

namespace MeteorStorm
{
    /// <summary>
    /// Controla o comportamento do projetil disparado pelo jogador:
    /// movimentacao linear veloz para cima, deteccao de colisao com meteoros,
    /// e auto-reciclagem no Pool de Objetos ao sair da tela.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BulletController : MonoBehaviour
    {
        [Header("--- Propriedades do Projetil ---")]
        [Tooltip("Velocidade do projetil em direcao ao topo da tela")]
        [SerializeField] private float speed = 18f;

        [Tooltip("Dano infligido ao atingir um meteoro")]
        [SerializeField] private int damage = 1;

        private float topBoundary;
        private Camera mainCamera;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            mainCamera = Camera.main;
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                if (spriteRenderer.sprite == null)
                    spriteRenderer.sprite = AssetLoader.LoadSprite("bullet.png");
                spriteRenderer.material = AssetLoader.GetUnlitMaterial();
                spriteRenderer.sortingOrder = 5;
            }

            UpdateScreenLimits();
        }

        private void OnEnable()
        {
            UpdateScreenLimits();
        }

        private void Update()
        {
            transform.Translate(Vector3.up * (speed * Time.deltaTime), Space.World);

            if (transform.position.y > topBoundary)
            {
                RecycleBullet();
            }
        }

        private void UpdateScreenLimits()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera != null)
            {
                topBoundary = mainCamera.ViewportToWorldPoint(new Vector3(0, 1.1f, 0)).y;
            }
            else
            {
                topBoundary = 7f;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            MeteorController meteor = collision.GetComponent<MeteorController>();
            if (meteor != null && meteor.gameObject.activeInHierarchy)
            {
                meteor.TakeDamage(damage);
                RecycleBullet();
            }
        }

        public void RecycleBullet()
        {
            gameObject.SetActive(false);
        }
    }
}
