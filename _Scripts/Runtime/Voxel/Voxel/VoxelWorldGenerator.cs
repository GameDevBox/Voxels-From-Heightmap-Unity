/*
 * VoxelWorldGenerator.cs
 * Created by Arian - GameDevBox
 * YouTube Channel: https://www.youtube.com/@GameDevBox
 *
 * 🎮 Want more Unity tips, tools, and advanced systems?
 * 🧠 Learn from practical examples and well-explained logic.
 * 📦 Subscribe to GameDevBox for more game dev content!
 *
 * -------------------------------
 * 📌 Description:
 * Generates a voxel world from a grayscale heightmap image.
 * Instantiates cubes (or prefabs) based on pixel brightness.
 * Includes resolution control and height scaling.
 *
 * 🛠 Optimization Feature:
 * Inspector button allows merging voxels into larger chunks.
 * Each click increases merge size: 2x2 → 4x4 → 8x8, etc.
 * Helps reduce draw calls and boost performance.
 *
 * ✅ Usage:
 * 1. Assign a heightmap texture and voxel prefab.
 * 2. Click "Generate Voxel World" in the inspector.
 * 3. Use "Optimize Voxels" to merge and simplify.
 * 4. Use "Clear World" to reset the scene.
 */

using System.Collections.Generic;
using UnityEngine;

public class VoxelWorldGenerator : MonoBehaviour
{
    [Header("Heightmap Settings")]
    [Tooltip("Grayscale heightmap image used to generate voxel terrain.")]
    public Texture2D heightmap;

    [Tooltip("Maximum height multiplier for terrain based on grayscale values.")]
    public float heightScale = 10f;

    [Tooltip("Size of each individual voxel (cube).")]
    public float voxelSize = 1f;

    [Tooltip("Sampling resolution for the heightmap. Higher values reduce voxel density.")]
    public int resolution = 1;

    [Header("Voxel Settings")]
    [Tooltip("Optional prefab to instantiate for each voxel. If null, a cube will be used.")]
    public GameObject voxelPrefab;

    [Header("Optimization")]
    [Tooltip("Current voxel grouping size for optimization.")]
    public int countToMerge = 50;


    private List<GameObject> voxels = new List<GameObject>();
    private GameObject voxelParent;

    [ContextMenu("Generate Voxel World")]
    public void GenerateVoxelWorld()
    {
        ClearWorld();

        if (heightmap == null)
        {
            Debug.LogError("No heightmap assigned.");
            return;
        }

        voxelParent = new GameObject("VoxelWorld");

        int width = heightmap.width;
        int height = heightmap.height;

        for (int x = 0; x < width; x += resolution)
        {
            for (int z = 0; z < height; z += resolution)
            {
                float normalizedHeight = heightmap.GetPixel(x, z).grayscale;
                int yHeight = Mathf.RoundToInt(normalizedHeight * heightScale);

                for (int y = 0; y < yHeight; y++)
                {
                    Vector3 position = new Vector3(x, y, z) * voxelSize;
                    GameObject voxel = Instantiate(voxelPrefab != null ? voxelPrefab : GameObject.CreatePrimitive(PrimitiveType.Cube), position, Quaternion.identity, voxelParent.transform);
                    voxel.transform.localScale = Vector3.one * voxelSize;
                    voxels.Add(voxel);
                }
            }
        }

        Debug.Log($"[VoxelWorldGenerator] Generated {voxels.Count} voxels at resolution {resolution}.");
    }

    [ContextMenu("Partial Random Merge Voxels")]
    public void PartialRandomMerge()
    {
        if (voxels.Count == 0)
        {
            Debug.LogWarning("No voxels to merge.");
            return;
        }

        countToMerge = Mathf.Min(countToMerge, voxels.Count);

        // Pick random voxels to merge
        List<GameObject> toMerge = new List<GameObject>();
        HashSet<int> pickedIndices = new HashSet<int>();

        System.Random rnd = new System.Random();

        while (toMerge.Count < countToMerge)
        {
            int idx = rnd.Next(voxels.Count);
            if (!pickedIndices.Contains(idx))
            {
                pickedIndices.Add(idx);
                toMerge.Add(voxels[idx]);
            }
        }

        // Prepare combine instances
        List<CombineInstance> combines = new List<CombineInstance>();
        Material sharedMat = null;

        foreach (GameObject voxel in toMerge)
        {
            if (voxel == null) continue;

            MeshFilter mf = voxel.GetComponent<MeshFilter>();
            MeshRenderer mr = voxel.GetComponent<MeshRenderer>();

            if (mf == null || mr == null) continue;

            if (sharedMat == null)
                sharedMat = mr.sharedMaterial;

            CombineInstance ci = new CombineInstance
            {
                mesh = mf.sharedMesh,
                transform = voxel.transform.localToWorldMatrix
            };
            combines.Add(ci);
        }

        // Create merged voxel chunk
        GameObject mergedChunk = new GameObject("MergedChunk");
        mergedChunk.transform.parent = voxelParent.transform;

        MeshFilter mergedMF = mergedChunk.AddComponent<MeshFilter>();
        MeshRenderer mergedMR = mergedChunk.AddComponent<MeshRenderer>();

        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combines.ToArray(), true, true);
        mergedMF.sharedMesh = combinedMesh;
        mergedMR.sharedMaterial = sharedMat;

        // Remove merged voxels from list and destroy their gameobjects
        foreach (GameObject voxel in toMerge)
        {
            voxels.Remove(voxel);
            if (voxel != null)
                DestroyImmediate(voxel);
        }

        // Add merged chunk to voxels list
        voxels.Add(mergedChunk);

        Debug.Log($"[VoxelWorldGenerator] Merged {countToMerge} voxels randomly. Remaining voxels: {voxels.Count}");
    }


    public void ClearWorld()
    {
        if (voxelParent != null)
            DestroyImmediate(voxelParent);

        voxels.Clear();

        Debug.Log("[VoxelWorldGenerator] Voxel world cleared.");
    }
}
