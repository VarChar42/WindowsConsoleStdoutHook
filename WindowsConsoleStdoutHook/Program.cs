using System.Diagnostics;

namespace WindowsConsoleStdoutHook
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var hooker = new HandleHooker(5612);
            hooker.PrepareProcess();

            while (true) Debug.Write(hooker.NextLine());
        }
    }
}