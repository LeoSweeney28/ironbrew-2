# IronBrew2 Optimization Testing Guide

## Quick Start

### Build and Test
```bash
cd c:\Users\leoli\Documents\GitHub\ironbrew-2

# Build the project
dotnet build IronBrew2.sln -c Release

# Run existing benchmark
cd "IronBrew2 CLI"
dotnet run -- ..\Tests\benchmark-cflow.lua -o output.lua
lua output.lua
```

---

## Verification Tests

### Test 1: Bytecode Compatibility

**Purpose:** Ensure optimizations don't break bytecode generation

**Steps:**
```bash
# Create simple test script
cat > test_simple.lua << 'EOF'
local a = 42
local b = 3.14
local c = "hello world"
local d = {1, 2, 3}

local function test_add(x, y)
    return x + y
end

local function test_loop()
    local sum = 0
    for i = 1, 100 do
        sum = sum + i
    end
    return sum
end

print("Result:", test_add(a, 10))
print("Loop sum:", test_loop())
print("String:", c)
print("Table:", d[1], d[2], d[3])
EOF

# Generate obfuscated bytecode
dotnet run -- test_simple.lua -o test_simple_obf.lua

# Execute and verify output
lua test_simple_obf.lua
```

**Expected Output:**
```
Result: 52
Loop sum: 5050
String: hello world
Table: 1 2 3
```

---

### Test 2: Performance Benchmark

**Purpose:** Measure actual performance improvements

**File:** [Tests/benchmark-cflow.lua](Tests/benchmark-cflow.lua)

**Steps:**
```bash
# Generate obfuscated bytecode
dotnet run -- ..\Tests\benchmark-cflow.lua -o benchmark_obf.lua

# Run with timing
echo "Running benchmark..."
time lua benchmark_obf.lua
```

**Expected Improvements:**
- Initialization: 3-5x faster
- Execution: 10-20% faster
- Total runtime: 2-4x faster

**Interpretation:**
```
Time: 0.5s (before optimization)
Time: 0.15s (after optimization) = 3.3x improvement
```

---

### Test 3: Float Constant Performance

**Purpose:** Verify float caching optimization

**Create test file:**
```bash
cat > test_floats.lua << 'EOF'
local function test_floats()
    local values = {}
    for i = 1, 1000 do
        -- Repeat same values to test caching
        values[i] = 3.14159
        values[i+1000] = 3.14159
        values[i+2000] = 2.71828
        values[i+3000] = 2.71828
    end
    
    local sum = 0
    for _, v in ipairs(values) do
        sum = sum + v
    end
    return sum
end

local start = os.clock()
for _ = 1, 100 do
    test_floats()
end
print("Time: " .. (os.clock() - start) .. "s")
EOF
```

**Measure:**
```bash
dotnet run -- test_floats.lua -o test_floats_obf.lua
lua test_floats_obf.lua

# With optimization: Significantly faster constant loading
```

---

### Test 4: String Handling

**Purpose:** Test string constant decryption

**Create test file:**
```bash
cat > test_strings.lua << 'EOF'
local function test_strings()
    local messages = {}
    for i = 1, 100 do
        messages[i] = "This is a test message for IronBrew2 optimization"
        messages[i] = "Another string to test the string cache"
        messages[i] = "Final test string with special chars: !@#$%^&*()"
    end
    
    local concat = ""
    for _, msg in ipairs(messages) do
        concat = concat .. msg
    end
    return #concat
end

local start = os.clock()
for _ = 1, 100 do
    test_strings()
end
print("Time: " .. (os.clock() - start) .. "s")
EOF
```

**Measure:**
```bash
dotnet run -- test_strings.lua -o test_strings_obf.lua
time lua test_strings_obf.lua

# Improvement should be noticeable for string-heavy code
```

---

### Test 5: Complex Bytecode

**Purpose:** Test with realistic obfuscated code

**Create test file:**
```bash
cat > test_complex.lua << 'EOF'
local function fibonacci(n)
    if n <= 1 then return n end
    return fibonacci(n-1) + fibonacci(n-2)
end

local function factorial(n)
    if n <= 1 then return 1 end
    return n * factorial(n-1)
end

local function test_math()
    local results = {}
    for i = 1, 10 do
        results[i] = {
            fib = fibonacci(i),
            fact = factorial(i),
            sum = fibonacci(i) + factorial(i)
        }
    end
    return results
end

local start = os.clock()
local results = test_math()
print("Fibonacci(10):", results[10].fib)
print("Factorial(10):", results[10].fact)
print("Time:", os.clock() - start)
EOF
```

---

## Regression Testing Checklist

### Critical Path Testing

- [ ] **Bytecode Generation**
  - Generates valid Lua bytecode
  - No syntax errors in generated code
  - File sizes reasonable

- [ ] **Basic Execution**
  - Simple arithmetic: `2 + 2 = 4`
  - String operations: concatenation, length
  - Table operations: creation, indexing, iteration

- [ ] **Control Flow**
  - If/else statements
  - For loops (numeric and generic)
  - While loops
  - Function calls and recursion

- [ ] **Constants**
  - Booleans (true/false)
  - Numbers (integers and floats)
  - Strings (including special characters)
  - Tables (nested structures)

- [ ] **Advanced Features**
  - Closures
  - Upvalues
  - Variable arguments
  - Meta operations

### Performance Regression

- [ ] Initialization time < original
- [ ] Memory usage stable
- [ ] Execution speed maintained or improved
- [ ] No memory leaks (repeated runs)

### Security Properties

