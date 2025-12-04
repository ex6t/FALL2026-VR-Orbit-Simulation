# VR Orbit Simulation

An educational VR demo focusing on Earth’s orbit, the Red Sun, the eight planets (and Pluto) that are in our solar system, and Kepler’s Laws of Planetary Motion. Developed in Unity (C#) for the CS490 Capstone Project, this simulation provides an immersive way for students to explore orbital mechanics and the solar system.

In Fall 2025, our group expanded the original project by adding a 2D Task Tablet system, new interactive minigames, and additional educational experiences.

---

## 🚀 Main Features

* **Interactive Solar System:** Explore Earth’s orbit and view close‑ups of each planet.
* **2D Task Tablet (A Button):** Pull up a floating VR tablet with multiple learning options:

  * **Planet Close‑Ups:** Select a planet to view it up close in 3D.
  * **Red Sun Minigame:** Experience stylized solar visuals through a small interactive activity.
  * **Seasons Simulation:** Stand on Earth as it rotates and watch seasons change in real time.
* **Immersive Learning:** Concepts are presented visually to help students understand how orbits and tilt shape seasonal cycles.
* **Smooth VR Support:** Compatible with Oculus, SteamVR, and OpenXR.

---

## 🎮 Keybinds

| Action                       | Control                       |
| ---------------------------- | ----------------------------- |
| Open Tablet                  | **A**                         |
| Close Tablet                 | **B**                         |
| Confirm Option               | **X**                         |
| Unused                       | **Y**                         |
| Move Up/Down in Tablet       | **Right Trigger**             |
| Movement (Tablet Navigation) | **Left Trigger (Hold North)** |

---

## 🛠️ Tech Stack

* **Language:** C#
* **Engine:** Unity (2022.3 LTS)
* **Physics:** Unity Rigidbody
* **VR Support:** Oculus / SteamVR / OpenXR

---

## ▶️ Download & Setup Guide

### 1. Unity Installation

1. Install **Unity Hub** and sign in.
2. Add **Unity 2022.3 LTS** under *Installs*.
3. Make sure to include:

   * Android Build Support
   * Windows Build Support
   * Visual Studio Community 2022
4. After installation, create a test 3D project to verify setup.

### 2. Steam & SteamVR Installation

1. Install **Steam**.
2. Search for and install **SteamVR**.
3. Launch SteamVR to confirm your VR headset is detected.
4. Keep SteamVR running before launching Unity VR scenes.

### 3. Meta Quest 3 Setup

1. Install **Meta Quest Developer Hub**.
2. Enable Developer Mode through the **Meta mobile app**.
3. Connect the headset with a USB‑C cable.

### 4. Visual Studio Installation

1. Install **Visual Studio 2022 Community Edition**.
2. Open it once to finalize installation.

### 5. Git & Repository Setup

1. Install Git or GitHub Desktop.
2. Clone the repository:

   ```bash
   git clone https://github.com/KellyLFrear/FALL2025-VR-Orbit-Simulation.git
   ```
3. Open the folder in Unity Hub via **Add Project → Select Folder**.

### 6. Unity XR Configuration

1. Go to **Edit → Project Settings → XR Plug‑in Management**.
2. Enable **OpenXR** under both Windows and Android.
3. Under **OpenXR Features**, enable:

   * Oculus Touch
   * Hand Tracking
4. In **Package Manager**, install:

   * XR Interaction Toolkit
   * Input System
   * TextMesh Pro
   * Universal Render Pipeline
   * Oculus XR Plugin

---

## 📘 Educational Concepts

* Kepler’s Laws of Planetary Motion
* Earth’s orbital path
* Axial tilt and seasonal shift
* Planetary scaling & spatial awareness
* Solar radiation (Red Sun experience)

---

## 📈 Ideas for Future Groups

* Build a tutorial or intro scene for first‑time VR users.
* Upgrade to Unity 2026 or the newest available version.
* Create a tidal‑wave minigame showing lunar gravitational influence.
* Create a solar‑wave minigame focusing on sun‑based radiation/wave patterns.
* For seasons, add a ground‑level seasons perspective where players watch the sun move throughout the year.
* Turn the coordinates on the Earth off/on

---

## 🪐 Acknowledgements
**This project was created in collaboration with Dr. Kostadinov, whose MATLAB code and research were translated into C# for use in this simulation! We also appreciate the help and feedback he's provided for us in order to make this project.**
* Earth Orbit Research Paper: https://gmd.copernicus.org/articles/7/1051/2014/
* Earth Orbit Model Source Code: https://zenodo.org/records/4346609

**We Also Appreciate The Help Of:**
* CS490 faculty & advisors
* Unity XR documentation
* NASA data resources
* First Verion Of The Capstone Project Was Created By @williamphong

**This Project Was Developed By:**
* Kelly Frear @kellylfrear
* Gavin Rosander @rosanderg913
* Arpita Godbole @arpitag2025
* Daniel Pangilian @dpg2003

---
