namespace StringEncryption
{
    using dnlib;
    using dnlib.DotNet;
    using StringEncryption.Obfuscation;

    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Write("Put your file path here: ");
                var file = Console.ReadLine().Trim('"');

                Console.WriteLine();

                var assembly = ModuleDefMD.Load(file);
                if (assembly == null) throw new NullReferenceException();

                XOR.Execute(assembly);
                StringToHex.Execute(assembly);
                Base64.Execute(assembly);
                StringToROT13.Execute(assembly);

                assembly.Write(file.Replace(".dll", "-enc.dll"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
