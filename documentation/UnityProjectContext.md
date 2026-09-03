# Unity Project Context

<!-- unity-onboarding:generated:start -->

## Project Summary

- Project root: `C:/Users/Apoorva/OneDrive/Documents/Git_Hub/TPS`
- Last analyzed: 2026-09-04
- Last analyzed commit: `8ebe8524ae12e8f906502357fdd13d45c4c00f04`
- Third-person shooter prototype centered on movement, cover, aiming, projectile weapons, and Cinemachine camera feedback.

## Confirmed Environment

- Unity version: 6000.4.2f1
- Render pipeline: Universal Render Pipeline 17.4.0
- Input system: Unity Input System 1.19.0 behind `PlayerInputReader`
- Target platforms: Unknown; no platform build was run during onboarding.

## Important Packages And Frameworks

| Area | Finding | Confidence | Evidence |
| --- | --- | --- | --- |
| Camera | Cinemachine 3.1.7 with impulse-based feedback | Confirmed | `Packages/manifest.json`, `Assets/Scripts/Camera/PlayerCameraFeedback.cs` |
| Rendering | Universal Render Pipeline 17.4.0 | Confirmed | `Packages/manifest.json`, `ProjectSettings/GraphicsSettings.asset` |
| Input | Input System 1.19.0 with a project-owned reader component | Confirmed | `Packages/manifest.json`, `Assets/Scripts/Core/PlayerInputReader.cs` |
| Testing | Unity Test Framework 1.6.0 is installed | Confirmed | `Packages/manifest.json` |

## Directory Structure

| Path | Purpose | Confidence | Evidence |
| --- | --- | --- | --- |
| `Assets/Scripts/Core` | Player, aim, cover, and weapon runtime controllers and states | Confirmed | Source files |
| `Assets/Scripts/Camera` | Cinemachine switching, offsets, and feedback | Confirmed | Source files |
| `Assets/Scripts/Data` | ScriptableObject gameplay configuration | Confirmed | `WeaponData.cs`, `Rifle_Data.asset` |
| `Assets/Scripts/Interface` | State interfaces | Confirmed | `IWeaponState.cs`, movement and cover interfaces |
| `Assets/Scripts/Testing` | Prototype projectile implementation | Confirmed | `SimpleProjectile.cs` |
| `Assets/Scenes` | Game scenes | Confirmed | `Testing.unity` |

## Assembly Boundaries

| Assembly | Responsibility | Key references | Notes |
| --- | --- | --- | --- |
| `Assembly-CSharp` | All project-owned runtime code | UnityEngine, Input System, Cinemachine | No first-party asmdefs were found. |

## Scenes And Startup Flow

- Build scenes: `Assets/Scenes/Testing.unity` (enabled)
- Likely startup scene: `Assets/Scenes/Testing.unity`
- Scene loading flow: No separate bootstrap or scene-loading system was found.

## Architecture

| Pattern | Finding | Confidence | Evidence |
| --- | --- | --- | --- |
| Component composition | Player systems are separate MonoBehaviours wired on `PlayerRoot` | Confirmed | `Testing.unity`, scripts under `Assets/Scripts/Core` |
| State machines | Movement, cover, and weapon behavior use small plain-C# states | Confirmed | State interfaces and state implementations |
| Event-driven feedback | Gameplay events drive camera presentation; weapon recoil listens to `ShotFired` | Confirmed | `WeaponController.cs`, `PlayerCameraFeedback.cs` |
| ScriptableObject configuration | `WeaponData` owns weapon gameplay values | Confirmed | `WeaponData.cs`, `Rifle_Data.asset` |

## Coding Conventions

- Namespace style: Project scripts currently use the global namespace.
- Serialized fields: Private `[SerializeField]` references are common; `WeaponData` exposes authoring fields publicly.
- Async: No project async convention was observed.
- Comments/docs: Section-divider comments and Inspector tooltips are common in newer scripts.

## Testing And Validation

- EditMode tests: None found.
- PlayMode tests: None found.
- CI/build validation: No CI configuration was found. `dotnet build Assembly-CSharp.csproj --no-restore` is available as a fallback compile check; Unity Editor validation remains authoritative.

## Available Unity Tooling

| Capability | Status | Evidence |
| --- | --- | --- |
| Unity MCP/editor connection | unavailable | No Unity MCP package/configuration or callable Unity tools were found. |
| Repository inspection | available | Workspace filesystem and generated C# project are accessible. |
| Unity Test Framework | available, no project tests found | Package manifest and source search. |

## Important Constraints

- Preserve the existing `ShotFired -> PlayerCameraFeedback -> CinemachineImpulseSource` ownership boundary.
- Treat `Testing.unity` and current uncommitted changes as user-owned work; make only targeted edits.
- Avoid adding pooling or weapon-specific camera settings until the vertical slice is validated.

## Unknowns And Confidence

- Runtime behavior and Inspector state cannot be confirmed without Unity Editor access.
- Target-platform support and build readiness remain unverified.
- Several gameplay and scene changes are currently uncommitted, so the working tree—not `HEAD`—is the source of truth.

## Source Files Inspected

- `ProjectSettings/ProjectVersion.txt`
- `ProjectSettings/GraphicsSettings.asset`
- `ProjectSettings/EditorBuildSettings.asset`
- `Packages/manifest.json`
- `Packages/packages-lock.json`
- `Assets/Scenes/Testing.unity`
- `Assets/Scripts/Core/WeaponController.cs`
- `Assets/Scripts/Core/AimController.cs`
- `Assets/Scripts/Core/PlayerInputReader.cs`
- `Assets/Scripts/Core/WeaponState/*.cs`
- `Assets/Scripts/Camera/PlayerCameraFeedback.cs`
- `Assets/Scripts/Data/WeaponData.cs`
- `Assets/Scripts/Testing/SimpleProjectile.cs`
- `Assets/Prefabs/Sphere.prefab`

<!-- unity-onboarding:generated:end -->
