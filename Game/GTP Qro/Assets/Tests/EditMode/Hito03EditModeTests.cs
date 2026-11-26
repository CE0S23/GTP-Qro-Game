using NUnit.Framework;
using UnityEngine;

namespace GTP.Tests
{
    /// <summary>Hito 03: canales de eventos ScriptableObject y navegación sin NavMesh.</summary>
    public sealed class Hito03EditModeTests
    {
        [Test]
        public void EventChannel_Raise_NotifiesListenersWithPosition()
        {
            GameEventChannel channel = GameEventChannel.CreateRuntime("prueba");
            Vector3 received = Vector3.zero;
            int calls = 0;
            channel.Raised += p => { received = p; calls++; };

            channel.Raise(new Vector3(1f, 2f, 3f));

            Assert.AreEqual(1, calls);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), received);
            Object.DestroyImmediate(channel);
        }

        [Test]
        public void EventChannel_WithoutListeners_DoesNotThrow()
        {
            GameEventChannel channel = GameEventChannel.CreateRuntime("vacio");
            Assert.DoesNotThrow(() => channel.Raise());
            Object.DestroyImmediate(channel);
        }

        [Test]
        public void GameEvents_AllChannelAssetsExistInResources()
        {
            foreach (string channelName in GameEvents.All)
                Assert.IsNotNull(Resources.Load<GameEventChannel>(GameEvents.Folder + "/" + channelName),
                    "Falta el asset del canal " + channelName + " (correr GTP > Construir escenas greybox)");
        }

        [Test]
        public void NavSteering_WithoutNavMesh_FallsBackToStraightLine()
        {
            var nav = new NavSteering();
            Vector3 dir = nav.DirectionTo(new Vector3(1000f, 0f, 1000f), new Vector3(1010f, 5f, 1000f));
            Assert.IsFalse(nav.HasNavPath);
            Assert.That(Vector3.Distance(Vector3.right, dir), Is.LessThan(0.001f));
        }
    }
}
