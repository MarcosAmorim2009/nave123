using System.Collections.Generic;
using UnityEngine;

namespace MeteorStorm
{
    public class SpawnManager : MonoBehaviour
    {
        public static SpawnManager Instance { get; private set; }

        [Header("--- Prefabs dos Meteoros ---")]
        [SerializeField] private GameObject meteorLargePrefab;
        [SerializeField] private GameObject meteorMediumPrefab;
        [SerializeField] private GameObject meteorSmallPrefab;

        [Header("--- Prefabs de Suporte ---")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject meteorExplosionPrefab;
        [SerializeField] private GameObject shipExplosionPrefab;

        [Header("--- Configuracoes de Pooling ---")]
        [SerializeField] private int initialBullets = 30;
        [SerializeField] private int initialMeteorsLarge = 15;
        [SerializeField] private int initialMeteorsMedium = 20;
        [SerializeField] private int initialMeteorsSmall = 25;
        [SerializeField] private int initialExplosions = 20;

        [Header("--- Progressao de Dificuldade ---")]
        [SerializeField] private float difficultyStepTime = 30f;
        [SerializeField] private float initialSpawnRate = 1.0f;

        private List<GameObject> bulletPool = new List<GameObject>();
        private List<GameObject> meteorLargePool = new List<GameObject>();
        private List<GameObject> meteorMediumPool = new List<GameObject>();
        private List<GameObject> meteorSmallPool = new List<GameObject>();
        private List<GameObject> explosionPool = new List<GameObject>();
        private List<GameObject> shipExplosionPool = new List<GameObject>();

        private float nextSpawnTime;
        private float difficultyTimer;
        private int currentDifficultyLevel = 0;
        private float currentSpawnRate = 1.0f;
        private float currentSpeedMultiplier = 1.0f;
        private bool isSpawningActive = false;

        private Camera mainCamera;
        private Transform poolContainer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            mainCamera = Camera.main;

            GameObject containerObj = new GameObject("_ObjectPoolContainer");
            containerObj.transform.SetParent(transform);
            poolContainer = containerObj.transform;

            EnsurePrefabs();
            InitializePools();
        }

        private void EnsurePrefabs()
        {
#if UNITY_EDITOR
            if (bulletPrefab == null) bulletPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bullet.prefab");
            if (meteorLargePrefab == null) meteorLargePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MeteorLarge.prefab");
            if (meteorMediumPrefab == null) meteorMediumPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MeteorMedium.prefab");
            if (meteorSmallPrefab == null) meteorSmallPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MeteorSmall.prefab");
            if (meteorExplosionPrefab == null) meteorExplosionPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MeteorExplosion.prefab");
            if (shipExplosionPrefab == null) shipExplosionPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerExplosion.prefab");
#endif
            // Fallback procedurais em runtime se ainda nulos
            if (bulletPrefab == null) bulletPrefab = CreateFallbackBulletPrefab();
            if (meteorLargePrefab == null) meteorLargePrefab = CreateFallbackMeteorPrefab(MeteorType.Large);
            if (meteorMediumPrefab == null) meteorMediumPrefab = CreateFallbackMeteorPrefab(MeteorType.Medium);
            if (meteorSmallPrefab == null) meteorSmallPrefab = CreateFallbackMeteorPrefab(MeteorType.Small);
            if (meteorExplosionPrefab == null) meteorExplosionPrefab = CreateFallbackExplosionPrefab("MeteorExplosion", Color.yellow);
            if (shipExplosionPrefab == null) shipExplosionPrefab = CreateFallbackExplosionPrefab("PlayerExplosion", Color.cyan);
        }

        private GameObject CreateFallbackBulletPrefab()
        {
            GameObject obj = new GameObject("Bullet_Template");
            obj.transform.SetParent(poolContainer);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = AssetLoader.LoadSprite("bullet.png");
            sr.material = AssetLoader.GetUnlitMaterial();
            sr.sortingOrder = 5;

            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.24f, 0.48f);

            Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            obj.AddComponent<BulletController>();
            obj.SetActive(false);
            return obj;
        }

