using UnityEngine;

namespace MeteorStorm
{
    public class ParallaxBackground : MonoBehaviour
    {
        [System.Serializable]
        public class BackgroundLayer
        {
            public Transform transformA;
            public Transform transformB;
            public float scrollSpeed = 0.5f;
            [HideInInspector] public float textureHeight = 10f;
        }

        [Header("--- Camadas Parallax ---")]
        [SerializeField] private BackgroundLayer farStars = new BackgroundLayer { scrollSpeed = 0.4f };
        [SerializeField] private BackgroundLayer nearStars = new BackgroundLayer { scrollSpeed = 1.2f };
        [SerializeField] private BackgroundLayer nebula = new BackgroundLayer { scrollSpeed = 0.2f };

        private void Awake()
        {
            if (farStars.transformA == null) farStars.transformA = transform.Find("StarsFar_A");
            if (farStars.transformB == null) farStars.transformB = transform.Find("StarsFar_B");
            if (nearStars.transformA == null) nearStars.transformA = transform.Find("NearStars_A");
            if (nearStars.transformB == null) nearStars.transformB = transform.Find("NearStars_B");
            if (nebula.transformA == null) nebula.transformA = transform.Find("Nebula_A");
            if (nebula.transformB == null) nebula.transformB = transform.Find("Nebula_B");

            Material unlit = AssetLoader.GetUnlitMaterial();
            SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in srs)
            {
                if (sr != null)
                {
                    sr.material = unlit;
                    if (sr.sprite == null)
                    {
                        if (sr.name.Contains("Nebula")) sr.sprite = AssetLoader.LoadSprite("bg_nebula.png");
                        else if (sr.name.Contains("StarsFar")) sr.sprite = AssetLoader.LoadSprite("bg_stars_far.png");
                        else if (sr.name.Contains("StarsNear")) sr.sprite = AssetLoader.LoadSprite("bg_stars_near.png");
                    }
                }
            }
        }

        private void Start()
        {
            InitLayer(farStars);
            InitLayer(nearStars);
            InitLayer(nebula);
        }

        private void InitLayer(BackgroundLayer layer)
        {
            if (layer.transformA != null && layer.transformB != null)
            {
                SpriteRenderer sr = layer.transformA.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    layer.textureHeight = sr.bounds.size.y;
                }
                else
                {
                    layer.textureHeight = 10f;
                }

                layer.transformB.position = layer.transformA.position + new Vector3(0, layer.textureHeight, 0);
            }
        }

        private void Update()
        {
            UpdateLayer(farStars);
            UpdateLayer(nearStars);
            UpdateLayer(nebula);
        }

        private void UpdateLayer(BackgroundLayer layer)
        {
            if (layer.transformA == null || layer.transformB == null) return;

            float delta = layer.scrollSpeed * Time.deltaTime;
            layer.transformA.position += Vector3.down * delta;
            layer.transformB.position += Vector3.down * delta;

            if (layer.transformA.position.y <= -layer.textureHeight)
            {
                layer.transformA.position = layer.transformB.position + new Vector3(0, layer.textureHeight, 0);
            }
            else if (layer.transformB.position.y <= -layer.textureHeight)
            {
                layer.transformB.position = layer.transformA.position + new Vector3(0, layer.textureHeight, 0);
            }
        }
    }
}
