# ⚙️ IronBrew2 Settings & Customization Guide

For advanced users who want to customize obfuscation behavior.

## 📍 Settings Location

File: `IronBrew2\Obfuscator\ObfuscationSettings.cs`

## 🎛️ Available Settings

### 1. **EncryptStrings** (bool)
**Default:** `true`

Encrypts all string constants in your code.

```
Example:
  Before: print("Hello World")
  After:  print(decrypt_string_1234)
```

**Performance Impact:** Medium
**Security Impact:** High
**Use Case:** Always enabled by default - critical for obfuscation

---

### 2. **EncryptImportantStrings** (bool)
**Default:** `true`

Specifically targets and encrypts important strings like function names, variable references.

**Performance Impact:** Low
**Security Impact:** High
**Recommendation:** Keep enabled

---

### 3. **ControlFlow** (bool)
**Default:** `true`

Obfuscates control flow by:
- Randomizing jump targets
- Adding fake conditionals
- Scrambling loop structures

```
Example:
  Before: if x > 5 then do_something() end
  After:  Complex jumps and conditions that achieve same result
```

**Performance Impact:** High
**Security Impact:** Very High
**Recommendation:** Keep enabled for sensitive code, disable for performance-critical code

---

### 4. **BytecodeCompress** (bool)
**Default:** `true`

Compresses the final bytecode output.

**Performance Impact:** None (saves space)
**Security Impact:** Minimal
**Recommendation:** Always keep enabled

---

### 5. **DecryptTableLen** (int)
**Default:** `500`

Controls the size of the decryption table. Larger = more complex decryption.

**Range:** 100 - 10000
**Performance Impact:** Higher value = slower runtime
**Security Impact:** Higher value = harder to reverse engineer

**Examples:**
```
100  - Small (fast, but weak)
500  - Medium (balanced - DEFAULT)
1000 - Large (slower, stronger)
```

**Recommendation:** 
- Performance-critical code: 100-200
- Balanced: 500-700
- Maximum security: 1000+

---

### 6. **PreserveLineInfo** (bool)
**Default:** `true`

Keeps debug line information in the bytecode.

**Performance Impact:** None
**Security Impact:** None (debug info removed in release builds)

**Use Case:**
- Keep enabled during development for debugging
- Disable in production for slightly smaller files

---

### 7. **Mutate** (bool)
**Default:** `true`

Mutates opcodes to use custom implementations.

**Examples:**
- Convert `a + b` to custom addition function
- Replace `a == b` with custom equality check

**Performance Impact:** Medium
**Security Impact:** High
**Recommendation:** Keep enabled

---

### 8. **SuperOperators** (bool)
**Default:** `true`

Combines multiple operations into single "super operations".

```
Example:
  Before: x = a + b; y = x * 2; print(y)
  After:  combined_operation_1(a, b)
```

**Performance Impact:** High
**Security Impact:** Very High
**Recommendation:** Keep enabled for sensitive code

---

### 9. **MaxMiniSuperOperators** (int)
**Default:** `120`

Maximum number of small combined operations.

**Range:** 1 - 500
**Performance Impact:** Higher = slower
**Security Impact:** Higher = better obfuscation

**Examples:**
```
50   - Light obfuscation (fast)
120  - Balanced (DEFAULT)
250+ - Heavy obfuscation (slow)
```

---

### 10. **MaxMegaSuperOperators** (int)
**Default:** `120`

Maximum number of large combined operations.

**Range:** 1 - 500
**Performance Impact:** Higher = slower
**Security Impact:** Higher = better obfuscation

---

### 11. **MaxMutations** (int)
**Default:** `200`

Maximum number of mutations applied to opcodes.

**Range:** 1 - 500
**Performance Impact:** Higher = slower
**Security Impact:** Higher = better obfuscation

**Examples:**
```
50   - Light mutations (fast)
200  - Balanced (DEFAULT)
500  - Maximum mutations (very slow)
```

---

## 🎯 Preset Configurations

### LIGHT Preset
Use for non-critical code or performance-sensitive applications:

```csharp
EncryptStrings = true;
EncryptImportantStrings = false;
ControlFlow = false;
BytecodeCompress = true;
DecryptTableLen = 100;
PreserveLineInfo = true;
Mutate = false;
SuperOperators = false;
MaxMiniSuperOperators = 20;
MaxMegaSuperOperators = 20;
MaxMutations = 50;
```

**Result:** Fast, small files, basic obfuscation

---

### BALANCED Preset
Use for most applications (RECOMMENDED):

