using Unity.Entities;
using UnityEngine;

namespace CustomLOD
{
    public class LODGroupBaker : Unity.Entities.Baker<LODGroup>
    {
        public override void Bake(LODGroup authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            var lods = authoring.GetLODs();
            if (lods.Length == 0)
                return;

            var lodComponent = new LODGroupComponent
            {
                ObjectSize = authoring.size,
                LODCount = lods.Length,
                CurrentLOD = 0,
                LOD0Entity = Entity.Null,
                LOD1Entity = Entity.Null,
                LOD2Entity = Entity.Null,
                LOD3Entity = Entity.Null,
                LOD4Entity = Entity.Null
            };

            var transitions = new LODTransitions();

            var lodBuffer = AddBuffer<LODLevelInfo>(entity);

            for (int i = 0; i < lods.Length && i < 5; i++)
            {
                var lod = lods[i];
                
                float screenHeightPercent = lod.screenRelativeTransitionHeight;
                float maxDistance = CalculateMaxDistance(authoring.size, screenHeightPercent);

                switch (i)
                {
                    case 0:
                        transitions.LOD0MaxDistance = maxDistance;
                        break;
                    case 1:
                        transitions.LOD1MaxDistance = maxDistance;
                        break;
                    case 2:
                        transitions.LOD2MaxDistance = maxDistance;
                        break;
                    case 3:
                        transitions.LOD3MaxDistance = maxDistance;
                        break;
                    case 4:
                        transitions.LOD4MaxDistance = maxDistance;
                        break;
                }

                if (lod.renderers != null && lod.renderers.Length > 0)
                {
                    var lodGameObject = lod.renderers[0]?.gameObject;
                    if (lodGameObject != null)
                    {
                        DependsOn(lodGameObject.transform);

                        var lodEntity = GetEntity(lodGameObject, TransformUsageFlags.Dynamic);
                        
                        switch (i)
                        {
                            case 0:
                                lodComponent.LOD0Entity = lodEntity;
                                break;
                            case 1:
                                lodComponent.LOD1Entity = lodEntity;
                                break;
                            case 2:
                                lodComponent.LOD2Entity = lodEntity;
                                break;
                            case 3:
                                lodComponent.LOD3Entity = lodEntity;
                                break;
                            case 4:
                                lodComponent.LOD4Entity = lodEntity;
                                break;
                        }

                        lodBuffer.Add(new LODLevelInfo
                        {
                            LODEntity = lodEntity,
                            MaxDistance = maxDistance,
                            ScreenHeightPercent = screenHeightPercent
                        });
                    }
                }

                transitions.CullDistance = i == 0 ? transitions.LOD0MaxDistance :
                    i == 1 ? transitions.LOD1MaxDistance :
                    i == 2 ? transitions.LOD2MaxDistance :
                    i == 3 ? transitions.LOD3MaxDistance :
                    transitions.LOD4MaxDistance;
            }

            AddComponent(entity, lodComponent);
            AddComponent(entity, transitions);
        }

        private float CalculateMaxDistance(float objectSize, float screenHeightPercent)
        {
            if (screenHeightPercent <= 0.0001f)
                return 10000f; 

            float fov = 60f * Mathf.Deg2Rad;
            float tanHalfFOV = Mathf.Tan(fov / 2f);
            
            float distance = (objectSize / (2f * tanHalfFOV)) / screenHeightPercent;
            
            return distance;
        }
    }
}