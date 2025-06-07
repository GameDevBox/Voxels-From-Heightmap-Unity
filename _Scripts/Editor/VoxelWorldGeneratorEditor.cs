/*
 * VoxelWorldGeneratorEditor.cs
 * Created by Arian - GameDevBox
 * YouTube Channel: https://www.youtube.com/@GameDevBox
 *
 * 🎮 Want more Unity tips, tools, and advanced systems?
 * 🧠 Learn from practical examples and well-explained logic.
 * 📦 Subscribe to GameDevBox for more game dev content!
 *
 * -------------------------------
 * 📌 Description:
 * Custom inspector for VoxelWorldGenerator.
 * Adds buttons for generating, optimizing, and clearing voxel worlds.
 *
 * ✅ Usage:
 * Automatically enhances the editor interface for easier testing.
 * No runtime effect — only affects the Unity Editor GUI.
 */

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoxelWorldGenerator))]
public class VoxelWorldGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        VoxelWorldGenerator generator = (VoxelWorldGenerator)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Voxel World"))
        {
            generator.GenerateVoxelWorld();
        }

        if (GUILayout.Button("Generate Optimized Voxel World"))
        {
            generator.PartialRandomMerge();
        }

        if (GUILayout.Button("Clear World"))
        {
            generator.ClearWorld();
        }
    }
}
