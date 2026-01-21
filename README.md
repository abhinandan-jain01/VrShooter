# VR Shooter (Unity) — In Progress

A small wave-based VR shooter prototype built in **Unity 6.3 LTS** using **XR Interaction Toolkit + OpenXR**.  
The goal of this project is to create a clean, portfolio-ready VR demo with solid gameplay fundamentals (waves, enemy behaviors, ammo economy, UI feedback, win/lose loop).

> **Status:** 🚧 Work in Progress (actively being developed)

---

## Gameplay Overview
- You are placed inside a simple room arena.
- Enemies spawn in waves (multiple enemy types).
- Eliminate enemies before you run out of time/ammo.
- **Game ends** on **player death** or **ammo depletion**.
- Win after clearing all waves.

---

## Key Features
- **Wave Spawning System**
  - Multiple waves with configurable enemy counts
  - Multiple spawn points
  - Drone spawns at a higher height than ground runners
- **Enemy AI (Two Movement Types)**
  - **Drone:** hover + wavy movement
  - **Runner:** aggressive criss-cross / zig-zag movement
- **Gun System**
  - Raycast shooting with visible laser line
  - Auto-aim assist (angle + distance based)
  - Cooldown-based firing
- **Ammo System**
  - Per-wave total ammo allocation
  - Magazine size: **40**
  - Reload uses reserve ammo
  - **Out of ammo = Game Over**
- **UI / Feedback**
  - HP, Wave, Alive enemies, Score, Ammo, Timer
  - Damage flash overlay
  - Game Over / Win panels
- **Dual Controls**
  - PC controls for quick testing (no headset required)
  - XR bindings for VR controller input (OpenXR)

---

## Tech Stack
- **Engine:** Unity 6.3 LTS
- **XR:** OpenXR + XR Interaction Toolkit
- **Input:** Unity Input System
- **UI:** TextMeshPro

---

## Controls
### PC (Testing)
- **Shoot:** Left Mouse Button
- **Reload:** (Configured in Inspector)
- **Restart:** `R`

### VR (OpenXR / XRI)
- **Shoot:** Right Trigger (binding depends on device profile)
- **Reload:** Configurable XR action
- **Restart:** Configurable XR action

> VR hardware is not required to test core gameplay. The project supports simulator/testing workflows.

---

## Setup / How to Run
1. Clone the repo:
   ```bash
   git clone <REPO_URL>
