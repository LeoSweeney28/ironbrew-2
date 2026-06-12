using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Utilities;

namespace IronBrew2.Obfuscator.Macros
{
    public static class StrEncrypt
    {
        private static Random _r = SharedRandom.Instance;

        // Compact byte-XOR function for Lua 5.1 (no bitwise operators needed)
        private const string LuaXorFunc =
            "local function x(a,b)local r=0 for p=1,8 do if a%2~=b%2 then r=r+2^(p-1)end a=(a-a%2)/2 b=(b-b%2)/2 end return r end";

        public static string EncryptAdditive(byte[] input, out byte[] key)
        {
            key = new byte[input.Length];
            byte[] result = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                key[i] = (byte)_r.Next(1, 255);
                result[i] = (byte)((input[i] + key[i]) % 256);
            }

            return GenerateLuaDecrypt("additive", result, key,
                (b, k) => $"({b} - {k} + 256) % 256");
        }

        public static string EncryptNot(byte[] input, out byte[] key)
        {
            key = new byte[0];
            byte[] result = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
                result[i] = (byte)(~input[i] & 0xFF);

            return GenerateLuaDecrypt("not", result, key,
                (b, k) => $"(255 - {b})");
        }

        public static string EncryptXorInline(byte[] input, out byte[] key)
        {
            int keyLen = Math.Max(1, Math.Min(input.Length, _r.Next(5, 50)));
            key = new byte[keyLen];
            _r.NextBytes(key);

            byte[] result = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
                result[i] = (byte)(input[i] ^ key[i % keyLen]);

            string keyStr = string.Join(",", key.Select(b => b.ToString()));
            string dataStr = string.Join(",", result.Select(b => b.ToString()));

            return $"(function()local k={{{keyStr}}};local d={{{dataStr}}};{LuaXorFunc};local r={{}};for i=1,#d do r[i]=string.char(x(string.byte(d,i),k[(i-1)%#{keyLen}+1]))end;return table.concat(r)end)()";
        }

        public static string EncryptArithmetic(byte[] input, out byte[] key)
        {
            int seed = _r.Next(1, 65535);
            int multiplier = _r.Next(1, 255);
            key = new byte[input.Length];

            byte[] result = new byte[input.Length];
            int state = seed;

            for (int i = 0; i < input.Length; i++)
            {
                state = (state * multiplier + 1) % 256;
                key[i] = (byte)state;
                result[i] = (byte)(input[i] ^ key[i]);
            }

            string dataStr = string.Join(",", result.Select(b => b.ToString()));

            return $"(function()local s={seed};local m={multiplier};local d={{{dataStr}}};{LuaXorFunc};local r={{}};for i=1,#d do s=(s*m+1)%256;r[i]=string.char(x(string.byte(d,i),s))end;return table.concat(r)end)()";
        }

        private static string GenerateLuaDecrypt(string name, byte[] data, byte[] key,
            Func<string, string, string> byteTransform)
        {
            string dataStr = string.Join(",", data.Select(b => b.ToString()));

            if (key.Length == 0)
            {
                return $"(function()local d={{{dataStr}}};local r={{}};for i=1,#d do r[i]=string.char({byteTransform("string.byte(d,i)", "0")})end;return table.concat(r)end)()";
            }
            else if (key.Length == 1)
            {
                return $"(function()local d={{{dataStr}}};local r={{}};for i=1,#d do r[i]=string.char({byteTransform("string.byte(d,i)", key[0].ToString())})end;return table.concat(r)end)()";
            }
            else
            {
                string keyStr = string.Join(",", key.Select(b => b.ToString()));
                return $"(function()local k={{{keyStr}}};local d={{{dataStr}}};local r={{}};for i=1,#d do r[i]=string.char({byteTransform("string.byte(d,i)", "k[i]")})end;return table.concat(r)end)()";
            }
        }

        public static string EncryptWithRandomStrategy(byte[] input)
        {
            byte[] key;
            int strategy = _r.Next(0, 4);

            switch (strategy)
            {
                case 0:
                    return EncryptAdditive(input, out key);
                case 1:
                    return EncryptNot(input, out key);
                case 2:
                    return EncryptXorInline(input, out key);
                case 3:
                    return EncryptArithmetic(input, out key);
                default:
                    return EncryptXorInline(input, out key);
            }
        }
    }
}
