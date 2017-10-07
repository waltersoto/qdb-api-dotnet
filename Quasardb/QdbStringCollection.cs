﻿using Quasardb.Native;

namespace Quasardb
{
    sealed unsafe class QdbStringCollection : qdb_buffer<qdb_string>
    {
        internal QdbStringCollection(qdb_handle handle) : base(handle)
        {
        }

        protected override qdb_string Dereference(void* p, ulong i)
        {
            return ((qdb_string*) p)[i];
        }

        protected override void Free()
        {
            qdb_api.qdb_free_results(_handle, Pointer, Size);
        }
    }
}