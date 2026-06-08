local Iterations = 100000
print('SETTABLE testing.')
local Start = os.clock();
local T = {}
for Idx = 1, Iterations do
	T[tostring(Idx)] = 'EPIC GAMER ' .. tostring(Idx)
end

print('Time:', os.clock() - Start .. 's');
