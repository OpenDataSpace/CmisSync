using System;

using CmisSync.Lib.Events;

using NUnit.Framework;

using Moq;
using CmisSync.Lib;

namespace TestLibrary.EventsTests
{
    [TestFixture]
    public class PermissionDeniedEventHandlerTest
    {
        private string Repo;
        private DotCMIS.Exceptions.CmisPermissionDeniedException Exception;
        private PermissionDeniedEvent PermissionDeniedEvent;

        [SetUp]
        public void SetUp ()
        {
            Repo = "repo";
            Exception = new DotCMIS.Exceptions.CmisPermissionDeniedException ();
            PermissionDeniedEvent = new Mock<PermissionDeniedEvent> (Exception).Object;
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullRepo ()
        {
            new PermissionDeniedEventHandler (null, delegate(string name) {
            });
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullCallback ()
        {
            new PermissionDeniedEventHandler (Repo, null);
        }

        [Test, Category("Fast")]
        public void ConstructorWithValidInput ()
        {
            var handler = new PermissionDeniedEventHandler (Repo, delegate(string name) {
            });
            Assert.AreEqual (PermissionDeniedEventHandler.PERMISSIONDENIEDHANDLERPRIORITY, handler.Priority);
        }

        [Test, Category("Fast")]
        public void HandleFirstEvent ()
        {
            int handled = 0;
            var handler = new PermissionDeniedEventHandler (Repo, delegate(string name) {
                handled ++;
            });
            Assert.IsTrue (handler.Handle (PermissionDeniedEvent));
            Assert.AreEqual (1, handled);
        }

        [Test, Category("Fast")]
        public void IgnoreNextEventsIfPasswordWasNotChanged ()
        {
            int handled = 0;
            var handler = new PermissionDeniedEventHandler (Repo, delegate(string name) {
                handled ++;
            });
            Assert.IsTrue (handler.Handle (PermissionDeniedEvent));
            Assert.IsTrue (handler.Handle (PermissionDeniedEvent));
            Assert.AreEqual (1, handled);
        }

        [Test, Category("Fast")]
        public void HandleNextEventAfterConfigHasBeenChanged ()
        {
            int handled = 0;
            var handler = new PermissionDeniedEventHandler (Repo, delegate(string name) {
                handled ++;
            });
            Assert.IsTrue (handler.Handle (PermissionDeniedEvent));
            Assert.IsTrue (handler.Handle (PermissionDeniedEvent));
            var changed = new Mock<RepoConfigChangedEvent> (new Mock<RepoInfo> (Repo, "").Object).Object;
            Assert.IsFalse (handler.Handle (changed));
            Assert.IsTrue (handler.Handle (PermissionDeniedEvent));
            Assert.AreEqual (2, handled);
        }

        [Test, Category("Fast")]
        public void HandleNextEventAfterASuccessfulLogin ()
        {
            int handled = 0;
            var handler = new PermissionDeniedEventHandler (Repo, delegate(string name) {
                handled ++;
            });
            Assert.IsTrue (handler.Handle (PermissionDeniedEvent));
            Assert.IsTrue (handler.Handle (PermissionDeniedEvent));
            Assert.IsFalse (handler.Handle (new SuccessfulLoginEvent (new Uri("http://example.com"))));
            Assert.IsTrue (handler.Handle (PermissionDeniedEvent));
            Assert.AreEqual (2, handled);
        }
    }
}

