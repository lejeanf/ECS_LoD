<objective>
Update the README.md to be concise, accurate, and relevant for the Unity ECS LOD package. The README should focus on essential information: what it does, how to install via Unity Package Manager, and basic usage. Remove outdated sections and verbose troubleshooting that clutters the documentation.

This matters because developers need quick, clear documentation to understand and use the package effectively without wading through excessive details.
</objective>

<context>
This is a Unity package for adding Level of Detail (LOD) support to Unity's Entity Component System (ECS). The package is designed for Unity Package Manager installation.

Key facts to verify:
- **Installation**: Unity Package Manager (add via git URL or local package)
- **Basic usage**: Just add a LODGroup component to any GameObject in a SubScene - the system handles the rest automatically
- **LODAuthoring**: This component is ONLY for debug visualization (optional) - confirm this is correct
- **Automatic baking**: The LODGroupBaker automatically converts LODGroup to ECS during baking

Current README issues:
- Too long and verbose (241 lines)
- Contains outdated installation instructions (manual file copying)
- Excessive troubleshooting and optimization sections
- References removed components (LODDebugAuthoring should be LODAuthoring)
- Includes implementation details that should be in code comments, not user docs

Target audience: Unity developers who want to add ECS LOD support to their projects

@README.md - Current verbose README
@package.json - Package metadata for reference
</context>

<requirements>
Create a concise, professional README with these sections:

1. **Brief description** (2-3 sentences): What it does, why it's useful
2. **Features** (bulleted list, 4-6 items max): Key capabilities
3. **Installation** (Unity Package Manager specific):
   - Install via git URL
   - Install via local package
4. **Quick Start** (simple, clear steps):
   - How to use the basic LOD system (just add LODGroup to GameObject in SubScene)
   - How to enable debug visualization (optional - add LODAuthoring component)
5. **Requirements**: Unity version, package dependencies
6. **License**: Keep existing if present

**Remove or drastically reduce:**
- Verbose "How It Works" sections (belongs in code docs)
- Extensive troubleshooting (move to wiki/issues if needed)
- Performance optimization code examples (too detailed for README)
- "Extending the System" section (advanced, not needed in README)
- Known limitations (unless critical)

**Tone**: Professional, concise, welcoming. Get developers up and running quickly.

**Length target**: 50-80 lines (vs current 241 lines)
</requirements>

<verification>
Before completing, verify:
1. **LODAuthoring is debug only**: Confirm this component is only for visualization, not required for basic LOD functionality
2. **Installation method**: Ensure instructions match Unity Package Manager workflow
3. **Accuracy**: All component names and setup steps are correct
4. **Conciseness**: No section exceeds 10 lines unless absolutely necessary
5. **Completeness**: A new user can install and use the package from the README alone
</verification>

<success_criteria>
- README is 50-80 lines (down from 241)
- Clear Unity Package Manager installation instructions
- Basic usage is obvious: add LODGroup to GameObject in SubScene
- Debug feature (LODAuthoring) clearly marked as optional
- Professional tone throughout
- No outdated or incorrect information
- Developers can get started in under 2 minutes of reading
</success_criteria>

<output>
Update the following file:
- `./README.md` - Rewrite to be concise and relevant

If the current README has both README.md and readme.md, consolidate into README.md (uppercase) only.
</output>
