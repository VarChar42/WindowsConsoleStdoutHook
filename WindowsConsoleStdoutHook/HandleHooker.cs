using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowsConsoleStdoutHook
{
    internal class HandleHooker
    {
        private const int StdOutputHandle = -11;
        private const long InvalidHandleValue = -1;
        private readonly int pid;
        private short currentLinePos;
        private IntPtr stdoutHandle;

        public HandleHooker(int pid)
        {
            this.pid = pid;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool ReadConsoleOutputCharacter(IntPtr hConsoleOutput,
            [Out] StringBuilder lpCharacter, uint nLength, RemoteConsoleCursor dwReadRemoteConsoleCursor,
            out uint lpNumberOfCharsRead);

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleScreenBufferInfo(
            IntPtr hConsoleOutput,
            out RemoteScreenBuffer lpRemoteScreenBuffer
        );

        private string ReadLine(IntPtr stdout)
        {
            if (stdout.ToInt32() == InvalidHandleValue)
                throw new Win32Exception(); // Bled .. des soit ned passiern


            if (!GetConsoleScreenBufferInfo(stdout, out var outInfo))
                throw new Win32Exception(); // Dann hod der target process ka console fenster.


            var lineSize = outInfo.dwSize.X;


            var linesToRead = (uint) (outInfo.dwCursorPosition.Y - currentLinePos);

            if (linesToRead < 1) return null;


            RemoteConsoleCursor dwReadRemoteConsoleCursor;
            dwReadRemoteConsoleCursor.X = 0;
            dwReadRemoteConsoleCursor.Y = currentLinePos;

            var nLength = (uint) lineSize * linesToRead + 2 * linesToRead;

            var result = new StringBuilder((int) nLength);
            var lpCharacter = new StringBuilder(lineSize);
            for (var i = 0; i < linesToRead; i++)
            {
                if (!ReadConsoleOutputCharacter(stdout, lpCharacter, (uint) lineSize, dwReadRemoteConsoleCursor,
                    out var lpNumberOfCharsRead))
                    throw new Win32Exception();
                result.AppendLine(lpCharacter.ToString(0, (int) lpNumberOfCharsRead - 1));
                dwReadRemoteConsoleCursor.Y++;
            }

            currentLinePos = outInfo.dwCursorPosition.Y;
            return result.ToString();
        }

        public void PrepareProcess()
        {
            if (!FreeConsole() || !AttachConsole(pid)) return;

            stdoutHandle = GetStdHandle(StdOutputHandle);
        }

        public string NextLine()
        {
            return ReadLine(stdoutHandle);
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct RemoteConsoleCursor
        {
            public short X;
            public short Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct RemoteScreenBuffer
        {
            public readonly RemoteConsoleCursor dwSize;
            public readonly RemoteConsoleCursor dwCursorPosition;
            private readonly short wAttributes;
            private readonly ConsoleRect srWindow;
            private readonly RemoteConsoleCursor dwMaximumWindowSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct ConsoleRect
        {
            private readonly short Left;
            private readonly short Top;
            private readonly short Right;
            private readonly short Bottom;
        }
    }
}