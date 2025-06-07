/*
 * HeightmapGeneratorPanel.cs
 * Created by Arian - GameDevBox
 * YouTube Channel: https://www.youtube.com/@GameDevBox
 *
 * 🎮 Want more Unity tips, tools, and advanced systems?
 * 🧠 Learn from practical examples and well-explained logic.
 * 📦 Subscribe to GameDevBox for more game dev content!
 * 
 * Custom Unity Editor Window for batch generating heightmaps using
 * a HeightmapGenerator ScriptableObject.
 * 
 * Features:
 * - Select a HeightmapGenerator asset to use as the noise/heightmap source.
 * - Specify the folder path to save generated heightmap textures.
 * - Set the number of heightmaps to generate in a batch.
 * - Optionally enable or disable the Read/Write flag on the saved textures.
 * - Automatically names generated textures with incremental indices to avoid overwriting.
 * - Saves heightmaps as PNG files in the specified folder.
 * - Refreshes Unity's AssetDatabase to make the textures immediately available.
 * 
 * Usage:
 * Open the window via the menu: Tools -> Heightmap Generator
 * Assign a HeightmapGenerator asset.
 * Set parameters like save folder, bulk amount, and Read/Write toggle.
 * Click "Generate Heightmaps" to create textures.
 */

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class HeightmapGeneratorPanel : EditorWindow
{
    private HeightmapGenerator generator;
    private string saveFolder = "Assets/GeneratedHeightmaps";
    private int bulkAmount = 5;
    private bool readWriteEnabled = false;

    [MenuItem("Tools/Heightmap Generator")]
    private static void OpenWindow()
    {
        GetWindow<HeightmapGeneratorPanel>("Heightmap Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Heightmap Generator", EditorStyles.boldLabel);
        generator = (HeightmapGenerator)EditorGUILayout.ObjectField(
            "Generator Settings", generator,
            typeof(HeightmapGenerator), false
        );

        saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);
        bulkAmount = EditorGUILayout.IntSlider("How Many Maps", bulkAmount, 1, 50);

        readWriteEnabled = EditorGUILayout.Toggle("Enable Read/Write", readWriteEnabled);

        if (GUILayout.Button("Generate Heightmaps"))
            GenerateBatch();
    }

    private void GenerateBatch()
    {
        if (generator == null)
        {
            Debug.LogError("Assign a HeightmapGenerator ScriptableObject first.");
            return;
        }

        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        int startIndex = GetNextFileIndex();

        string[] savedPaths = new string[bulkAmount];

        for (int i = 0; i < bulkAmount; i++)
        {
            int fileIndex = startIndex + i;
            string name = $"Heightmap_{fileIndex}";

            Texture2D tex = generator.GenerateTexture(name);
            SaveTexture(tex, saveFolder);

            savedPaths[i] = Path.Combine(saveFolder, tex.name + ".png");
        }

        AssetDatabase.Refresh();

        for (int i = 0; i < savedPaths.Length; i++)
        {
            SetTextureReadWrite(savedPaths[i], readWriteEnabled);
        }

        Debug.Log($"{bulkAmount} heightmaps generated in {saveFolder} (starting at {startIndex})");
    }

    private int GetNextFileIndex()
    {
        if (!Directory.Exists(saveFolder))
            return 0;

        var existingFiles = Directory.GetFiles(saveFolder, "Heightmap_*.png");
        int maxIndex = -1;

        foreach (var file in existingFiles)
        {
            string filename = Path.GetFileNameWithoutExtension(file);
            if (filename.StartsWith("Heightmap_"))
            {
                string numberPart = filename.Substring("Heightmap_".Length);
                if (int.TryParse(numberPart, out int index))
                {
                    if (index > maxIndex)
                        maxIndex = index;
                }
            }
        }

        return maxIndex + 1;
    }

    private void SaveTexture(Texture2D texture, string folderPath)
    {
        string path = Path.Combine(folderPath, texture.name + ".png");
        File.WriteAllBytes(path, texture.EncodeToPNG());
    }

    private void SetTextureReadWrite(string assetPath, bool enable)
    {
        string unityPath = assetPath.Replace(Application.dataPath, "Assets").Replace("\\", "/");

        TextureImporter importer = AssetImporter.GetAtPath(unityPath) as TextureImporter;
        if (importer != null)
        {
            if (importer.isReadable != enable)
            {
                importer.isReadable = enable;
                importer.SaveAndReimport();
            }
        }
        else
        {
            Debug.LogWarning($"Failed to get TextureImporter for path: {unityPath}");
        }
    }

}
#endif