        private GameObject CreateFallbackMeteorPrefab(MeteorType type)
        {
            string name = "Meteor_" + type;
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(poolContainer);

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            string sName = type == MeteorType.Large ? "meteor_large.png" : (type == MeteorType.Medium ? "meteor_medium.png" : "meteor_small.png");
            sr.sprite = AssetLoader.LoadSprite(sName);
            sr.material = AssetLoader.GetUnlitMaterial();
            sr.sortingOrder = 3;

            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = type == MeteorType.Large ? 0.54f : (type == MeteorType.Medium ? 0.42f : 0.28f);

            Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            MeteorController mc = obj.AddComponent<MeteorController>();
            mc.Setup(type, 1f);

            obj.SetActive(false);
            return obj;
        }

        private GameObject CreateFallbackExplosionPrefab(string name, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(poolContainer);
            ParticleSystem ps = obj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startLifetime = 0.5f;
            main.startSpeed = 6f;
            main.loop = false;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

            ParticleSystemRenderer psr = obj.GetComponent<ParticleSystemRenderer>();
            psr.material = AssetLoader.GetUnlitMaterial();
            psr.sortingOrder = 10;

            obj.SetActive(false);
            return obj;
        }

        private void Start()
        {
            currentSpawnRate = initialSpawnRate;
        }

        private void Update()
        {
            if (!isSpawningActive || GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                return;

            difficultyTimer += Time.deltaTime;
            if (difficultyTimer >= difficultyStepTime)
            {
                difficultyTimer = 0f;
                AdvanceDifficulty();
            }

            if (Time.time >= nextSpawnTime)
            {
                float spawnInterval = 1f / Mathf.Max(0.1f, currentSpawnRate);
                nextSpawnTime = Time.time + spawnInterval;
                SpawnMeteorWave();
            }
        }

        private void AdvanceDifficulty()
        {
            currentDifficultyLevel++;
            currentSpawnRate = initialSpawnRate + currentDifficultyLevel * 1.0f;
            currentSpeedMultiplier = 1.0f + (currentDifficultyLevel * 0.18f);
        }

        private void SpawnMeteorWave()
        {
            SpawnSingleMeteor();

            if (currentDifficultyLevel >= 2 && Random.value < 0.35f)
            {
                SpawnSingleMeteor();
            }
            if (currentDifficultyLevel >= 4 && Random.value < 0.25f)
            {
                SpawnSingleMeteor();
            }
        }

        private void SpawnSingleMeteor()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera == null) return;

            float randomViewportX = Random.Range(0.06f, 0.94f);
            Vector3 spawnWorldPos = mainCamera.ViewportToWorldPoint(new Vector3(randomViewportX, 1.08f, 0f));
            spawnWorldPos.z = 0f;

            float roll = Random.value;
            MeteorType chosenType;
            GameObject meteorObj = null;

            if (roll < 0.40f)
            {
                chosenType = MeteorType.Large;
                meteorObj = GetPooledObject(meteorLargePool, meteorLargePrefab);
            }
            else if (roll < 0.75f)
            {
                chosenType = MeteorType.Medium;
                meteorObj = GetPooledObject(meteorMediumPool, meteorMediumPrefab);
            }
            else
            {
                chosenType = MeteorType.Small;
                meteorObj = GetPooledObject(meteorSmallPool, meteorSmallPrefab);
            }

