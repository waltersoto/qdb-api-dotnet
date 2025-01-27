﻿using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quasardb.Exceptions;
using Quasardb.Query;
using Quasardb.TimeSeries;

namespace Quasardb.Tests.Query
{
    [TestClass]
    public class Query
    {
        private readonly QdbCluster _cluster = QdbTestCluster.Instance;

        public QdbTable CreateTable(string alias = null)
        {
            var ts = _cluster.Table(alias ?? RandomGenerator.CreateUniqueAlias());
            ts.Create(new QdbColumnDefinition[] {
                new QdbBlobColumnDefinition("the_blob"),
                new QdbDoubleColumnDefinition("the_double"),
                new QdbInt64ColumnDefinition("the_int64"),
                new QdbStringColumnDefinition("the_string"),
                new QdbTimestampColumnDefinition("the_ts"),
            });
            return ts;
        }

        public QdbBlobPointCollection InsertBlobPoints(QdbTable ts, DateTime time, int count)
        {
            Random random = new Random();
            var r = new QdbBlobPointCollection(count);

            var column = ts.BlobColumns["the_blob"];
            for (int i = 0; i < count; ++i)
            {
                var value = new byte[32];
                random.NextBytes(value);
                column.Insert(time, value);
                r.Add(time, value);
                time = time.AddSeconds(1);
            }
            return r;
        }

        public QdbDoublePointCollection InsertDoublePoints(QdbTable ts, DateTime time, int count)
        {
            Random random = new Random();
            var r = new QdbDoublePointCollection(count);

            var column = ts.DoubleColumns["the_double"];
            for (int i = 0; i < count; ++i)
            {
                var value = random.NextDouble();
                column.Insert(time, value);
                r.Add(time, value);
                time = time.AddSeconds(1);
            }
            return r;
        }

        public QdbInt64PointCollection InsertInt64Points(QdbTable ts, DateTime time, int count)
        {
            Random random = new Random();
            var r = new QdbInt64PointCollection(count);

            var column = ts.Int64Columns["the_int64"];
            for (int i = 0; i < count; ++i)
            {
                var value = random.Next();
                column.Insert(time, value);
                r.Add(time, value);
                time = time.AddSeconds(1);
            }
            return r;
        }

        public static string GenerateRandomAlphanumericString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random = new Random();
            var randomString = new string(Enumerable.Repeat(chars, length)
                                                    .Select(s => s[random.Next(s.Length)]).ToArray());
            return randomString;
        }

        public QdbStringPointCollection InsertStringPoints(QdbTable ts, DateTime time, int count)
        {
            var r = new QdbStringPointCollection(count);

            var column = ts.StringColumns["the_string"];
            for (int i = 0; i < count; ++i)
            {
                var value = GenerateRandomAlphanumericString(32);

                column.Insert(time, value);
                r.Add(time, value);
                time = time.AddSeconds(1);
            }
            return r;
        }

        public QdbStringPointCollection InsertInvalidStringPoints(QdbTable ts, DateTime time, int count)
        {
            var r = new QdbStringPointCollection(count);

            var column = ts.StringColumns["the_string"];

            byte[] bytes = { (byte)'\xfe', (byte)'\xfe', (byte)'\xff', (byte)'\xff' };
            var value = Encoding.UTF8.GetString(bytes);

            for (int i = 0; i < count; ++i)
            {
                column.Insert(time, value);
                r.Add(time, value);
                time = time.AddSeconds(1);
            }
            return r;
        }

        public QdbTimestampPointCollection InsertTimestampPoints(QdbTable ts, DateTime time, int count)
        {
            Random random = new Random();
            var r = new QdbTimestampPointCollection(count);

            var column = ts.TimestampColumns["the_ts"];
            for (int i = 0; i < count; ++i)
            {
                var value = DateTime.Today.AddSeconds(random.NextDouble());
                column.Insert(time, value);
                r.Add(time, value);
                time = time.AddSeconds(1);
            }
            return r;
        }

        private void CheckColumns(QdbColumnNameCollection columns)
        {
            Assert.AreEqual(7, columns.Count);
            Assert.AreEqual("$timestamp", columns[0]);
            Assert.AreEqual("$table", columns[1]);
            Assert.AreEqual("the_blob", columns[2]);
            Assert.AreEqual("the_double", columns[3]);
            Assert.AreEqual("the_int64", columns[4]);
            Assert.AreEqual("the_string", columns[5]);
            Assert.AreEqual("the_ts", columns[6]);
        }

        #region Query failure tests

        [TestMethod]
        [ExpectedException(typeof(QdbQueryException))]
        public void ThrowsForNullQuery()
        {
            _cluster.Query(null);
        }

