/*
 * GPUVoxelTerrainEditor.cs
 * Created by Arian - GameDevBox
 * YouTube Channel: https://www.youtube.com/@GameDevBox
 *
 * 📌 Description:
 * Custom inspector for GPUVoxelTerrain.
 * Adds editor buttons for generating, clearing, and exporting voxel terrain.
 */

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GPUVoxelTerrain))]
public class GPUVoxelTerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GPUVoxelTerrain terrain = (GPUVoxelTerrain)target;

        GUILayout.Space(10);
        GUILayout.Label("Voxel Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Voxel Terrain"))
        {
            terrain.GenerateVoxels();
        }

        if (GUILayout.Button("Clear Voxels"))
        {
            terrain.ClearVoxels();
        }

        if (GUILayout.Button("Export Voxel Mesh"))
        {
            terrain.ExportVoxelMesh();
        }
    }
}
