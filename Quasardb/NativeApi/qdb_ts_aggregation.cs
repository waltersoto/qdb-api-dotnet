﻿using System;
using System.Runtime.InteropServices;

// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable InconsistentNaming

namespace Quasardb.NativeApi
{
    [StructLayout(LayoutKind.Sequential)]
    struct qdb_ts_aggregation
    {
        public qdb_ts_range range;
        public qdb_ts_double_point result;
    };
}
