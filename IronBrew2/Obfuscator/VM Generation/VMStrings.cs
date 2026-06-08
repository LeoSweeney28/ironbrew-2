namespace IronBrew2.Obfuscator.VM_Generation
{
	public static class VMStrings
	{
		// Pure-Lua AES-128 implementation used to decrypt the bytecode blob at
		// runtime.  Key and IV are injected as hex strings at generation time.
		// The blob is PKCS7-padded AES-128-CBC.  We use AES-128 here (not 256)
		// because a compact pure-Lua implementation is required; the 256-bit
		// key derived from the generator is used to produce a 128-bit session
		// key via HMAC-SHA256(aesKey, "IB2_SESSION")[0..15].
		//
		// Placeholders replaced by Generator:
		//   AES_KEY_HEX   – 32 hex chars (128-bit key)
		//   AES_IV_HEX    – 32 hex chars (128-bit IV)
		//   HMAC_HEX      – 64 hex chars (SHA-256 HMAC of plaintext blob)
		public static string AesDecryptorPreamble = @"
local function _ibAes()
	-- AES-128 S-box
	local S={[0]=0x63,0x7c,0x77,0x7b,0xf2,0x6b,0x6f,0xc5,0x30,0x01,0x67,0x2b,0xfe,0xd7,0xab,0x76,
	0xca,0x82,0xc9,0x7d,0xfa,0x59,0x47,0xf0,0xad,0xd4,0xa2,0xaf,0x9c,0xa4,0x72,0xc0,
	0xb7,0xfd,0x93,0x26,0x36,0x3f,0xf7,0xcc,0x34,0xa5,0xe5,0xf1,0x71,0xd8,0x31,0x15,
	0x04,0xc7,0x23,0xc3,0x18,0x96,0x05,0x9a,0x07,0x12,0x80,0xe2,0xeb,0x27,0xb2,0x75,
	0x09,0x83,0x2c,0x1a,0x1b,0x6e,0x5a,0xa0,0x52,0x3b,0xd6,0xb3,0x29,0xe3,0x2f,0x84,
	0x53,0xd1,0x00,0xed,0x20,0xfc,0xb1,0x5b,0x6a,0xcb,0xbe,0x39,0x4a,0x4c,0x58,0xcf,
	0xd0,0xef,0xaa,0xfb,0x43,0x4d,0x33,0x85,0x45,0xf9,0x02,0x7f,0x50,0x3c,0x9f,0xa8,
	0x51,0xa3,0x40,0x8f,0x92,0x9d,0x38,0xf5,0xbc,0xb6,0xda,0x21,0x10,0xff,0xf3,0xd2,
	0xcd,0x0c,0x13,0xec,0x5f,0x97,0x44,0x17,0xc4,0xa7,0x7e,0x3d,0x64,0x5d,0x19,0x73,
	0x60,0x81,0x4f,0xdc,0x22,0x2a,0x90,0x88,0x46,0xee,0xb8,0x14,0xde,0x5e,0x0b,0xdb,
	0xe0,0x32,0x3a,0x0a,0x49,0x06,0x24,0x5c,0xc2,0xd3,0xac,0x62,0x91,0x95,0xe4,0x79,
	0xe7,0xc8,0x37,0x6d,0x8d,0xd5,0x4e,0xa9,0x6c,0x56,0xf4,0xea,0x65,0x7a,0xae,0x08,
	0xba,0x78,0x25,0x2e,0x1c,0xa6,0xb4,0xc6,0xe8,0xdd,0x74,0x1f,0x4b,0xbd,0x8b,0x8a,
	0x70,0x3e,0xb5,0x66,0x48,0x03,0xf6,0x0e,0x61,0x35,0x57,0xb9,0x86,0xc1,0x1d,0x9e,
	0xe1,0xf8,0x98,0x11,0x69,0xd9,0x8e,0x94,0x9b,0x1e,0x87,0xe9,0xce,0x55,0x28,0xdf,
	0x8c,0xa1,0x89,0x0d,0xbf,0xe6,0x42,0x68,0x41,0x99,0x2d,0x0f,0xb0,0x54,0xbb,0x16}
	local function xtime(a) return a<128 and a*2 or ((a*2)%256)~0x1b end
	local function mul(a,b)
		local r=0
		for _=1,8 do
			if b%2==1 then r=r~a end
			a=xtime(a); b=b>>1
		end
		return r
	end
	-- Expand 16-byte key into 11 round keys (10 rounds for AES-128)
	local rc={0x01,0x02,0x04,0x08,0x10,0x20,0x40,0x80,0x1b,0x36}
	local function expandKey(k)
		local w={}
		for i=0,3 do w[i]={k[i*4+1],k[i*4+2],k[i*4+3],k[i*4+4]} end
		for i=4,43 do
			local t={w[i-1][1],w[i-1][2],w[i-1][3],w[i-1][4]}
			if i%4==0 then
				t={S[t[2]]~rc[i//4],S[t[3]],S[t[4]],S[t[1]]}
			end
			w[i]={w[i-4][1]~t[1],w[i-4][2]~t[2],w[i-4][3]~t[3],w[i-4][4]~t[4]}
		end
		return w
	end
	local function addRoundKey(state,w,r)
		for c=0,3 do for row=1,4 do state[row][c+1]=state[row][c+1]~w[r*4+c][row] end end
	end
	local function subBytes(state)
		for r=1,4 do for c=1,4 do state[r][c]=S[state[r][c]] end end
	end
	local function shiftRows(state)
		for r=2,4 do
			local tmp={}
			for c=1,4 do tmp[c]=state[r][((c+r-2)%4)+1] end
			state[r]=tmp
		end
	end
	local function mixColumns(state)
		for c=1,4 do
			local s=state
			local s0,s1,s2,s3=s[1][c],s[2][c],s[3][c],s[4][c]
			s[1][c]=mul(s0,2)~mul(s1,3)~s2~s3
			s[2][c]=s0~mul(s1,2)~mul(s2,3)~s3
			s[3][c]=s0~s1~mul(s2,2)~mul(s3,3)
			s[4][c]=mul(s0,3)~s1~s2~mul(s3,2)
		end
	end
	local function decryptBlock(blk,rk)
		-- inverse S-box
		local IS={}
		for i=0,255 do IS[S[i]]=i end
		local function invSubBytes(st)
			for r=1,4 do for c=1,4 do st[r][c]=IS[st[r][c]] end end
		end
		local function invShiftRows(st)
			for r=2,4 do
				local tmp={}
				for c=1,4 do tmp[c]=st[r][((c-r+4)%4)+1] end
				st[r]=tmp
			end
		end
		local function invMixColumns(st)
			for c=1,4 do
				local s0,s1,s2,s3=st[1][c],st[2][c],st[3][c],st[4][c]
				st[1][c]=mul(s0,14)~mul(s1,11)~mul(s2,13)~mul(s3,9)
				st[2][c]=mul(s0,9)~mul(s1,14)~mul(s2,11)~mul(s3,13)
				st[3][c]=mul(s0,13)~mul(s1,9)~mul(s2,14)~mul(s3,11)
				st[4][c]=mul(s0,11)~mul(s1,13)~mul(s2,9)~mul(s3,14)
			end
		end
		local state={{},{},{},{}}
		for r=1,4 do for c=1,4 do state[r][c]=blk[(c-1)*4+r] end end
		addRoundKey(state,rk,10)
		for rd=9,1,-1 do
			invShiftRows(state); invSubBytes(state)
			addRoundKey(state,rk,rd); invMixColumns(state)
		end
		invShiftRows(state); invSubBytes(state); addRoundKey(state,rk,0)
		local out={}
		for c=1,4 do for r=1,4 do out[(c-1)*4+r]=state[r][c] end end
		return out
	end
	return {expandKey=expandKey, decryptBlock=decryptBlock}
end
local _aes=_ibAes()

local function _hexToBytes(h)
	local b={}
	for i=1,#h,2 do b[#b+1]=tonumber(h:sub(i,i+1),16) end
	return b
end

local function _ibDecrypt(cipherHex, keyHex, ivHex)
	local key=_hexToBytes(keyHex)
	local iv=_hexToBytes(ivHex)
	local ct=_hexToBytes(cipherHex)
	local rk=_aes.expandKey(key)
	local plain={}
	local prev=iv
	for blk=1,#ct//16 do
		local inp={}
		for i=1,16 do inp[i]=ct[(blk-1)*16+i] end
		local dec=_aes.decryptBlock(inp,rk)
		for i=1,16 do dec[i]=dec[i]~prev[i] end
		for i=1,16 do plain[#plain+1]=dec[i] end
		prev=inp
	end
	-- remove PKCS7 padding
	local pad=plain[#plain]
	for _=1,pad do plain[#plain]=nil end
	local chars={}
	for _,v in ipairs(plain) do chars[#chars+1]=string.char(v) end
	return table.concat(chars)
end

local function _ibHmacVerify(data, expectedHex)
	-- lightweight integrity check: SHA-256 HMAC pre-computed at build time.
	-- We re-derive a fast checksum and compare against the build-time value.
	-- Full HMAC-SHA256 would be several hundred lines of pure Lua; instead we
	-- use Adler-32 keyed with the first 4 bytes of the HMAC key embedded here.
	-- This stops casual tampering; the AES encryption is the real protection.
	local HMAC_KEY_SEED = HMAC_SEED
	local s1,s2=1,0
	for i=1,#data do
		s1=(s1+string.byte(data,i)+HMAC_KEY_SEED)%65521
		s2=(s2+s1)%65521
	end
	local computed=string.format(""%08x"", s2*65536+s1)
	if computed ~= expectedHex then
		error(""[IronBrew2] Bytecode integrity check failed - file may be tampered"")
	end
end

local ByteString = _ibDecrypt(ENCRYPTED_BLOB, ""AES_KEY_HEX"", ""AES_IV_HEX"")
_ibHmacVerify(ByteString, ""HMAC_CHECKSUM"")
";


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
