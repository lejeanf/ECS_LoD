using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace CustomLOD
{
    /// <summary>
    /// Alternative system using DynamicBuffer for more flexible LOD management
    /// Better for objects with varying numbers of LOD levels
    /// NOTE: Make sure CameraPositionUpdateSystem is included for this to work
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(CameraPositionUpdateSystem))]
    public partial struct LODUpdateSystemBuffered : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LODGroupComponent>();
            state.RequireForUpdate<MainCameraPosition>();
            // Disable this system by default - enable only if you want to use buffer-based approach
            state.Enabled = false;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Get camera position from singleton
            var cameraPosition = SystemAPI.GetSingleton<MainCameraPosition>().Position;

            var ecbSingleton = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            // Process each LOD group
            foreach (var (lodGroup, lodBuffer, localToWorld, entity) 
                in SystemAPI.Query<RefRW<LODGroupComponent>, DynamicBuffer<LODLevelInfo>, RefRO<LocalToWorld>>()
                    .WithEntityAccess())
            {
                if (lodBuffer.Length == 0)
                    continue;

                float distance = math.distance(cameraPosition, localToWorld.ValueRO.Position);
                
                // Find appropriate LOD level
                int newLODLevel = -1;
                for (int i = 0; i < lodBuffer.Length; i++)
                {
                    if (distance <= lodBuffer[i].MaxDistance)
                    {
                        newLODLevel = i;
                        break;
                    }
                }

                // If no LOD level found, use the last one or cull
                if (newLODLevel == -1 && lodBuffer.Length > 0)
                {
                    // Check if we should cull or use last LOD
                    float lastLODDistance = lodBuffer[lodBuffer.Length - 1].MaxDistance;
                    if (distance <= lastLODDistance * 1.5f) // Grace distance
                    {
                        newLODLevel = lodBuffer.Length - 1;
                    }
                }

                // Update if changed
                if (newLODLevel != lodGroup.ValueRO.CurrentLOD)
                {
                    // Disable old LOD
                    if (lodGroup.ValueRO.CurrentLOD >= 0 && lodGroup.ValueRO.CurrentLOD < lodBuffer.Length)
                    {
                        var oldEntity = lodBuffer[lodGroup.ValueRO.CurrentLOD].LODEntity;
                        if (oldEntity != Entity.Null)
                        {
                            ecb.AddComponent<Disabled>(oldEntity);
                        }
                    }

                    // Enable new LOD
                    if (newLODLevel >= 0 && newLODLevel < lodBuffer.Length)
                    {
                        var newEntity = lodBuffer[newLODLevel].LODEntity;
                        if (newEntity != Entity.Null)
                        {
                            ecb.RemoveComponent<Disabled>(newEntity);
                        }
                    }

                    lodGroup.ValueRW.CurrentLOD = newLODLevel;
                }
            }
        }
    }

    /// <summary>
    /// System to handle LOD fade mode (cross-fade between LODs)
    /// More advanced feature - implement if needed
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(LODUpdateSystem))]
    public partial struct LODFadeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // Disabled by default - enable if you want cross-fade support
            state.Enabled = false;
        }

        public void OnUpdate(ref SystemState state)
        {
            // TODO: Implement cross-fade logic
            // Would need to:
            // 1. Enable both current and next LOD
            // 2. Set shader properties for fade values
            // 3. Gradually transition over fade duration
        }
    }
}