using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MeteorStorm
{
    /// <summary>
    /// Utilitario central e resiliente para carregamento de assets (Sprites, Audio, Fontes e Materiais).
    /// Garante que todos os elementos visuais e sonoros funcionem imediatamente em qualquer versao
    /// da Unity (Unity 2020, 2021, 2022, Unity 6), tanto no Editor quanto em tempo de execucao.
    /// </summary>
    public static class AssetLoader
    {
        private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
        private static Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();
        private static Material unlitMaterial;
        private static Font defaultFont;

        /// <summary>
        /// Retorna um Material Unlit compativel com o renderizador atual (URP 2D Unlit ou Sprites/Default).
        /// Garante que sprites espaciais, tiros e meteoros sejam 100% visíveis e brilhantes.
        /// </summary>
        public static Material GetUnlitMaterial()
        {
            if (unlitMaterial != null) return unlitMaterial;

            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("UI/Default");

            if (shader != null)
            {
                unlitMaterial = new Material(shader);
                unlitMaterial.name = "MeteorStorm_UnlitMaterial";
            }
            return unlitMaterial;
        }

        /// <summary>
        /// Carrega uma Sprite do projeto de forma infalivel.
        /// </summary>
        public static Sprite LoadSprite(string fileName, float pixelsPerUnit = 100f)
        {
            if (spriteCache.TryGetValue(fileName, out Sprite cached) && cached != null)
                return cached;

#if UNITY_EDITOR
            string assetPath = "Assets/Sprites/" + fileName;
            Sprite editorSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (editorSprite != null)
            {
                spriteCache[fileName] = editorSprite;
                return editorSprite;
            }
#endif

            // Fallback direto do disco via leitura binaria de PNG
            string fullPath = Path.Combine(Application.dataPath, "Sprites", fileName);
            if (!File.Exists(fullPath))
            {
                fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Sprites", fileName);
            }

            if (File.Exists(fullPath))
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(fullPath);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (tex.LoadImage(bytes))
                    {
                        tex.filterMode = FilterMode.Bilinear;
                        tex.wrapMode = TextureWrapMode.Clamp;
                        Sprite s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
                        spriteCache[fileName] = s;
                        return s;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[AssetLoader] Falha ao ler sprite {fileName} do disco: {ex.Message}");
                }
            }

            return null;
        }

        /// <summary>
        /// Carrega um AudioClip de forma infalivel.
        /// </summary>
        public static AudioClip LoadAudioClip(string fileName)
        {
            if (audioCache.TryGetValue(fileName, out AudioClip cached) && cached != null)
                return cached;

#if UNITY_EDITOR
            string assetPath = "Assets/Audio/" + fileName;
            AudioClip editorClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (editorClip != null)
            {
                audioCache[fileName] = editorClip;
                return editorClip;
            }
#endif

            // Fallback: parser rapido de arquivo WAV PCM 16-bit
            string fullPath = Path.Combine(Application.dataPath, "Audio", fileName);
            if (!File.Exists(fullPath))
            {
                fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Audio", fileName);
            }

            if (File.Exists(fullPath))
            {
                try
                {
                    byte[] wav = File.ReadAllBytes(fullPath);
                    if (wav.Length > 44)
                    {
                        int channels = System.BitConverter.ToInt16(wav, 22);
                        int sampleRate = System.BitConverter.ToInt32(wav, 24);
                        int dataLength = wav.Length - 44;
                        int sampleCount = dataLength / 2;
                        float[] floatData = new float[sampleCount];

                        for (int i = 0; i < sampleCount; i++)
                        {
                            short s = System.BitConverter.ToInt16(wav, 44 + i * 2);
                            floatData[i] = s / 32768f;
                        }

                        AudioClip clip = AudioClip.Create(Path.GetFileNameWithoutExtension(fileName), sampleCount / channels, channels, sampleRate, false);
                        clip.SetData(floatData, 0);
                        audioCache[fileName] = clip;
                        return clip;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[AssetLoader] Falha ao carregar audio {fileName}: {ex.Message}");
                }
            }

            return null;
        }

        /// <summary>
        /// Obtem uma fonte funcional para renderizacao de textos da UI sem falhas.
        /// </summary>
        public static Font GetDefaultFont(int size = 24)
        {
            if (defaultFont != null) return defaultFont;

            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (defaultFont == null)
            {
                try
                {
                    defaultFont = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Segoe UI", "Tahoma", "Helvetica" }, size);
                }
                catch { }
            }
            return defaultFont;
        }
    }
}
