/*
 * GPUVoxelTerrain.cs
 * Created by Arian - GameDevBox
 * YouTube Channel: https://www.youtube.com/@GameDevBox
 *
 * 🎮 Want more Unity tips, tools, and advanced systems?
 * 🧠 Learn from practical examples and well-explained logic.
 * 📦 Subscribe to GameDevBox for more game dev content!
 *
 * -------------------------------
 * 📌 Description:
 * A GPU-accelerated voxel terrain generator using GPU Instancer Pro.
 * Converts a heightmap texture into a dense voxel field, optimized for GPU instancing.
 * Includes editor tools to generate, clear, and export voxel meshes.
 *
 * ✅ Usage:
 * 1. Assign a grayscale heightmap texture and a voxel prefab with a MeshFilter.
 * 2. Click "Generate Voxel Terrain" to create the terrain using instancing.
 * 3. Optionally, export the voxel meshes for baking or further editing.
 * 4. Requires GPU Instancer Pro and appropriate scripting symbol definition.
 *
 * ⚠️ Note:
 * This script only functions properly if 'GPU_INSTANCER_PRO' is defined in scripting symbols.
 * Export feature works only inside the Unity Editor.
 */

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;
using GPUInstancerPro;
using Unity.Burst;

public class GPUVoxelTerrain : MonoBehaviour
{
    [Header("Data & Prefabs")]
    public Texture2D heightmap;
    public GameObject voxelPrefab;
    public GPUIProfile gpuProfile;

    [Header("Voxel Settings")]
    [Range(16, 1024)]
    public int resolution = 128;

    [Range(1f, 1000f)]
    public float heightMultiplier = 10f;

    public Vector3 voxelScale = Vector3.one;

    [Tooltip("How many bottom layers to skip (underground cutoff)")]
    [Range(0, 200)]
    public int undergroundCutoff = 0;

    private int renderKey;
    private Matrix4x4[] voxelMatrices;
    private bool isInitialized;

    [Header("Export Settings")]
    [Tooltip("Nuumber of tringles in each Chunk")]
    public int chunkSize = 10000;

    public void GenerateVoxels()
    {
        if (!heightmap || !voxelPrefab || !gpuProfile)
        {
            Debug.LogWarning("Missing heightmap, prefab, or profile.");
            return;
        }

        RegisterVoxels();
    }

    public void ClearVoxels()
    {
        DisposeRenderer();
    }

    public void ExportVoxelMesh()
    {
#if UNITY_EDITOR
        if (voxelMatrices == null || voxelMatrices.Length == 0)
        {
            Debug.LogWarning("No voxels to export. Generate terrain first.");
            return;
        }

        Mesh sourceMesh = voxelPrefab.GetComponent<MeshFilter>()?.sharedMesh;
        if (!sourceMesh)
        {
            Debug.LogError("voxelPrefab is missing a MeshFilter with mesh.");
            return;
        }

        int totalChunks = Mathf.CeilToInt(voxelMatrices.Length / (float)chunkSize);
        string folder = "Assets/GeneratedMeshes";

        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "GeneratedMeshes");

        for (int chunk = 0; chunk < totalChunks; chunk++)
        {
            int start = chunk * chunkSize;
            int end = Mathf.Min(start + chunkSize, voxelMatrices.Length);
            int count = end - start;

            List<CombineInstance> combine = new List<CombineInstance>(count);

            for (int i = start; i < end; i++)
            {
                combine.Add(new CombineInstance
                {
                    mesh = sourceMesh,
                    transform = voxelMatrices[i]
                });
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            combinedMesh.CombineMeshes(combine.ToArray(), true, true);

            string path = $"{folder}/VoxelTerrain_{resolution}x{resolution}_Chunk{chunk}_{System.DateTime.Now:yyyyMMdd_HHmmss}.asset";
            AssetDatabase.CreateAsset(combinedMesh, path);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"✅ Exported {totalChunks} voxel mesh chunks to {folder}");
#else
    Debug.LogWarning("Export only works in the Unity Editor.");
#endif
    }

    private void OnEnable()
    {
        DisposeRenderer();
    }

    private void OnDisable()
    {
        DisposeRenderer();
    }

    public void RegisterVoxels()
    {
        DisposeRenderer();
        GenerateVoxelMatrices();

        if (GPUICoreAPI.RegisterRenderer(this, voxelPrefab, gpuProfile, out renderKey))
        {
            GPUICoreAPI.SetTransformBufferData(renderKey, voxelMatrices);
            isInitialized = true;
        }
        else
        {
            Debug.LogError("Failed to register GPUI renderer.");
        }
    }

    public void DisposeRenderer()
    {
        if (!isInitialized)
            return;

        GPUICoreAPI.DisposeRenderer(renderKey);
        isInitialized = false;
    }

    [BurstCompile]
    private void GenerateVoxelMatrices()
    {
        List<Matrix4x4> matrices = new List<Matrix4x4>();

        for (int x = 0; x < resolution; x++)
        {
            for (int z = 0; z < resolution; z++)
            {
                float u = x / (float)(resolution - 1);
                float v = z / (float)(resolution - 1);
                float heightValue = heightmap.GetPixelBilinear(u, v).grayscale;
                int yMax = Mathf.RoundToInt(heightValue * heightMultiplier);

                int yStart = Mathf.Min(undergroundCutoff, yMax);

                for (int y = yStart; y < yMax; y++)
                {
                    Vector3 position = new Vector3(
                        x * voxelScale.x,
                        y * voxelScale.y,
                        z * voxelScale.z
                    );

                    Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, voxelScale);
                    matrices.Add(matrix);
                }
            }
        }

        voxelMatrices = matrices.ToArray();
        Debug.Log($"[GPUVoxelTerrain] Generated {voxelMatrices.Length} voxels with cutoff={undergroundCutoff} at resolution {resolution}.");
    }

}