using NUnit.Framework;
using SharpiesMafia.Hubs;
using SharpiesMafia.Models;
using Microsoft.EntityFrameworkCore;

namespace SharpiesMafia.Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        private MafiaHub _hub;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MafiaContext>()
                .UseInMemoryDatabase(databaseName: "Add_Test_Mafia_Db")
                .Options;

            var _dbContext = new MafiaContext(options);

            _hub = new MafiaHub(_dbContext);
        }

        [Test]
        public void GenerateCodeMethodCanGenerateFourDigitCode()
        {
            Assert.AreEqual(4, _hub.GenerateCode().ToString().Length);
        }
    }
}