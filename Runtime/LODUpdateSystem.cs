using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace CustomLOD
{
    /// <summary>
    /// System that updates LOD levels based on distance from camera
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct LODUpdateSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LODGroupComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Get camera position (main camera)
            var cameraPosition = GetCameraPosition();

            // Job to update LOD levels
            var updateJob = new UpdateLODJob
            {
                CameraPosition = cameraPosition,
                EntityCommandBuffer = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            };

            state.Dependency = updateJob.ScheduleParallel(state.Dependency);
        }

        private float3 GetCameraPosition()
        {
            // Try to get main camera position
            var camera = Camera.main;
            if (camera != null)
            {
                return camera.transform.position;
            }
            return float3.zero;
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
            // Calculate distance from camera to object
            float3 objectPosition = localToWorld.Position;
            float distance = math.distance(CameraPosition, objectPosition);

            // Determine which LOD level should be active
            int newLODLevel = DetermineLODLevel(distance, transitions, lodGroup.LODCount);

            // If LOD level changed, update active entities
            if (newLODLevel != lodGroup.CurrentLOD)
            {
                // Disable current LOD (if any)
                if (lodGroup.CurrentLOD >= 0)
                {
                    Entity currentEntity = GetLODEntity(lodGroup, lodGroup.CurrentLOD);
                    if (currentEntity != Entity.Null)
                    {
                        EntityCommandBuffer.AddComponent<Disabled>(chunkIndex, currentEntity);
                    }
                }

                // Enable new LOD (if not culled)
                if (newLODLevel >= 0)
                {
                    Entity newEntity = GetLODEntity(lodGroup, newLODLevel);
                    if (newEntity != Entity.Null)
                    {
                        EntityCommandBuffer.RemoveComponent<Disabled>(chunkIndex, newEntity);
                    }
                }

                // Update current LOD
                lodGroup.CurrentLOD = newLODLevel;
            }
        }

        [BurstCompile]
        private int DetermineLODLevel(float distance, in LODTransitions transitions, int lodCount)
        {
            // Check if beyond cull distance
            if (distance > transitions.CullDistance)
                return -1; // Culled

            // Check each LOD level
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

            // Beyond all LOD levels but not culled yet
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
