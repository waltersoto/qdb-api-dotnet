﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quasardb.Tests.Helpers;

namespace Quasardb.Tests.QdbStreamTests
{
    [TestClass]
    public class CanSeek
    {
        [TestMethod]
        public void ReturnsTrue()
        {
            var stream = QdbTestCluster.CreateAndOpenStream();
            Assert.IsTrue(stream.CanSeek);
        }

        [TestMethod]
        public void ReturnsFalse()
        {
            var stream = QdbTestCluster.CreateAndOpenStream();
            stream.Close();
            Assert.IsFalse(stream.CanSeek);
        }
    }
}