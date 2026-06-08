# 🚀 IronBrew2 Quick Start Guide

Get up and running in 5 minutes!

## Step 1: Prerequisites Check
Make sure you have these installed:

```bash
# Check .NET Core
dotnet --version

# Check Lua tools
where luac
where luajit
```

If any are missing, install them:
- **.NET Core 3.1+**: https://dotnet.microsoft.com/download
- **Lua**: https://www.lua.org/download.html

## Step 2: Build the Project (First Time Only)

### Option A: Using Menu
```
1. Double-click: obfuscate.bat
2. Press: 2
3. Wait for build to complete
```

### Option B: Command Line
```bash
cd c:\Users\leoli\Documents\GitHub\ironbrew-2
dotnet build
```

## Step 3: Obfuscate Your First File

### Option A: Easy Menu
```
1. Double-click: obfuscate.bat
2. Press: 1
3. Enter file path: C:\path\to\your\file.lua
4. Press: 2 (for BALANCED preset)
5. Done! Check "out.lua" in the output folder
```

### Option B: Super Quick
```
1. Drag and drop your .lua file onto: quick-obfuscate.bat
2. Wait for completion
3. Your obfuscated file opens automatically
```

### Option C: Command Line
```bash
cd c:\Users\leoli\Documents\GitHub\ironbrew-2\IronBrew2\ CLI
dotnet run -- C:\path\to\your\file.lua
```

## 📝 Testing with Sample File

A test file is included at: `test-sample.lua`

Test it:
```bash
1. Drag test-sample.lua onto quick-obfuscate.bat
   OR
   Run: obfuscate.bat and use menu option 1
```

## 📂 Output

Your obfuscated file will be here:
```
IronBrew2 CLI\bin\Debug\netcoreapp3.1\out.lua
```

## 🎯 Obfuscation Presets

| Preset | Speed | Obfuscation | Best For |
|--------|-------|-------------|----------|
| **LIGHT** | ⚡ Fast | Basic | Non-critical code |
| **BALANCED** | ⚡⚡ Medium | Strong | Most code (recommended) |
| **HEAVY** | ⚡⚡⚡ Slow | Maximum | Sensitive code |

## ⚠️ Important Notes

1. **Always test** your obfuscated code before using in production
2. **Keep backups** of original source files
3. **Your code must be valid Lua 5.1**
4. **Output is standalone** - no runtime dependencies needed

## 🔧 Troubleshooting

| Problem | Solution |
|---------|----------|
| "dotnet is not recognized" | Reinstall .NET Core and restart |
| "luac not found" | Install Lua, add to PATH, restart |
| File doesn't exist | Check path is correct, use absolute path |
| Obfuscation hangs | Try LIGHT preset, check file size |
| Output doesn't work | Test original file first, ensure Lua 5.1 compatible |

## 💡 Next Steps

- Read `BAT_FILES_GUIDE.md` for detailed file documentation
- Check `README.md` for technical details
- Explore `ObfuscationSettings.cs` to customize settings
- See `TEST_SAMPLE.lua` for example code

## 🎉 You're Ready!

Your IronBrew2 obfuscator is ready to use. Pick your preferred method and start obfuscating!

**Recommended for beginners:** Use `obfuscate.bat` menu option 1 with BALANCED preset.

---

Questions? Check the included documentation files or visit the GitHub repository.
