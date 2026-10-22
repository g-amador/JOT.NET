You are helping me continue JOT.NET, a modern C#/.NET reimplementation of the original Java-based JOT (Just One Thing) Modular Multi-purpose Game Engine.

The project is experimental and not production-ready.

Always inspect the actual repository before proposing changes.

Verify existing implementations, project references, and dependencies before recommending new code.

The repository is the source of truth.

If the repository and this prompt disagree:
1. Report the inconsistency.
2. Explain the impact.
3. Follow the repository.

Do not introduce new systems, layers, managers, services, patterns, or abstractions unless an immediate requirement exists in the current phase.

Prefer concrete implementations over speculative abstractions.

Do not implement items from future phases merely because they may eventually be needed.

Current phase requirements always take priority over future extensibility.

Prefer the smallest implementation that validates the architecture.

Validation is more important than feature completeness.

A minimal working implementation is preferred over a generalized framework.

When multiple valid next steps exist:

1. Prefer validation of existing architecture.
2. Prefer completing existing systems.
3. Prefer fixing inconsistencies.
4. Only then add new functionality.

# PROJECT GITHUB REPO
https://github.com/g-amador/JOT.NET/tree/V1.0

# PROJECT ORGANIZATION
Directory structure:
```text
JOT.NET/
├── AI.md                           # Prompts for expanding the game
├── ARCHITECTURE.md                 # Detailed project architecture & directory structure
├── LICENSE                         # License details
├── README.md                       # Main project documentation
├── assets/
│   └── sounds/                     # Game-ready PCM WAV library (categorized)
│       ├── Collisions/
│       ├── Effects/
│       ├── Music/
│       ├── Powerups/
│       ├── Ships/
│       ├── UI/
│       └── Weapons/
└── src/
    ├── Directory.Packages.props
    ├── JOT.NET.slnx
    ├── client-browser/
    │   └── JOT.Client.Browser/
    │       ├── _Imports.razor
    │       ├── App.razor
    │       ├── JOT.Client.Browser.csproj
    │       ├── Program.cs
    │       ├── Layout/
    │       │   ├── MainLayout.razor
    │       │   ├── MainLayout.razor.css
    │       │   ├── NavMenu.razor
    │       │   └── NavMenu.razor.css
    │       ├── Pages/
    │       │   ├── Counter.razor
    │       │   ├── Home.razor
    │       │   ├── NotFound.razor
    │       │   └── Weather.razor
    │       ├── Properties/
    │       │   └── launchSettings.json
    │       └── wwwroot/
    │           ├── index.html
    │           ├── css/
    │           │   └── app.css
    │           └── sample-data/
    │               └── weather.json
    ├── client-desktop/
    │   └── JOT.Client.Desktop/
    │       ├── JOT.Client.Desktop.csproj
    │       └── Program.cs
    ├── demos/
    │   ├── examples/
    │   │   └── AudioDemo/
    │   │       ├── AudioDemo.csproj
    │   │       └── Program.cs
    │   ├── smoke/
    │   │   └── CoreRenderingSmoke/
    │   │       ├── CoreRenderingSmoke.csproj
    │   │       └── Program.cs
    │   └── templates/
    │       ├── CoreTemplate/
    │       │   ├── CoreTemplate.csproj
    │       │   └── Program.cs
    │       └── FullTemplate/
    │           ├── FullTemplate.csproj
    │           └── Program.cs
    ├── engine/
    │   ├── core/
    │   │   ├── JOT.Core.Audio/
    │   │   │   ├── JOT.Core.Audio.csproj
    │   │   │   ├── Listener.cs
    │   │   │   ├── SoundEmitter.cs
    │   │   │   └── WaveFile.cs
    │   │   ├── JOT.Core.IO/
    │   │   │   └── JOT.Core.IO.csproj
    │   │   ├── JOT.Core.Math/
    │   │   │   ├── JOT.Core.Math.csproj
    │   │   │   ├── Geometry/
    │   │   │   │   ├── AxisAlignedBoundingBox.cs
    │   │   │   │   ├── BoundingSphere.cs
    │   │   │   │   ├── Frustum.cs
    │   │   │   │   ├── OrientedBoundingBox.cs
    │   │   │   │   ├── Plane.cs
    │   │   │   │   ├── Ray.cs
    │   │   │   │   └── Intersections/
    │   │   │   │       ├── BoxIntersections.cs
    │   │   │   │       ├── FrustumIntersections.cs
    │   │   │   │       ├── RayIntersections.cs
    │   │   │   │       └── SphereIntersections.cs
    │   │   │   └── Transformations/
    │   │   │       └── Transform.cs
    │   │   ├── JOT.Core.Physics/
    │   │   │   └── JOT.Core.Physics.csproj
    │   │   ├── JOT.Core.Rendering/
    │   │   │   ├── JOT.Core.Rendering.csproj
    │   │   │   ├── Cameras/
    │   │   │   │   ├── Camera.cs
    │   │   │   │   ├── ICamera.cs
    │   │   │   │   ├── OrthographicCamera.cs
    │   │   │   │   └── PerspectiveCamera.cs
    │   │   │   ├── Materials/
    │   │   │   │   └── IMaterial.cs
    │   │   │   ├── Meshes/
    │   │   │   │   ├── IMesh.cs
    │   │   │   │   └── Mesh.cs
    │   │   │   ├── Renderers/
    │   │   │   │   ├── IRenderer.cs
    │   │   │   │   └── SimpleRenderer.cs
    │   │   │   └── Textures/
    │   │   │       └── ITexture.cs
    │   │   └── JOT.Core.Util/
    │   │       └── JOT.Core.Util.csproj
    │   ├── framework/
    │   │   ├── JOT.Framework.Application/
    │   │   │   └── JOT.Framework.Application.csproj
    │   │   ├── JOT.Framework.GUI/
    │   │   │   └── JOT.Framework.GUI.csproj
    │   │   └── JOT.Framework.Managers/
    │   │       └── JOT.Framework.Managers.csproj
    │   ├── infrastructure/
    │   │   ├── JOT.Infrastructure.Assimp/
    │   │   │   └── JOT.Infrastructure.Assimp.csproj
    │   │   ├── JOT.Infrastructure.DotNetty/
    │   │   │   └── JOT.Infrastructure.DotNetty.csproj
    │   │   └── JOT.Infrastructure.Silk/
    │   │       ├── JOT.Infrastructure.Silk.csproj
    │   │       ├── Audio/
    │   │       │   ├── IAudioDevice.cs
    │   │       │   ├── IAudioSource.cs
    │   │       │   ├── SilkAudioDevice.cs
    │   │       │   └── SilkAudioSource.cs
    │   │       ├── Graphics/
    │   │       │   ├── IGraphicsContext.cs
    │   │       │   ├── IShaderProgram.cs
    │   │       │   ├── IVertexBuffer.cs
    │   │       │   ├── SilkGraphicsContext.cs
    │   │       │   ├── SilkShaderProgram.cs
    │   │       │   └── SilkVertexBuffer.cs
    │   │       ├── Input/
    │   │       │   ├── IInputContext.cs
    │   │       │   ├── IKeyboard.cs
    │   │       │   ├── KeyCode.cs
    │   │       │   ├── SilkInputContext.cs
    │   │       │   └── SilkKeyboard.cs
    │   │       ├── Timing/
    │   │       │   ├── ITimer.cs
    │   │       │   └── SilkTimer.cs
    │   │       └── Windowing/
    │   │           ├── IWindow.cs
    │   │           └── SilkWindow.cs
    │   └── toolkits/
    │       ├── JOT.Toolkit.AI/
    │       │   └── JOT.Toolkit.AI.csproj
    │       ├── JOT.Toolkit.ECS/
    │       │   └── JOT.Toolkit.ECS.csproj
    │       ├── JOT.Toolkit.Geometry/
    │       │   └── JOT.Toolkit.Geometry.csproj
    │       ├── JOT.Toolkit.Networking/
    │       │   └── JOT.Toolkit.Networking.csproj
    │       ├── JOT.Toolkit.Physics/
    │       │   └── JOT.Toolkit.Physics.csproj
    │       └── JOT.Toolkit.Serialization/
    │           └── JOT.Toolkit.Serialization.csproj
    └── server/
        └── JOT.Server/
            ├── JOT.Server.csproj
            └── Program.cs
```

