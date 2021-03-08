using System.Diagnostics;

namespace WindowsConsoleStdoutHook
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var hooker = new HandleHooker(18836);
            var hooker2 = new HandleHooker(22680);


            while (true)
            {
                Debug.WriteLine("[18836]: " + hooker.NextLine().Trim());
                Debug.WriteLine("[22680]: " + hooker2.NextLine().Trim());
            }
        }
    }
}