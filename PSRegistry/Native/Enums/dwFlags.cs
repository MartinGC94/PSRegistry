using System;

namespace MartinGC94.PSRegistry.Native.Enums
{
    [Flags]
    internal enum DwFlags
    {
        // Type restrictions
        RRF_RT_ANY = 0x0000FFFF,
        RRF_RT_REG_NONE = 0x00000001,
        RRF_RT_REG_SZ = 0x00000002,
        RRF_RT_REG_EXPAND_SZ = 0x00000004,
        RRF_RT_REG_BINARY = 0x00000008,
        RRF_RT_REG_DWORD = 0x00000010,
        RRF_RT_REG_MULTI_SZ = 0x00000020,
        RRF_RT_REG_QWORD = 0x00000040,

        // Combined type restrictions
        RRF_RT_DWORD = RRF_RT_REG_BINARY | RRF_RT_REG_DWORD,
        RRF_RT_QWORD = RRF_RT_REG_BINARY | RRF_RT_REG_QWORD,

        // Additional flags
        RRF_SUBKEY_WOW6464KEY = 0x00010000,
        RRF_SUBKEY_WOW6432KEY = 0x00020000,
        RRF_NOEXPAND = 0x10000000,
        RRF_ZEROONFAILURE = 0x20000000

    }
}