# Goals
- Modular architecture with replaceable subsystems
- Clear layer boundaries
- Strict layer dependency rules
- MMO-oriented long-term design
- Educational value and maintainability
- Desktop-first, browser later
- Incremental validation of the foundation

When evaluating the next step, first ask:
"Does this directly help validate the current phase?"

If the answer is no, do not implement it.

# PRIMARY OBJECTIVE

Build a clean, modular, educational, and maintainable game engine foundation that:

- Preserves clear architectural boundaries.
- Supports replaceable subsystems.
- Validates each layer incrementally.
- Remains suitable for future MMO-oriented development.
- Supports desktop clients first.
- Supports browser clients later.
- Prioritizes correctness and architecture over feature breadth.

# ARCHITECTURE

Layer order:

Infrastructure
    ↑
Core
    ↑
Toolkits
    ↑
Framework

Dependency rules:

- A module may extend any module from a lower layer, not only the adjacent one:
  - Framework may extend Toolkits, Core, and Infrastructure.
  - Toolkits may extend Core and Infrastructure.
  - Core may extend Infrastructure and Core modules.
- Infrastructure is the lowest layer and must not depend on higher layers.
- Dependencies flow toward lower layers.

## Rules

- A layer may depend only on itself or lower layers.
- No circular dependencies.
- No cross-layer shortcuts.
- Lower layers must not know about higher layers.
- Third-party libraries belong in Infrastructure.
- Platform-specific code belongs in Infrastructure.
- Keep Core independent of third-party libraries and platform APIs;
  Core reaches them only through Infrastructure contracts.
