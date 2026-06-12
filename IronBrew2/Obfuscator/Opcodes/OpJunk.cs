using System;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Utilities;

namespace IronBrew2.Obfuscator.Opcodes
{
    public class OpJunk : VOpcode
    {
        private static Random _r = SharedRandom.Instance;

        // Junk opcodes are never matched by real instructions - they exist
        // only to bloat the dispatch tree and confuse static analysis.
        public override bool IsInstruction(Instruction instruction) =>
            false;

        public override string GetObfuscated(ObfuscationContext context)
        {
            // Generate random dead code that will never execute
            int mode = _r.Next(0, 5);
            int r1 = _r.Next(0, 256);
            int r2 = _r.Next(0, 256);
            int r3 = _r.Next(0, 256);
            int r4 = _r.Next(0, 256);

            switch (mode)
            {
                case 0:
                    return $"Stk[{r1}]=Stk[{r2}]+Stk[{r3}];Stk[{r4}]=Stk[{r1}]*Stk[{r2}];";
                case 1:
                    return $"local _j{r1}=Stk[{r2}];Stk[{r3}]=_j{r1}..Stk[{r4}];";
                case 2:
                    return $"if Stk[{r1}]==Stk[{r2}]then Stk[{r3}]=true else Stk[{r4}]=false end;";
                case 3:
                    return $"Stk[{r1}]={{}};for _j{r2}=1,Stk[{r3}]do Stk[{r1}][_j{r2}]=Stk[{r4}]end;";
                case 4:
                    return $"Stk[{r1}]=Stk[{r2}]/Stk[{r3}];Stk[{r4}]=Stk[{r1}]^{(r1+r2)};";
                default:
                    return $"Stk[{r1}] = nil;";
            }
        }
    }
}
