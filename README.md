# Cubic Chess

A three-dimensional chess game built with Unity, featuring innovative 4×4×4 cubic gameplay mechanics that extend traditional chess into 3D space.

<img width="1672" height="936" alt="063589ae28a159028881faaad5179686" src="https://github.com/user-attachments/assets/4afde8b5-d43c-4f35-8a57-0be4f76ee663" />

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

## 🎨 Customization

later haha

## 📝 License

I'll add this later.

---

*Experience chess in a whole new dimension!* 🎲♟️
