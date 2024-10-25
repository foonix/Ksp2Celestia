using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Celestia.Editor
{
    public class FixExtractedDxt5nm
    {
        [MenuItem("Assets/Fix Extracted DTX5nm")]
        static void FixDxt5nm()
        {
            var srcFilePath = AssetDatabase.GetAssetPath(Selection.activeObject);
            Debug.Log($"{AssetDatabase.GetAssetPath(Selection.activeObject)}");
            var bytes = File.ReadAllBytes(srcFilePath);
            Texture2D raw = new(2048, 2048, TextureFormat.ARGB32, false, true, true);
            var result = ImageConversion.LoadImage(raw, bytes);
            Debug.Log($"loaded: {result}");

            var normalsAsColors = raw.GetPixels();

            for (int i = 0; i < normalsAsColors.Length; i++)
            {
                // unpack and calculate z per UnityCG.cginc UnpackNormalmapRGorAG()
                var packednormal = (Vector4)normalsAsColors[i];
                packednormal.x *= packednormal.w;

                Vector3 normal = new(packednormal.x, packednormal.y);
                normal *= 2f;
                normal -= Vector3.one;
                var normalXY = new Vector2(normal.x, normal.y);
                normal.z = Mathf.Sqrt(1f - Mathf.Clamp01(Vector2.Dot(normalXY, normalXY)));

                // Now that we have all 3 vector components, repack as RGB.
                normal += Vector3.one;
                normal /= 2f;
                normalsAsColors[i] = new Color(normal.x, normal.y, normal.z, 0);
            }

            raw.SetPixels(normalsAsColors);
            var outBytes = raw.EncodeToPNG();

            var fileName = Path.GetFileNameWithoutExtension(srcFilePath) + "_fixed.png";
            var directory = Path.GetDirectoryName(srcFilePath);
            var destFilePath = Path.Combine(directory, fileName);

            File.WriteAllBytes(destFilePath, outBytes);
            AssetDatabase.ImportAsset(destFilePath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"Wrote {destFilePath}");
            var importer = AssetImporter.GetAtPath(destFilePath) as TextureImporter;
            importer.textureType = TextureImporterType.NormalMap;
            importer.SaveAndReimport();

        }

        [MenuItem("Assets/Fix Extracted DTX5nm", true)]
        static bool FixDxt5nmValidate()
        {
            return Selection.activeObject.GetType() == typeof(Texture2D);
        }
    }
}