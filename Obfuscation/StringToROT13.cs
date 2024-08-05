using dnlib.DotNet.Emit;
using dnlib.DotNet;
using System;

namespace StringEncryption.Obfuscation
{
    public static class StringToRot13Utils
    {
        public static string StringToRot13(string input)
        {
            char[] arr = input.ToCharArray();
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] >= 'a' && arr[i] <= 'z')
                {
                    arr[i] = (char)(((arr[i] - 'a' + 13) % 26) + 'a');
                }
                else if (arr[i] >= 'A' && arr[i] <= 'Z')
                {
                    arr[i] = (char)(((arr[i] - 'A' + 13) % 26) + 'A');
                }
            }
            return new string(arr);
        }
    }
    public class StringToROT13
    {
        public static string Rot13Encrypt(string input)
        {
            return StringToRot13Utils.StringToRot13(input);
        }

        public static void Execute(ModuleDefMD module)
        {
            var injDec = new Injector(module, typeof(StringToRot13Utils));
            var decryptCall = injDec.FindMember("StringToRot13") as MethodDef;

            foreach (TypeDef type in module.Types)
            {
                if (type.IsGlobalModuleType || type.Namespace == "Costura")
                    continue;
                foreach (MethodDef method in type.Methods)
                {
                    if (method.HasBody)
                    {
                        var instr = method.Body.Instructions;
                        method.Body.SimplifyBranches();
                        instr.OptimizeBranches();

                        for (var i = 0; i < instr.Count; i++)
                        {
                            if (instr[i].OpCode != OpCodes.Ldstr) continue;

                            string oldString = instr[i].Operand.ToString();
                            string encryptedString = Rot13Encrypt(oldString);
                            instr[i].OpCode = OpCodes.Nop;
                            instr[i].Operand = null;
                            instr.Insert(i + 1, Instruction.Create(OpCodes.Ldstr, encryptedString));
                            instr.Insert(i + 2, Instruction.Create(OpCodes.Call, decryptCall));
                            i += 2;
                        }

                        method.Body.SimplifyBranches();
                        instr.OptimizeBranches();
                    }
                }
            }
        }

    }
}
