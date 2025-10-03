# Cubic Chess

A three-dimensional chess game built with Unity, featuring innovative 4×4×4 cubic gameplay mechanics that extend traditional chess into 3D space.


https://github.com/user-attachments/assets/7dd87901-cfab-4abe-a92f-2b6e4a4f0f98


<img width="706" height="400" alt="image" src="https://github.com/user-attachments/assets/38db16c2-2237-4213-bf3c-625f5a3dbf84" />

<img width="836" height="468" alt="063589ae28a159028881faaad5179686" src="https://github.com/user-attachments/assets/4afde8b5-d43c-4f35-8a57-0be4f76ee663" />
<img width="836" height="455" alt="c5aaf419d6d07c1da05b2fe92de92c8c" src="https://github.com/user-attachments/assets/952c4754-3eba-49e0-8051-08c27b1d9f69" />


The game is still under development and subject to incompleteness during gameplay. The **Hard** difficulty is the same as **medium**. **Very happy if anyone wish to contribute/connect**

## 🚀 Future Vision: A Reinforcement Learning AI

I wish the next step for this game's development is to develop a **AI bot** to play this game through reinforcement learning: creating an AI that learns to master the game entirely through self-play.

This involves training a neural network using reinforcement learning, allowing the AI to discover complex, non-obvious strategies and achieve a level of play beyond what can be achieved with hand-coded rules.

### The Roadmap
The plan to achieve this is broken down into the following key stages:

1.  **Unity ML-Agents Integration**: The project will be integrated with the official **Unity ML-Agents Toolkit**, which provides the framework for training intelligent agents in a game environment.
2.  **Defining the RL Environment**: We will configure the game to serve as a training environment. This involves defining the core components:
    * **Observations**: The board state will be converted into a numerical vector that the neural network can understand.
    * **Actions**: The AI's decisions (which piece to move and where) will be defined as the action space.
    * **Rewards**: A simple and powerful reward signal will be used: `+1` for winning a game, `-1` for losing, and `0` for all other moves.
3.  **Self-Play Training**: The core of the project will be to create a robust self-play training loop. The AI will play against itself millions of times, gradually improving from random moves to sophisticated, grandmaster-level strategy.
4.  **Model Integration**: The final trained model (`.onnx` file) will be integrated back into the game as a new, formidable AI opponent in the "AI Robot" mode.

### 🤝 Join the Development!

This is an ambitious but incredibly exciting project. I am actively looking for collaborators who are passionate about the intersection of game development and artificial intelligence. If you have experience or a strong interest in any of the following areas, I would love to hear from you:

* **Reinforcement Learning (RL)**
* **Unity ML-Agents Toolkit**
* **Game AI Architecture**
* **Python (for training) & C# (for game logic)**

If you'd like to contribute, share ideas, or just discuss the future of 3D chess AI, please reach out to me, Kaicheng Liang, at **kaicheng@andrew.cmu.edu**. Happily!

## 🎮 Features

### Game Modes
- **Local 2-Player**: Play against a friend on the same device with automatic camera switching between turns
- **AI Robot**: Challenge an AI opponent with configurable difficulty and response time

### 3D Chess Mechanics
- **4×4×4 Cubic Board**: Traditional chess reimagined in three dimensions
- **Enhanced Piece Movement**: All pieces can move in 3D space while maintaining their core movement patterns
- **Pawn Promotion**: Pawns can promote when reaching the opposite edge of any face

### Visual Features
- **Dynamic Camera System**: Smooth camera transitions and user-controlled rotation
- **Visual Indicators**: 
  - Highlighted possible moves (green cubes)
  - Attack possibilities (red cubes) 
  - Check warnings (warning cubes)
- **Piece Animations**: Smooth movement animations and eating transitions
- **3D Grid System**: Clear visual representation of the cubic chess board

## 🎯 How to Play

### Running the Game

Pre-built versions are available in the `output/` directory:
- `Cubic Chess_win.zip` - Windows build
- `Cubic Chess_macOS.zip` - macOS build

### Basic Rules
3D Chess follows traditional chess rules but extended into three dimensions:

1. **Piece Movement**: 
   - **Rook**: Moves in straight lines along X, Y, or Z axes
   - **Bishop**: Moves diagonally across XY, XZ, or YZ planes
   - **Queen**: Combines Rook and Bishop movements
   - **King**: Moves one space in any direction
   - **Knight**: L-shaped moves extended to 3D
   - **Pawn**: Moves forward in Y-axis, attacks diagonally

2. **Special Mechanics**:
   - **Board Revolution**: Players can rotate the entire board during their turn
   - **3D Check**: Kings must avoid threats from all three dimensions
   - **Promotion**: Pawns promote when reaching any edge of the cube

### Controls
- **Mouse**: 
  - Click and drag to rotate camera
  - Click to select pieces and moves
- **Keyboard**:
  - ADWS for camera rotation
  - RF for zooming

## 🛠️ Technical Details

### Architecture
The game uses a modular architecture with separate managers for different systems:

- **BoardManager**: Core game logic, move validation, and board state
- **GameManager_Local2P**: Handles local two-player gameplay
- **GameManager_Robot**: Manages AI opponent functionality
- **CameraManager**: Controls camera movement and transitions
- **CubeManager**: Manages visual indicators and board highlighting
- **GridManager**: Handles the 3D grid visualization
- **UIManager**: User interface and game state displays

## 🚀 Installation & Setup

### Requirements
- Unity 2022.3 LTS or later
1. Clone or download the repository
2. Open the project in Unity
3. Load the main scene from `Assets/Scenes/`

## 📁 Project Structure

```
Assets/
├── Scripts/           # Core game logic
│   ├── Board Manager.cs      # Main game state and move logic
│   ├── Game Manager_*.cs     # Game mode controllers  
│   ├── Camera Manager.cs     # Camera control system
│   ├── Cube Manager.cs       # Visual indicators
│   └── Moveable Object.cs    # Chess piece behavior
├── Scenes/            # Unity scenes
├── Resources/         # Game assets and prefabs
├── Materials and Shaders/    # Visual materials
└── Settings/          # Game configuration
```

## 📝 License

Under application.

---

*Experience chess in a whole new dimension!* 🎲♟️
