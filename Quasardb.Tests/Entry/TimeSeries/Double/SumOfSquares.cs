﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quasardb.Exceptions;
using Quasardb.TimeSeries;

namespace Quasardb.Tests.Entry.TimeSeries.Double
{
    [TestClass]
    public class SumOfSquares
    {
        readonly QdbDoublePointCollection _points = new QdbDoublePointCollection
        {
            {new DateTime(2012, 11, 02), 0},
            {new DateTime(2014, 06, 30), 42 },
            {new DateTime(2016, 02, 04), 666}
        };

        [TestMethod]
        public void ThrowsColumnNotFound()
        {
            var col = QdbTestCluster.GetNonExistingDoubleColumn();

            try
            {
                col.First();
                Assert.Fail("No exception thrown");
            }
            catch (QdbColumnNotFoundException e)
            {
                Assert.AreEqual(col.Series.Alias, e.Alias);
                Assert.AreEqual(col.Name, e.Column);
            }
        }

        [TestMethod]
        public void GivenNoArgument_ReturnsSumOfSquaresOfTimeSeries()
        {
            var col = QdbTestCluster.CreateEmptyDoubleColumn();
            col.Insert(_points);

            var result = col.SumOfSquares();

            Assert.AreEqual(42 * 42 + 666 * 666, result);
        }

        [TestMethod]
        public void GivenInRangeInterval_ReturnsMinPointOfInterval()
        {
            var col = QdbTestCluster.CreateEmptyDoubleColumn();
            col.Insert(_points);

            var interval = new QdbTimeInterval(_points[0].Time, _points[2].Time);
            var result = col.SumOfSquares(interval);

            Assert.AreEqual(42 * 42, result);
        }

        [TestMethod]
        public void GivenOutOfRangeInterval_ReturnsNan()
        {
            var col = QdbTestCluster.CreateEmptyDoubleColumn();
            col.Insert(_points);

            var interval = new QdbTimeInterval(new DateTime(3000, 1, 1), new DateTime(4000, 1, 1));
            var result = col.SumOfSquares(interval);

            Assert.IsTrue(double.IsNaN(result));
        }

        [TestMethod]
        public void GivenSeveralIntervals_ReturnsSumOfSquaresOfEach()
        {
            var col = QdbTestCluster.CreateEmptyDoubleColumn();
            col.Insert(_points);

            var intervals = new[]
            {
                new QdbTimeInterval(new DateTime(2012, 1, 1), new DateTime(2015, 12, 31)),
                new QdbTimeInterval(new DateTime(2014, 1, 1), new DateTime(2017, 12, 31)),
                new QdbTimeInterval(new DateTime(2016, 6, 1), new DateTime(2018, 12, 31))
            };

            var results = col.SumOfSquares(intervals).ToArray();

            Assert.AreEqual(3, results.Length);
            Assert.AreEqual(42 * 42, results[0]);
            Assert.AreEqual(42 * 42 + 666 * 666, results[1]);
            Assert.IsTrue(double.IsNaN(results[2]));
        }

        [TestMethod]
        public void ThrowsEmptyColumn()
        {
            var col = QdbTestCluster.CreateEmptyDoubleColumn();

            try
            {
                col.SumOfSquares();
                Assert.Fail("No exception thrown");
            }
            catch (QdbEmptyColumnException e)
            {
                Assert.AreEqual(col.Series.Alias, e.Alias);
                Assert.AreEqual(col.Name, e.Column);
            }
        }

    }
}
