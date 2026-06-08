-- Sample Lua file for testing IronBrew2
-- This is a simple test program that demonstrates basic Lua features

function hello()
    print("Hello, World!")
end

function add(a, b)
    return a + b
end

function greet(name)
    if name == nil then
        name = "Friend"
    end
    print("Hello, " .. name .. "!")
end

function count_to_n(n)
    for i = 1, n do
        print("Count: " .. i)
    end
end

-- Main program
print("=== IronBrew2 Test Script ===")
hello()

local result = add(5, 3)
print("5 + 3 = " .. result)

greet("Alice")
greet()

count_to_n(5)

print("=== Test Complete ===")
