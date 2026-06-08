# IronBrew2 Batch File Guide

Two convenient batch files have been created to make running the obfuscator easy:

## 📋 Files

### 1. `obfuscate.bat` - Full-Featured Menu
**The main launcher with an interactive menu system.**

#### How to Use:
- **Double-click** `obfuscate.bat` from Windows Explorer
- A colorful menu will appear with options
- Follow the on-screen prompts

#### Features:
- ✅ Interactive menu with 5 options
- ✅ Drag-and-drop support (paste file path)
- ✅ Multiple obfuscation presets (HEAVY, BALANCED, LIGHT)
- ✅ Build project directly from menu
- ✅ Open output folder automatically
- ✅ Built-in help documentation
- ✅ Error checking and validation
- ✅ Color-coded messages (green for success, red for errors)

#### Menu Options:
```
[1] Obfuscate a Lua file     - Full obfuscation with preset selection
[2] Build the project first  - Compile the C# project
[3] Open output folder       - View generated files
[4] View help/documentation  - Learn how to use the tool
[5] Exit                     - Close the program
```

#### Example Workflow:
```
1. Start obfuscate.bat
2. Choose option [1]
3. Paste path to your file: C:\Users\YourName\game.lua
4. Choose preset [2] for BALANCED
5. Wait for completion
6. Your file is now obfuscated as "out.lua"
```

---

### 2. `quick-obfuscate.bat` - Instant Obfuscation
**Fast drag-and-drop launcher for quick obfuscation.**

#### How to Use:
**Option A: Drag and Drop**
- Drag a `.lua` file onto `quick-obfuscate.bat` in Windows Explorer
- Obfuscation starts immediately
- Output folder opens automatically when done

**Option B: Command Line**
```batch
quick-obfuscate.bat C:\path\to\your\file.lua
```

#### Features:
- ⚡ Instant processing (no menu)
- ✅ Simple drag-and-drop interface
- ✅ Auto-opens output folder on success
- ✅ Error handling with clear messages
- ✅ Uses BALANCED preset automatically

#### Best For:
- Quick obfuscation of single files
- Workflow integration
- Batch processing scripts

---

## 🎯 Which One Should I Use?

### Use `obfuscate.bat` if you:
- Want to build the project first
- Need to try different obfuscation presets
- Want access to help documentation
- Prefer an interactive menu
- Are new to the tool

### Use `quick-obfuscate.bat` if you:
- Already have the project built
- Just want to obfuscate quickly
- Prefer drag-and-drop
- Are familiar with the tool
- Want minimal steps

---

## 🔧 Requirements

Before using these batch files, ensure:
1. **.NET Core 3.1+** is installed
2. **Lua tools** are installed:
   - `luac` - Lua compiler
   - `luajit` - Lua JIT compiler
3. **Project is built** (or use `obfuscate.bat` option [2] to build)

### Check Requirements:
```batch
dotnet --version
where luac
where luajit
```

---

## 📁 File Structure

After obfuscation completes:
```
IronBrew2 CLI/
├── bin/Debug/netcoreapp3.1/
│   ├── out.lua              ← Your obfuscated file
│   ├── temp/                ← Temporary files (can delete)
│   │   ├── t0.lua
│   │   ├── t1.lua
│   │   ├── t2.lua
│   │   └── t3.lua
│   └── ...
└── ...
```

**Important:** `out.lua` is your final obfuscated file. Copy it wherever you need it.

---

## 🐛 Troubleshooting

### Error: "File not found"
- ✅ Check the file path is correct
- ✅ Make sure the file is a `.lua` file
- ✅ Use absolute paths (full path from C:\...)

### Error: "dotnet is not recognized"
- ✅ Install .NET Core 3.1+ from https://dotnet.microsoft.com/
- ✅ Restart your computer after installation

### Error: "luac not found" or "luajit not found"
- ✅ Install Lua from https://www.lua.org/download.html
- ✅ Make sure the bin folder is in your Windows PATH

### Obfuscation hangs or takes forever
- ✅ Your file might be very large
- ✅ Try using the LIGHT preset
- ✅ Check if your Lua code has syntax errors

### Output file doesn't work
- ✅ Test with a simple script first
- ✅ Make sure your original .lua file runs fine
- ✅ Check for Lua 5.1 compatibility (some 5.2+ features not supported)

---

## 💡 Tips

1. **Always test obfuscated code** before deploying to production
2. **Keep backups** of your original source files
3. **Use BALANCED preset** unless you have a specific reason not to
4. **For very large files**, use LIGHT preset to save time
5. **Verify output** works the same as input before using in production

---

## 🎨 Color Meanings

When using `obfuscate.bat`:
- 🟢 **Green** = Success message
- 🔴 **Red** = Error message
- ⚪ **Gray** = Normal text
- 🟡 **Yellow** = Highlight/Important

---

## 📝 Notes

- These batch files are Windows-only (use Linux/Mac terminal with `dotnet run -- file.lua`)
- Both batch files can be run from any directory
- You can create shortcuts to these batch files for easier access
- To customize obfuscation settings, edit `ObfuscationSettings.cs`

---

## 🚀 Getting Started

**First time setup:**
```batch
1. Run: obfuscate.bat
2. Choose option [2] to build
3. Choose option [1] to obfuscate your first file
4. Select BALANCED preset
```

That's it! You're ready to obfuscate Lua code! 🎉
