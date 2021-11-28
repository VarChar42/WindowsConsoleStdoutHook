#region usings

using System;
using System.Diagnostics;

#endregion

namespace WindowConsoleStdoutHook
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var hooker = new HandleHooker(4260);

            hooker.NewOutLine += HandleOutLine;
            hooker.NewErrLine += HandleErrLine;
            hooker.HookedConsoleClosing += HandleClose;
            hooker.ReadException += HandleException;

            hooker.Hook();
            Console.WriteLine("Hooked");
            hooker.Start();
        }

        #region Event Handlers

        private static void HandleClose(object? sender, EventArgs e)
        {
            Debug.WriteLine("Closing!!");
        }

        public static void HandleErrLine(object? sender, string line)
        {
            Debug.WriteLine(line);
        }

        private static void HandleException(object? sender, Exception e)
        {
            Debug.WriteLine(e.ToString());
        }

        public static void HandleOutLine(object? sender, string line)
        {
            Debug.WriteLine(line);
        }

        #endregion
    }
}