        [TestMethod]
        [ExpectedException(typeof(QdbQueryException))]
        public void ThrowsForEmptyQuery()
        {
            _cluster.Query("");
        }

        [TestMethod]
        [ExpectedException(typeof(QdbQueryException))]
        public void ThrowsForInvalidQuery()
        {
            _cluster.Query("select * from");
        }

        [TestMethod]
        [ExpectedException(typeof(QdbQueryException))]
        public void ThrowsWhenTsDoesntExist()
        {
            _cluster.Query("select * from this_ts_doesnt_exist in range(2017, +10d)");
        }

        [TestMethod]
        [ExpectedException(typeof(QdbQueryException))]
        public void ThrowsWhenUntagged()
        {
            _cluster.Query("select * from find(tag='this_tag_doesnt_exist') in range(2017, +10d)");
        }

        [TestMethod]
        [ExpectedException(typeof(QdbQueryException))]
        public void ThrowsWhenColumnNotFound()
        {
            var ts = _cluster.Table(RandomGenerator.CreateUniqueAlias());
            ts.Create(new QdbColumnDefinition[] { });
            try
            {
                _cluster.Query("select this_column_doesnt_exist from " + ts.Alias + " in range(2017, +10d)");
            }
            finally
            {
                ts.Remove();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(DecoderFallbackException))]
        public void ThrowsWhenInvalidString()
        {
            var startTime = DateTime.Now;
            QdbTable ts = CreateTable();
            var insertedStringData = InsertInvalidStringPoints(ts, startTime, 10);
            try
            {
                var results = _cluster.Query("select * from " + ts.Alias);
                CheckColumns(results.ColumnNames);

                var rows = results.Rows;
                Assert.AreEqual(10L, rows.Count);
                for (int i = 0; i < 10L; ++i)
                {
                    var row = rows[i];
                    Assert.AreEqual(insertedStringData[i].Value, row["the_string"].Value);
                    Assert.AreEqual(ts.Alias, row["$table"].StringValue);
                }
            }
            finally
            {
                ts.Remove();
            }
        }

        #endregion

        #region Data tests

        [TestMethod]
        public void ReturnsEmptyResult()
        {
            QdbTable ts = CreateTable();
            try
            {
                var results = _cluster.Query("select * from " + ts.Alias + " in range(2016, +1y)");
                CheckColumns(results.ColumnNames);

                var rows = results.Rows;
                Assert.AreEqual(0, rows.Count);
            }
            finally
            {
                ts.Remove();
            }
        }

        [TestMethod]
        public void ReturnsEmptyResultWhenNoResultsAreAvailable()
        {
            QdbTable ts = CreateTable();
            try
            {
                var results = _cluster.Query("DROP TABLE " + ts.Alias);

                var rows = results.Rows;
                Assert.AreEqual(0, rows.Count);
            }
            finally
            {
            }
        }

        [TestMethod]
        public void ReturnsInsertedDataWithStarSelect_NonNullBlob()
        {
            var startTime = DateTime.Now;
            QdbTable ts = CreateTable();
            var insertedBlobData = InsertBlobPoints(ts, startTime, 10);
            try
            {
                var results = _cluster.Query("select * from " + ts.Alias);
                CheckColumns(results.ColumnNames);

                var rows = results.Rows;
                Assert.AreEqual(10L, rows.Count);
                for (int i = 0; i < 10L; ++i)
                {
                    var row = rows[i];
                    //Assert.AreEqual(insertedBlobData[i].Value, (byte[])row["the_blob"].Value);
                    Assert.AreEqual(System.Text.Encoding.Default.GetString(insertedBlobData[i].Value),
                        System.Text.Encoding.Default.GetString((byte[])row["the_blob"].Value));
                    Assert.AreEqual(null, row["the_double"].Value);
                    Assert.AreEqual(null, row["the_int64"].Value);
                    Assert.AreEqual(null, row["the_string"].Value);
                    Assert.AreEqual(null, row["the_ts"].Value);
                    Assert.AreEqual(ts.Alias, row["$table"].StringValue);
                }
            }
            finally
            {
                ts.Remove();
            }
        }

        [TestMethod]
        public void ReturnsInsertedDataWithStarSelect_NonNullDouble()
        {
            var startTime = DateTime.Now;
            QdbTable ts = CreateTable();
            var insertedDoubleData = InsertDoublePoints(ts, startTime, 10);
            try
            {
                var results = _cluster.Query("select * from " + ts.Alias);
                CheckColumns(results.ColumnNames);

                var rows = results.Rows;
                Assert.AreEqual(10L, rows.Count);
                for (int i = 0; i < 10L; ++i)
                {
                    var row = rows[i];
                    Assert.AreEqual(null, row["the_blob"].Value);
                    Assert.AreEqual(insertedDoubleData[i].Value, row["the_double"].Value);
                    Assert.AreEqual(null, row["the_int64"].Value);
                    Assert.AreEqual(null, row["the_string"].Value);
                    Assert.AreEqual(null, row["the_ts"].Value);
                    Assert.AreEqual(ts.Alias, row["$table"].StringValue);
                }
            }
            finally
            {
                ts.Remove();
            }
        }

