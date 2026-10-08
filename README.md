# 🎮 JOT.NET

<p align="center">
  <img src="docs/images/jot-logo.pngalign="center">
  <strong>A Modular Multi-purpose Game Engine for .NET</strong>
</p>

<p align="center">
  Infrastructure • Core • Toolkits • Framework
</p>

---

JOT.NET is a modern C# port of the original JOT (Just One Thing) Modular Multi-purpose Game Engine.

Designed for experimentation, education, prototyping, and massively multiplayer online games, JOT.NET aims to provide a clean, extensible, and fully modular architecture where every subsystem can be understood, modified, replaced, or rebuilt.

The goal of JOT.NET is not to become the largest engine, but to remain understandable, extensible, and replaceable.

---

<a id="table-of-contents"></a>

# 📚 Table of Contents

- #️-installation
- #-usage
- #️-architecture
- #-core-principles
- #-mmo-first-philosophy
- #-browser-client-strategy
- #-layer-responsibilities
- #-project-organization
- #-templates-and-examples
- [️-technology-stack
- [️-roadmap
- [-contact--license-notice
- [-final-rule

---

# ⚙️ Installation

## Prerequisites

### 🪟 Windows

Required:

- Windows 10 or newer
- .NET SDK 9.0+
- Git
- Visual Studio 2022, VS Code, or Rider
- OpenGL 4.6 or Vulkan compatible GPU

Verify installation:

```powershell
dotnet --version
git --version
```

### 🐧 Linux

Required:

- 64-bit Linux distribution
- .NET SDK 9.0+
- Git
- GCC / build-essential
- OpenGL 4.6 or Vulkan compatible GPU

Example:

```bash
sudo apt update
sudo apt install git build-essential
```

Verify installation:

```bash
dotnet --version
git --version
```

### 🍎 macOS

Required:

- macOS Sonoma or newer
- .NET SDK 9.0+
- Git
- Xcode command-line tools

Install:

```bash
xcode-select --install
```

Verify:

```bash
dotnet --version
git --version
```

## Clone Repository

```bash
git clone https://github.com/g-amador/JOT.NET.git
cd JOT.NET
```

## Restore Dependencies

```bash
dotnet restore
```

## Build Solution

```bash
dotnet build
```

🔝 #table-of-contents

---

# 🚀 Usage

## Core Template

Run:

```bash
dotnet run --project demos/templates/core-template
```

Uses:

```text
Infrastructure + Core
```

Ideal for:

- Learning the engine
- Lightweight applications
- Custom engine development

## Full Template

Run:

```bash
dotnet run --project demos/templates/full-template
```

Uses:

```text
Infrastructure + Core + Toolkits + Framework
```

Ideal for:

- Complete games
- Prototypes
- MMO experiments

## Examples

Examples are located at:

```text
demos/examples
```

Example:

```bash
dotnet run --project demos/examples/rendering-showcase
```

🔝 #table-of-contents

---

# 🏗️ Architecture

<p align="center">
  docs/images/architecture.png
</p>

```text
Framework
    ↓
Toolkits
    ↓
Core
    ↓
Infrastructure
```

Each layer may only depend on itself or lower layers.

Benefits:

- Replaceability
- Testability
- Maintainability
- Scalability

🔝 #table-of-contents

---

# 📖 Core Principles

## Everything Is Replaceable

No subsystem is permanent.

Rendering, networking, AI, physics, asset pipelines, and GUI systems must be replaceable.

---

## Dependencies Flow Downward

```text
Framework
    ↓
Toolkits
    ↓
Core
    ↓
Infrastructure
```

Rules:

- No circular dependencies
- No upward dependencies
- No cross-layer shortcuts

---

## Infrastructure Is an Adapter Layer

Current technologies:

- Silk.NET
- AssimpNet
- System.Numerics
- DotNetty
- Protocol Buffers

Infrastructure must never contain:

- Game logic
- AI
- Scene management
- Gameplay systems

---

## Core Must Remain Minimal

Core includes:

- Math
- Input
- Resources
- Rendering foundations
- Basic collision detection
- Utilities

---

## Toolkits Are Optional

Examples:

- AI
- Networking
- ECS
- Serialization
- Geometry Generation
- Extended Physics

---

## Framework Exists to Reduce Boilerplate

Examples:

- Asset Management
- Scene Management
- State Management
- GUI Systems

🔝 #table-of-contents

---

# 🌐 MMO-First Philosophy

JOT.NET is designed with multiplayer support from the very beginning.

Key considerations:

- Replication
- Serialization
- Bandwidth efficiency
- Scalability
- Deterministic state

Single-player is treated as a special case of multiplayer.

🔝 #table-of-contents

---

# 🌍 Browser Client Strategy

Desktop is the primary target.

Future targets include:

- WebAssembly
- WebGPU
- WebSockets

```text
Desktop Client
      │
      ▼
 Shared Core
      ▲
      │
 Browser Client
```

🔝 #table-of-contents

---

# 🧩 Layer Responsibilities

```text
┌─────────────────────────┐
│        Framework        │
│ Scenes, Assets, GUI     │
└─────────────▲───────────┘
              │
┌─────────────┴───────────┐
│        Toolkits         │
│ AI, Networking, ECS     │
└─────────────▲───────────┘
              │
┌─────────────┴───────────┐
│          Core           │
│ Math, Physics, Input    │
└─────────────▲───────────┘
              │
┌─────────────┴───────────┐
│     Infrastructure      │
│ Silk.NET, AssimpNet     │
│ DotNetty, Numerics      │
└─────────────────────────┘
```

### Infrastructure

- Windowing
- Rendering APIs
- Audio APIs
- Networking adapters

### Core

- Resources
- Input
- Rendering foundations
- Physics foundations

### Toolkits

- AI
- ECS
- Networking
- Serialization
- Extended Physics

### Framework

- Scene System
- Asset Management
- GUI
- Application Management

🔝 #table-of-contents

---

# 📂 Project Organization

```text
JOT.NET/
├── assets/
├── demos/
│   ├── templates/
│   │   ├── core-template/
│   │   └── full-template/
│   └── examples/
├── docs/
│   └── images/
├── engine/
│   ├── infrastructure/
│   ├── core/
│   ├── toolkits/
│   └── framework/
├── client-desktop/
├── client-browser/
├── server/
├── LICENSE
└── README.md
```

🔝 #table-of-contents

---

# 🧪 Templates and Examples

## Core Template

Demonstrates:

```text
Infrastructure + Core
```

## Full Template

Demonstrates:

```text
Infrastructure + Core + Toolkits + Framework
```

## Examples

Examples may include:

- Rendering Showcase
- Physics Showcase
- Networking Showcase
- AI Showcase
- MMO Prototype

🔝 #table-of-contents

---

# 🛠️ Technology Stack

| Purpose | Technology |
|----------|------------|
| Rendering / Input / Audio | Silk.NET |
| Mathematics | System.Numerics |
| Model Loading | AssimpNet |
| Networking | DotNetty |
| Serialization | Protocol Buffers |

Technologies may evolve.

Architectural principles must not.

🔝 #table-of-contents

---

# 🗺️ Roadmap

## Phase 1

- Infrastructure
- Windowing
- Rendering
- Input

## Phase 2

- Core Systems
- Resources
- Mathematics
- Physics Foundations

## Phase 3

- Networking Toolkit
- ECS Toolkit
- AI Toolkit

## Phase 4

- Framework
- Scene Management
- GUI
- Asset Management

## Phase 5

- MMO Prototype
- Browser Client

🔝 #table-of-contents

---

# 📜 Contact & License Notice

JOT.NET is released under the Apache License 2.0.

You are free to:

- Use
- Modify
- Distribute
- Extend
- Port

the engine in accordance with the license.

Please keep the following in mind:

- Do not claim authorship of JOT or JOT.NET.
- If you create games, extensions, demos, templates, or ports, I would be delighted to hear about them.

📧 **g.n.p.amador@gmail.com**

🔝 #table-of-contents

---

# ✅ Final Rule

When choosing between two designs, prefer the one that:

1. Introduces fewer dependencies.
2. Produces cleaner module boundaries.
3. Is easier to replace.
4. Is easier to understand.
5. Keeps Core small.

If a feature does not belong in Core, it belongs in a Toolkit or outside the engine entirely.

> **JOT.NET is not built to be the biggest engine.**
>
> **JOT.NET is built to be the engine that can be understood, modified, and rebuilt.**

🔝 #table-of-contents