```csharp
EncryptStrings = true;
EncryptImportantStrings = true;
ControlFlow = true;
BytecodeCompress = true;
DecryptTableLen = 500;
PreserveLineInfo = true;
Mutate = true;
SuperOperators = true;
MaxMiniSuperOperators = 120;
MaxMegaSuperOperators = 120;
MaxMutations = 200;
```

**Result:** Good balance of speed and security

---

### HEAVY Preset
Use for sensitive code or when reverse-engineering protection is critical:

```csharp
EncryptStrings = true;
EncryptImportantStrings = true;
ControlFlow = true;
BytecodeCompress = true;
DecryptTableLen = 1000;
PreserveLineInfo = false;
Mutate = true;
SuperOperators = true;
MaxMiniSuperOperators = 300;
MaxMegaSuperOperators = 300;
MaxMutations = 500;
```

**Result:** Maximum obfuscation, slower execution

---

## 🔧 How to Apply Custom Settings

### Step 1: Open the Settings File
```
File: IronBrew2\Obfuscator\ObfuscationSettings.cs
```

### Step 2: Edit the Constructor
In the `ObfuscationSettings()` method, modify the values:

```csharp
public ObfuscationSettings()
{
    EncryptStrings = true;              // ← Modify here
    EncryptImportantStrings = true;     // ← Or here
    ControlFlow = true;
    BytecodeCompress = true;
    DecryptTableLen = 500;              // ← Change number
    PreserveLineInfo = true;
    Mutate = true;
    SuperOperators = true;
    MaxMiniSuperOperators = 120;
    MaxMegaSuperOperators = 120;
    MaxMutations = 200;
}
```

### Step 3: Rebuild
```bash
dotnet build
```

### Step 4: Use the Custom Settings
Run obfuscation normally - it will use your custom settings.

---

## 📊 Performance vs Security Trade-offs

| Setting | High Value | Low Value | Impact |
|---------|-----------|-----------|--------|
| `DecryptTableLen` | Secure | Fast | Runtime |
| `MaxMutations` | Secure | Fast | Obfuscation |
| `MaxMiniSuperOperators` | Secure | Fast | Both |
| `ControlFlow` | Secure | Fast | Obfuscation |
| `SuperOperators` | Secure | Fast | Both |

---

## 💡 Optimization Tips

### For Maximum Speed:
```csharp
ControlFlow = false;
SuperOperators = false;
Mutate = false;
DecryptTableLen = 100;
MaxMutations = 50;
```

### For Maximum Security:
```csharp
ControlFlow = true;
SuperOperators = true;
Mutate = true;
DecryptTableLen = 1000;
MaxMutations = 500;
PreserveLineInfo = false;
```

### For Balanced Approach (DEFAULT):
Keep default values - they're optimized for general use.

---

## 🧪 Testing Custom Settings

After modifying settings:

1. **Rebuild the project:**
   ```bash
   dotnet build
   ```

2. **Test with sample file:**
   ```bash
   dotnet run -- test-sample.lua
   ```

3. **Verify output runs correctly:**
   ```bash
   lua out.lua
   ```

4. **Check file size:**
   ```
   out.lua size in bytes
   ```

5. **Compare performance:**
   ```bash
   time lua out.lua
   ```

---

## ⚠️ Important Notes

1. **All settings are applied globally** - they affect all obfuscations until you rebuild
2. **Always test** obfuscated output before deploying
3. **Settings are persistent** - they stay until you change them again
4. **Higher values don't always mean better** - test for your specific use case
5. **Default BALANCED preset** is recommended for most users

---

## 🔍 Comparing Obfuscations

To compare different settings:

1. Note down current settings
2. Save your test file
3. Obfuscate it (creates `out.lua`)
4. Move `out.lua` to a backup location
5. Change settings in code
6. Rebuild and obfuscate again
7. Compare file sizes and performance

---

## 📚 Advanced Topics

### Custom Encryption Keys
*(Currently not exposed in settings - would require modifying `ConstantEncryption.cs`)*

### Runtime Performance Profiling
Enable Lua profiling to measure impact of obfuscation settings:
```lua
-- In your obfuscated code
local start = os.clock()
-- your code here
local elapsed = os.clock() - start
print("Time: " .. elapsed .. " seconds")
```

### Reverse Engineering Prevention
For maximum protection against reverse engineering:
- Enable all encryption options
- Use HEAVY preset
- Use large `DecryptTableLen` values
- Keep `PreserveLineInfo = false`

---

Questions? Check QUICK_START.md or BAT_FILES_GUIDE.md for more information.
