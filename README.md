<div align="center">

# Commander

A lightweight in-game command console for Unity.

[![Release](https://img.shields.io/github/v/release/Natteens/commander?style=flat-square)](https://github.com/Natteens/commander/releases)
[![Unity](https://img.shields.io/badge/Unity-2021.3%2B-000000?style=flat-square&logo=unity)](https://unity.com)
[![License](https://img.shields.io/github/license/Natteens/commander?style=flat-square)](LICENSE.md)

</div>

Commander adds a runtime console for development builds without tying commands to a specific game architecture. It includes command discovery, argument parsing, history, autocomplete and simple diagnostic overlays.

## Features

- Attribute-based custom commands.
- Command history and autocomplete.
- Built-in FPS and memory diagnostics.
- Unity Input System support.

## Requirements

- Unity 2021.3 or newer.
- Unity Input System 1.3.0 or newer.

## Installation

Add the package through `Window > Package Manager > Add package from git URL`:

```text
https://github.com/Natteens/commander.git
```

Or add it to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "commander": "https://github.com/Natteens/commander.git"
  }
}
```

## Quick start

1. Add `ConsoleController` to a GameObject.
2. Enter Play Mode and press `F1`.
3. Run `help` to list the commands available in the current build.

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

## Documentation

Setup details, controls and command authoring are available in [Documentation](Documentation~/index.md).

## License

MIT. See [LICENSE.md](LICENSE.md).