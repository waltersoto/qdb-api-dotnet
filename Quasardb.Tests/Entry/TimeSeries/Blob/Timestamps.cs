﻿using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quasardb.Exceptions;
using Quasardb.TimeSeries;

namespace Quasardb.Tests.Entry.TimeSeries.Blob
{
    [TestClass]
    public class Timestamps
    {
        readonly QdbBlobPoint[] _points =
        {
            new QdbBlobPoint(new DateTime(2012, 11, 02), Encoding.UTF8.GetBytes("Hello World!")),
            new QdbBlobPoint(new DateTime(2014, 06, 30), RandomGenerator.CreateRandomContent()),
            new QdbBlobPoint(new DateTime(2016, 02, 04), RandomGenerator.CreateRandomContent())
        };

        [TestMethod]
        public void GivenNoArgument_ReturnsTimestampsOfTimeSeries()
        {
            var col = QdbTestCluster.CreateEmptyBlobColumn();
            col.Insert(_points);

            var result = col.Timestamps();

            CollectionAssert.AreEqual(_points.Select(x => x.Time).ToList(), result.ToList());
        }

        [TestMethod]
        public void GivenInRangeInterval_ReturnsTimestampsOfInterval()
        {
            var col = QdbTestCluster.CreateEmptyBlobColumn();
            col.Insert(_points);

            var interval = new QdbTimeInterval(_points[0].Time, _points[2].Time);
            var result = col.Timestamps(interval);

            CollectionAssert.AreEqual(_points.Take(2).Select(x => x.Time).ToList(), result.ToList());
        }

        [TestMethod]
        public void GivenOutOfRangeInterval_ReturnsEmptyCollection()
        {
            var col = QdbTestCluster.CreateEmptyBlobColumn();
            col.Insert(_points);

            var interval = new QdbTimeInterval(new DateTime(3000, 1, 1), new DateTime(4000, 1, 1));
            var result = col.Timestamps(interval);

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void GivenSeveralIntervals_ReturnsTimestampsOfEach()
        {
            var col = QdbTestCluster.CreateEmptyBlobColumn();
            col.Insert(_points);

            var intervals = new[]
            {
                new QdbTimeInterval(new DateTime(2014, 1, 1), new DateTime(2014, 12, 31)),
                new QdbTimeInterval(new DateTime(2016, 1, 1), new DateTime(2016, 12, 31)),
                new QdbTimeInterval(new DateTime(2018, 1, 1), new DateTime(2018, 12, 31))
            };

            var result = col.Timestamps(intervals);

            CollectionAssert.AreEqual(_points.Skip(1).Select(x => x.Time).ToList(), result.ToList());
        }
    }
}
