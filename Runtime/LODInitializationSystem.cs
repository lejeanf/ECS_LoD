using Unity.Burst;
using Unity.Entities;

namespace CustomLOD
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct LODInitializationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LODGroupComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            UnityEngine.Debug.Log($"[LODInitializationSystem] Running! Found LODGroups to process.");
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var lodGroup in SystemAPI.Query<RefRO<LODGroupComponent>>())
            {
                UnityEngine.Debug.Log($"LOD0: {lodGroup.ValueRO.LOD0Entity}, LOD1: {lodGroup.ValueRO.LOD1Entity}, LOD2: {lodGroup.ValueRO.LOD2Entity}");

                if (lodGroup.ValueRO.LOD1Entity != Entity.Null)
                {
                    ecb.AddComponent<Disabled>(lodGroup.ValueRO.LOD1Entity);
                    UnityEngine.Debug.Log($"Added Disabled to LOD1 entity: {lodGroup.ValueRO.LOD1Entity}");
                }

                if (lodGroup.ValueRO.LOD2Entity != Entity.Null)
                {
                    ecb.AddComponent<Disabled>(lodGroup.ValueRO.LOD2Entity);
                    UnityEngine.Debug.Log($"Added Disabled to LOD2 entity: {lodGroup.ValueRO.LOD2Entity}");
                }

                if (lodGroup.ValueRO.LOD3Entity != Entity.Null)
                {
                    ecb.AddComponent<Disabled>(lodGroup.ValueRO.LOD3Entity);
                    UnityEngine.Debug.Log($"Added Disabled to LOD3 entity: {lodGroup.ValueRO.LOD3Entity}");
                }

                if (lodGroup.ValueRO.LOD4Entity != Entity.Null)
                {
                    ecb.AddComponent<Disabled>(lodGroup.ValueRO.LOD4Entity);
                    UnityEngine.Debug.Log($"Added Disabled to LOD4 entity: {lodGroup.ValueRO.LOD4Entity}");
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            state.Enabled = false;
        }
    }
}