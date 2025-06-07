/*
 * VoxelGenerationSystem.cs
 * Created by Arian - GameDevBox
 * YouTube Channel: https://www.youtube.com/@GameDevBox
 * 
 * -------------------------------
 * 📌 Description:
 * ECS System that generates voxels based on a heightmap blob asset.
 * 
 * - Queries entities tagged with VoxelTag and configured with VoxelGenerationConfig and HeightmapBlobReference.
 * - Reads grayscale heightmap data from a blob asset to determine voxel heights.
 * - Collects voxel positions into a native array.
 * - Schedules a parallel job (VoxelSpawnJob) to instantiate voxel entities at calculated positions.
 * - Marks entities with VoxelGenerationDone component to prevent regeneration.
 * 
 * Uses Burst compilation and Jobs for high-performance parallel voxel spawning.
 * Employs EntityCommandBuffer to safely instantiate entities within the job.
 * 
 * -------------------------------
 * ✅ Usage:
 * 1. Add VoxelGeneratorAuthoring component to a GameObject to create config and blob assets.
 * 2. The system automatically runs on entities with VoxelTag that have not completed generation.
 * 3. Voxels are spawned as entities with positions scaled by voxelScale.
 * 4. After spawning, the system disables itself until new voxels are required.
 */

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct VoxelGenerationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<VoxelTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        var commandBufferSystem = new EntityCommandBuffer(Allocator.TempJob);

        foreach (var (config, heightmapRef, entity) in
            SystemAPI.Query<VoxelGenerationConfig, HeightmapBlobReference>()
                     .WithAll<VoxelTag>()
                     .WithNone<VoxelGenerationDone>()
                     .WithEntityAccess())
        {
            if (!heightmapRef.Value.IsCreated)
            {
                UnityEngine.Debug.LogWarning("Heightmap blob asset is not created.");
                continue;
            }

            ref var heights = ref heightmapRef.Value.Value.heights;
            int2 size = config.heightmapSize;

            var voxelPositions = new NativeList<float3>(Allocator.TempJob);

            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.y; z++)
                {
                    int flippedZ = size.y - 1 - z;
                    float pixel = heights[flippedZ * size.x + x];
                    int height = (int)math.round(pixel * config.heightMultiplier);

                    for (int y = 0; y < height; y++)
                    {
                        float3 pos = new float3(x, y, z) * config.voxelScale;
                        voxelPositions.Add(pos);
                    }
                }
            }

            // Schedule parallel spawn job
            var spawnJob = new VoxelSpawnJob
            {
                voxelPrefab = config.voxelPrefab,
                voxelPositions = voxelPositions.AsDeferredJobArray(),
                ecb = commandBufferSystem.AsParallelWriter()
            };

            state.Dependency = spawnJob.Schedule(voxelPositions.Length, 64, state.Dependency);
            state.Dependency.Complete();

            commandBufferSystem.AddComponent<VoxelGenerationDone>(entity);

            voxelPositions.Dispose();
        }

        commandBufferSystem.Playback(state.EntityManager);
        commandBufferSystem.Dispose();
    }

    [BurstCompile]
    private struct VoxelSpawnJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> voxelPositions;
        [ReadOnly] public Entity voxelPrefab;
        public EntityCommandBuffer.ParallelWriter ecb;

        public void Execute(int index)
        {
            var entity = ecb.Instantiate(index, voxelPrefab);
            ecb.SetComponent(index, entity, LocalTransform.FromPosition(voxelPositions[index]));
        }
    }
}
