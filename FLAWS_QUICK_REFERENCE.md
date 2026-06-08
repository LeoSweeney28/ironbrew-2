# IronBrew2 Security Flaws - Quick Reference

## The 10 Core Flaws (Ranked by Severity)

### 🔴 CRITICAL - Can be broken in seconds/minutes

#### 1. **Single-Byte XOR Encryption**
- **What:** All bytecode encrypted with 8-bit key (0-255 only)
- **How to Break:** Brute force all 256 keys (< 1ms)
- **Where:** `ObfuscationContext.cs:68`
- **Impact:** Entire bytecode exposed

#### 2. **String Decryption Key Embedded in Code**
- **What:** XOR table visible in generated Lua output
- **How to Break:** Copy the XOR table from source code
- **Where:** `ConstantEncryption.cs:28`
- **Impact:** All strings instantly decrypted

#### 3. **No Integrity Protection**
- **What:** VMIntegrityCheck class is completely empty
- **How to Break:** Modify bytecode with no detection
- **Where:** `Encryption/VMIntegrityCheck.cs`
- **Impact:** Code can be injected/modified undetected

#### 4. **Weak Key Generation**
- **What:** Limited entropy (only 256 possibilities)
- **How to Break:** Test all 256 keys
- **Where:** `SecureRandom.cs:22`
- **Impact:** All possible keys can be found instantly

---

### 🟠 HIGH - Can be broken in minutes/hours

#### 5. **Weak VM Dispatch**
- **What:** Opcode mapping reconstructible from bytecode structure
- **How to Break:** Pattern analysis of instruction operations
- **Where:** `Generator.cs:547-582`
- **Impact:** VM structure and instruction mapping recovered

#### 6. **No Metamorphic Protection**
- **What:** Same VM structure used for all obfuscations
- **How to Break:** One analysis defeats all binaries
- **Where:** Entire VM structure is static
- **Impact:** Single tool can deobfuscate ANY IronBrew2 output

#### 7. **Weak Control Flow Obfuscation**
- **What:** Condition flips and jump patterns are obvious
- **How to Break:** Trace the inverted jumps
- **Where:** `Control Flow/Types/*.cs`
- **Impact:** Logic can be reconstructed

---

### 🟡 MEDIUM - Additional weaknesses

#### 8. **Limited Mutations**
- **What:** Only register permutation, not semantic changes
- **How to Break:** Dataflow analysis
- **Where:** `Generator.cs:100-126`
- **Impact:** Mutations easily reversed

#### 9. **Optional String Encryption**
- **What:** Can be disabled or uses weak encryption
- **How to Break:** Strings visible or easily decrypted
- **Where:** `ConstantEncryption.cs:140`
- **Impact:** Strings may be plaintext

#### 10. **Poor Code Quality**
- **What:** Incomplete implementations, untested features
- **How to Break:** Features don't work as intended
- **Where:** Multiple files (StrEncrypt.cs, Bounce.cs)
- **Impact:** Unreliable obfuscation

---

## Quick Attack Guide

### For Attackers: How to Defeat IronBrew2

```python
# Step 1: Decrypt bytecode (< 1 second)
for key in range(256):
    decrypted = bytes(b ^ key for b in bytecode)
    if decrypted.startswith(b'\x1bLua'):
        print(f"Key: {key}")
        break

# Step 2: Extract strings (< 1 second)  
# Read the XOR table from generated code (it's visible!)
xor_table = extract_xor_table_from_lua_code()
for i, encrypted_char in enumerate(encrypted_string):
    plaintext[i] = encrypted_char ^ xor_table[i % len(xor_table)]

# Step 3: Decompile bytecode (< 1 minute)
# Use standard Lua decompiler (e.g., luadec)
decompile(decrypted_bytecode)

# Step 4: Analyze control flow (< 5 minutes)
# Trace inverted jumps and condition flips
analyze_control_flow(decompiled_code)

# Result: Complete source code recovered
```

### For Defenders: What to Do

1. **Don't use IronBrew2 for security-critical code**
2. **Don't store secrets in obfuscated bytecode**
3. **Don't rely on it for license key protection**
4. **Consider proper encryption instead of obfuscation**
5. **If you must use it, add your own encryption layer on top**

---

## Technical Summary

| Issue | Type | Effort to Fix | Value Added |
|-------|------|---------------|-------------|
| 8-bit XOR | Architecture | Medium | High |
| Embedded keys | Design | Medium | High |
| No HMAC | Missing feature | Low | High |
| Weak keys | Implementation | Low | Low |
| Predictable dispatch | Architecture | High | Medium |
| Static VM structure | Architecture | High | High |
| Weak control flow | Implementation | High | Medium |
| Limited mutations | Implementation | Medium | Low |

---

## Severity Comparison

**What IronBrew2 CAN do:**
- Make code less readable to humans
- Add noise to code
- Obfuscate variable names
- Scramble instruction order

**What IronBrew2 CANNOT do:**
- Protect proprietary algorithms
- Prevent code inspection
- Secure license keys
- Prevent tampering
- Protect against automated tools
- Resist cryptanalysis

---

## Bottom Line

**IronBrew2 is good for:** Light obfuscation against casual analysis

**IronBrew2 is NOT good for:** Any actual security requirement

Any determined reverse engineer can defeat IronBrew2 in:
- **Minutes:** Extract strings and bytecode
- **Hours:** Recover control flow
- **Days:** Reverse engineered source code

The gap between "obfuscation" and "security" is huge. IronBrew2 provides one, not the other.

---

## For More Details

See `SECURITY_FLAWS.md` for:
- Detailed analysis of each flaw
- Code examples
- Practical attack demonstrations
- Impact assessment
- Recommendations for fixes

---

**Last Updated:** 2026-06-08  
**Status:** Complete vulnerability analysis  
**Recommendation:** Use for obfuscation only, not security
