# 🎮 JOT.NET

<p align="center">
  <strong>A Modular Multi-purpose Game Engine for .NET</strong>
</p>

<p align="center">
  Infrastructure • Core • Toolkits • Framework
</p>

---

JOT.NET is a modern C#/.NET reimplementation of the original Java-based JOT (Just One Thing) Modular Multi-purpose Game Engine.

Designed for experimentation, education, prototyping, and massively multiplayer online games (MMOGs), JOT.NET provides a modular architecture where engine subsystems can be understood, extended, replaced, or rewritten independently.

The project aims to modernize the original engine while preserving its strongest architectural ideas: clear layer separation, replaceable subsystems, and an MMO-oriented design philosophy.

---

## 🚧 Project Status

JOT.NET is currently under active development.

Current priorities:

- Establishing the engine architecture
- Implementing the Infrastructure layer
- Building the desktop client foundation
- Building the browser client foundation
- Defining the Core engine systems
- Preparing the networking architecture for MMO support

At this stage the project should be considered **experimental and not production ready**.

---

## 🎯 Design Goals

JOT.NET is designed around the following objectives:

- ✅ Modular architecture
- ✅ Replaceable subsystems
- ✅ MMO-first design
- ✅ Browser client support
- ✅ Educational value
- ✅ Clean dependency flow
- ✅ Modern .NET ecosystem
- ✅ Long-term maintainability

Every feature added to the engine should support one or more of these goals.

---

## 📑 <a name="toc">Table of Contents</a>

