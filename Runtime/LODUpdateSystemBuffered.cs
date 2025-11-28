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
            state.Enabled = false;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var cameraPosition = SystemAPI.GetSingleton<MainCameraPosition>().Position;

            var ecbSingleton = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (lodGroup, lodBuffer, localToWorld, entity) 
                in SystemAPI.Query<RefRW<LODGroupComponent>, DynamicBuffer<LODLevelInfo>, RefRO<LocalToWorld>>()
                    .WithEntityAccess())
            {
                if (lodBuffer.Length == 0)
                    continue;

                var distance = math.distance(cameraPosition, localToWorld.ValueRO.Position);
                
                var newLODLevel = -1;
                for (var i = 0; i < lodBuffer.Length; i++)
                {
                    if (!(distance <= lodBuffer[i].MaxDistance)) continue;
                    newLODLevel = i;
                    break;
                }

                if (newLODLevel == -1 && lodBuffer.Length > 0)
                {
                    var lastLODDistance = lodBuffer[lodBuffer.Length - 1].MaxDistance;
                    if (distance <= lastLODDistance * 1.5f) // Grace distance
                    {
                        newLODLevel = lodBuffer.Length - 1;
                    }
                }

                if (newLODLevel == lodGroup.ValueRO.CurrentLOD) continue;
                if (lodGroup.ValueRO.CurrentLOD >= 0 && lodGroup.ValueRO.CurrentLOD < lodBuffer.Length)
                {
                    var oldEntity = lodBuffer[lodGroup.ValueRO.CurrentLOD].LODEntity;
                    if (oldEntity != Entity.Null)
                    {
                        ecb.AddComponent<Disabled>(oldEntity);
                    }
                }

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

    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(LODUpdateSystem))]
    public partial struct LODFadeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
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