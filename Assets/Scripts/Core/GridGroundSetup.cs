using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.Core
{
    [ExecuteAlways]
    public class GridGroundSetup : MonoBehaviour
    {
        [Header("Grid Settings")]
        public int gridSize = 256;
        public int checkSize = 32;
        public Color color1 = new Color(0.25f, 0.75f, 0.25f);
        public Color color2 = new Color(0.10f, 0.45f, 0.10f);
        public Vector2 tiling = new Vector2(25f, 25f);

        private void Awake()
        {
            AutoDetectAndApplyLevelTheme();
        }

        private void Start()
        {
            AutoDetectAndApplyLevelTheme();
        }

        public void AutoDetectAndApplyLevelTheme()
        {
            LevelData selectedLevel = GameManager.Instance != null ? GameManager.Instance.selectedLevel : null;
            UpdateColorsForLevel(selectedLevel);
        }

        public void UpdateColorsForLevel(LevelData level)
        {
            if (level != null && level.groundColorPrimary.a > 0.05f)
            {
                color1 = level.groundColorPrimary;
                color2 = level.groundColorSecondary;
            }
            else if (level != null)
            {
                string lname = level.levelName != null ? level.levelName.ToLower() : "";

                if (lname.Contains("ярмарк") || lname.Contains("fair") || lname.Contains("площад"))
                {
                    // 🌾 ТЕМА СЕНА / СОЛОМЫ (Ярмарка)
                    color1 = new Color(0.88f, 0.78f, 0.50f);
                    color2 = new Color(0.68f, 0.58f, 0.32f);
                }
                else if (lname.Contains("фудкорт") || lname.Contains("молл") || lname.Contains("court") || lname.Contains("mall"))
                {
                    // 🏬 ТЕМА СЕРОГО КАФЕЛЯ (Фудкорт в молле)
                    color1 = new Color(0.75f, 0.78f, 0.82f);
                    color2 = new Color(0.42f, 0.45f, 0.50f);
                }
                else
                {
                    // 🌳 ТЕМА ЗЕЛЕНОГО ГАЗОНА (Ресторанный Бульвар / Дефолт)
                    color1 = new Color(0.25f, 0.75f, 0.25f);
                    color2 = new Color(0.10f, 0.45f, 0.10f);
                }
            }

            ApplyCheckerboardMaterial();
        }

        [ContextMenu("Generate Checkerboard Material")]
        public void ApplyCheckerboardMaterial()
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer == null) return;

            Texture2D texture = new Texture2D(gridSize, gridSize, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Repeat;

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    bool isColor1 = ((x / checkSize) + (y / checkSize)) % 2 == 0;
                    texture.SetPixel(x, y, isColor1 ? color1 : color2);
                }
            }
            texture.Apply(false, false);

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Standard");

            Material mat = new Material(shader);
            mat.name = "M_Checkerboard_LevelTheme";

            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
            mat.mainTexture = texture;

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);

            mat.mainTextureScale = tiling;
            renderer.sharedMaterial = mat;
        }
    }
}