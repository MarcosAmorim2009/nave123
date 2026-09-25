#pragma warning disable CS0618
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MeteorStorm.Editor
{
    /// <summary>
    /// Script auxiliar do Unity Editor para construir, verificar e salvar
    /// nativamente as cenas MainGame e SampleScene usando as APIs do Unity.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneSetupHelper
    {
        static SceneSetupHelper()
        {
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying)
                {
                    EnsureSceneSetup();
                }
            };
        }

        [MenuItem("Meteor Storm/Recriar Ambas as Cenas (MainGame e SampleScene)", false, 1)]
        public static void SetupBothScenes()
        {
            Debug.Log("--- [Meteor Storm] Construindo e salvando MainGame.unity e SampleScene.unity ---");
            BuildAndSaveScene("Assets/Scenes/MainGame.unity");
            BuildAndSaveScene("Assets/Scenes/SampleScene.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("--- [Meteor Storm] Ambas as cenas salvas com sucesso no formato nativo da Unity! ---");
        }

        public static void BuildAndSaveScene(string scenePath)
        {
            UnityEngine.SceneManagement.Scene scene;
            try
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
            catch
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            }

            // 1. Camera
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.transform.position = new Vector3(0, 0, -10);
            }
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.04f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;

            if (cam.GetComponent<AudioListener>() == null)
            {
                cam.gameObject.AddComponent<AudioListener>();
            }

            // 2. GameManager
            GameManager gm = Object.FindObjectOfType<GameManager>();
            if (gm == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gm = gmObj.AddComponent<GameManager>();
            }

            // 3. AudioManager
            AudioManager audioMgr = Object.FindObjectOfType<AudioManager>();
            if (audioMgr == null)
            {
                GameObject audioObj = new GameObject("AudioManager");
                audioMgr = audioObj.AddComponent<AudioManager>();
            }
            audioMgr.SetClips(
                AssetLoader.LoadAudioClip("sfx_shoot.wav"),
                AssetLoader.LoadAudioClip("sfx_damage.wav"),
                AssetLoader.LoadAudioClip("sfx_explosion_small.wav"),
                AssetLoader.LoadAudioClip("sfx_explosion_large.wav"),
                AssetLoader.LoadAudioClip("sfx_gameover.wav"),
                AssetLoader.LoadAudioClip("music_arcade_theme.wav")
            );

            // 4. SpawnManager
            SpawnManager spawnMgr = Object.FindObjectOfType<SpawnManager>();
            if (spawnMgr == null)
            {
                GameObject spawnObj = new GameObject("SpawnManager");
                spawnMgr = spawnObj.AddComponent<SpawnManager>();
            }
            spawnMgr.SetPrefabs(
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bullet.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MeteorLarge.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MeteorMedium.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MeteorSmall.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MeteorExplosion.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerExplosion.prefab")
            );

            // 5. Player
            PlayerController player = Object.FindObjectOfType<PlayerController>();
            if (player == null)
            {
                GameObject playerObj = new GameObject("Player");
                playerObj.transform.position = new Vector3(0, -3.8f, 0);

                SpriteRenderer sr = playerObj.AddComponent<SpriteRenderer>();
                sr.sprite = AssetLoader.LoadSprite("player_ship.png");
                sr.material = AssetLoader.GetUnlitMaterial();
                sr.sortingOrder = 10;

                CircleCollider2D col = playerObj.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.42f;

                Rigidbody2D rb = playerObj.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;

                player = playerObj.AddComponent<PlayerController>();
            }

            // 6. Background
            ParallaxBackground pb = Object.FindObjectOfType<ParallaxBackground>();
            if (pb == null)
            {
                GameObject bgRoot = new GameObject("Background");
                bgRoot.transform.localScale = new Vector3(2.2f, 2.2f, 1f);

                Material unlitMat = AssetLoader.GetUnlitMaterial();
                CreateBgPiece(bgRoot.transform, "Nebula_A", "bg_nebula.png", unlitMat, -30, 0f, 3f);
                CreateBgPiece(bgRoot.transform, "Nebula_B", "bg_nebula.png", unlitMat, -30, 5.12f, 3f);
                CreateBgPiece(bgRoot.transform, "StarsFar_A", "bg_stars_far.png", unlitMat, -20, 0f, 2f);
                CreateBgPiece(bgRoot.transform, "StarsFar_B", "bg_stars_far.png", unlitMat, -20, 5.12f, 2f);
                CreateBgPiece(bgRoot.transform, "StarsNear_A", "bg_stars_near.png", unlitMat, -10, 0f, 1f);
                CreateBgPiece(bgRoot.transform, "StarsNear_B", "bg_stars_near.png", unlitMat, -10, 5.12f, 1f);

                pb = bgRoot.AddComponent<ParallaxBackground>();
            }

            // 7. Canvas e UI
            UIManager uiMgr = Object.FindObjectOfType<UIManager>();
            if (uiMgr == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<GraphicRaycaster>();
                uiMgr = canvasObj.AddComponent<UIManager>();

                Font defaultFont = AssetLoader.GetDefaultFont();

                CreateText(canvasObj.transform, "ScoreText", "SCORE: 000000", defaultFont, 34, FontStyle.Bold, new Color(1f, 0.9f, 0.2f), new Vector2(1, 1), new Vector2(-220, -45), new Vector2(400, 50), TextAnchor.MiddleRight);
                CreateText(canvasObj.transform, "HighScoreText", "HIGH SCORE: 000000", defaultFont, 24, FontStyle.Normal, new Color(0.4f, 0.85f, 1f), new Vector2(1, 1), new Vector2(-220, -90), new Vector2(400, 40), TextAnchor.MiddleRight);
                CreateText(canvasObj.transform, "TimerText", "00:00", defaultFont, 38, FontStyle.Bold, Color.white, new Vector2(0.5f, 1), new Vector2(0, -45), new Vector2(240, 55), TextAnchor.MiddleCenter);

                GameObject livesPanel = new GameObject("LivesPanel");
                livesPanel.transform.SetParent(canvasObj.transform, false);
                RectTransform lpRt = livesPanel.AddComponent<RectTransform>();
                lpRt.anchorMin = new Vector2(0, 1);
                lpRt.anchorMax = new Vector2(0, 1);
                lpRt.anchoredPosition = new Vector2(160, -45);
                lpRt.sizeDelta = new Vector2(280, 55);
                CreateText(livesPanel.transform, "LivesText", "♥ ♥ ♥", defaultFont, 42, FontStyle.Bold, new Color(1f, 0.2f, 0.35f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(280, 55), TextAnchor.MiddleLeft);

                GameObject goPanel = new GameObject("GameOverPanel");
                goPanel.transform.SetParent(canvasObj.transform, false);
                RectTransform gRt = goPanel.AddComponent<RectTransform>();
                gRt.anchorMin = new Vector2(0.5f, 0.5f);
                gRt.anchorMax = new Vector2(0.5f, 0.5f);
                gRt.anchoredPosition = Vector2.zero;
                gRt.sizeDelta = new Vector2(640, 460);

                Image panelBg = goPanel.AddComponent<Image>();
                panelBg.color = new Color(0.05f, 0.06f, 0.1f, 0.94f);

                CreateText(goPanel.transform, "TitleText", "GAME OVER", defaultFont, 56, FontStyle.Bold, new Color(1f, 0.22f, 0.28f), new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(550, 70), TextAnchor.MiddleCenter);
                CreateText(goPanel.transform, "FinalScoreText", "Pontuação Final: 0", defaultFont, 30, FontStyle.Bold, Color.white, new Vector2(0.5f, 1), new Vector2(0, -140), new Vector2(500, 40), TextAnchor.MiddleCenter);
                CreateText(goPanel.transform, "HighScoreText", "Maior Pontuação: 0", defaultFont, 28, FontStyle.Bold, new Color(1f, 0.85f, 0.2f), new Vector2(0.5f, 1), new Vector2(0, -190), new Vector2(500, 40), TextAnchor.MiddleCenter);
                CreateText(goPanel.transform, "SurvivedTimeText", "Tempo Sobrevivido: 00:00", defaultFont, 26, FontStyle.Bold, new Color(0.4f, 0.9f, 1f), new Vector2(0.5f, 1), new Vector2(0, -240), new Vector2(500, 40), TextAnchor.MiddleCenter);

                GameObject btnObj = new GameObject("RestartButton");
                btnObj.transform.SetParent(goPanel.transform, false);
                RectTransform btnRt = btnObj.AddComponent<RectTransform>();
                btnRt.anchorMin = new Vector2(0.5f, 0);
                btnRt.anchorMax = new Vector2(0.5f, 0);
                btnRt.anchoredPosition = new Vector2(0, 100);
                btnRt.sizeDelta = new Vector2(280, 64);
                Image btnImg = btnObj.AddComponent<Image>();
                btnImg.color = new Color(0.16f, 0.45f, 0.95f, 1f);
                Button btn = btnObj.AddComponent<Button>();
                btn.targetGraphic = btnImg;
                CreateText(btnObj.transform, "Text", "REINICIAR", defaultFont, 28, FontStyle.Bold, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280, 64), TextAnchor.MiddleCenter);

                CreateText(goPanel.transform, "RestartHint", "Ou pressione [R] para reiniciar", defaultFont, 20, FontStyle.Normal, new Color(0.65f, 0.65f, 0.7f), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(400, 30), TextAnchor.MiddleCenter);
                goPanel.SetActive(false);

                uiMgr.AutoFindReferences();
            }

            // 8. EventSystem
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }

            // 9. MeteorSpawner
            if (GameObject.Find("MeteorSpawner") == null)
            {
                new GameObject("MeteorSpawner");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void CreateBgPiece(Transform parent, string name, string spriteFile, Material mat, int sortOrder, float yPos, float zPos)
        {
            GameObject piece = new GameObject(name);
            piece.transform.SetParent(parent);
            piece.transform.localPosition = new Vector3(0, yPos, zPos);
            SpriteRenderer sr = piece.AddComponent<SpriteRenderer>();
            sr.sprite = AssetLoader.LoadSprite(spriteFile);
            sr.material = mat;
            sr.sortingOrder = sortOrder;
        }

        private static Text CreateText(Transform parent, string name, string text, Font font, int size, FontStyle style, Color color, Vector2 anchor, Vector2 pos, Vector2 sizeDelta, TextAnchor alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;

            Text txt = obj.AddComponent<Text>();
            txt.text = text;
            txt.font = font;
            txt.fontSize = size;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = alignment;
            txt.raycastTarget = false;
            return txt;
        }

        private static void EnsureSceneSetup()
        {
            if (Object.FindObjectOfType<PlayerController>() == null)
            {
                SetupBothScenes();
            }
        }
    }
}
#endif