        [TestMethod]
        public void ReturnsInsertedDataWithStarSelect_NonNullInt64()
        {
            var startTime = DateTime.Now;
            QdbTable ts = CreateTable();
            var insertedInt64Data = InsertInt64Points(ts, startTime, 10);
            try
            {
                var results = _cluster.Query("select * from " + ts.Alias);
                CheckColumns(results.ColumnNames);

                var rows = results.Rows;
                Assert.AreEqual(10L, rows.Count);
                for (int i = 0; i < 10L; ++i)
                {
                    var row = rows[i];
                    Assert.AreEqual(null, row["the_blob"].Value);
                    Assert.AreEqual(null, row["the_double"].Value);
                    Assert.AreEqual(insertedInt64Data[i].Value, row["the_int64"].Value);
                    Assert.AreEqual(null, row["the_string"].Value);
                    Assert.AreEqual(null, row["the_ts"].Value);
                    Assert.AreEqual(ts.Alias, row["$table"].StringValue);
                }
            }
            finally
            {
                ts.Remove();
            }
        }

        [TestMethod]
        public void ReturnsNullValues()
        {
            QdbTable ts = CreateTable();
            for (int i = 0; i < 10L; ++i)
            {
                _cluster.Query($"INSERT INTO {ts.Alias} ($timestamp, the_blob, the_double, the_int64, the_string, the_ts) VALUES (now(), NULL, NULL, NULL, NULL, NULL)");
            }
            try
            {
                var results = _cluster.Query("select * from " + ts.Alias);
                CheckColumns(results.ColumnNames);

                var rows = results.Rows;
                Assert.AreEqual(10L, rows.Count);
                for (int i = 0; i < 10L; ++i)
                {
                    var row = rows[i];
                    Assert.AreEqual(null, row["the_blob"].Value);
                    Assert.AreEqual(null, row["the_double"].Value);
                    Assert.AreEqual(null, row["the_int64"].Value);
                    Assert.AreEqual(null, row["the_string"].Value);
                    Assert.AreEqual(null, row["the_ts"].Value);
                    Assert.AreEqual(ts.Alias, row["$table"].StringValue);
                }
            }
            finally
            {
                ts.Remove();
            }
        }

        [TestMethod]
        public void ReturnsInsertedDataWithColumnSelect()
        {
            var startTime = DateTime.Now;
            QdbTable ts = CreateTable();
            var insertedDoubleData = InsertDoublePoints(ts, startTime, 10);
            try
            {
                var results = _cluster.Query("select the_double from " + ts.Alias);
                Assert.AreEqual(1L, results.ColumnNames.Count);
                Assert.AreEqual("the_double", results.ColumnNames[0]);

                var rows = results.Rows;
                Assert.AreEqual(10L, rows.Count);
                for (int i = 0; i < 10L; ++i)
                {
                    var row = rows[i];
                    Assert.AreEqual(insertedDoubleData[i].Value, row[0].Value);
                }
            }
            finally
            {
                ts.Remove();
            }
        }

        [TestMethod]
        public void ReturnsInsertedDataWithSpecificSelect()
        {
            var startTime = DateTime.Now;
            QdbTable ts = CreateTable();
            var insertedDoubleData = InsertDoublePoints(ts, startTime, 10);
            try
            {
                var results = _cluster.Query("select $timestamp, $table, the_double from " + ts.Alias);
                Assert.AreEqual(3, results.ColumnNames.Count);
                Assert.AreEqual("$timestamp", results.ColumnNames[0]);
                Assert.AreEqual("$table", results.ColumnNames[1]);
                Assert.AreEqual("the_double", results.ColumnNames[2]);

                var rows = results.Rows;
                Assert.AreEqual(10L, rows.Count);
                for (int i = 0; i < 10L; ++i)
                {
                    var row = rows[i];
                    Assert.AreEqual(startTime.AddSeconds(i), row[0].TimestampValue);
                    Assert.AreEqual(ts.Alias, row[1].StringValue);
                    Assert.AreEqual(insertedDoubleData[i].Value, row[2].Value);
                }
            }
            finally
            {
                ts.Remove();
            }
        }

