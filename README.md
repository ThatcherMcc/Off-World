# Off-World

[Short Demo Video on YouTube](https://www.youtube.com/watch?v=f-bmZSFRsHM&ab_channel=YeetleBandeetle)

[![Off World Game Short Demo](https://img.youtube.com/vi/f-bmZSFRsHM/maxresdefault.jpg)](https://www.youtube.com/watch?v=f-bmZSFRsHM)
## Project Overview

`Off-World` is a unique __Action RPG with Creature Evolution__ developed in Unity. It features procedural world generation, sophisticated AI systems for various creatures, and engaging core gameplay mechanics, aiming to deliver an immersive experience in a alien environment.

## Key Features

This project showcases a range of gameplay and technical implementations:

* **Advanced AI Systems:**
    * **RockBoss AI & Animations:** Comprehensive boss behavior defined by a behavior tree, integrated with complex animation sets for specific combat sequences.
    * **Wolf AI:** Robust behavior includes pathfinding, aggressive pursuit (aggro states), searching for lost targets, and intelligent return-to-base logic based on recent movement history.
    * **Enemy AI (General & Frog):** Implementation of foundational enemy behaviors and specific AI logic for different enemy types, such as the Frog enemy.
    * **Villager System:** Dynamic villager behaviors, including chat interactions and general movement/life cycles within the game world.

* **Procedural Content Generation & Terrain:**
    * Highly optimized procedural generation for vast, explorable terrains.
    * Dynamic terrain features, tree placement (with LOD for performance).
    * Advanced terrain shaders for visual fidelity.

* **Core Gameplay Mechanics:**
    * **Player Systems:** Core player mechanics including movement, attack controller, interaction system, and robust healthbar implementation.
    * **Object Interaction:** Physics-based mechanics for grabbable objects, including specific drop force for realistic interaction.
    * **Combat & Damage:** Basic enemy and core combat functionalities.
    * **Item Management:** Integration of Blender models and systems for power items.

* **User Interface:**
    * Implementation of a functional Main Menu and Healthbar.

## Technical Stack & Development Environment

* **Engine:** Unity 2022.3.18f1
* **Language:** C#
* **Version Control:** Git

## Installation & Setup

To get a local copy up and running, follow these simple steps.

### Prerequisites

* Unity Hub installed.
* Unity Editor version 2022.3.18f1.
* Git client installed.

### Steps

1.  **Clone the repository:**
    ```bash
    git clone [https://github.com/ThatcherMcc/Off-World-Game](https://github.com/ThatcherMcc/Off-World-Game)
    cd Off-World
    ```
2.  **Open the project in Unity Hub:**
    * Open Unity Hub.
    * Click "Add" and navigate to the cloned `Off-World` directory.
    * Select the folder and click "Add Project."
    * Ensure the correct Unity Editor version is selected (as per prerequisites) and click "Open Project."
3.  **Unity will automatically import packages and generate necessary files** (like `Library/`, `Temp/`, etc., which are ignored by `.gitignore`). This might take a few minutes for the first import.

## How to Play

1.  Once the project opens in Unity Editor, navigate to `Assets/Scenes/Game.unity` (or your primary scene).
2.  Press the "Play" button in the Unity Editor to start the game.
3.  Let me know what you think

## Project Structure

.  
├── Assets/                 # All game-specific assets (scripts, models, textures, scenes, prefabs)  
├── Packages/               # Unity Package Manager manifest files (external packages referenced here)  
├── ProjectSettings/        # Core Unity project configuration files  
├── .gitignore              # Git ignore rules for Unity-specific generated files  
└── README.md               # This file  

## Current Status & Roadmap

**Current Status:**
* Core AI systems for Wolf and RockBoss are implemented.
* Procedural terrain generation is functional.
* Basic player mechanics and UI elements are in place.

**Roadmap (Future Plans):**
* Expand enemy variety and behaviors.
* Develop additional interactive elements and quests for villagers.
* Implement a full inventory.
* Optimize performance for larger world segments.
* Add sound effects and music.

## Contact

* **Thatcher M** - [https://github.com/ThatcherMcc]
* **Project Link:** [https://github.com/ThatcherMcc/Off-World-Game]
