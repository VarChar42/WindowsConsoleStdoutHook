using System.Diagnostics;

namespace WindowsConsoleStdoutHook
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var hooker = new HandleHooker(18852);
            hooker.PrepareProcess();

            while (true) Debug.Write(hooker.NextLine());
        }
    }
}