        [TestMethod]
        public void ReturnsInsertedDataWithCountSelect()
        {
            var startTime = DateTime.Now;
            QdbTable ts = CreateTable();
            var insertedDoubleData = InsertDoublePoints(ts, startTime, 10);
            try
            {
                var results = _cluster.Query("select count(the_double) from " + ts.Alias);
                Assert.AreEqual(1L, results.ColumnNames.Count);
                Assert.AreEqual("count(the_double)", results.ColumnNames[0]);

                var rows = results.Rows;
                Assert.AreEqual(1L, rows.Count);
                Assert.AreEqual(10L, rows[0]["count(the_double)"].Value);
            }
            finally
            {
                ts.Remove();
            }
        }

        [TestMethod]
        public void ReturnsInsertedDataWithSumSelect()
        {
            var startTime = DateTime.Now;
            QdbTable ts = CreateTable();
            var insertedDoubleData = InsertDoublePoints(ts, startTime, 10);
            try
            {
                var results = _cluster.Query("select sum(the_double) from " + ts.Alias);
                Assert.AreEqual(1L, results.ColumnNames.Count);
                Assert.AreEqual("sum(the_double)", results.ColumnNames[0]);

                var rows = results.Rows;
                Assert.AreEqual(1L, rows.Count);
                Assert.AreEqual(insertedDoubleData.Select(x => x.Value).Sum(), rows[0]["sum(the_double)"].Value);
            }
            finally
            {
                ts.Remove();
            }
        }

        [TestMethod]
        public void ReturnsInsertedMultiDataWithStarSelect()
        {
            var startTime = DateTime.Now;
            QdbTable ts = CreateTable();
            var insertedBlobData = InsertBlobPoints(ts, startTime, 10);
            var insertedDoubleData = InsertDoublePoints(ts, startTime, 10);
            var insertedInt64Data = InsertInt64Points(ts, startTime, 10);
            var insertedStringData = InsertStringPoints(ts, startTime, 10);
            var insertedTimestampData = InsertTimestampPoints(ts, startTime, 10);
            try
            {
                var results = _cluster.Query("select * from " + ts.Alias);
                CheckColumns(results.ColumnNames);

                var rows = results.Rows;
                Assert.AreEqual(10L, rows.Count);
                for (int i = 0; i < 10L; ++i)
                {
                    var row = rows[i];
                    CollectionAssert.AreEqual(insertedBlobData[i].Value, row["the_blob"].BlobValue);
                    Assert.AreEqual(insertedDoubleData[i].Value, row["the_double"].Value);
                    Assert.AreEqual(insertedInt64Data[i].Value, row["the_int64"].Value);
                    Assert.AreEqual(insertedStringData[i].Value, row["the_string"].Value);
                    Assert.AreEqual(insertedTimestampData[i].Value, row["the_ts"].Value);
                    Assert.AreEqual(ts.Alias, row["$table"].StringValue);
                }
            }
            finally
            {
                ts.Remove();
            }
        }

        [TestMethod]
        public void ReturnsInsertedMultiDataWithStarSelect_WithNulls()
        {
            var startTime = DateTime.Now;
            QdbTable ts = CreateTable();
            var insertedBlobData = InsertBlobPoints(ts, startTime, 9);
            var insertedDoubleData = InsertDoublePoints(ts, startTime, 9);
            var insertedInt64Data = InsertInt64Points(ts, startTime, 9);
            var insertedStringData = InsertStringPoints(ts, startTime, 9);
            var insertedTimestampData = InsertTimestampPoints(ts, startTime, 9);

            ts.BlobColumns["the_blob"].Insert(startTime.AddSeconds(9), new byte[] { 10 });
            insertedBlobData.Add(startTime.AddSeconds(9), new byte[] { 10 });
            insertedDoubleData.Add(startTime.AddSeconds(9), null);
            insertedInt64Data.Add(startTime.AddSeconds(9), null);
            insertedStringData.Add(startTime.AddSeconds(9), null);
            insertedTimestampData.Add(startTime.AddSeconds(9), null);
            try
            {
                var results = _cluster.Query("select * from " + ts.Alias);
                CheckColumns(results.ColumnNames);

                var rows = results.Rows;
                Assert.AreEqual(10L, rows.Count);
                for (int i = 0; i < 10L; ++i)
                {
                    var row = rows[i];
                    CollectionAssert.AreEqual(insertedBlobData[i].Value, row["the_blob"].BlobValue);
                    Assert.AreEqual(insertedDoubleData[i].Value, row["the_double"].Value);
                    Assert.AreEqual(insertedInt64Data[i].Value, row["the_int64"].Value);
                    Assert.AreEqual(insertedStringData[i].Value, row["the_string"].Value);
                    Assert.AreEqual(insertedTimestampData[i].Value, row["the_ts"].Value);
                    Assert.AreEqual(ts.Alias, row["$table"].StringValue);
                }
            }
            finally
            {
                ts.Remove();
            }
        }

        #endregion
    }
}
