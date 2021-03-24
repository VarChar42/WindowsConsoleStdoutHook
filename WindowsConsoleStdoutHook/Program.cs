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
                hooker.PrepareProcess();
                Debug.WriteLine("[18836]: " + hooker.NextOutLine().Trim());
                hooker2.PrepareProcess();
                Debug.WriteLine("[22680]: " + hooker2.NextErrLine().Trim());
            }
        }
    }
}