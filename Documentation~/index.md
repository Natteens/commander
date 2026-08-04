# Commander documentation

Commander is an in-game command console for Unity. This page covers the parts that are useful after the initial setup.

## Setup

1. Add `ConsoleController` to a GameObject in the scene.
2. Enter Play Mode.
3. Press `F1` to open or close the console.
4. Run `help` to see the commands available in the current build.

Commander uses the Unity Input System. The package declares the compatible dependency in `package.json`.

## Runtime controls

| Input | Action |
| --- | --- |
| `F1` | Open or close the console |
| `Tab` | Complete the current command |
| `Up` / `Down` | Browse command history |
| `Page Up` / `Page Down` | Scroll the log |
| Mouse wheel | Scroll the log |

## Built-in commands

The package includes common development commands such as:

- `help` to list commands.
- `clear` to clear the console output.
- `fps` and `memory` to toggle diagnostic overlays.
- `time` to change the game time scale.
- `list` to inspect objects available to the console.

The exact list is reported by `help`, which should be treated as the source of truth for the installed version.

## Custom commands

Methods can be exposed through the command attribute:

```csharp
using Commander;
using UnityEngine;

public sealed class PlayerCommands : MonoBehaviour
{
    [Command("heal", "Restore player health")]
    public void Heal(float amount = 50f)
    {
        Debug.Log($"Heal requested: {amount}");
    }
}
```

Keep command methods small. They should validate their input and delegate real game logic to the appropriate system instead of becoming a second gameplay API.

## Main components

- `ConsoleController` owns the runtime console lifecycle.
- `CommandRegistry` stores the available commands.
- `CommandExecutor` resolves and invokes commands.
- `ParameterParser` converts command text into supported parameter types.
- `ConsoleUI` presents input, history, suggestions and output.

## Build use

A development console can expose sensitive game state. Review which commands are included in release builds and keep any authentication or build-gating rules appropriate for the project.

## Troubleshooting

- **The console does not open:** confirm the Input System dependency resolved and the controller is active.
- **A command is missing:** run `help`, then verify the target object and command registration are active.
- **Arguments fail to parse:** check the method signature and use a supported parameter type.

See the package source for the exact public API available in the installed version.