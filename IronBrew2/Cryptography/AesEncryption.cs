using System;
using System.Security.Cryptography;

namespace IronBrew2.Cryptography
{
	public static class AesEncryption
	{
		private const int BlockSize = 128;

		/// <summary>
		/// Encrypts data using AES-256-CBC with random IV
		/// </summary>
		public static byte[] Encrypt(byte[] plaintext, byte[] key)
		{
			if (plaintext == null)
				throw new ArgumentNullException(nameof(plaintext));
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (key.Length != 16 && key.Length != 24 && key.Length != 32)
				throw new ArgumentException("Key must be 128, 192, or 256 bits (16, 24, or 32 bytes)", nameof(key));

			using (var aes = new AesCryptoServiceProvider())
			{
				aes.KeySize = key.Length * 8;
				aes.BlockSize = BlockSize;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;
				aes.Key = key;

				byte[] iv = aes.IV;

				using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
				{
					byte[] encrypted = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);

					byte[] result = new byte[iv.Length + encrypted.Length];
					Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
					Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

					return result;
				}
			}
		}

		/// <summary>
		/// Decrypts data encrypted with AES-256-CBC
		/// </summary>
		public static byte[] Decrypt(byte[] ciphertext, byte[] key)
		{
			if (ciphertext == null)
				throw new ArgumentNullException(nameof(ciphertext));
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (key.Length != 16 && key.Length != 24 && key.Length != 32)
				throw new ArgumentException("Key must be 128, 192, or 256 bits (16, 24, or 32 bytes)", nameof(key));
			if (ciphertext.Length < 16)
				throw new ArgumentException("Ciphertext too short (must contain IV)", nameof(ciphertext));

			using (var aes = new AesCryptoServiceProvider())
			{
				aes.KeySize = key.Length * 8;
				aes.BlockSize = BlockSize;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;
				aes.Key = key;

				byte[] iv = new byte[16];
				Buffer.BlockCopy(ciphertext, 0, iv, 0, 16);
				aes.IV = iv;

				using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
				{
					byte[] encrypted = new byte[ciphertext.Length - 16];
					Buffer.BlockCopy(ciphertext, 16, encrypted, 0, encrypted.Length);

					byte[] plaintext = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
					return plaintext;
				}
			}
		}

		/// <summary>
		/// Generates a random 256-bit AES key
		/// </summary>
		public static byte[] GenerateKey()
		{
			byte[] key = new byte[32];
			SecureRandom.NextBytes(key);
			return key;
		}
	}
}
