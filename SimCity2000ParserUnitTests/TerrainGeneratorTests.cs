using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimCityToMinecraft;
using SimCity2000Parser;
using Substrate;
using Substrate.Core;

namespace SimCity2000ParserUnitTests
{
    [TestClass]
    public class TerrainGeneratorTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            TerrainGenerator tg = new TerrainGenerator(null, null, null);
            Assert.Equals(tg.LinearInterpolate(5, 10, 10, 0), 5);
        }
    }
}
