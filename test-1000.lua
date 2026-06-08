local Iterations = 1000
print('CLOSURE testing.')
local Start = os.clock();
for Idx = 1, Iterations do
	(function()
		if (not true) then
			print'Hey gamer.';
		end;
	end)();
end;
print('Time:', os.clock() - Start .. 's');

print('SETTABLE testing.')
Start = os.clock();
local T = {}
for Idx = 1, Iterations do
	T[tostring(Idx)] = 'EPIC GAMER ' .. tostring(Idx)
end
print('Time:', os.clock() - Start .. 's');

print('GETTABLE testing.')
Start = os.clock();
for Idx = 1, Iterations do
	T[1] = T[tostring(Idx)]
end
print('Time:', os.clock() - Start .. 's');
