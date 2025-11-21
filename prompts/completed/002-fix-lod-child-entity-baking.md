<objective>
Fix the architectural issue where LODGroupBaker tries to add components to child entities, which violates Unity ECS baking rules. The error "Entity doesn't belong to the current authoring component" on line 97 occurs because the baker is attempting to call AddComponent() on entities that belong to other GameObjects.

This requires restructuring the baking approach to follow Unity ECS principles: each GameObject can only modify its own entity, not entities from other GameObjects.
</objective>

<context>
In Unity ECS baking, each GameObject gets its own baker instance. When you call `GetEntity()` on a different GameObject, you get a reference to that entity, but you don't "own" it - you cannot add/remove components on it from your baker.

The current LODGroupBaker attempts to:
1. Get entities from child LOD renderer GameObjects using `GetEntity(lodGameObject, TransformUsageFlags.Dynamic)` (line 69)
2. Add components to those child entities: `LODChildTag` (line 97) and `Disabled` (line 105)

This violates Unity's baking rules and causes the InvalidOperationException.

Key files:
@Runtime/LODGroupBaker.cs - The baker that needs restructuring
@Runtime/LODComponents.cs - Component definitions (may need to check LODChildTag usage)
@Runtime/LODAuthoring.cs - Existing authoring component

The LODChildTag component is used to:
- Store which LOD level the child belongs to
- Reference back to the parent LODGroup entity
</context>

<root_cause>
**The fundamental issue**: You cannot add components to entities from other GameObjects in a baker. The LODGroupBaker is trying to modify child renderer entities, which is not allowed.

From the Unity ECS documentation:
> "A baker can only add components to the primary entity (from GetEntity()) or to additional entities it creates itself (from CreateAdditionalEntity()). You cannot add components to entities from other GameObjects."
</root_cause>

<requirements>
Restructure the baking approach to properly handle LOD child entities while following Unity ECS baking rules:

1. **Stop adding components to child entities from parent baker**: Remove AddComponent() calls for lodEntity
2. **Maintain LOD functionality**: The system must still know which entities are LOD children and their levels
3. **Preserve entity references**: The parent still needs to reference child entities (LOD0Entity, LOD1Entity, etc.)
4. **Enable/disable mechanism**: LOD children still need to be disabled/enabled based on LOD level

Consider these architectural approaches:

**Option A: Create separate child authoring component**
- Create a new `LODChildAuthoring` MonoBehaviour that gets added to each LOD renderer GameObject
- This authoring component has its own baker that adds the LODChildTag
- The parent LODGroup can set up these authoring components at edit time or use a different mechanism

**Option B: Use LinkedEntityGroup pattern**
- Store child entity references in a buffer on the parent
- Use systems to manage child state at runtime instead of baking time
- Don't add LODChildTag to children; instead, systems query the parent's buffer

**Option C: Create additional entities instead of using child GameObjects**
- Use CreateAdditionalEntity() to create LOD level entities that the baker owns
- Copy renderer data to these additional entities
- This avoids trying to modify entities from other GameObjects

**Option D: Use PostProcessBaker or IBaker.CreateAdditionalEntity**
- Research if there's a later baking pass that can modify child entities after they're fully baked

Deeply consider which approach best fits the LOD system's requirements and Unity ECS patterns.
</requirements>

<implementation>
Thoroughly analyze the current system to determine the best approach:

1. **Read the LODComponents.cs file** to understand:
   - What LODChildTag is used for at runtime
   - Whether it's queried by systems
   - If it can be replaced with a different pattern

2. **Determine the simplest solution** that:
   - Follows Unity ECS baking best practices
   - Minimizes changes to existing code
   - Maintains all LOD functionality
   - Is robust during prefab manipulation

3. **Implement the chosen approach**:
   - If creating a child authoring component: Create `LODChildAuthoring.cs` with its own baker
   - If using buffer pattern: Modify how children are tracked and accessed
   - If creating additional entities: Restructure to use CreateAdditionalEntity()
   - Remove all AddComponent() calls that target child entities from LODGroupBaker

4. **Handle the Disabled component**:
   - LOD levels other than LOD0 need to start disabled
   - This must be done without violating baking rules
   - Consider: Can this be done in a system at runtime instead?

5. **Update documentation**:
   - Add comments explaining the architectural pattern chosen
   - Document why child entities are handled this way
   - Explain the Unity ECS baking constraints

**WHY this architectural change is necessary**: Unity's baking system enforces strict ownership rules to ensure deterministic, reproducible baking. When multiple bakers try to modify the same entity, the results become unpredictable. By respecting these boundaries, we ensure stable, reliable baking behavior.
</implementation>

<verification>
After implementing the fix:

1. **Build verification**: No compilation errors
2. **Baking verification**:
   - Move LOD prefabs in SubScenes - no InvalidOperationException
   - Check Unity console for any baking warnings
3. **Functional verification**:
   - LOD switching still works at runtime
   - Correct LOD levels are visible/hidden based on distance
   - Parent-child relationships are maintained
4. **Entity verification**:
   - Inspect entities in Entity Debugger
   - Verify component data is correct
   - Check that entity references are valid
5. **Edge case testing**:
   - Duplicate prefabs
   - Nested hierarchies
   - Multiple LODGroup components in same scene

Document any changes in behavior or system requirements.
</verification>

<success_criteria>
- No InvalidOperationException when moving LOD prefabs
- All LOD functionality works correctly
- Code follows Unity ECS baking ownership rules
- No components added to entities from other GameObjects
- Clear architecture that's maintainable and understandable
- Systems can properly query and manage LOD entities
</success_criteria>

<output>
Modify and/or create files as needed:
- `./Runtime/LODGroupBaker.cs` - Remove AddComponent() calls for child entities, restructure as needed
- `./Runtime/LODChildAuthoring.cs` - NEW FILE (if using child authoring approach)
- Document the chosen architectural pattern and reasoning

Include detailed comments explaining the Unity ECS baking constraints and why this approach was chosen.
</output>
