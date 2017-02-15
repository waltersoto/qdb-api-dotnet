﻿using System;
using Quasardb.Exceptions;
using Quasardb.ManagedApi;

namespace Quasardb
{
    /// <summary>
    /// A double-ended queue in the database.
    /// </summary>
    /// <example>
    /// Here is how to put a blob in the database:
    /// <code language="c#">
    /// byte[] myData;
    /// var cluster = new QdbCluster("qdb://127.0.0.1:2836");
    ///
    /// cluster.Deque("some name").PushBack(myData);
    /// </code>
    /// <code language="vb">
    /// Dim myData As Byte()
    /// Dim cluster = New QdbCluster("qdb://127.0.0.1:2836")
    ///
    /// cluster.Deque("some name").PushBack(myData)
    /// </code>
    /// </example>
    public sealed class QdbDeque : QdbEntry
    {
        internal QdbDeque(QdbApi api, string alias) : base(api, alias)
        {
        }

        /// <summary>
        /// Gets the value at the end of the queue.
        /// </summary>
        /// <returns>The value at the end of the queue, or null if the queue is empty.</returns>
        /// <exception cref="QdbAliasNotFoundException">The queue doesn't exists in the database.</exception>
        /// <exception cref="QdbIncompatibleTypeException">The matching entry in the database is not a queue.</exception>
        public byte[] Back()
        {
            return Api.DequeBack(Alias);
        }

        /// <summary>
        /// Gets the value at the beginning of the queue.
        /// </summary>
        /// <returns>The value at the beginning of the queue, or null if the queue is empty.</returns>
        /// <exception cref="QdbAliasNotFoundException">The queue doesn't exists in the database.</exception>
        /// <exception cref="QdbIncompatibleTypeException">The matching entry in the database is not a queue.</exception>
        public byte[] Front()
        {
            return Api.DequeFront(Alias);
        }

        /// <summary>
        /// Dequeues a value from the end of the queue.
        /// </summary>
        /// <returns>The value from the end of the queue, or null if the queue is empty.</returns>
        /// <exception cref="QdbAliasNotFoundException">The queue doesn't exists in the database.</exception>
        /// <exception cref="QdbIncompatibleTypeException">The matching entry in the database is not a queue.</exception>
        public byte[] PopBack()
        {
            return Api.DequePopBack(Alias);
        }

        /// <summary>
        /// Dequeues a value from the beginning of the queue.
        /// </summary>
        /// <returns>The value from the beginning of the queue, or null if the queue is empty.</returns>
        /// <exception cref="QdbAliasNotFoundException">The queue doesn't exists in the database.</exception>
        /// <exception cref="QdbIncompatibleTypeException">The matching entry in the database is not a queue.</exception>
        public byte[] PopFront()
        {
            return Api.DequePopFront(Alias);
        }

        /// <summary>
        /// Enqueues a value at the end of the queue.
        /// </summary>
        /// <param name="content">The value to enqueue.</param>
        /// <exception cref="QdbIncompatibleTypeException">The matching entry in the database is not a queue.</exception>
        public void PushBack(byte[] content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            Api.DequePushBack(Alias, content);
        }

        /// <summary>
        /// Enqueues a value at the beginning of the queue.
        /// </summary>
        /// <param name="content">The value to enqueue.</param>
        /// <exception cref="QdbIncompatibleTypeException">The matching entry in the database is not a queue.</exception>
        public void PushFront(byte[] content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            Api.DequePushFront(Alias, content);
        }

        /// <summary>
        /// Gets the number of elements in the queue.
        /// </summary>
        /// <returns>The length of the queue</returns>
        /// <exception cref="QdbAliasNotFoundException">The queue doesn't exists in the database.</exception>
        /// <exception cref="QdbIncompatibleTypeException">The matching entry in the database is not a queue.</exception>
        public long Size()
        {
            return Api.DequeSize(Alias);
        }

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element</param>
        /// <returns>The content of the element.</returns>
        /// <exception cref="QdbAliasNotFoundException">The queue doesn't exists in the database.</exception>
        /// <exception cref="QdbIncompatibleTypeException">The matching entry in the database is not a queue.</exception>
        public byte[] GetAt(long index)
        {
            return Api.DequeGetAt(Alias, index);
        }

        /// <summary>
        /// Sets the element at the specified index.
        /// </summary>
        /// <remarks>
        /// This methods cannot add new elements to the deque; it only replaces an existing one.
        /// </remarks>
        /// <param name="index">The zero-based index of the element</param>
        /// <param name="content">The new content</param>
        /// <exception cref="QdbAliasNotFoundException">The queue doesn't exists in the database.</exception>
        /// <exception cref="QdbIncompatibleTypeException">The matching entry in the database is not a queue.</exception>
        /// <exception cref="QdbOutOfBoundsException">The index is out of bounds.</exception>
        public void SetAt(long index, byte[] content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            Api.DequeSetAt(Alias, index, content);
        }

        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element</param>
        /// <exception cref="QdbAliasNotFoundException">The queue doesn't exists in the database.</exception>
        /// <exception cref="QdbIncompatibleTypeException">The matching entry in the database is not a queue.</exception>
        public byte[] this[long index]
        {
            get { return GetAt(index); }
            set { SetAt(index, value); }
        }
    }
}
