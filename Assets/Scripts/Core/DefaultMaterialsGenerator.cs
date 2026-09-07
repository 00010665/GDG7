using UnityEngine;

namespace FoodSurvivors.Core
{
    public static class DefaultMaterialsGenerator
    {
        private static Material _playerMat;
        private static Material _enemyNormalMat;
        private static Material _enemyFastMat;
        private static Material _bossMat;
        private static Material _foodMat;
        private static Material _expGemMat;
        private static Material _arenaCheckerMat;

        public static Material GetPlayerMaterial()
        {
            if (_playerMat == null) _playerMat = CreateSolidColorMaterial("M_Player", Color.cyan);
            return _playerMat;
        }

        public static Material GetEnemyNormalMaterial()
        {
            if (_enemyNormalMat == null) _enemyNormalMat = CreateSolidColorMaterial("M_Enemy_Normal", Color.red);
            return _enemyNormalMat;
        }

        public static Material GetEnemyFastMaterial()
        {
            if (_enemyFastMat == null) _enemyFastMat = CreateSolidColorMaterial("M_Enemy_Fast", new Color(1f, 0.5f, 0f));
            return _enemyFastMat;
        }

        public static Material GetBossMaterial()
        {
            if (_bossMat == null) _bossMat = CreateSolidColorMaterial("M_Boss", new Color(0.6f, 0f, 0.8f));
            return _bossMat;
        }

        public static Material GetFoodMaterial()
        {
            if (_foodMat == null) _foodMat = CreateSolidColorMaterial("M_Food", Color.yellow);
            return _foodMat;
        }

        public static Material GetExpGemMaterial()
        {
            if (_expGemMat == null) _expGemMat = CreateSolidColorMaterial("M_ExpGem", Color.green);
            return _expGemMat;
        }

        public static Material GetArenaCheckerboardMaterial()
        {
            if (_arenaCheckerMat == null)
            {
                Color green1 = new Color(0.25f, 0.75f, 0.25f);
                Color green2 = new Color(0.10f, 0.45f, 0.10f);
                _arenaCheckerMat = CreateCheckerboardMaterial("M_Arena_Checker", green1, green2, 256, 32, 25f);
            }
            return _arenaCheckerMat;
        }

        public static Material CreateSolidColorMaterial(string matName, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            Material mat = new Material(shader);
            mat.name = matName;

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            return mat;
        }

        public static Material CreateCheckerboardMaterial(string matName, Color color1, Color color2, int gridSize, int checkSize, float tiling)
        {
            Texture2D tex = new Texture2D(gridSize, gridSize, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    bool isColor1 = ((x / checkSize) + (y / checkSize)) % 2 == 0;
                    tex.SetPixel(x, y, isColor1 ? color1 : color2);
                }
            }
            tex.Apply(false, false);

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            Material mat = new Material(shader);
            mat.name = matName;

            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            mat.mainTexture = tex;

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);

            mat.mainTextureScale = new Vector2(tiling, tiling);
            return mat;
        }
    }
}