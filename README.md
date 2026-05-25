# Unity Ballistic Launch System

**A realistic ballistic trajectory simulation in Unity3D**  
*Physics-based projectile launcher with automatic angle calculation*

---

## 📋 Project Description

This project implements a **ballistic launch simulation** in Unity. The system calculates the required **launch angles** (horizontal and vertical) to hit a given target based on the initial position and target coordinates. After computing the optimal trajectory parameters, it simulates the projectile's flight using Unity's built-in or custom physics.

The main goal is to demonstrate realistic ballistic physics, including gravity, initial velocity, and precise angle solving for hitting moving or static targets.

---

## ✨ Key Features

- Support for different modes: static launcher and dynamic drone
- Automatic calculation of **horizontal (azimuth)** and **vertical (elevation)** launch angles in Launcher mode
- Automatic calculation of **release point** and **time to release** in Drone mode
- Real-time projectile simulation using Unity Physics
- User-controlled camera for better trajectory viewing
- Support for different initial velocities and gravitational conditions
- Clean and modular C# architecture
- Ready for integration into games, simulations, or educational tools

---

## 🛠 Technologies

- **Unity3D** (compatible with Unity 2021.3+)
- **C#** — Core logic and ballistic calculations
- **Unity Physics** — Projectile simulation
- ShaderLab & HLSL — Visual effects / materials

---

## 📁 Project Structure
```
unity-ballistic-launch-system/
├── Assets/                  # Main Unity folder
│   ├── Scripts/             # C# scripts (BallisticCalculator.cs, Launcher.cs, etc.)
│   ├── Prefabs/             # Projectile, launcher, target prefabs
│   ├── Scenes/              # Main simulation scene
│   ├── Materials/
│   └── ...
├── Packages/                # Unity package dependencies
├── ProjectSettings/         # Unity project settings
├── My project.slnx          # Visual Studio solution
├── .gitignore
└── README.md
text---
```

## 🚀 How to Run

### Prerequisites
- Unity Hub installed
- Unity Editor **2021.3 LTS** or newer (recommended)

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/anastasiia-kolomiiets/unity-ballistic-launch-system.git
2. Open the project in Unity
- Open Unity Hub
- Click "Add project from disk"
- Select the cloned folder
- Open the project with your Unity version

3. Run the simulation
- Open the main scene (usually located in Assets/Scenes/SampleScene.unity)
- Press Play button in the Unity Editor
- Use the UI or input controls to set target position and launch

---

## How It Works

1. User sets mode, initial position and target position, initial speed and air resistance data.
2. The system solves the ballistic equations to find the required angles or release information.
3. A projectile is instantiated with the calculated velocity vector.
4. Unity Physics or custom physics mechanic takes over — simulating gravity, drag (if enabled), and collision.

---

## Roadmap (Planned Improvements)

- Trajectory prediction line (dashed path or smoke visualization) ✅
- Still or moving drone simulation ✅

---

## Screenshots
<div align="center">
  
### Launcher mode
<img width="1418" height="884" alt="Screenshot 2026-05-25 at 11 17 06" src="https://github.com/user-attachments/assets/b642b49b-3415-4ea1-be2e-371aa2ce5ec7" />
  
### Drone mode
<img width="1415" height="884" alt="Screenshot 2026-05-25 at 11 18 14" src="https://github.com/user-attachments/assets/84678ee7-fd6e-4dde-b560-67dbc8f581b3" />

</div>

---

## Author
Anastasiia Kolomiiets
- GitHub: @anastasiia-kolomiiets

---

## License
This project is open-source. License not specified — feel free to contact the author for usage rights.

---

Built with ❤️ and Unity Physics
Last updated: March 2026
