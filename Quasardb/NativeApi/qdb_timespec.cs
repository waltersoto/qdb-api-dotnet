﻿using System.Runtime.InteropServices;
using qdb_time_t = System.Int64;

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace Quasardb.NativeApi
{
    [StructLayout(LayoutKind.Sequential)]
    struct qdb_timespec
    {
        public qdb_time_t tv_sec;
        public qdb_time_t tv_nsec;

        public static qdb_timespec MinValue => new qdb_timespec {tv_sec = 0, tv_nsec = 0};
        public static qdb_timespec MaxValue => new qdb_timespec {tv_sec = qdb_time_t.MaxValue, tv_nsec = 999999999};
    };
}