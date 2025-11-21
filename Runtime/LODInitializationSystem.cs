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
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var lodGroup in SystemAPI.Query<RefRO<LODGroupComponent>>())
            {
                
                if (lodGroup.ValueRO.LOD1Entity != Entity.Null)
                    ecb.AddComponent<Disabled>(lodGroup.ValueRO.LOD1Entity);
                
                if (lodGroup.ValueRO.LOD2Entity != Entity.Null)
                    ecb.AddComponent<Disabled>(lodGroup.ValueRO.LOD2Entity);
                
                if (lodGroup.ValueRO.LOD3Entity != Entity.Null)
                    ecb.AddComponent<Disabled>(lodGroup.ValueRO.LOD3Entity);
                
                if (lodGroup.ValueRO.LOD4Entity != Entity.Null)
                    ecb.AddComponent<Disabled>(lodGroup.ValueRO.LOD4Entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            state.Enabled = false;
        }
    }
}