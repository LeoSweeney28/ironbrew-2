using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IronBrew2.Cryptography;
using IronBrew2.Utilities;

namespace IronBrew2.Obfuscator.Encryption
{
	public class Decryptor
	{
		public int[] Table;
		public int SLen = 0;

		public string Name;

		public string Encrypt(byte[] bytes)
		{
			List<byte> encrypted = new List<byte>();

			int L = Table.Length;

			for (var index = 0; index < bytes.Length; index++)
				encrypted.Add((byte) (bytes[index] ^ Table[index % L]));

			return $"((function(b)IB_INLINING_START(true);local function xor(b,c)IB_INLINING_START(true);local d,e=1,0;while b>0 and c>0 do local f,g=b%2,c%2;if f~=g then e=e+d end;b,c,d=(b-f)/2,(c-g)/2,d*2 end;if b<c then b=c end;while b>0 do local f=b%2;if f>0 then e=e+d end;b,d=(b-f)/2,d*2 end;return e end;local c=\"\"local e=string.sub;local h=string.char;local t = {{}} for j=0, 255 do local x=h(j);t[j]=x;t[x]=j;end;local f=\"{string.Join("", Table.Select(t => "\\" + t.ToString()))}\" for g=1,#b do local x=(g-1) % {Table.Length}+1 c=c..t[xor(t[e(b,g,g)],t[e(f, x, x)])];end;return c;end)(\"{string.Join("", encrypted.Select(t => "\\" + t.ToString()))}\"))";
		}

		// Encrypts bytes with this Decryptor's (unique, per-call) Table and emits
		// a call to the shared decrypt helper, passing both the ciphertext and the
		// key. Unlike Encrypt(), which bakes a single shared Table into a bespoke
		// IIFE per string, this lets every string use its own random key while the
		// decryption logic itself (the helper function) is only emitted once -
		// avoiding both the repeated-key/Vigenere weakness of a single shared
		// Table and the size cost of duplicating the decryptor per string.
		public string EncryptCall(byte[] bytes, string funcName)
		{
			List<byte> encrypted = new List<byte>();

			int L = Table.Length;

			for (var index = 0; index < bytes.Length; index++)
				encrypted.Add((byte) (bytes[index] ^ Table[index % L]));

			string data = string.Join("", encrypted.Select(t => "\\" + t.ToString()));
			string key  = string.Join("", Table.Select(t => "\\" + t.ToString()));

			// Parenthesized so that call-sugar like print"foo" (which becomes
			// print<replacement>) still parses as a call rather than the
			// replacement's leading identifier merging into "print"'s name.
			return $"({funcName}(\"{data}\",\"{key}\"))";
		}

		// Emits the shared decrypt helper referenced by EncryptCall(). Reuses the
		// same arithmetic xor bit-trick as Encrypt() (Lua 5.1 has no bitwise ops),
		// but builds the result via table.concat over an array instead of
		// repeated string concatenation.
		public static string GenerateDecryptHelper(string funcName) =>
			$"local function {funcName}(b,f)local function xor(b,c)local d,e=1,0;while b>0 and c>0 do local g,h=b%2,c%2;if g~=h then e=e+d end;b,c,d=(b-g)/2,(c-h)/2,d*2 end;if b<c then b=c end;while b>0 do local g=b%2;if g>0 then e=e+d end;b,d=(b-g)/2,d*2 end;return e end;local t={{}};for j=0,255 do local x=string.char(j);t[j]=x;t[x]=j end;local L=#f;local r={{}};for i=1,#b do r[i]=t[xor(t[string.sub(b,i,i)],t[string.sub(f,(i-1)%L+1,(i-1)%L+1)])] end;return table.concat(r) end;";

		public Decryptor(string name, int maxLen)
		{
			Name = name;
			Table = SecureRandom.NextInts(maxLen, 0, 256);
		}
	}
	
	public class ConstantEncryption
	{
		private string _src;
		private ObfuscationSettings _settings;
		private Encoding LuaBytecodeEncoding => EncodingConstants.LuaBytecodeEncoding;

		public static byte[] UnescapeLuaString(string str)
		{
			List<byte> bytes = new List<byte>();
			
			int i = 0;
			while (i < str.Length)
			{
				char cur = str[i++];
				if (cur == '\\')
				{
					char next = str[i++];

					switch (next)
					{
						case 'a':
							bytes.Add((byte) '\a');
							break;

						case 'b':
							bytes.Add((byte) '\b');
							break;

						case 'f':
							bytes.Add((byte) '\f');
							break;

						case 'n':
							bytes.Add((byte) '\n');
							break;

						case 'r':
							bytes.Add((byte) '\r');
							break;

						case 't':
							bytes.Add((byte) '\t');
							break;

						case 'v':
							bytes.Add((byte) '\v');
							break;

						default:
						{
							if (!char.IsDigit(next))
								bytes.Add((byte) next);
							else // \001, \55h, etc
							{
								string s = next.ToString(); 
								for (int j = 0; j < 2; j++, i++)
								{
									if (i == str.Length)
										break;

									char n = str[i];
									if (char.IsDigit(n))
										s = s + n;
									else
										break;
								}

								bytes.Add((byte) int.Parse(s));
							}

							break;
						}
					}
				}
				else
					bytes.Add((byte) cur);
			}

			return bytes.ToArray();
		}

