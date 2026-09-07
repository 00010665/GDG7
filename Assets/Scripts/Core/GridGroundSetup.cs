using UnityEngine;

namespace FoodSurvivors.Core
{
    [ExecuteAlways]
    public class GridGroundSetup : MonoBehaviour
    {
        [Header("Grid Settings")]
        public int gridSize = 256;
        public int checkSize = 32;
        public Color color1 = new Color(0.25f, 0.75f, 0.25f); // Ярко-зеленый
        public Color color2 = new Color(0.10f, 0.45f, 0.10f); // Темно-зеленый
        public Vector2 tiling = new Vector2(25f, 25f);

        private void Awake()
        {
            ApplyCheckerboardMaterial();
        }

        private void Start()
        {
            ApplyCheckerboardMaterial();
        }

        private void OnValidate()
        {
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

            // Для Unity 6 URP сначала ищем Unlit шейдеры!
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Standard");

            Material mat = new Material(shader);
            mat.name = "M_Checkerboard_Unlit";

            // Назначаем текстуру в св-ва URP Unlit
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
            mat.mainTexture = texture;

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);

            mat.mainTextureScale = tiling;
            renderer.sharedMaterial = mat;

            Debug.Log($"[GridGroundSetup] ✅ Создан и применен шахматный материал для Unity 6 (Шейдер: {shader.name})");
        }
    }
}