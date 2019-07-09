using System;
using System.Linq;
using NUnit.Framework;
using SharpiesMafia.Controllers;

namespace SharpiesMafia.Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        private HomeController _home;

        [SetUp]
        public void Setup()
        {
            _home = new HomeController();
        }

        [Test]
        public void GenerateCodeMethodCanGenerateFourDigitCode()
        {
            Assert.AreEqual(4, _home.GenerateCode().ToString().Length) ;
        }
    }
}