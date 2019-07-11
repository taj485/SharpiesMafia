using System;
using System.Linq;
using NUnit.Framework;
using SharpiesMafia.Hubs;

namespace SharpiesMafia.Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        private MafiaHub _hub;

        [SetUp]
        public void Setup()
        {
            _hub = new MafiaHub();
        }

        //[Test]
        //public void GenerateCodeMethodCanGenerateFourDigitCode()
        //{
        //    Assert.AreEqual(4, _hub.GenerateCode().ToString().Length) ;
        //}
    }
}