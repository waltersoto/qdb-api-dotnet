﻿using Quasardb.Exceptions;
using Quasardb.Interop;

namespace Quasardb
{
    /// <summary>
    /// An entry in a quasardb database.
    /// </summary>
    public abstract class QdbEntry
    {
        internal QdbEntry(qdb_handle handle, string alias)
        {
            Alias = alias;
            Handle = handle;
        }
        
        /// <summary>
        /// The alias of the entry in the database.
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// The C API handle.
        /// </summary>
        internal qdb_handle Handle { get; private set; }

        /// <summary>
        /// Deletes the entry.
        /// </summary>
        /// <exception cref="QdbAliasNotFoundException">The entry doesn't exists in the database.</exception>
        public void Remove()
        {
            var error = qdb_api.qdb_remove(Handle, Alias);
            QdbExceptionThrower.ThrowIfNeeded(error);
        }
    }
}