using NUnit.Framework;
using System.Collections.Generic;
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

        //[Test]
        //public void ShowsOnlyUsersWithSpecifiedGameId()
        //{
        //    List<User> TestUserGroup = new List<User> ();
        //    TestUserGroup.Add(new User() { name = "Anton", game_id = 1234 });
        //    TestUserGroup.Add(new User() { name = "Belle", game_id = 1234 });
        //    TestUserGroup.Add(new User() { name = "Ovie", game_id = 5678 });
        //    var result = _hub.GetSpecificGameUsers(1234);
        //    Assert.AreEqual(, result);
        //}
    }
}