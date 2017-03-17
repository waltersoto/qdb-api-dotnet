﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quasardb.Exceptions;
using Quasardb.TimeSeries;

namespace Quasardb.Tests.Entry.TimeSeries.Double
{
    [TestClass]
    public class Max
    {
        readonly QdbDoublePointCollection _points = new QdbDoublePointCollection
        {
            {new DateTime(2012, 11, 02), 0},
            {new DateTime(2014, 06, 30), 42 },
            {new DateTime(2016, 02, 04), 666} // <- max is here
        };

        [TestMethod]
        public void ThrowsAliasNotFound()
        {
            var ts = QdbTestCluster.CreateEmptyDoubleColumn();

            try
            {
                ts.First();
                Assert.Fail("No exception thrown");
            }
            catch (QdbAliasNotFoundException e)
            {
                Assert.AreEqual(ts.Series.Alias, e.Alias);
            }
        }

        [TestMethod]
        public void GivenNoArgument_ReturnsMaxPointOfTimeSeries()
        {
            var ts = QdbTestCluster.CreateEmptyDoubleColumn();
            ts.Insert(_points);

            var result = ts.Max();

            Assert.AreEqual(_points[2], result);
        }

        [TestMethod]
        public void GivenInRangeInterval_ReturnsMinPointOfInterval()
        {
            var ts = QdbTestCluster.CreateEmptyDoubleColumn();
            ts.Insert(_points);

            var interval = new QdbTimeInterval(_points[0].Time, _points[2].Time);
            var result = ts.Max(interval);

            Assert.AreEqual(_points[1], result);
        }

        [TestMethod]
        public void GivenOutOfRangeInterval_ReturnsNull()
        {
            var ts = QdbTestCluster.CreateEmptyDoubleColumn();
            ts.Insert(_points);

            var interval = new QdbTimeInterval(new DateTime(3000, 1, 1), new DateTime(4000, 1, 1));
            var result = ts.Max(interval);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GivenSeveralIntervals_ReturnsMaxOfEach()
        {
            var ts = QdbTestCluster.CreateEmptyDoubleColumn();
            ts.Insert(_points);

            var intervals = new[]
            {
                new QdbTimeInterval(new DateTime(2012, 1, 1), new DateTime(2015, 12, 31)),
                new QdbTimeInterval(new DateTime(2014, 1, 1), new DateTime(2017, 12, 31)),
                new QdbTimeInterval(new DateTime(2016, 6, 1), new DateTime(2018, 12, 31))
            };

            var results = ts.Max(intervals).ToArray();

            Assert.AreEqual(3, results.Length);
            Assert.AreEqual(_points[1], results[0]);
            Assert.AreEqual(_points[2], results[1]);
            Assert.IsNull(results[2]);
        }
    }
}
