using UnityEngine;

namespace FoodSurvivors.Core
{
    /// <summary>
    /// Генератор и кэш ярких материалов для арены, игрока, врагов, блюд, гемов.
    /// Используется, чтобы все объекты были легко различимы на зелёном шахматном полу.
    /// </summary>
    public static class DefaultMaterialsGenerator
    {
        private static Material _playerMat;
        private static Material _enemyNormalMat;
        private static Material _enemyFastMat;
        private static Material _bossMat;
        private static Material _foodMat;
        private static Material _expGemMat;

        private static Material _arenaCheckerMat;

        private const int TEX_SIZE = 256;
        private const int CHECK_SIZE = 32;
        private const float TILING = 20f;

        // Получить материал игрока (ярко-голубой)
        public static Material GetPlayerMaterial()
        {
            if (_playerMat == null)
            {
                _playerMat = CreateSolidColorMaterial("M_Player", Color.cyan);
            }
            return _playerMat;
        }

        // Получить материал обычного врага (красный)
        public static Material GetEnemyNormalMaterial()
        {
            if (_enemyNormalMat == null)
            {
                _enemyNormalMat = CreateSolidColorMaterial("M_Enemy_Normal", Color.red);
            }
            return _enemyNormalMat;
        }

        // Получить материал быстрого врага (оранжевый)
        public static Material GetEnemyFastMaterial()
        {
            if (_enemyFastMat == null)
            {
                _enemyFastMat = CreateSolidColorMaterial("M_Enemy_Fast", new Color(1f, 0.5f, 0f));
            }
            return _enemyFastMat;
        }

        // Получить материал босса (фиолетовый)
        public static Material GetBossMaterial()
        {
            if (_bossMat == null)
            {
                _bossMat = CreateSolidColorMaterial("M_Boss", new Color(0.6f, 0f, 0.8f));
            }
            return _bossMat;
        }

        // Получить материал еды / снарядов (ярко-жёлтый)
        public static Material GetFoodMaterial()
        {
            if (_foodMat == null)
            {
                _foodMat = CreateSolidColorMaterial("M_Food", Color.yellow);
            }
            return _foodMat;
        }

        // Получить материал гема опыта (зелёный/золотистый)
        public static Material GetExpGemMaterial()
        {
            if (_expGemMat == null)
            {
                _expGemMat = CreateSolidColorMaterial("M_ExpGem", Color.green);
            }
            return _expGemMat;
        }

        // Получить зелёный шахматный материал пола арены
        public static Material GetArenaCheckerboardMaterial()
        {
            if (_arenaCheckerMat == null)
            {
                Color green1 = new Color(0.20f, 0.65f, 0.20f); // ярко-зелёный
                Color green2 = new Color(0.10f, 0.40f, 0.10f); // тёмно-зелёный
                _arenaCheckerMat = CreateCheckerboardMaterial("M_Arena_Checker", green1, green2, TEX_SIZE, CHECK_SIZE, TILING);
            }
            return _arenaCheckerMat;
        }

        // Создать одноцветный материал через шейдер по приоритету (URP/Standard/Sprites)
        public static Material CreateSolidColorMaterial(string matName, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            Material mat = new Material(shader);
            mat.name = matName;

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }
            return mat;
        }

        // Создать шахматный материал с тайлингом
        public static Material CreateCheckerboardMaterial(string matName, Color color1, Color color2,
                                                          int gridSize, int checkSize, float tiling)
        {
            Texture2D tex = new Texture2D(gridSize, gridSize, TextureFormat.RGBA32, false);
            tex.name = "T_Checker";
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.anisoLevel = 0;

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    bool isColor1 = ((x / checkSize) + (y / checkSize)) % 2 == 0;
                    tex.SetPixel(x, y, isColor1 ? color1 : color2);
                }
            }
            tex.Apply(false, false);

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            Material mat = new Material(shader);
            mat.name = matName;
            mat.mainTexture = tex;

            if (mat.HasProperty("_BaseMap"))
            {
                mat.SetTexture("_BaseMap", tex);
            }
            if (mat.HasProperty("_MainTex"))
            {
                mat.SetTexture("_MainTex", tex);
            }

            mat.mainTextureScale = new Vector2(tiling, tiling);
            mat.mainTextureOffset = Vector2.zero;
            return mat;
        }

        // Назначить материалу визуальный объект по компоненту Renderer
        public static void ApplyToRenderer(Renderer renderer, Material material)
        {
            if (renderer == null || material == null) return;
            renderer.sharedMaterial = material;
        }
    }
}
