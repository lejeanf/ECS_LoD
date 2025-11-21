using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace CustomLOD
{
    /// <summary>
    /// Singleton component to store camera position for LOD calculations
    /// </summary>
    public struct MainCameraPosition : IComponentData
    {
        public float3 Position;
    }

    /// <summary>
    /// System to update the main camera position (runs before LOD system)
    /// This system is NOT Burst-compiled because it needs to access Camera.main
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(LODUpdateSystem))]
    public partial class CameraPositionUpdateSystem : SystemBase
    {
        protected override void OnCreate()
        {
            // Create singleton entity for camera position
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(entity, new MainCameraPosition { Position = float3.zero });
        }

        protected override void OnUpdate()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                UnityEngine.Debug.LogWarning("[CameraPositionUpdateSystem] Camera.main is NULL!");
                return;
            }

            SystemAPI.SetSingleton(new MainCameraPosition { Position = camera.transform.position });
        }
    }

    /// <summary>
    /// Main LOD update system - now fully Burst-compatible
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(CameraPositionUpdateSystem))]
    public partial struct LODUpdateSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginPresentationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<LODGroupComponent>();
            state.RequireForUpdate<MainCameraPosition>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Get camera position from singleton
            var cameraPosition = SystemAPI.GetSingleton<MainCameraPosition>().Position;

            var updateJob = new UpdateLODJob
            {
                CameraPosition = cameraPosition,
                EntityCommandBuffer = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            };

            state.Dependency = updateJob.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct UpdateLODJob : IJobEntity
    {
        public float3 CameraPosition;
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

        [BurstCompile]
        public void Execute(
            Entity entity,
            [ChunkIndexInQuery] int chunkIndex,
            ref LODGroupComponent lodGroup,
            in LocalToWorld localToWorld,
            in LODTransitions transitions)
        {
            float3 objectPosition = localToWorld.Position;
            float distance = math.distance(CameraPosition, objectPosition);

            int newLODLevel = DetermineLODLevel(distance, transitions, lodGroup.LODCount);

            if (newLODLevel != lodGroup.CurrentLOD)
            {
                if (lodGroup.CurrentLOD >= 0)
                {
                    Entity currentEntity = GetLODEntity(lodGroup, lodGroup.CurrentLOD);
                    if (currentEntity != Entity.Null)
                    {
                        EntityCommandBuffer.AddComponent<Disabled>(chunkIndex, currentEntity);
                    }
                }

                if (newLODLevel >= 0)
                {
                    Entity newEntity = GetLODEntity(lodGroup, newLODLevel);
                    if (newEntity != Entity.Null)
                    {
                        EntityCommandBuffer.RemoveComponent<Disabled>(chunkIndex, newEntity);
                    }
                }

                lodGroup.CurrentLOD = newLODLevel;
            }
        }

        [BurstCompile]
        private int DetermineLODLevel(float distance, in LODTransitions transitions, int lodCount)
        {
            if (distance > transitions.CullDistance)
                return -1; 

            if (lodCount > 0 && distance <= transitions.LOD0MaxDistance)
                return 0;
            if (lodCount > 1 && distance <= transitions.LOD1MaxDistance)
                return 1;
            if (lodCount > 2 && distance <= transitions.LOD2MaxDistance)
                return 2;
            if (lodCount > 3 && distance <= transitions.LOD3MaxDistance)
                return 3;
            if (lodCount > 4 && distance <= transitions.LOD4MaxDistance)
                return 4;

            return lodCount - 1;
        }

        [BurstCompile]
        private Entity GetLODEntity(in LODGroupComponent lodGroup, int lodLevel)
        {
            return lodLevel switch
            {
                0 => lodGroup.LOD0Entity,
                1 => lodGroup.LOD1Entity,
                2 => lodGroup.LOD2Entity,
                3 => lodGroup.LOD3Entity,
                4 => lodGroup.LOD4Entity,
                _ => Entity.Null
            };
        }
    }
}