- Infrastructure never implements Core contracts.
- Prefer existing abstractions before creating new ones.

## Contracts vs Implementations

- Infrastructure owns the contracts that wrap third-party libraries
  (windowing, graphics, input, timing, asset loading, networking).
- Infrastructure implements those contracts. They are library glue, not engine concepts.
- Core uses Infrastructure and builds engine concepts on top of it.
- Core may implement Core contracts. Infrastructure never implements Core contracts.
- Nothing from Core is implemented in Infrastructure.
- Infrastructure contains no engine logic — only library adapters.
- Core never references third-party libraries or platform APIs directly;
  it goes through Infrastructure contracts.
- Toolkits may depend on Core contracts and Infrastructure.
- Framework may depend on Toolkits, Core, and Infrastructure.
- Lower layers must never depend on higher layers.

# LAYER RESPONSABILITIES

## Infrastructure

Adapters and integrations:
- Windowing
- Input
- Graphics contexts
- Timing
- Audio devices
- Asset importing
- Networking integrations
- External libraries

## Core

Minimal engine concepts:
- Math
- Rendering abstractions
- Audio (positional sound)
- Input abstractions
- Resource abstractions
- Basic collision concepts
- Utilities

## Toolkits

Optional advanced systems:
- ECS
- AI
- Networking
- Serialization
- Geometry
- Physics

## Framework

Optional higher-level features:
- Application lifecycle
- Scenes
- Assets
- State management
- GUI

# ROADMAP

Roadmap progress:
✅ Phase 1 - Infrastructure
✅ Phase 2 - Core
⬜ Core Template validation (Next)
⬜ Phase 3 - Toolkits
⬜ Phase 4 - Framework
⬜ Phase 5 - MMO Prototype
⬜ Phase 6 - Browser Client

# CURRENT STATUS

## Phase 1 — JOT.Infrastructure.Silk

Status: COMPLETED

Phase 1 completion criteria have been satisfied:

1. Application opens a window.
2. Graphics initialize successfully.
3. Background color is displayed.
4. Keyboard input is processed.
5. Escape closes the application.
6. Application exits cleanly.

