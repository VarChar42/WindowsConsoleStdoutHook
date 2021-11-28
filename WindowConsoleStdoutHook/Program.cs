using System.Diagnostics;

namespace WindowConsoleStdoutHook
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var hooker = new HandleHooker(8976);


            hooker.NewOutLine += HandleOutLine;
            hooker.NewErrLine += HandleErrLine;


            hooker.Hook();
            Console.WriteLine("You have been hooked");
            hooker.Start();

        }

        public static void HandleOutLine(object? sender, string line)
        {
            Debug.WriteLine(line);
        }

        public static void HandleErrLine(object? sender, string line)
        {
            Debug.WriteLine(line);
        }
    }
}