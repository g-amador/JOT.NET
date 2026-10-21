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
    │   └── templates/
    │       ├── CoreTemplate/
    │       │   ├── CoreTemplate.csproj
    │       │   └── Program.cs
    │       └── FullTemplate/
    │           ├── FullTemplate.csproj
    │           └── Program.cs
    ├── engine/
    │   ├── core/
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
    │   │   │   │   └── IMesh.cs
    │   │   │   ├── Renderers/
    │   │   │   │   └── IRenderer.cs
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
    │   │       ├── Graphics/
    │   │       │   ├── IGraphicsContext.cs
    │   │       │   └── SilkGraphicsContext.cs
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

- Framework may depend on Toolkits, Core, and Infrastructure.
- Toolkits may depend on Core and Infrastructure.
- Core may depend on Infrastructure.
- Infrastructure must not depend on higher layers.
- Dependencies flow toward lower layers.

## Rules

- A layer may depend only on itself or lower layers.
- No circular dependencies.
- No cross-layer shortcuts.
- Lower layers must not know about higher layers.
- Third-party libraries belong in Infrastructure.
- Platform-specific code belongs in Infrastructure.
- Keep Core independent of platform and graphics APIs.
- Prefer existing abstractions before creating new ones.

## Contracts vs Implementations

- Core owns engine-facing contracts and abstractions.
- Infrastructure owns platform-specific implementations.
- Infrastructure may implement Core contracts.
- Core must never depend on Infrastructure implementations.
- Toolkits may depend on Core contracts.
- Framework may depend on Toolkits and Core.
- Lower layers must never depend on higher layers.

# LAYER RESPONSABILITIES

## Infrastructure

Adapters and integrations:
- Windowing
- Input
- Graphics contexts
- Timing
- Asset importing
- Networking integrations
- External libraries

## Core

Minimal engine concepts:
- Math
- Rendering abstractions
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
🔄 Phase 2 - Core (Active)
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

Status: ACTIVE PHASE

All new development should prioritize the active phase unless:
- Fixing a bug.
- Correcting an architectural issue.
- Completing a dependency required by the active phase.

Current Core work consists of two areas:

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

Potential engine concepts:
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

Math and Rendering should remain independent of platform, graphics API, and third-party rendering libraries.

# CORE TEMPLATE VALIDATION

The purpose of CoreTemplate is to prove the architecture end-to-end.

Success criteria:
1. Open a window.
2. Display a background color.
3. Close with Escape.

Nothing more.

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