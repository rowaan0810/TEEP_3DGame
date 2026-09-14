# EndlessRunner

A simplified endless runner inspired by Subway Surfers, built in Unity 6 as a TEEP project.

## Features
- **3-Lane Running**: Dodge obstacles by switching lanes, jumping, and ducking
- **Webcam Pose Control**: Use your body to play — step, jump, and crouch (coming Week 3)
- **Easy Mode**: Reduced movement for elderly/low-mobility users (coming Week 3)
- **AI Pose Challenges**: Match random poses mid-run for power-ups (coming Week 4)
- **Keyboard Controls**: Full arrow key / WASD support

## Controls (Keyboard)
| Key | Action |
|-----|--------|
| ←/A | Move left lane |
| →/D | Move right lane |
| ↑/W/Space | Jump |
| ↓/S | Duck/Roll |
| R | Restart (after game over) |
| Escape | Quit |

## Quick Start
1. Open the project in Unity 6 (6000.4.7f1)
2. Open `Assets/Scenes/MainScene` (or any empty scene)
3. Create an empty GameObject and add the `SceneSetup` component
4. Press Play — the scene auto-builds and the game starts

## Project Structure
```
Assets/
├── Scripts/
│   ├── Core/           # GameManager, GameSpeed
│   ├── Input/          # IGameInput, KeyboardInput, InputManager
│   ├── Player/         # PlayerController, PlayerCollision, PlayerAnimator
│   ├── Track/          # TrackManager, TrackSegment
│   └── SceneSetup.cs   # Auto-creates scene hierarchy
├── Prefabs/
├── Scenes/
└── Materials/
```

## Development Timeline
- **Week 1** (Sep 14–21): Core runner mechanics, keyboard input
- **Week 2** (Sep 22–28): Obstacles, scoring, UI, game loop
- **Week 3** (Sep 29–Oct 5): MediaPipe pose control, Easy Mode
- **Week 4** (Oct 6–9): AI Pose Challenges, polish
- **Weeks 5–6** (Oct 10–25): AI-generated 3D assets, extra features
