﻿namespace Quasardb.Native
{
    internal enum qdb_query_result_value_type
    {
        None = -1,
        Double = 0,
        Blob = 1,
        Int64 = 2,
        Timestamp = 3,
        Count = 4,
        String = 5,
        Symbol = 6
    }
}
