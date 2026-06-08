namespace IronBrew2.Obfuscator.VM_Generation
{
	public static class VMStrings
	{
		public static string VMP1 = @"
	local BitXOR = bit and bit.bxor or function(a,b)
	    local p,c=1,0
	    while a>0 and b>0 do
	        local ra,rb=a%2,b%2
	        if ra~=rb then c=c+p end
	        a,b,p=(a-ra)/2,(b-rb)/2,p*2
	    end
	    if a<b then a=b end
	    while a>0 do
	        local ra=a%2
	        if ra>0 then c=c+p end
	        a,p=(a-ra)/2,p*2
	    end
	    return c
	end

	local BitMasks = {}
	for i=0,31 do BitMasks[i]=2^i end

	local function gBit(Bit, Start, End)
		if End then
			local Res = (Bit / BitMasks[Start - 1]) % BitMasks[End - Start + 1];
			return Res - Res % 1;
		else
			local Plc = BitMasks[Start - 1];
	        return (Bit % (Plc + Plc) >= Plc) and 1 or 0;
		end;
	end;

	local Pos = 1;

	local function gBits32()
	    local W, X, Y, Z = Byte(ByteString, Pos, Pos + 3);

		W = BitXOR(W, XOR_KEY)
		X = BitXOR(X, XOR_KEY)
		Y = BitXOR(Y, XOR_KEY)
		Z = BitXOR(Z, XOR_KEY)

	    Pos	= Pos + 4;
	    return (Z*16777216) + (Y*65536) + (X*256) + W;
	end;

	local function gBits8()
	    local F = BitXOR(Byte(ByteString, Pos, Pos), XOR_KEY);
	    Pos = Pos + 1;
	    return F;
	end;

	local function gBits16()
	    local W, X = Byte(ByteString, Pos, Pos + 2);

		W = BitXOR(W, XOR_KEY)
		X = BitXOR(X, XOR_KEY)

	    Pos	= Pos + 2;
	    return (X*256) + W;
	end;

	local FloatCache = {[0]=0}
	local function gFloat()
		local Left = gBits32();
		local Right = gBits32();
		local CacheKey = Right * 4294967296 + Left;
		if FloatCache[CacheKey] then return FloatCache[CacheKey] end;

		local IsNormal = 1;
		local Mantissa = (gBit(Right, 1, 20) * (2 ^ 32))
						+ Left;
		local Exponent = gBit(Right, 21, 31);
		local Sign = ((-1) ^ gBit(Right, 32));
		local Result;
		if (Exponent == 0) then
			if (Mantissa == 0) then
				Result = Sign * 0;
			else
				Exponent = 1;
				IsNormal = 0;
				Result = LDExp(Sign, Exponent - 1023) * (IsNormal + (Mantissa / (2 ^ 52)));
			end;
		elseif (Exponent == 2047) then
	        Result = (Mantissa == 0) and (Sign * (1 / 0)) or (Sign * (0 / 0));
		else
			Result = LDExp(Sign, Exponent - 1023) * (IsNormal + (Mantissa / (2 ^ 52)));
		end;
		FloatCache[CacheKey] = Result;
		return Result;
	end;

	local gSizet = gBits32;
	local function gString(Len)
	    local Str;
	    if (not Len) then
	        Len = gSizet();
	        if (Len == 0) then
	            return '';
	        end;
	    end;

	    Str	= Sub(ByteString, Pos, Pos + Len - 1);
	    Pos = Pos + Len;

		local FStr = {}
		for Idx = 1, #Str do
			FStr[Idx] = Char(BitXOR(Byte(Sub(Str, Idx, Idx)), XOR_KEY))
		end

	    return Concat(FStr);
	end;

	local gInt = gBits32;
	local function _R(...) return {...}, Select('#', ...) end

	local function Deserialize()
	    local Instrs = {};
	    local Functions = {};
		local Lines = {};
	    local Chunk =
		{
			Instrs,
			Functions,
			nil,
			Lines
		};
		local ConstCount = gBits32()
	    local Consts = {}

		for Idx=1, ConstCount do
			local Type =gBits8();
			local Cons;

			if(Type==CONST_BOOL) then Cons = (gBits8() ~= 0);
			elseif(Type==CONST_FLOAT) then Cons = gFloat();
			elseif(Type==CONST_STRING) then Cons = gString();
			end;

			Consts[Idx] = Cons;
		end;
	";

		public static string VMP2 = @"
	local function Wrap(Chunk, Upvalues, Env)
		local Instr  = Chunk[1];
		local Proto  = Chunk[2];
		local Params = Chunk[3];

		return function(...)
			local Instr  = Instr;
			local Proto  = Proto;
			local Params = Params;

			local _R = _R
			local InstrPoint = 1;
			local Top = -1;

			local Vararg = {};
			local Args	= {...};

			local PCount = Select('#', ...) - 1;

			local Lupvals	= {};
			local Stk		= {};

			for Idx = 0, PCount do
				if (Idx >= Params) then
					Vararg[Idx - Params] = Args[Idx + 1];
				else
					Stk[Idx] = Args[Idx + 1];
				end;
			end;

			local Varargsz = PCount - Params + 1

			local Inst;
			local Enum;
			local A, B, C;

			while true do
				Inst		= Instr[InstrPoint];
				Enum		= Inst[OP_ENUM];
				A, B, C = Inst[OP_A], Inst[OP_B], Inst[OP_C];"
;

		public static string VMP3 = @"
			InstrPoint	= InstrPoint + 1;
		end;
    end;
end;
return Wrap(Deserialize(), {}, GetFEnv())();
";

		public static string VMP2_LI = @"
	local PCall = pcall
	local function Wrap(Chunk, Upvalues, Env)
		local Instr = Chunk[1];
		local Proto = Chunk[2];
		local Params = Chunk[3];

		return function(...)
			local InstrPoint = 1;
			local Top = -1;

			local Args = {...};
			local PCount = Select('#', ...) - 1;

			local function Loop()
				local Instr  = Instr;
				local Const  = Const;
				local Proto  = Proto;
				local Params = Params;

				local _R = _R
				local Vararg = {};

				local Lupvals	= {};
				local Stk		= {};

				for Idx = 0, PCount do
					if (Idx >= Params) then
						Vararg[Idx - Params] = Args[Idx + 1];
					else
						Stk[Idx] = Args[Idx + 1];
					end;
				end;

				local Varargsz = PCount - Params + 1

				local Inst;
				local Enum;
				local A, B, C;

				while true do
					Inst		= Instr[InstrPoint];
					Enum		= Inst[OP_ENUM];
					A, B, C = Inst[OP_A], Inst[OP_B], Inst[OP_C];"
;

		public static string VMP3_LI = @"
				InstrPoint	= InstrPoint + 1;
			end;
		end;

		A, B = _R(PCall(Loop))
		if not A[1] then
			local line = Chunk[7][InstrPoint] or '?'
			error('ERROR IN IRONBREW SCRIPT [LINE ' .. line .. ']:' .. A[2])
		else
			return Unpack(A, 2, B)
		end;
	end;
end;
return Wrap(Deserialize(), {}, GetFEnv())();
";
	}
}
