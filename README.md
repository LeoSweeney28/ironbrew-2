# IronBrew2

VM-based obfuscator for Lua 5.1. Takes a Lua script, compiles it to bytecode,
and re-emits it as a custom virtual machine (written in Lua) that interprets
an obfuscated/encrypted form of that bytecode.

## Pipeline

1. `luac` compiles the input script and checks it's valid Lua 5.1.
2. `luajit` + the bundled `Lua/Minifier` (luasrcdiet) strips comments.
3. String constants are optionally encrypted (`ConstantEncryption`).
4. The script is recompiled, deserialized into an IR `Chunk`, and (optionally)
   passed through control-flow obfuscation (`Obfuscator/Control Flow`).
5. `Generator` emits a Lua VM with the obfuscated bytecode embedded
   (XOR or AES-128-CBC encrypted, depending on settings).
6. The result is minified again and written out with a watermark header.

## Building

Requires the .NET SDK (targets `netcoreapp3.1`):

```
dotnet build IronBrew2_Core.sln
```

## Usage

```
cd "IronBrew2 CLI/bin/Debug/netcoreapp3.1"
./"IronBrew2 CLI" <input.lua> [options]
```

Options:

- `--preset <light|balanced|heavy>` — obfuscation strength (default: `balanced`)
- `--output, -o <file>` — output path (default: `out.lua`)
- `--stats, -s` — print size/compression stats
- `--no-validate` — skip syntax validation of the generated output
- `--presets` — describe the available presets
- `--help, -h` — usage info

`obfuscate.bat` / `quick-obfuscate.bat` provide a simple drag-and-drop launcher
on Windows.

## Project layout

- `IronBrew2/` — core library (bytecode reader/writer, IR, obfuscation passes,
  VM generator, cryptography helpers)
- `IronBrew2 CLI/` — command-line front end
- `Lua/Minifier/` — vendored `luasrcdiet` used to strip comments/whitespace
- `Tests/` — Lua scripts used as obfuscation/regression fixtures
