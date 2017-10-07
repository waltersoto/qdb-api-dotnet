﻿using System;
using Quasardb.Exceptions;
using Quasardb.Native;

namespace Quasardb.ManagedApi
{
    partial class QdbApi
    {
        public long IntegerAdd(string alias, long addend)
        {
            long result;
            var error = qdb_api.qdb_int_add(_handle, alias, addend, out result);
            QdbExceptionThrower.ThrowIfNeeded(error, alias: alias);
            return result;
        }

        public long IntegerGet(string alias)
        {
            long value;
            var error = qdb_api.qdb_int_get(_handle, alias, out value);
            QdbExceptionThrower.ThrowIfNeeded(error, alias: alias);
            return value;
        }

        public void IntegerPut(string alias, long value, DateTime? expiryTime)
        {
            var error = qdb_api.qdb_int_put(_handle, alias, value, qdb_time.FromOptionalDateTime(expiryTime));
            QdbExceptionThrower.ThrowIfNeeded(error, alias: alias);
        }

        public bool IntegerUpdate(string alias, long value, DateTime? expiryTime = null)
        {
            var error = qdb_api.qdb_int_update(_handle, alias, value, qdb_time.FromOptionalDateTime(expiryTime));

            switch (error)
            {
                case qdb_error_t.qdb_e_ok:
                    return false;

                case qdb_error_t.qdb_e_ok_created:
                    return true;

                default:
                    throw QdbExceptionFactory.Create(error, alias: alias);
            }
        }
    }
}
