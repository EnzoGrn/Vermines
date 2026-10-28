using NUnit.Framework;

namespace CelticEdge.Tutorial.Tests {

    using CelticEdge.Tutorial.Authoring;

    public class CommandTests {

        [Test]
        public void ArgumentsAreReadByAnyOfTheirNames()
        {
            var cmd = TutorialTable.ParseCommand("AddGold(Number=5, player=1)");

            Assert.AreEqual("AddGold", cmd.Name);
            Assert.AreEqual( 5, cmd.GetInt("amount", "number"));
            Assert.AreEqual( 1, cmd.GetInt("player", "player"));
            Assert.AreEqual(-1, cmd.GetInt(-1      , "missing"));
            Assert.IsTrue(cmd.Is("addgold"));
            Assert.IsFalse(cmd.IsBuiltIn);
        }

        [Test]
        public void CommandsRoundTripThroughText()
        {
            var cmd   = new TutorialCommand("GiveItem", ("item", "sword"), ("count", 2));
            var again = TutorialTable.ParseCommand(cmd.ToString());

            Assert.AreEqual("sword", again.Get("item"));
            Assert.AreEqual(2, again.GetInt("count"));
        }
    }
}
