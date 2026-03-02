# ⚔️ ÖTÜKEN

> A 3D Action-RPG developed with Unity 6000.3.8.f1 — inspired by the Souls-like genre, built to be accessible.

![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)
![Status](https://img.shields.io/badge/Status-In%20Development-yellow)

---

## 📖 About

**ÖTÜKEN** is a third-person 3D Action-RPG built in Unity. Taking inspiration from Souls-like games, the project aims to deliver a satisfying monster-slaying experience without punishing difficulty — making the genre more accessible while still offering tactical depth through a rich combat system and an evolving storyline.

---

## ✨ Features

### 🧠 Enemy AI System
- **Patrol Behavior** — Enemies roam randomly within a 10×10 meter area around their spawn points, waiting 2 seconds at each patrol node and occasionally stopping to idle
- **Post-Death Reset** — When the player dies, enemies return to their spawn points, restore full health, and resume normal patrol
- **Aggressive Indicators** — Orange rings appear on enemies that are actively hostile; the selected target is highlighted green (passive) or red (aggressive) with a pulsing rotation effect

### ⚔️ Combat & Damage System
- **Area-of-Effect Attacks** — Sword swings damage all enemies within range using `SphereCast` / `OverlapSphere` techniques, enabling multi-enemy engagements
- **Hit Flash Feedback** — Enemies flash red (40% blend) for 0.15 seconds when hit; all mesh renderers are affected, with original materials preserved via a renderer cache
- **Block Mechanic** — Blocking reduces incoming damage by 80% when the enemy attacks from the front (within an 80° tolerance); attacks from the back or side bypass the block

### 🏃 Player Controls
- **Input System** — Built on Unity's new Input System package
- **Movement** — Walk, sprint, jump, and fall with Animator Controller-driven transitions
- **Jump Fix** — Double-jumping / "flying" is prevented; jumps are only allowed when grounded (`isGrounded`) and not already mid-jump
- **Death Cleanup** — On death, input is disabled, physics is set to kinematic, all animation bools are reset, and the collider is turned off so enemies can pass through

### 🎯 Target Selection
- **Lock-On System** — Press `Z` to cycle through valid targets
- **Dead Enemy Filtering** — `IsEnemySelectable()` filters out inactive, dead, or already-removed enemies so only living targets can be locked on

### 🧩 Character & Enemy Integration
- **Multiple Enemy Types** — Zombie characters with their own animation sets, all sharing the same modular `EnemyAI` script
- **Animation State Machine** — Idle, Walk, Run, Attack, and Death states with smooth transitions
- **Asset Pipeline** — Character models and animations sourced from Mixamo; large FBX and texture files managed via **Git LFS**

### 🖥️ UI & UX
- **Main Menu** — Start, Settings, and Quit buttons; username input persisted via `PlayerPrefs`
- **Nick System** — Player's chosen name is displayed above the health bar in-game
- **Healing Objects** — 3D interactable objects heal the player on click within 10 meters; color feedback (green = in range, red = too far) via Raycast
- **Feedback Messages** — "Too far!" warning when out of range; console debug logs for block and damage events; Scene-view Gizmos for interaction radius

---

## 🛠️ Tech Stack

| Area | Details |
|---|---|
| Engine | Unity 2022.3 LTS |
| Language | C# |
| Physics | Rigidbody (`linearVelocity` — updated API) |
| Input | Unity New Input System |
| Animation | Animator Controller + State Machine |
| Assets | Mixamo (characters & animations) |
| Version Control | Git + Git LFS |

---

## 🗂️ Project Structure

```
ÖTÜKEN/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/          # PlayerLocomotion, InputManager, HealthSystem
│   │   ├── Enemy/           # EnemyAI, HealthSystem, HitFlash
│   │   ├── Combat/          # AttackSystem, BlockMechanic, TargetSelector
│   │   └── UI/              # MainMenu, NickSystem, HealObject
│   ├── Animations/          # Animator Controllers & clips
│   ├── Models/              # FBX models (tracked with Git LFS)
│   └── Scenes/
├── .gitattributes           # Git LFS config
└── README.md
```

---

## 🚀 Getting Started

### Prerequisites
- Unity 6000.3.8.f1
- Git LFS (`git lfs install`)

### Setup
```bash
git clone https://github.com/your-username/otuken.git
cd otuken
git lfs pull
```

Then open the project folder in **Unity Hub**.

---

## 🗺️ Roadmap

- [x] Third-person camera & locomotion system
- [x] Basic combat (attack, block, health, death)
- [x] Enemy AI (patrol, chase, attack)
- [x] AoE attack system
- [x] Hit flash visual feedback
- [x] Target lock-on system
- [x] Main menu & nick system
- [ ] Inventory & item pickup system
- [ ] Multiple levels / environments
- [ ] Boss encounters
- [ ] Story & cutscenes

---

## 👤 Author

**Can Polat Doğan**

---

## 📄 License

This project was developed as a university course project. All rights reserved to the author.
