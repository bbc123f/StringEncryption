using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StringEncryption.Obfuscation
{
    internal class XOR
    {
        class StringToXorUtil
        {
            public static string XorString(string input, string key)
            {
                if (string.IsNullOrEmpty(input))
                    throw new ArgumentException("Input cannot be null or empty.");

                if (string.IsNullOrEmpty(key))
                    throw new ArgumentException("Key cannot be null or empty.");

                int keyLength = key.Length;
                StringBuilder sb = new StringBuilder(input.Length);

                for (int i = 0; i < input.Length; i++)
                {
                    char c = input[i];
                    char k = key[i % keyLength];
                    sb.Append((char)(c ^ k));
                }

                return sb.ToString();
            }
        }

        static byte[] GenerateRandomKey()
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                return aes.Key;
            }
        }

        public static void Execute(ModuleDefMD module)
        {
            var injDec = new Injector(module, typeof(StringToXorUtil), module.GlobalType);
            var decryptCall = injDec.FindMember("XorString") as MethodDef;
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
                            if (!string.IsNullOrEmpty(oldString) && oldString.Length > 0)
                            {
                                var aesKey = Convert.ToBase64String(GenerateRandomKey());
                                string encryptedString = StringToXorUtil.XorString(oldString, aesKey);
                                instr[i].OpCode = OpCodes.Nop;
                                instr[i].Operand = null;
                                instr.Insert(i + 1, Instruction.Create(OpCodes.Ldstr, encryptedString));
                                instr.Insert(i + 2, Instruction.Create(OpCodes.Ldstr, aesKey));
                                instr.Insert(i + 3, Instruction.Create(OpCodes.Call, decryptCall));
                                i += 3;
                            }
                        }

                        method.Body.SimplifyBranches();
                        instr.OptimizeBranches();
                    }
                }
            }
        }

    }
}
