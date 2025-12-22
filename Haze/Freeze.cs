using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WpfApp6.Services.Launch
{
    public static class Freeze69
    {
        private const int THREAD_SUSPEND_RESUME = 0x0002;

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenThread(int dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint SuspendThread(IntPtr hThread);

        public static void Freeze(this Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(THREAD_SUSPEND_RESUME, false, (uint)thread.Id);
                if (pOpenThread == IntPtr.Zero)
                    continue;

                SuspendThread(pOpenThread);
            }
        }
    }
}