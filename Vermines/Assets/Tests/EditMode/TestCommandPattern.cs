using NUnit.Framework;
using UnityEngine;
using OMGG.DesignPattern;

namespace Test.OMGG.Pattern {

    public class Player {

        public Vector3 Position = Vector3.zero;

        public void PlayerMove(Vector3 movement)
        {
            Position += movement;
        }
    }

    public class MovePlayer : ICommand {

        public Player  _Player;
        public Vector3 _Movement;

        public MovePlayer(Player player, Vector3 movement)
        {
            _Player   = player;
            _Movement = movement;
        }

        public CommandResponse Execute()
        {
            _Player.PlayerMove(_Movement);

            return new CommandResponse(CommandStatus.Success, "Player moved.");
        }

        public void Undo()
        {
            _Player.PlayerMove(-_Movement);
        }
    }

    public class TestCommandPattern {

        [Test]
        public void ExecuteAndUndoCommands()
        {
            Player  player   = new();
            Vector3 movement = new(1, 0, 0);

            ICommand movePlayer = new MovePlayer(player, movement);

            CommandResponse response = CommandInvoker.ExecuteCommand(movePlayer);

            if (response.Status != CommandStatus.Success)
                Assert.Fail("Command failed.");
            Assert.AreEqual(new Vector3(1, 0, 0), player.Position);

            CommandInvoker.UndoCommand();

            Assert.AreEqual(new Vector3(0, 0, 0), player.Position);

            CommandInvoker.ExecuteCommand(movePlayer);
            CommandInvoker.ExecuteCommand(movePlayer);

            Assert.AreEqual(new Vector3(2, 0, 0), player.Position);

            CommandInvoker.UndoCommand();

            Assert.AreEqual(new Vector3(1, 0, 0), player.Position);
        }
    }
}