Build and runtime validation have been performed.

## Phase 2 — Core

Status: COMPLETED

Phase 2 completion criteria have been satisfied:

1. JOT.Core.Math: engine concepts on System.Numerics (Transform, Ray, Plane, AxisAlignedBoundingBox, OrientedBoundingBox, BoundingSphere, Frustum), including frustum extraction and box/sphere/ray intersection helpers. Build validated.
2. JOT.Core.Rendering: first Core-side implementation (Mesh, SimpleRenderer) on the Infrastructure glue. Runtime validated: 120 frame triangle render, clean exit (CoreRenderingSmoke).
3. JOT.Core.Audio: positional audio (Listener, SoundEmitter, WaveFile) on the Infrastructure audio glue. Runtime validated: device, listener, WAV assets, and engine-synthesized melodies play positionally, clean exit (AudioDemo; criteria checklist below).

Build and runtime validation have been performed.

Full Phase 2 validation checklist:

Validation basis: Math items are build-validated; Rendering and Audio items are runtime-validated by the disposable smoke harnesses. Unticked boxes await your confirmation.

Math (JOT.Core.Math):
- [ ] Transform composes world matrices from position, rotation, and scale.
- [ ] Ray and Plane model engine geometry without replacing System.Numerics.
- [ ] AxisAlignedBoundingBox and OrientedBoundingBox change basis via Transform().
- [ ] BoundingSphere transforms volumes and is built from point clouds (CreateFromPoints).
- [ ] Frustum extracts planes from perspective and orthographic projections (row-vector conventions, [0,1] depth).
- [ ] Frustum.Contains(Vector3) performs point-inclusion culling tests.
- [ ] FrustumIntersections tests spheres and axis-aligned boxes against the frustum.

Rendering (CoreRenderingSmoke):
- [ ] IGraphicsContext glue gains CreateShaderProgram, CreateVertexBuffer, and DrawTriangles.
- [ ] SilkShaderProgram compiles and links vertex and fragment shader sources.
- [ ] SilkVertexBuffer uploads vertex data and issues triangle draws.
- [ ] Core.Rendering.Meshes.Mesh implements IMesh without referencing any third-party namespace.
- [ ] SimpleRenderer draws meshes through a camera's view and projection matrices.
- [ ] A triangle renders for 120 frames and the application exits cleanly (exit 0).

Audio (AudioDemo):
- [ ] Native OpenAL binaries are provisioned for the harness (OpenAL.Soft).
- [ ] Audio device opens successfully (OpenAL device and context lifecycle).
- [ ] Listener position and orientation are applied to the device.
- [ ] Menu accepts number-key choices (1-5 and Q).
- [ ] Procedural sine wave is synthesized and plays.
- [ ] WAV file (assets/sounds/Weapons/pdp1_fire.wav) loads and plays.
- [ ] Super Mario, Tetris, and Zelda: Song of Storms play from transcribed note tables.
- [ ] All audio is rendered by the engine (no Console.Beep) and plays positionally to the right of the listener.
- [ ] Any key stops playback early.
- [ ] Q quits the menu, the device disposes, and the application exits cleanly.

Solution and docs:
- [ ] JOT.Core.Audio and both smoke harnesses are wired under /Demos in JOT.NET.slnx.
- [ ] AI.md, README.md, and ARCHITECTURE.md trees and statuses match the implemented state.

### JOT.Core.Math

JOT.Core.Math exists to provide engine concepts, not replacements for System.Numerics.

Use System.Numerics directly for:
1. Vector2
2. Vector3
3. Vector4
4. Quaternion
5. Matrix4x4

Only add types that represent genuine engine concepts.

Do not create replacement vector or matrix types.

Implemented engine concepts:
- Transform
- Ray
- Plane
- BoundingBox (Object aligned and axis aligned)
- BoundingSphere
- Frustum

Only add types that provide clear engine value.

### JOT.Core.Rendering

Create API-independent abstractions such as:
1. IRenderer
2. ICamera
3. IMesh
4. ITexture
5. IMaterial

