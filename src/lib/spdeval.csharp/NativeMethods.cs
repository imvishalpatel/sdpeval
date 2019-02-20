﻿using System;
using System.Runtime.InteropServices;

namespace spdeval.csharp
{
    public static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SystemInfo
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public int dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public IntPtr dwActiveProcessorMask;
            public int dwNumberOfProcessors;
            public int dwProcessorType;
            public int dwAllocationGranularity;
            public short wProcessorLevel;
            public short wProcessorRevision;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern void GetSystemInfo(ref SystemInfo lpSystemInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern void GetNativeSystemInfo(out SystemInfo lpSystemInfo);

        public static SystemInfo GetSystemInfo()
        {
            SystemInfo systemInfo = default(SystemInfo);
            GetSystemInfo(ref systemInfo);
            return systemInfo;
        }

        public static SystemInfo GetNativeSystemInfo()
        {
            GetNativeSystemInfo(out var systemInfo);
            return systemInfo;
        }
    }
}
