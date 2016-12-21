﻿using Quasardb.Exceptions;
using Quasardb.NativeApi;

namespace Quasardb.ManagedApi
{
    partial class QdbApi
    {
        public bool AttachTag(string alias, string tag)
        {
            var error = qdb_api.qdb_attach_tag(_handle, alias, tag);

            switch (error)
            {
                case qdb_error_t.qdb_e_tag_already_set:
                    return false;

                case qdb_error_t.qdb_e_ok:
                    return true;

                default:
                    throw QdbExceptionFactory.Create(error);
            }
        }

        public QdbAliasCollection GetTagged(string tag)
        {
            var result = new QdbAliasCollection(_handle);

            var error = qdb_api.qdb_get_tagged(_handle, tag, out result.Pointer, out result.Size);
            QdbExceptionThrower.ThrowIfNeeded(error);

            return result;
        }

        public QdbAliasCollection GetTags(string alias)
        {
            var result = new QdbAliasCollection(_handle);

            var error = qdb_api.qdb_get_tags(_handle, alias, out result.Pointer, out result.Size);
            QdbExceptionThrower.ThrowIfNeeded(error);

            return result;
        }

        public bool HasTag(string alias, string tag)
        {
            var error = qdb_api.qdb_has_tag(_handle, alias, tag);

            switch (error)
            {
                case qdb_error_t.qdb_e_tag_not_set:
                    return false;

                case qdb_error_t.qdb_e_ok:
                    return true;

                default:
                    throw QdbExceptionFactory.Create(error);
            }
        }

        public bool DetachTag(string alias, string tag)
        {
            var error = qdb_api.qdb_detach_tag(_handle, alias, tag);

            switch (error)
            {
                case qdb_error_t.qdb_e_tag_not_set:
                    return false;

                case qdb_error_t.qdb_e_ok:
                    return true;

                default:
                    throw QdbExceptionFactory.Create(error);
            }
        }
    }
}