Do not add new Core abstractions unless they are required by a current implementation or validation objective.

Do not expose OpenGL, DirectX, Vulkan, or Silk.NET details in Core.

Rendering implementations belong in Infrastructure.

Implemented so far: Mesh (first IMesh implementation) and SimpleRenderer, runtime-validated by CoreRenderingSmoke.

Math and Rendering should remain independent of platform, graphics API, and third-party rendering libraries.

### JOT.Core.Audio

Builds positional audio on the Infrastructure audio glue (OpenAL via Silk.NET).

Audio validation is covered by the Audio block of the full Phase 2
validation checklist above (device, listener, menu, sine, WAV,
engine-synthesized melodies, positional playback, early stop, clean exit).

# CORE TEMPLATE VALIDATION

The purpose of CoreTemplate is to prove the architecture end-to-end.

The scene mirrors the original Java JOT template: a textured ground
plane with textured models standing in wireframe debug containers
(AABB, OBB, or Sphere envelopes), a selectable sky container filling
the horizon, and directional light casting shadows. Additional
validation features may be added as long as they depend at most on
Core.

Success criteria:
1. Open a window.
2. Close with Escape.
3. 3D sound.
4. Multiple camera views.
5. Light effects (e.g., directional shadows on the ground).
6. Model loading cow, brick cube, fighter, cone from assets/models.
7. Sky box OR sky dome: selectable off, box, or dome, textured from one sky texture in assets/textures.
8. Toggle on/off showing textures.
9. Toggle on/off showing triangles (wireframe).
10. Toggle on/off showing AABB, OBB, or Sphere debug containers.

Nothing more.

Asset placeholder names (models land in assets/models, textures in
assets/textures; real files arrive later and the template resolves
them by path, failing loudly with a clear message when missing):

| File                          | Placeholder   | Used for                     |
|-------------------------------|---------------|------------------------------|
| assets/models/cow.obj         | cow           | grazed model with a bell     |
| assets/models/brick_cube.obj  | brick cube    | textured cubes               |
| assets/models/fighter.obj     | fighter       | spacecraft model             |
| assets/models/cone.obj        | mountain cone | cone next to a test emitter  |
| assets/textures/cow.png       | cow material  | cow model material           |
| assets/textures/brick.png     | brick         | cube faces                   |
| assets/textures/grass.png     | grass         | textured ground plane        |
| assets/textures/clouds.png    | clouds sky    | the active sky container     |

The ground plane is built procedurally (no model file required).
Wireframe debug containers and the ground grid reuse Geometry from
JOT.Core.Math.

# EXPLICIT NON-GOALS

Do not implement:
- AI
- ECS
- Networking
- Physics
- Framework features
- Browser features
- MMO systems

until the current milestone is validated.

# WORKING STYLE

Before making changes:
1. Audit the repository.
2. Inspect project references and dependencies.
3. Identify implemented, incomplete, and inconsistent areas.
4. Report architecture or build issues.

Then:
1. Propose the smallest useful next step.
2. Explain why it belongs in the current phase.
3. Implement incrementally.
4. Avoid broad refactors unless requested.

When an architectural violation is found:
1. Describe it.
2. Explain why it violates the architecture.
3. Propose the smallest correction.

# BUILD & VALIDATION

Before claiming completion:
- Build affected projects.
- Report build warnings.
- Report build errors.
- State exactly which projects were built.
- State exactly what was executed.
- Do not claim runtime validation unless runtime validation was actually performed.
- State what could not be executed and why.

Do not claim runtime validation if it was not performed.

# CODING GUIDELINES

- Follow existing repository conventions.
- Prefer modern idiomatic C#.
- Favor simplicity over extensibility.
- Avoid premature abstraction.
- Keep dependencies minimal.
- Preserve architectural boundaries.
- Do not invent repository details.
- Ask questions only when a decision genuinely blocks progress.
- Prefer concrete types until a second implementation is required.

This version is about half the size, but retains nearly all of the architectural guidance that will actually influence an AI agent's behavior.