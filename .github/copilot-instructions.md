# Antarctic Civil War - AI Coding Agent Instructions

## Project Overview
Antarctic Civil War is a Unity 2D tower defense/RTS game featuring penguins in combat scenarios. The game uses Firebase for backend data, custom pathfinding, and a unit-based combat system with ScriptableObject-driven design patterns.

## Core Architecture

### Singleton Pattern Usage
- Most managers extend `SingleTonBehaviour<T>` (note typo in class name)
- Key singletons: `UnitManager`, `TilemapManager`, `GameManager`, `InventoryManager`
- Access pattern: `UnitManager.instance.methodName()`

### Unit System Architecture
- **Unit.cs**: Base unit class containing all core components (data, controller, mover, etc.)
- **UnitData.cs**: ScriptableObject defining unit stats with tier-based arrays for HP/damage
- **UnitController.cs**: Handles FSM states (Idle, Follow, Attack, Die, Skill)
- **UnitManager.cs**: Central manager for ally/enemy lists, spawning, and item effects

### Pathfinding System
- Custom A* implementation in `Assets/Scripts/Manager/PathFinding/`
- **AstarPathfinder.cs**: Grid-based pathfinding with collision detection
- **AstarMover.cs**: Movement component using pathfinding results
- **MinHeap.cs**: Priority queue for A* algorithm
- Uses 2D physics for collision detection with configurable layer masks

### Data Management
- **Firebase Integration**: `FirebaseManager.cs` loads items and units from Firestore
- **ItemDB/UnitDB**: Data classes for Firebase objects
- **ScriptableObjects**: Used for unit data, status effects, and unit groups
- Always use `await FirebaseManager.ItemLoadData()` and `UnitLoadData()` before accessing data

## Development Patterns

### Component Communication
- Units use composition: Unit → UnitController → various components
- FSM pattern for unit states with string-based state switching
- Event-driven architecture for combat target assignment

### Scene Management
- **MainScene.unity**: Primary gameplay scene
- **LobbyScene.unity**: Unit selection/preparation
- **TitleScene.unity**: Main menu
- Tilemap-based level design with spawn point management

### Event System Architecture
- **StageEventCandidate.cs**: Base class for random story events with choice system
- **Individual Event Prefabs**: Each event is a separate MonoBehaviour prefab that can override base behavior
- **EventManager.cs**: Singleton managing event UI, integrates with StageRoundManager
- **EventResultUI.cs**: Displays choice outcomes and rewards
- **StageRoundManager.cs**: Controls round progression, calls EventManager for Event rounds
- Events use choice-based branching with rewards (gold, units, items, health changes)
- Combat/Shop/Rest rounds handled separately in StageRoundManager

### UI Architecture
- UI organized by feature in `Assets/Scripts/UI/`
- Market system for unit purchasing
- Inventory system with passive item effects
- Reward system using UnitGroupSO for grouped unit rewards

## Key Conventions

### Naming Patterns
- Korean comments frequently used alongside English code
- File organization: `Manager/`, `Unit/`, `UI/` as primary directories
- Prefab naming: `pf` prefix (e.g., `pfUnit`, `pfPeddler`)

### Data Structures
- Unit stats use arrays indexed by tier level: `unitHP[tierLevel]`
- Faction system: Royal, Resistance, Mercenary, Boss
- Tier types: Normal, Special, Minion, Boss

### State Management
- String-based state switching: "IdleState", "AttackState", "FollowState"
- Combat phases controlled by `FightState.cs` button system
- Boss spawning timer system in `GameManager.cs`

## External Dependencies
- **Firebase SDK**: For data persistence and synchronization
- **A* Pathfinding Project**: Custom implementation (not external package)
- **External Dependency Manager**: For Unity package management

## Common Workflows
- Unit spawning: Create prefab → Register with UnitManager → Apply item effects
- Combat flow: Assign targets → Change states → FSM handles behavior
- Item effects: Apply to existing units + auto-apply to new units
- Tilemap validation: Check walkable tiles before spawning/moving

## Important Files to Reference
- `Assets/Scripts/Player/UnitManager.cs`: Central unit coordination
- `Assets/Scripts/Manager/TilemapManager.cs`: Spatial management
- `Assets/Scripts/Firebase/FirebaseManager.cs`: Data loading patterns
- `Assets/Scripts/Data/UnitData.cs`: Core data structure definitions
- `Assets/Scripts/Unit/UnitControll/Default/Unit.cs`: Base unit implementation
- `Assets/Scripts/Stage/StageRoundManager.cs`: Main stage progression controller
- `Assets/Scripts/Stage/EventManager.cs`: Event UI management, integrates with StageRoundManager
- `Assets/Scripts/Stage/StageEventCandidate.cs`: Base class for individual event prefabs
- `Assets/Scripts/Stage/Events/`: Individual event prefab scripts (AncientRelicEvent, etc.)

## Build Configuration
- Unity project with iOS/Android build support
- Firebase configuration required for data access
- External Dependency Manager handles package resolution