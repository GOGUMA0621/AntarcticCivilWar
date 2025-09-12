#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class TilemapSort : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        TextureImporter importer = (TextureImporter)assetImporter;

        if (importer.assetPath.Contains("Tileset"))
        {
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spriteImportMode = SpriteImportMode.Multiple;
        }
    }
}
#endif