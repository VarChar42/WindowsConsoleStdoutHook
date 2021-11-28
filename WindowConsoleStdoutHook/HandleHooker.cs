using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowConsoleStdoutHook
{
    internal class HandleHooker
    {

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


        private const int STD_INPUT_HANDLE = -10;
        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_ERROR_HANDLE = -12;
        private const long InvalidHandleValue = -1;
        private readonly int pid;
        private short currentLinePos;
        private IntPtr stdoutHandle;
        private IntPtr stderrHandle;


        public event EventHandler<string> NewOutLine;
        public event EventHandler<string> NewErrLine;

        public HandleHooker(int pid)
        {
            this.pid = pid;
        }

        public void Hook()
        {
            PrepareProcess();
        }

        public void Start() {
            while (true)
            {
                string outLine = ReadLine(stdoutHandle);
                string errLine = ReadLine(stderrHandle);

                if (outLine != null) NewOutLine?.Invoke(this, outLine);
                if (errLine != null) NewErrLine?.Invoke(this, errLine);

                Thread.Sleep(500);
            }
        }


        private string ReadLine(IntPtr stdout)
        {
            if (stdout.ToInt32() == InvalidHandleValue)
                throw new Win32Exception("Cannot get console handle");


            if (!GetConsoleScreenBufferInfo(stdout, out var outInfo))
                throw new Win32Exception("Target process does not have console handle");

            var lineSize = outInfo.dwSize.X;

            var linesToRead = (uint)(outInfo.dwCursorPosition.Y - currentLinePos);

            if (linesToRead < 1) return null;


            RemoteConsoleCursor remoteCursor;
            remoteCursor.X = 0;
            remoteCursor.Y = currentLinePos;

            var nLength = (uint)lineSize * linesToRead + 2 * linesToRead;

            var result = new StringBuilder((int)nLength); // Buffer whole output
            var lineBuilder = new StringBuilder(lineSize); // Buffer current line
            for (var i = 0; i < linesToRead; i++)
            {
                if (!ReadConsoleOutputCharacter(stdout, lineBuilder, (uint)lineSize, remoteCursor,
                    out var lpNumberOfCharsRead))
                    throw new Win32Exception();
                result.AppendLine(lineBuilder.ToString(0, (int)lpNumberOfCharsRead - 1));
                remoteCursor.Y++;
            }

            currentLinePos = outInfo.dwCursorPosition.Y;
            return result.ToString();
        }

        private void PrepareProcess()
        {
            if (!FreeConsole() || !AttachConsole(pid)) return;

            stdoutHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            stderrHandle = GetStdHandle(STD_ERROR_HANDLE);
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