using System;
using System.Reflection;

class Program
{
    static void Main()
    {
        var asm = Assembly.LoadFrom(@"C:\Users\Nolan\AppData\Roaming\XIVLauncher\addon\Hooks\dev\Dalamud.dll");
        foreach (var type in asm.GetTypes())
        {
            if (type.Name.Contains("PlayerCharacter") || type.Name.Contains("IPlayerCharacter") || type.Name.Contains("ObjectKind"))
            {
                Console.WriteLine(type.FullName);
            }
        }
    }
}
