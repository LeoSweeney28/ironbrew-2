using System;
using IronBrew2.Cryptography;

namespace IronBrew2.Utilities
{
	// A single Random instance shared across the whole obfuscation pipeline.
	// .NET's parameterless Random() constructor seeds from Environment.TickCount,
	// which has ~15ms resolution - instances created back-to-back (as the
	// generator and control-flow passes used to do) can end up with identical
	// seeds and therefore identical sequences, correlating supposedly-independent
	// obfuscation choices. Seeding once from a cryptographic RNG avoids that.
	public static class SharedRandom
	{
		public static Random Instance { get; } = new Random(SecureRandom.NextInt(int.MinValue, int.MaxValue));
	}
}
