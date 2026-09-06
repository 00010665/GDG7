using UnityEngine;

[ExecuteAlways]
public class GridGroundSetup : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridSize = 256;
    public int checkSize = 32;
    public Color color1 = new Color(0.8f, 0.8f, 0.8f);
    public Color color2 = new Color(0.5f, 0.5f, 0.5f);
    public Vector2 tiling = new Vector2(20f, 20f);

    private void Awake()
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

        // Создаем процедурную текстуру шахматной доски
        Texture2D texture = new Texture2D(gridSize, gridSize);
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
        texture.Apply();

        // Создаем и настраиваем материал
        Shader defaultShader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Sprites/Default");
        Material mat = new Material(defaultShader);
        mat.name = "M_Checkerboard_Grid";
        mat.mainTexture = texture;
        mat.mainTextureScale = tiling;

        renderer.sharedMaterial = mat;
    }
}