		public string EncryptStrings()
		{
			const string encRegex = @"(['""])?(?(1)((?:[^\\]|\\.)*?)\1|\[(=*)\[(.*?)\]\3\])";
			
			if (_settings.EncryptStrings)
			{
				Regex r       = new Regex(encRegex, RegexOptions.Singleline | RegexOptions.Compiled);

				int indDiff = 0;
				var   matches = r.Matches(_src);

				const string decryptFunc = "IRONBREW_STR_DECRYPT";

				foreach (Match m in matches)
				{
					string before = _src.Substring(0, m.Index        + indDiff);
					string after  = _src.Substring(m.Index + indDiff + m.Length);

					string captured = m.Groups[2].Value + m.Groups[4].Value;

					if (captured.StartsWith("[STR_ENCRYPT]"))
						captured = captured.Substring(13);

					byte[] bytes = m.Groups[2].Value != "" ? UnescapeLuaString(captured) : LuaBytecodeEncoding.GetBytes(captured);

					// Each string gets its own random key, instead of every string in
					// the program being XORed against the same repeating table - that
					// shared-key scheme let an attacker crib-drag/frequency-analyze the
					// whole program at once. The (small) shared decrypt helper below is
					// emitted once regardless of how many strings are encrypted.
					int keyLen = Math.Max(1, Math.Min(bytes.Length, _settings.DecryptTableLen));
					Decryptor dec = new Decryptor(decryptFunc, keyLen);

					string nStr = before + dec.EncryptCall(bytes, decryptFunc);
					nStr += after;

					indDiff += nStr.Length - _src.Length;
					_src    =  nStr;
				}

				if (matches.Count > 0)
					_src = Decryptor.GenerateDecryptHelper(decryptFunc) + _src;
			}

			else
			{
				Regex r = new Regex(encRegex, RegexOptions.Singleline | RegexOptions.Compiled);
				var matches = r.Matches(_src);

				int indDiff = 0;
				int n       = 0;

				foreach (Match m in matches)
				{
					string captured = m.Groups[2].Value + m.Groups[4].Value;
					
					if (!captured.StartsWith("[STR_ENCRYPT]"))
						continue;

					captured = captured.Substring(13);
					Decryptor dec = new Decryptor("IRONBREW_STR_ENCRYPT" + n++, m.Length);

					string before = _src.Substring(0, m.Index + indDiff);
					string after = _src.Substring(m.Index + indDiff + m.Length);

					string nStr = before + dec.Encrypt(m.Groups[2].Value != ""
						              ? UnescapeLuaString(captured)
						              : LuaBytecodeEncoding.GetBytes(captured));
					nStr += after;

					indDiff += nStr.Length - _src.Length;
					_src = nStr;
				}
			}
			
			// When EncryptStrings already ran, every string literal in _src has
			// already been replaced by a (digits-and-backslashes-only) ciphertext
			// escape sequence from EncryptCall - no "important" keyword can ever
			// match those, making this pass a no-op that costs a full regex sweep
			// over the (now much larger) source. Only run it when EncryptStrings
			// left the original string literals in place.
			if (_settings.EncryptImportantStrings && !_settings.EncryptStrings)
			{
				Regex r = new Regex(encRegex, RegexOptions.Singleline | RegexOptions.Compiled);
				var matches = r.Matches(_src);

				int indDiff = 0;
				int n = 0;

				List<string> sTerms = new List<string>() {"http", "function", "metatable", "local"};

				foreach (Match m in matches)
				{
					string captured = m.Groups[2].Value + m.Groups[4].Value;
					if (captured.StartsWith("[STR_ENCRYPT]"))
						captured = captured.Substring(13);

					bool cont = false;

					foreach (string search in sTerms)
					{
						if (captured.ToLower().Contains(search.ToLower()))
							cont = true;
					}

					if (!cont)
						continue;

					Decryptor dec = new Decryptor("IRONBREW_STR_ENCRYPT_IMPORTANT" + n++, m.Length);

					string before = _src.Substring(0, m.Index + indDiff);
					string after = _src.Substring(m.Index + indDiff + m.Length);

					string nStr = before + dec.Encrypt(m.Groups[2].Value != ""
						              ? UnescapeLuaString(captured)
						              : LuaBytecodeEncoding.GetBytes(captured));

					nStr += after;

					indDiff += nStr.Length - _src.Length;
					_src = nStr;
				}
			}

			return _src;
		}

		public ConstantEncryption(ObfuscationSettings settings, string source)
		{
			_settings = settings;
			_src = source;
		}
	}
}