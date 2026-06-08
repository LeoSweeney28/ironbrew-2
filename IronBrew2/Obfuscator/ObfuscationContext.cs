using System;
using System.Collections.Generic;
using System.Linq;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Cryptography;
using IronBrew2.Extensions;

namespace IronBrew2.Obfuscator
{
	public enum ChunkStep
	{
		ParameterCount,
		StringTable,
		Instructions,
		Functions,
		LineInfo,
		StepCount
	}

	public enum InstructionStep1
	{
		Type,
		A,
		B,
		C,
		StepCount
	}

	public enum InstructionStep2
	{
		Op,
		Bx,
		D,
		StepCount
	}
	
	public class ObfuscationContext
	{
		public Chunk HeadChunk;
		public ChunkStep[] ChunkSteps;
		public InstructionStep1[] InstructionSteps1;
		public InstructionStep2[] InstructionSteps2;
		public int[] ConstantMapping;

		public Dictionary<Opcode, VOpcode> InstructionMapping = new Dictionary<Opcode, VOpcode>();

		// Legacy XOR keys (kept for backward compatibility)
		public int PrimaryXorKey;
		public int IXorKey1;
		public int IXorKey2;

		// New secure encryption keys
		public byte[] AesEncryptionKey;
		public byte[] HmacKey;
		public string EncryptionKeyHex;
		public string HmacKeyHex;

		public ObfuscationContext(Chunk chunk)
		{
			HeadChunk = chunk;
			ChunkSteps = Enumerable.Range(0, (int) ChunkStep.StepCount).Select(i => (ChunkStep) i).ToArray();
			ChunkSteps.Shuffle();

			InstructionSteps1 = Enumerable.Range(0, (int) InstructionStep1.StepCount).Select(i => (InstructionStep1) i).ToArray();
			InstructionSteps1.Shuffle();

			InstructionSteps2 = Enumerable.Range(0, (int) InstructionStep2.StepCount).Select(i => (InstructionStep2) i).ToArray();
			InstructionSteps2.Shuffle();

			ConstantMapping = Enumerable.Range(0, 4).ToArray();
			ConstantMapping.Shuffle();

			// Legacy keys (weak - kept for backward compatibility)
			PrimaryXorKey = SecureRandom.NextInt(0, 256);
			IXorKey1 = SecureRandom.NextInt(0, 256);
			IXorKey2 = SecureRandom.NextInt(0, 256);

			// New secure keys
			AesEncryptionKey = Cryptography.AesEncryption.GenerateKey();
			HmacKey = IntegrityProtection.GenerateKey();
			EncryptionKeyHex = BitConverter.ToString(AesEncryptionKey).Replace("-", "").ToLower();
			HmacKeyHex = BitConverter.ToString(HmacKey).Replace("-", "").ToLower();
		}
	}
}