# VR Orbit Simulation

A Unity VR project about Earth's orbit, the solar system, seasons, habitable zones,
and Kepler's laws. This repository continues the Fall 2025 capstone project.

## Setup

1. Install **Unity 6000.5.10f1** through Unity Hub. For Quest builds, include
   Android Build Support, Android SDK/NDK Tools, and OpenJDK.
2. Clone this repository and add its root folder to Unity Hub. The correct folder
   contains `Assets`, `Packages`, and `ProjectSettings`.
3. Let Unity finish importing. If prompted, import **TMP Essentials**.
   TMP Examples & Extras are optional and are not needed here.
4. Open `Assets/Scenes/OrbitalModel.unity`.

The project uses the Built-in Render Pipeline, OpenXR 1.17.1, and the Input System.
XR Interaction Toolkit 2.6.4 is embedded in
`Packages/com.unity.xr.interaction.toolkit` with Unity 6.5 editor compatibility
changes. Keep this directory in source control. The inherited rig still uses
XRI 2 locomotion; changing to XRI 3 requires a separate rig migration.

## Controls

| Action | Quest controller |
| --- | --- |
| Open task tablet | A |
| Close tablet or return from planet inspection | B |
| Select tablet row | Ray + trigger, or X |
| Move between tablet rows | Left thumbstick up/down |
| Next / previous planet | Right / left trigger |
| Snap turn | Right thumbstick left/right |
| Continue Red Sun or Seasons | A |
| Teleport on the floor | Point and release trigger |

The original learning activities remain available. The guided learning redesign
is the next development phase; it has not replaced the inherited lesson flow yet.

## Project folders

| Folder | Contents |
| --- | --- |
| `Assets/Scenes` | Seven inherited learning scenes and Seasons assets |
| `Assets/Global` | Simulation clock, audio, and space backgrounds |
| `Assets/Hub` | Podiums, buttons, speed control, and room navigation |
| `Assets/Sandbox/Simulation` | Planet models, orbital calculations, tablets, and lessons |
| `Assets/XR` and `Assets/XRI` | Rig, controller art, and XR settings |
| `Assets/Samples` | Retained XRI input, controller, and teleport support |
| `Assets/FutureAssets` | Science-fiction props; some are already referenced by scenes |
| `Assets/Plugins` | Required vendor tools, planet art, and reusable assets |
| `SourceArt/Earth` | Editable Blender source, kept outside Unity imports |

The existing FBX and its metadata provide the Earth/coordinate model used by
Unity. Blender is not required to import or run the project.

## Building

Quest 3 standalone is the first target. In **File > Build Profiles**, select
Android and use the existing IL2CPP / ARM64 settings. Keep the enabled scenes in
this order: `OrbitalModel`, `HabitableZone1`, `S1View`, `S2View`, `S3View`, `Seasons`.
`EarthView` is retained for future integration and is not enabled in the build.
Save builds outside the repository. Windows PC VR is a separate later build.

## Working on the code

Keep scripts small and direct: normal MonoBehaviour methods, clear field names,
and brief comments where the behavior needs explaining. Keep existing Inspector
field names, script metadata, and button callback names when editing a component.
Changes to research calculations, units, epochs, and model positioning need their
own review; do not mix them into UI or XR cleanup.

The current visual orbits and Red Sun presentation include inherited
approximations. Scientific calculation wiring and content review remain part of
future development. Physical headset input, comfort, performance, sliders, and
Android pause/resume still require a Quest acceptance pass.

## Credits and research

Original project: [KellyLFrear/FALL2025-VR-Orbit-Simulation](https://github.com/KellyLFrear/FALL2025-VR-Orbit-Simulation).
Fall 2025 contributors: Kelly Frear, Gavin Rosander, Arpita Godbole, and Daniel
Pangilian. The project also builds on earlier capstone work by williamphong.

Dr. Kostadinov's research and MATLAB model were translated into the retained C#
orbital calculations. Keep the research attribution with future model work:

- [Kostadinov and Gilb, Earth Orbit model](https://gmd.copernicus.org/articles/7/1051/2014/)
- [Earth Orbit source archive](https://zenodo.org/records/4346609)

Third-party asset ownership remains with its authors. See
[third-party notices](THIRD_PARTY_NOTICES.md) and the licenses included with vendor
packages.
