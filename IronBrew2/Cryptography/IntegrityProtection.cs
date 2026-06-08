using System;
using System.Security.Cryptography;
using System.Text;

namespace IronBrew2.Cryptography
{
	public static class IntegrityProtection
	{
		/// <summary>
		/// Creates an HMAC-SHA256 signature for data integrity verification
		/// </summary>
		public static byte[] CreateSignature(byte[] data, byte[] key)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			using (var hmac = new HMACSHA256(key))
			{
				return hmac.ComputeHash(data);
			}
		}

		/// <summary>
		/// Verifies HMAC-SHA256 signature
		/// </summary>
		public static bool VerifySignature(byte[] data, byte[] signature, byte[] key)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));
			if (signature == null)
				throw new ArgumentNullException(nameof(signature));
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			byte[] computedSignature = CreateSignature(data, key);
			return ConstantTimeComparison(signature, computedSignature);
		}

		/// <summary>
		/// Constant-time comparison to prevent timing attacks
		/// </summary>
		private static bool ConstantTimeComparison(byte[] a, byte[] b)
		{
			if (a.Length != b.Length)
				return false;

			int result = 0;
			for (int i = 0; i < a.Length; i++)
			{
				result |= a[i] ^ b[i];
			}

			return result == 0;
		}

		/// <summary>
		/// Generates a random 256-bit HMAC key
		/// </summary>
		public static byte[] GenerateKey()
		{
			byte[] key = new byte[32];
			SecureRandom.NextBytes(key);
			return key;
		}

		/// <summary>
		/// Creates a signed and encrypted package with metadata
		/// Format: [Version(1)][Timestamp(8)][IV(16)][Encrypted(N)][Signature(32)]
		/// </summary>
		public static byte[] CreateSecurePackage(byte[] data, byte[] encryptionKey, byte[] hmacKey)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));
			if (encryptionKey == null)
				throw new ArgumentNullException(nameof(encryptionKey));
			if (hmacKey == null)
				throw new ArgumentNullException(nameof(hmacKey));

			byte version = 1;
			byte[] timestamp = BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

			byte[] encrypted = AesEncryption.Encrypt(data, encryptionKey);

			byte[] package = new byte[1 + 8 + encrypted.Length];
			package[0] = version;
			Buffer.BlockCopy(timestamp, 0, package, 1, 8);
			Buffer.BlockCopy(encrypted, 0, package, 9, encrypted.Length);

			byte[] signature = CreateSignature(package, hmacKey);

			byte[] result = new byte[package.Length + 32];
			Buffer.BlockCopy(package, 0, result, 0, package.Length);
			Buffer.BlockCopy(signature, 0, result, package.Length, 32);

			return result;
		}

		/// <summary>
		/// Verifies and decrypts a secure package
		/// </summary>
		public static byte[] VerifyAndDecryptPackage(byte[] securePackage, byte[] encryptionKey, byte[] hmacKey)
		{
			if (securePackage == null)
				throw new ArgumentNullException(nameof(securePackage));
			if (encryptionKey == null)
				throw new ArgumentNullException(nameof(encryptionKey));
			if (hmacKey == null)
				throw new ArgumentNullException(nameof(hmacKey));

			if (securePackage.Length < 41)
				throw new ArgumentException("Secure package too short", nameof(securePackage));

			byte[] package = new byte[securePackage.Length - 32];
			byte[] signature = new byte[32];

			Buffer.BlockCopy(securePackage, 0, package, 0, package.Length);
			Buffer.BlockCopy(securePackage, package.Length, signature, 0, 32);

			if (!VerifySignature(package, signature, hmacKey))
				throw new CryptographicException("Package signature verification failed - data may be tampered");

			byte version = package[0];
			if (version != 1)
				throw new CryptographicException($"Unsupported package version: {version}");

			byte[] timestamp = new byte[8];
			Buffer.BlockCopy(package, 1, timestamp, 0, 8);
			long timestampValue = BitConverter.ToInt64(timestamp, 0);
			var packageTime = DateTimeOffset.FromUnixTimeSeconds(timestampValue);

			var now = DateTimeOffset.UtcNow;
			var age = now - packageTime;

			if (age.TotalHours > 24)
				throw new CryptographicException($"Package is too old ({age.TotalHours:F1} hours)");

			byte[] encrypted = new byte[package.Length - 9];
			Buffer.BlockCopy(package, 9, encrypted, 0, encrypted.Length);

			byte[] decrypted = AesEncryption.Decrypt(encrypted, encryptionKey);
			return decrypted;
		}
	}
}