1. [📜 Origins](#origins)
2. [🏗️ Architecture Overview](#architecture-overview)
3. [📖 Core Principles](#core-principles)
4. [🌐 MMO-First Philosophy](#mmo-first-philosophy)
5. [🌍 Browser Client Strategy](#browser-client-strategy)
6. [🧩 Layer Responsibilities](#layer-responsibilities)
7. [📂 Project Organization](#project-organization)
8. [🛠️ Technology Stack](#technology-stack)</a>
9. [⚙️ Installation](#installation)
10. [🚀 Usage](#usage)
11. [🧪 Templates and Examples](#templates-and-examples)
12. [🤝 Contributing](#contributing)
13. [📄 License](#license)
14. [📬 Contact](#contact)

---

## 📜 <a name="origins">Origins</a>

JOT.NET is a modern C#/.NET reimplementation of the original Java-based JOT (Just One Thing) Modular Multi-purpose Game Engine.

The original JOT project was created as a modular game engine for experimentation, education, research, and massively multiplayer online games (MMOGs). Its primary objective was to demonstrate that game engines can be designed as collections of replaceable, well-isolated components rather than as monolithic systems.

JOT.NET preserves the architectural vision of the original project while modernizing the technology stack, development workflow, and target platforms.

The result is a new engine built around contemporary .NET technologies while remaining faithful to the principles of modularity, extensibility, and educational value that inspired the original work.

### 🔗 Original Project

- GitHub Repository: <https://github.com/g-amador/JOT>
- Original Engine: JOT – A Modular Multi-purpose Game Engine
- Research Paper: JOT – A Modular Multi-purpose Minimalistic Massively Multiplayer Online Game Engine

### ✅ What Carries Forward

JOT.NET preserves the most important ideas from the original engine:

- Layered architecture
- Modular design
- Replaceable subsystems
- Clear separation between Core, Toolkits, and Framework
- Research and educational focus
- MMO-oriented design philosophy

### 🔄 Modernization

JOT.NET adopts a modern .NET ecosystem while retaining the conceptual architecture of the original project.

| Original JOT (Java) | JOT.NET (.NET) |
|---------------------|----------------|
| JOGL / JogAmp | Silk.NET |
| Apache Commons Math | System.Numerics |
| Assimp Java Bindings | AssimpNet |
| JGroups | DotNetty |
| NetBeans | Visual Studio, VS Code, Rider |
| Maven | .NET SDK, NuGet, Central Package Management |
| Java Desktop Client | Desktop + Browser Clients |

### 🎯 Why a Reimplementation Instead of a Direct Port?

The goal of JOT.NET is not simply to translate Java code into C#.

Instead, the project uses the original architecture as a foundation while taking advantage of modern .NET tooling, improved package management, cross-platform support, browser technologies, and contemporary software engineering practices.

This approach allows the engine to evolve independently while preserving the ideas that made the original JOT unique.

[Back to Table of Contents](#toc)

---

## 🏗️ <a name="architecture-overview">Architecture Overview</a>

JOT.NET follows a layered architecture:

```text
Framework
    ↓
Toolkits
    ↓
Core
    ↓
Infrastructure
```

Dependencies always flow downward.

- No circular dependencies
- No cross-layer shortcuts
- External libraries are isolated in Infrastructure

[Back to Table of Contents](#toc)

---

## 📖 <a name="core-principles">Core Principles</a>

The architecture of JOT.NET is guided by a small set of fundamental principles.

These principles are intended to remain stable even as technologies, libraries, and implementation details evolve over time.

---

### ♻️ Everything Is Replaceable

Subsystems should be replaceable with alternative implementations without affecting the rest of the engine.

---

### ⬇️ Dependencies Flow Downward

Dependencies must always flow toward lower layers.

```text
Framework
    ↓
Toolkits
    ↓
Core
    ↓
Infrastructure
```

The following rules apply:

- A layer may depend only on itself or lower layers.
- Circular dependencies are forbidden.
- Cross-layer shortcuts are forbidden.
- Lower layers must never be aware of higher layers.

These rules help prevent architectural drift as the project grows.

---

### 🔌 Infrastructure Is an Adapter Layer

Infrastructure exists to integrate external technologies and platform-specific functionality.

Infrastructure is responsible for:

- Window creation
- Graphics contexts
- Audio contexts
- Model importing
- Networking adapters
- Third-party library integration

Infrastructure is **not** responsible for:

- Game logic
- Scene management
- AI
- Gameplay systems
- Framework services

Infrastructure adapts external technologies to the engine, but should contain as little engine-specific behavior as possible.

---

### ⚙️ Core Must Remain Minimal

The Core layer contains the minimum functionality required to classify JOT.NET as a game engine.

Core responsibilities include:

- Mathematics
- Resource abstractions
- Rendering abstractions
- Basic collision detection
- Input abstractions
- Utility systems

A complete application should be buildable using only:

```text
Infrastructure + Core
```

If a feature is not universally required by most games, it probably does not belong in Core.

---

### 🧰 Toolkits Are Optional

Toolkits extend the capabilities of the engine without increasing the complexity of Core.

Examples include:

- Artificial Intelligence
- Networking
- Serialization
- ECS
- Geometry generation
- Advanced physics systems

The absence of a toolkit must never prevent the engine from functioning.

Toolkits exist to provide advanced functionality while keeping Core small and focused.

---

### 🏛️ Framework Exists to Reduce Boilerplate

The Framework layer exists to simplify application development.

Typical framework responsibilities include:

- Scene management
- Asset management
- State management
- GUI integration
- Application lifecycle management

The Framework should improve productivity without becoming mandatory.

Developers should be free to work directly with Core when greater control or educational value is desired.

---

### 🎓 Educational Value Matters

JOT.NET is intended to be more than a game engine.

It is also intended to be:

- A learning platform
- A research platform
- A teaching tool
- A reference implementation of modular engine architecture

Whenever reasonable, the architecture should favor clarity and maintainability over unnecessary complexity.

The engine should remain approachable to students, hobbyists, researchers, and professionals alike.

---

### 🚀 Modern Technologies, Stable Principles

Technologies may change. The architectural principles should not.

The architectural principles should remain stable.

Current technologies include:

- Silk.NET
- AssimpNet
- System.Numerics
- DotNetty
- Blazor WebAssembly
- MudBlazor (planned)
- Protocol Buffers

Future technologies may replace some of these choices, but the core principles of modularity, replaceability, educational value, and clean dependency flow should remain unchanged.

[Back to Table of Contents](#toc)

---

## 🌐 <a name="mmo-first-philosophy">MMO-First Philosophy</a>

Networking is not an afterthought in JOT.NET.

Unlike many game engines that begin as single-player frameworks and later add networking capabilities, JOT.NET is designed with multiplayer and MMO architectures in mind from the start.

Key design considerations include:

- Client/server separation
- State synchronization
- Replication
- Serialization
- Scalability
- Distributed systems

This philosophy influences the design of the Core, Toolkits, and Framework layers even when building single-player applications.

### 🏗️ Designed for Growth

JOT.NET aims to support a wide range of applications, from local prototypes to persistent online worlds.

Long-term goals include:

- Dedicated game servers
- Browser-based clients
- Shared simulation systems
- MMO-oriented networking toolkits

Single-player games remain fully supported. JOT.NET simply treats networking as a first-class architectural concern from the start.

[Back to Table of Contents](#toc)

---

## 🌍 <a name="browser-client-strategy">Browser Client Strategy</a>

JOT.NET aims to support both desktop and browser-based applications through a shared engine architecture.

The browser client is built using:

- Blazor WebAssembly
- MudBlazor (planned)
- WebSockets

The objective is to maximize code reuse between desktop and browser clients while remaining lightweight, accessible, and multiplayer-friendly.

Browser support is considered a strategic goal of the project and an important step toward making JOT.NET available across a wider range of platforms.

[Back to Table of Contents](#toc)

---

## 🧩 <a name="layer-responsibilities">Layer Responsibilities</a>

JOT.NET is organized into four layers, each with a clearly defined responsibility.

```text
Framework
    ↓
Toolkits
    ↓
Core
    ↓
Infrastructure
```

### 🔌 Infrastructure

Provides adapters for external technologies and platform-specific functionality.

Examples:

- Windowing and input
- Rendering backends
- Model importing
- Networking integrations

Projects:

- JOT.Infrastructure.Silk
- JOT.Infrastructure.Assimp
- JOT.Infrastructure.Numerics
- JOT.Infrastructure.DotNetty

### ⚙️ Core

Provides the minimum functionality required by all applications built with JOT.NET.

Examples:

- Mathematics
- Resource abstractions
- Rendering abstractions
- Basic collision detection
- Common utilities

Projects:

- JOT.Core.Math
- JOT.Core.IO
- JOT.Core.Util
- JOT.Core.Physics
- JOT.Core.Rendering

### 🧰 Toolkits

Provide optional, advanced functionality that extends the Core layer.

Examples:

- Artificial Intelligence
- Networking
- Serialization
- ECS
- Geometry generation
- Advanced physics

Projects:

- JOT.Toolkit.AI
- JOT.Toolkit.ECS
- JOT.Toolkit.Geometry
- JOT.Toolkit.Networking
- JOT.Toolkit.Physics
- JOT.Toolkit.Serialization

### 🏛️ Framework

Provides high-level systems intended to reduce development boilerplate.

Examples:

- Scene management
- Asset management
- State management
- GUI integration

Projects:

- JOT.Framework.Application
- JOT.Framework.GUI
- JOT.Framework.Managers

The Framework is optional. Applications can be built using only the Infrastructure and Core layers or can leverage Toolkits and Framework for increased productivity.

[Back to Table of Contents](#toc)

---

## 📂 <a name="project-organization">Project Organization</a>

```text
src/
├── JOT.NET.slnx
├── client-browser/
├── client-desktop/
├── demos/
├── engine/
│   ├── infrastructure/
│   ├── core/
│   ├── toolkits/
│   └── framework/
└── server/
```

The solution is organized around the engine's layered architecture, with dedicated projects for infrastructure, core systems, optional toolkits, application frameworks, clients, servers, and example templates.

[Back to Table of Contents](#toc)

---

## 🛠️ <a name="technology-stack">Technology Stack</a>

| Purpose | Technology |
|----------|------------|
| Desktop Client | Silk.NET |
| Browser Client | Blazor WebAssembly |
| Browser UI | MudBlazor (planned) |
| Mathematics | System.Numerics |
| Model Loading | AssimpNet |
| Networking | DotNetty |
| Serialization | Protocol Buffers |
| Package Management | Central Package Management (CPM) |

Technologies may evolve over time, but the architectural principles of modularity, replaceability, and clean dependency flow should remain stable.

[Back to Table of Contents](#toc)

---

## ⚙️ <a name="installation">Installation</a>

### Prerequisites

JOT.NET targets the modern .NET ecosystem and supports Windows, Linux, and macOS.

Required software:

- .NET SDK 9.0 or newer
- Git
- A modern IDE:
  - Visual Studio 2022
  - Visual Studio Code
  - JetBrains Rider

The desktop client additionally requires graphics hardware capable of running Silk.NET-based applications.

### Clone the Repository

```bash
git clone https://github.com/g-amador/JOT.NET.git
cd JOT.NET
```

### Restore Dependencies

```bash
dotnet restore
```

### Build the Solution

```bash
dotnet build
```

### Verify the Installation

Run one of the template projects:

```bash
dotnet run --project src/demos/templates/CoreTemplate
```

If the template builds and runs successfully, your environment is correctly configured.

[Back to Table of Contents](#toc)

---

## 🚀 <a name="usage">Usage</a>

Run:

dotnet run --project src/demos/templates/CoreTemplate

or

dotnet run --project src/demos/templates/FullTemplate
`

[Back to Table of Contents](#toc)

---

## 🧪 <a name="templates-and-examples">Templates and Examples</a>

JOT.NET provides two official templates that demonstrate different levels of engine usage.

### ⚙️ Core Template

Uses only:

```text
Infrastructure + Core
```

This template demonstrates the minimum functionality required to build applications with JOT.NET.

### 🏛️ Full Template

Uses:

```text
Infrastructure + Core + Toolkits + Framework
```

This template demonstrates how the higher layers reduce development effort and application boilerplate.

### 🚀 Examples

Additional examples will showcase individual engine features, such as:

- Rendering
- Physics
- AI
- Networking
- MMO-oriented systems

[Back to Table of Contents](#toc)

---

## 🤝 <a name="contributing">Contributing</a>

Contributions are welcome, including:

- Bug reports
- Feature requests
- Documentation improvements
- Examples and demos
- Engine components and toolkits

When contributing, please respect the layered architecture and keep Core minimal.

[Back to Table of Contents](#toc)

---

## 📄 <a name="license">License</a>

JOT.NET is released under the **MIT License**.

See the `LICENSE` file for details.

[Back to Table of Contents](#toc)

---

## 📬 <a name="contact">Contact</a>

If you have questions, suggestions, feedback, or would like to share a project built with JOT.NET, feel free to get in touch:

📧 **g.n.p.amador@gmail.com**

If you extend JOT.NET, create templates, demos, games, or ports based on it, I would be delighted to hear about it.

[Back to Table of Contents](#toc)

---