            if (meteorObj != null)
            {
                meteorObj.transform.position = spawnWorldPos;
                meteorObj.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                meteorObj.SetActive(true);

                MeteorController controller = meteorObj.GetComponent<MeteorController>();
                if (controller != null)
                {
                    controller.Setup(chosenType, currentSpeedMultiplier);
                }
            }
        }

        public void SpawnBullet(Vector3 position)
        {
            GameObject bullet = GetPooledObject(bulletPool, bulletPrefab);
            if (bullet != null)
            {
                bullet.transform.position = position;
                bullet.transform.rotation = Quaternion.identity;
                bullet.SetActive(true);
            }
        }

        public void SpawnMeteorExplosion(Vector3 position, MeteorType type)
        {
            GameObject explosion = GetPooledObject(explosionPool, meteorExplosionPrefab);
            if (explosion != null)
            {
                explosion.transform.position = position;
                float scale = (type == MeteorType.Large) ? 1.5f : (type == MeteorType.Medium ? 1.0f : 0.7f);
                explosion.transform.localScale = Vector3.one * scale;
                explosion.SetActive(true);

                ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Clear();
                    ps.Play();
                }
            }
        }

        public void SpawnShipExplosion(Vector3 position)
        {
            GameObject explosion = GetPooledObject(shipExplosionPool, shipExplosionPrefab != null ? shipExplosionPrefab : meteorExplosionPrefab);
            if (explosion != null)
            {
                explosion.transform.position = position;
                explosion.transform.localScale = Vector3.one * 2.2f;
                explosion.SetActive(true);

                ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Clear();
                    ps.Play();
                }
            }
        }

        public void StartSpawning()
        {
            isSpawningActive = true;
            difficultyTimer = 0f;
            currentDifficultyLevel = 0;
            currentSpawnRate = initialSpawnRate;
            currentSpeedMultiplier = 1.0f;
            nextSpawnTime = Time.time + 0.5f;
        }

        public void StopSpawning()
        {
            isSpawningActive = false;
        }

        public void ClearAllActiveObjects()
        {
            DeactivateAllInPool(bulletPool);
            DeactivateAllInPool(meteorLargePool);
            DeactivateAllInPool(meteorMediumPool);
            DeactivateAllInPool(meteorSmallPool);
            DeactivateAllInPool(explosionPool);
            DeactivateAllInPool(shipExplosionPool);
        }

        private void DeactivateAllInPool(List<GameObject> pool)
        {
            foreach (var obj in pool)
            {
                if (obj != null && obj.activeInHierarchy)
                    obj.SetActive(false);
            }
        }

        private void InitializePools()
        {
            PrewarmPool(bulletPool, bulletPrefab, initialBullets);
            PrewarmPool(meteorLargePool, meteorLargePrefab, initialMeteorsLarge);
            PrewarmPool(meteorMediumPool, meteorMediumPrefab, initialMeteorsMedium);
            PrewarmPool(meteorSmallPool, meteorSmallPrefab, initialMeteorsSmall);
            PrewarmPool(explosionPool, meteorExplosionPrefab, initialExplosions);
            PrewarmPool(shipExplosionPool, shipExplosionPrefab, 5);
        }

        private void PrewarmPool(List<GameObject> pool, GameObject prefab, int count)
        {
            if (prefab == null) return;

            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(prefab, poolContainer);
                obj.SetActive(false);
                pool.Add(obj);
            }
        }

        private GameObject GetPooledObject(List<GameObject> pool, GameObject prefab)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null && !pool[i].activeInHierarchy)
                {
                    return pool[i];
                }
            }

            if (prefab != null)
            {
                GameObject newObj = Instantiate(prefab, poolContainer);
                newObj.SetActive(false);
                pool.Add(newObj);
                return newObj;
            }

            return null;
        }

        public void SetPrefabs(GameObject bullet, GameObject meteorL, GameObject meteorM, GameObject meteorS, GameObject expMeteor, GameObject expShip)
        {
            bulletPrefab = bullet;
            meteorLargePrefab = meteorL;
            meteorMediumPrefab = meteorM;
            meteorSmallPrefab = meteorS;
            meteorExplosionPrefab = expMeteor;
            shipExplosionPrefab = expShip;
        }
    }
}