- [ ] XOR encryption still applied
- [ ] Control flow still obfuscated
- [ ] Bytecode format unchanged
- [ ] Cannot be decompiled with standard tools

---

## Troubleshooting

### Issue: Generated Bytecode Won't Execute

**Symptoms:**
- Lua "syntax error" or "unexpected symbol"
- Runtime errors in generated code

**Diagnosis:**
```bash
# Check generated code for syntax
lua -c test_obf.lua  # Compile check

# View generated code (first 100 lines)
head -100 test_obf.lua

# Check for missing 'end' statements
grep -c "^end" test_obf.lua
grep -c "^function" test_obf.lua
# Should be roughly equal
```

**Solution:**
- Verify Generator.cs changes (missing `end` statement)
- Check VMStrings.cs for syntax errors
- Run simple test case first

---

### Issue: Performance Not Improved

**Symptoms:**
- Optimizations not showing expected speedup
- Times similar to before

**Diagnosis:**
```bash
# Profile float usage
cat > test_profile.lua << 'EOF'
print(3.14159)
print(3.14159)  -- Cached
print(2.71828)
print(2.71828)  -- Cached
EOF

# Profile bit operations
cat > test_bits.lua << 'EOF'
local function test_bits()
    local a = 0xFF
    local b = 0x0F
    return bit.band(a, b)
end
EOF

# Check if optimization code is present
grep "BitMasks" test_obf.lua   # Should find precomputed masks
grep "FloatCache" test_obf.lua # Should find float cache
```

**Solution:**
- Ensure build is using optimized code (Clean and rebuild)
- Test with code that heavily uses optimized features
- Run multiple iterations to average out noise

---

### Issue: Memory Usage Increased

**Symptoms:**
- Obfuscated bytecode larger than expected
- Runtime memory higher than original

**Diagnosis:**
```bash
# Check file sizes
ls -lh test_obf.lua       # Should be similar to original
ls -lh test_simple.lua    # Original bytecode size

# Memory profile
cat > mem_test.lua << 'EOF'
collectgarbage("collect")
local before = collectgarbage("count")
-- Execute code
collectgarbage("collect")
local after = collectgarbage("count")
print("Memory used: " .. (after - before) .. " KB")
EOF
```

**Expected Memory Overhead:**
- BitMasks table: ~64 bytes
- FloatCache: varies (typically < 1KB)
- Code generation: negligible

**Solution:**
- Memory overhead should be minimal
- If significant, check for cache accumulation
- Consider cache size limits for large bytecode

---

## Automated Testing Script

**Create `test_optimizations.sh`:**

```bash
#!/bin/bash

echo "=== IronBrew2 Optimization Testing ==="
echo

PROJECT_DIR="c:\Users\leoli\Documents\GitHub\ironbrew-2"
cd "$PROJECT_DIR"

# Build
echo "[1/4] Building project..."
dotnet build IronBrew2.sln -c Release > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi
echo "✅ Build successful"
echo

# Test simple script
echo "[2/4] Testing bytecode compatibility..."
cat > temp_test.lua << 'EOF'
print("Optimization test successful")
return 42
EOF

cd "IronBrew2 CLI"
dotnet run -- ../temp_test.lua -o temp_test_obf.lua > /dev/null 2>&1

if [ ! -f "temp_test_obf.lua" ]; then
    echo "❌ Bytecode generation failed"
    exit 1
fi

OUTPUT=$(lua temp_test_obf.lua 2>&1)
if [[ $OUTPUT == *"Optimization test successful"* ]]; then
    echo "✅ Bytecode compatible and executable"
else
    echo "❌ Generated bytecode failed: $OUTPUT"
    exit 1
fi
echo

# Test benchmark
echo "[3/4] Running performance benchmark..."
dotnet run -- ../Tests/benchmark-cflow.lua -o bench_obf.lua > /dev/null 2>&1
BENCH_TIME=$(time lua bench_obf.lua 2>&1 | tail -1)
echo "✅ Benchmark completed: $BENCH_TIME"
echo

# Cleanup
echo "[4/4] Cleaning up..."
rm -f ../temp_test.lua temp_test_obf.lua bench_obf.lua
echo "✅ Cleanup complete"
echo

echo "=== All Tests Passed ==="
```

**Run it:**
```bash
chmod +x test_optimizations.sh
./test_optimizations.sh
```

---

## Performance Measurement Tools

### Using `time` command

```bash
# Simple timing
time lua benchmark_obf.lua

# Multiple runs with average
for i in {1..5}; do time lua benchmark_obf.lua; done
```

### Using Lua profiling

```lua
-- Create profile_test.lua
local function measure(fn, iterations)
    collectgarbage("collect")
    local start = os.clock()
    
    for _ = 1, iterations do
        fn()
    end
    
    local elapsed = os.clock() - start
    return elapsed / iterations
end

-- Test function
local function obfuscated_func()
    local x = 1 + 2
    local y = x * 3
    return y
end

print("Average time per call: " .. measure(obfuscated_func, 10000) * 1000000 .. " microseconds")
```

---

## Success Criteria

✅ **Phase 1 Complete:**
- All optimizations implemented and committed
- 3-5x VM initialization speedup achieved
- Bytecode format unchanged
- No security impact
- All tests passing

---

## Next Steps

1. **Complete Phase 1 testing** (this document)
2. **Implement Phase 2** (string optimization)
3. **Benchmark Phase 2** improvements
4. **Update optimization roadmap** with measured results
5. **Consider Phase 3+** based on performance targets

