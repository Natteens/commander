<div align="center">

# Commander

**Open a console inside the running game and ask it what is happening.**

A lightweight development console for Unity with discoverable commands, argument parsing,
autocomplete, history and a small set of runtime diagnostics.

[![Release](https://img.shields.io/github/v/release/Natteens/commander?sort=semver&label=release&style=flat-square)](https://github.com/Natteens/commander/releases)
[![Unity](https://img.shields.io/badge/Unity-2021.3%2B-000000?style=flat-square&logo=unity)](https://unity.com)
[![Input System](https://img.shields.io/badge/Input_System-1.3.0%2B-555555?style=flat-square)](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/manual/index.html)
[![License](https://img.shields.io/github/license/Natteens/commander?style=flat-square)](./LICENSE.md)

[Why Commander?](#why-commander) · [Installation](#installation) · [Quick Start](#quick-start) · [Documentation](#documentation)

</div>

---

## Why Commander?

Debug menus are useful until every new action needs another button, panel or temporary shortcut.
Commander gives development tools a common entry point instead: type a command, inspect the result
and keep the gameplay UI out of the process.

Commands can live beside the systems they operate on. The console discovers them, parses their
arguments and presents them through a searchable runtime interface, while the game remains free to
own the actual behavior.

<table>
<tr>
<td width="50%"><strong>Discoverable commands</strong><br><sub>Expose small development actions through attributes and list them from the console.</sub></td>
<td width="50%"><strong>Useful input flow</strong><br><sub>Autocomplete, history and scrollable output make repeated testing less tedious.</sub></td>
</tr>
<tr>
<td width="50%"><strong>Typed arguments</strong><br><sub>Command text is converted into supported C# and Unity value types before invocation.</sub></td>
<td width="50%"><strong>Runtime diagnostics</strong><br><sub>Built-in helpers cover common checks such as FPS, memory and time scale.</sub></td>
</tr>
</table>

## Installation

Commander requires Unity **2021.3** or newer. The Unity Input System dependency is declared by the
package.

In the Package Manager, choose **Add package from git URL** and paste:

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

For a production project, pin the URL to a release tag instead of following `main`.

## Quick Start

Add `ConsoleController` to an active GameObject, enter Play Mode and press `F1`. The `help` command
shows the commands available in the installed version.

A method becomes a command with one attribute:

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

Keep command methods narrow. Validation and game rules should remain in the systems that already own
them; Commander is the development doorway, not a second gameplay architecture.

## Documentation

The main README stays focused on what Commander is and how to try it. The detailed reference lives
in [Documentation](./Documentation~/index.md), including:

- runtime controls and built-in commands;
- custom command authoring and supported arguments;
- the main runtime components;
- build-safety notes and troubleshooting.

See the [changelog](./CHANGELOG.md) for release history.

## License

MIT. See [LICENSE.md](./LICENSE.md).
