using NUnit.Framework;

namespace GTP.Tests
{
    public sealed class GameSessionTests
    {
        [Test]
        public void Score_NeverGoesNegative()
        {
            var s = new GameSession(3, 12f);
            s.AddScore(100);
            s.AddScore(-250);
            Assert.AreEqual(0, s.Score);
        }

        [Test]
        public void LoseLife_ReturnsFalseOnLastLife_AndCountsDetections()
        {
            var s = new GameSession(3, 12f);
            Assert.IsTrue(s.LoseLife());
            Assert.IsTrue(s.LoseLife());
            Assert.IsFalse(s.LoseLife());
            Assert.AreEqual(0, s.Lives);
            Assert.AreEqual(3, s.Detections);
        }

        [Test]
        public void Battery_ClampsAndRaisesDepletedOnce()
        {
            var s = new GameSession(3, 2f);
            int depleted = 0;
            s.BatteryDepleted += () => depleted++;
            s.DrainBattery(5f);
            s.DrainBattery(5f);
            Assert.AreEqual(0f, s.Battery);
            Assert.AreEqual(1, depleted);
            s.AddBattery(500f);
            Assert.AreEqual(100f, s.Battery);
        }

        [Test]
        public void Route_IsSavedOnlyOnce()
        {
            var s = new GameSession(3, 12f);
            Assert.IsTrue(s.TryChooseRoute(Route.Bien));
            Assert.IsFalse(s.TryChooseRoute(Route.Mal));
            Assert.AreEqual(Route.Bien, s.Route);
        }

        [Test]
        public void Collect_IsUnique()
        {
            var s = new GameSession(3, 12f);
            Assert.IsTrue(s.TryCollect("Barrio/Bateria_01"));
            Assert.IsFalse(s.TryCollect("Barrio/Bateria_01"));
            Assert.IsFalse(s.TryCollect(""));
        }

        [Test]
        public void RestoreLevelStart_RevertsProgressAndRefillsLives()
        {
            var s = new GameSession(3, 12f);
            s.AddScore(300);
            s.MarkLevelStart();
            s.AddScore(500);
            s.TryCollect("x");
            s.LoseLife();
            s.DrainBattery(4f);
            s.RestoreLevelStart();
            Assert.AreEqual(300, s.Score);
            Assert.AreEqual(3, s.Lives);
            Assert.AreEqual(12f, s.Battery, 0.001f);
            Assert.IsFalse(s.IsCollected("x"));
        }
    }
}
