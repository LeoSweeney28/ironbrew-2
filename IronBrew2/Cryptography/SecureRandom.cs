using System;
using System.Security.Cryptography;

namespace IronBrew2.Cryptography
{
	public static class SecureRandom
	{
		private static readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();
		private static readonly object _syncLock = new object();

		public static int NextInt(int minInclusive, int maxExclusive)
		{
			if (minInclusive >= maxExclusive)
				throw new ArgumentException("minInclusive must be less than maxExclusive");

			lock (_syncLock)
			{
				long range = (long)maxExclusive - minInclusive;
				byte[] randomNumber = new byte[8];
				_rng.GetBytes(randomNumber);
				long randomLong = BitConverter.ToInt64(randomNumber, 0) & long.MaxValue;
				return minInclusive + (int)(randomLong % range);
			}
		}

		public static void NextBytes(byte[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));

			lock (_syncLock)
			{
				_rng.GetBytes(buffer);
			}
		}

		public static byte[] NextBytes(int length)
		{
			if (length < 0)
				throw new ArgumentException("length must be non-negative");

			byte[] buffer = new byte[length];
			NextBytes(buffer);
			return buffer;
		}

		public static int[] NextInts(int count, int minInclusive, int maxExclusive)
		{
			int[] result = new int[count];
			for (int i = 0; i < count; i++)
				result[i] = NextInt(minInclusive, maxExclusive);
			return result;
		}
	}
}
