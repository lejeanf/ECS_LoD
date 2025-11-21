<objective>
Fix the InvalidOperationException that occurs when moving a prefab with LODGroup and LODAuthoring components in a Unity SubScene. The error "Entity doesn't belong to the current authoring component" indicates that the LODGroupBaker is trying to reference entities from child GameObjects that are not properly associated with the current baking context.

This fix is critical for making the ECS LOD system stable and production-ready, as users need to be able to move and manipulate prefabs with LOD components in SubScenes without encountering baking errors.
</objective>

<context>
This is a Unity ECS package that adds Level of Detail (LOD) compatibility to the Entity Component System. The package uses Unity's baking system to convert LODGroup GameObjects into ECS entities with LOD components.

Key files:
@Runtime/LODGroupBaker.cs - Contains the baker that processes LODGroup components
@Runtime/LODAuthoring.cs - Contains debug authoring component
@Runtime/LODComponents.cs - Contains the ECS component definitions

The error occurs specifically when:
1. A prefab with LODGroup and LODAuthoring components is placed in a SubScene
2. The user attempts to move the prefab within the scene
3. During the rebake, the LODGroupBaker tries to access child renderer entities using GetEntity()
4. Unity throws: "Entity doesn't belong to the current authoring component"

This happens because GetEntity() is being called on child GameObjects (lod.renderers[0].gameObject) that may not be within the scope of the current baker's authoring component.
</context>

<root_cause_analysis>
Thoroughly analyze the LODGroupBaker.cs file to identify why the entity references become invalid:

1. The baker uses `GetEntity(lodGameObject, TransformUsageFlags.Dynamic)` on line 63 to get entities for child renderers
2. When a prefab is moved in a SubScene, Unity may rebake the hierarchy
3. The child renderer GameObjects might not be within the baking scope of the LODGroup baker
4. GetEntity() expects the GameObject to be a direct dependency or properly declared

The core issue: **The baker is trying to reference child GameObject entities without properly declaring them as dependencies in the baking context.**
</root_cause_analysis>

<requirements>
Fix the LODGroupBaker to handle entity references correctly during baking, specifically:

1. **Declare dependencies properly**: Use DependsOn() or GetEntity() with the correct flags to ensure child GameObjects are within baking scope
2. **Handle hierarchy correctly**: Ensure the baker can safely reference child renderer GameObjects
3. **Maintain existing functionality**: The LOD system should continue to work as before
4. **Prevent rebaking errors**: Moving prefabs in SubScenes should not trigger the error
5. **Follow Unity ECS best practices**: Use proper baker patterns for hierarchical GameObject conversion

Consider multiple approaches:
- Using `DependsOn()` to explicitly declare dependencies on child GameObjects
- Using `GetEntity()` with appropriate TransformUsageFlags
- Restructuring how child entities are discovered and referenced
- Using additional baking passes or dependencies if needed
</requirements>

<implementation>
Review the Unity ECS Baking documentation patterns for handling child GameObjects:

1. **Option A: Use DependsOn()**: Explicitly declare dependencies on child renderers before calling GetEntity()
   ```csharp
   DependsOn(lodGameObject.transform);
   var lodEntity = GetEntity(lodGameObject, TransformUsageFlags.Dynamic);
   ```

2. **Option B: Use GetComponent<>() patterns**: Get child entities through Unity's baker component access
   ```csharp
   // Access children through proper baker APIs
   ```

3. **Option C: Restructure baking approach**: Consider if the LOD children should be baked separately or if the parent should handle them differently

Choose the approach that:
- Follows Unity ECS 1.0+ best practices
- Is most robust during prefab manipulation
- Maintains clean entity relationships
- Has minimal performance impact

**WHY proper dependency declaration matters**: Unity's baking system needs to know which GameObjects are related so it can rebake them correctly when the hierarchy changes. Without explicit dependencies, child entities may not be in scope when the parent is rebaked, causing the "doesn't belong to current authoring component" error.
</implementation>

<verification>
After implementing the fix, verify:

1. **Build the project**: Ensure no compilation errors
2. **Test prefab movement**:
   - Create a test prefab with LODGroup and LODAuthoring in a SubScene
   - Move the prefab around in the scene
   - Verify no InvalidOperationException occurs
3. **Test LOD functionality**: Ensure LOD switching still works correctly
4. **Check entity relationships**: Verify parent/child entity references are correct
5. **Test edge cases**:
   - Moving multiple LOD prefabs simultaneously
   - Duplicating LOD prefabs
   - Nested LOD hierarchies (if supported)

Log your verification results and any warnings that appear during baking.
</verification>

<success_criteria>
- No InvalidOperationException when moving LOD prefabs in SubScenes
- LOD system continues to function correctly (entities created, LOD switching works)
- Code follows Unity ECS baking best practices
- All entity references are valid and properly scoped
- No console warnings during baking
</success_criteria>

<output>
Modify the following file:
- `./Runtime/LODGroupBaker.cs` - Fix the entity reference issue with proper dependency declarations

Include inline comments explaining WHY the fix works and what Unity ECS pattern is being followed.
</output>
