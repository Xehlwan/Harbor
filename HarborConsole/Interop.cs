﻿using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace Harbor.Console
{
    internal static class Interop
    {
        private const int stdOutputHandle = -11;

        /// <summary>
        /// Activate Virtual Terminal commands on Windows console, through the use of Win32-API calls.
        /// </summary>
        public static void ActivateVT()
        {
            const uint vtFlags = 0x0004 | 0x0008;
            SafeFileHandle handle = GetOutHandle();
            uint flags = GetOuputMode(handle);
            flags |= vtFlags;
            SetOutputMode(handle, flags);
        }

        public static void HideConsole() => FreeConsole();

        public static void ShowConsole()
        {
            AllocConsole();
            TextWriter writer = new StreamWriter(System.Console.OpenStandardOutput()) { AutoFlush = true };
            TextWriter error = new StreamWriter(System.Console.OpenStandardError()) {AutoFlush = true};
            TextReader reader = new StreamReader(System.Console.OpenStandardInput());
            System.Console.SetOut(writer);
            System.Console.SetError(error);
            System.Console.SetIn(reader);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode([In] SafeFileHandle hConsoleHandle, [Out] out uint lpMode);

        private static uint GetOuputMode(SafeFileHandle handle)
        {
            uint flags;

            if (!GetConsoleMode(handle, out flags)) throw new Win32Exception();

            return flags;
        }

        private static SafeFileHandle GetOutHandle()
        {
            var handle = new SafeFileHandle(GetStdHandle(stdOutputHandle), false);

            if (handle.IsInvalid) throw new Win32Exception();

            return handle;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle([In] [MarshalAs(UnmanagedType.U4)] int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode([In] SafeFileHandle hConsoleHandle, [In] uint dwMode);

        private static void SetOutputMode(SafeFileHandle handle, uint flags)
        {
            if (!SetConsoleMode(handle, flags)) throw new Win32Exception();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole([In] int dwProcessId);
    }
}