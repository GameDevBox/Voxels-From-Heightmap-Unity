/*
 * VoxelGeneratorAuthoring.cs
 * Created by Arian - GameDevBox
 * YouTube Channel: https://www.youtube.com/@GameDevBox
 *
 * 🎮 Want more Unity tips, tools, and advanced systems?
 * 🧠 Learn from practical examples and well-explained logic.
 * 📦 Subscribe to GameDevBox for more game dev content!
 *
 * -------------------------------
 * 📌 Description:
 * Authoring component for generating voxel worlds using Unity DOTS (ECS).
 * Converts a heightmap texture into a BlobAsset for efficient data access.
 * Sets up ECS components with voxel prefab reference, scaling, and height configuration.
 * Includes a Baker class that runs at conversion time to prepare ECS data.
 *
 * ✅ Usage:
 * Attach to a GameObject, assign heightmap and prefab.
 * Enable "generateOnStart" to automatically create voxel data on scene load.
 * Works with DOTS systems to drive voxel instantiation and rendering.
 */

using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using System;

public class VoxelGeneratorAuthoring : MonoBehaviour
{
    public Texture2D heightmap;
    public GameObject voxelPrefab;
    public float voxelScale = 1f;
    public int heightMultiplier = 20;
    public bool generateOnStart = true;

    class Baker : Baker<VoxelGeneratorAuthoring>
    {
        public override void Bake(VoxelGeneratorAuthoring authoring)
        {
            if (!authoring.generateOnStart) return;

            var entity = GetEntity(TransformUsageFlags.None);

            // Convert heightmap to blob asset
            var blobBuilder = new BlobBuilder(Allocator.Temp);
            ref var heightmapBlob = ref blobBuilder.ConstructRoot<HeightmapBlob>();
            var heightArray = blobBuilder.Allocate(ref heightmapBlob.heights,
                authoring.heightmap.width * authoring.heightmap.height);

            // Copy heightmap data
            Color[] pixels = authoring.heightmap.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                heightArray[i] = pixels[i].grayscale;
            }

            var blobAsset = blobBuilder.CreateBlobAssetReference<HeightmapBlob>(Allocator.Persistent);
            blobBuilder.Dispose();

            // Add components
            AddComponent(entity, new VoxelGenerationConfig
            {
                voxelPrefab = GetEntity(authoring.voxelPrefab, TransformUsageFlags.None),
                voxelScale = authoring.voxelScale,
                heightMultiplier = authoring.heightMultiplier,
                heightmapSize = new int2(authoring.heightmap.width, authoring.heightmap.height)
            });

            AddComponent(entity, new HeightmapBlobReference { Value = blobAsset });
            AddComponent<VoxelTag>(entity);
        }
    }
}

public struct VoxelGenerationConfig : IComponentData
{
    public Entity voxelPrefab;
    public float voxelScale;
    public int heightMultiplier;
    public int2 heightmapSize;
}

public struct VoxelTag : IComponentData { }
public struct VoxelGenerationDone : IComponentData { }


// Blob asset for heightmap storage
public struct HeightmapBlob
{
    public BlobArray<float> heights;
}

public struct HeightmapBlobReference : IComponentData
{
    public BlobAssetReference<HeightmapBlob> Value;
}