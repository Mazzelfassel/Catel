﻿namespace Catel.Tests.Messaging
{
    using System;
    using Catel.Messaging;
    using NUnit.Framework;

    #region Classes
    public class MessageSender
    {
        #region Methods
        public bool SendMessage(IMessageMediator messageMediator, string message)
        {
            return SendMessage(messageMediator, message, null);
        }

        public bool SendMessage(IMessageMediator messageMediator, string message, object tag)
        {
            return messageMediator.SendMessage(message, tag);
        }
        #endregion
    }

    public class MessageRecipient
    {
        #region Properties
        public int MessagesReceived { get; private set; }

        public int MessagesReceivedViaMessageMediatorWithTag { get; private set; }

        public int MessagesReceivedViaMessageMediatorWithoutTag { get; private set; }
        #endregion

        #region Methods
        public void OnMessage(string message)
        {
            MessagesReceived++;
        }

        public void AnotherOnMessage(string message)
        {
            MessagesReceived++;
        }

        public void YetAnotherOnMessage(string message)
        {
            MessagesReceived++;
        }

        [MessageRecipient]
        public void OnMessageWithoutTag(string message)
        {
            MessagesReceivedViaMessageMediatorWithoutTag++;
        }

        [MessageRecipient(Tag = "tag")]
        public void OnMessageWithTag(string message)
        {
            MessagesReceivedViaMessageMediatorWithTag++;
        }

        public void SubscribeViaMessageMediatorHelper(IMessageMediator messageMediator)
        {
            MessageMediatorHelper.SubscribeRecipient(this, messageMediator);
        }

        public void UnsubscribeViaMessageMediatorHelper(IMessageMediator messageMediator)
        {
            MessageMediatorHelper.UnsubscribeRecipient(this, messageMediator);
        }
        #endregion
    }
    #endregion

    public class MessageMediatorFacts
    {
        #region Nested type: TheCleanUpMethod
        [TestFixture]
        public class TheCleanUpMethod
        {
            #region Methods
            [TestCase]
            public void CleanUpWorksWhenNoHandlersRegistered()
            {
                var mediator = new MessageMediator();

                mediator.CleanUp();
            }

            [TestCase]
            public void CleanUpKeepsNonGarbageCollectedHandlersRegistered()
            {
                var mediator = new MessageMediator();
                var recipient = new MessageRecipient();

                mediator.Register<string>(recipient, recipient.OnMessage);

                mediator.CleanUp();

                Assert.IsTrue(mediator.IsRegistered<string>(recipient, recipient.OnMessage));
            }

            [TestCase, Explicit]
            public void CleanUpClearsGarbageCollectedHandlers()
            {
                var mediator = new MessageMediator();
                var recipient = new MessageRecipient();

                mediator.Register<string>(recipient, recipient.OnMessage);

                recipient = null;
                GC.Collect();

                mediator.CleanUp();

                Assert.AreEqual(0, mediator.GetRegisteredHandlers<string>().Count);
            }
            #endregion
        }
        #endregion

        #region Nested type: TheIsMessageRegisteredMethod
        [TestFixture]
        public class TheIsMessageRegisteredMethod
        {
            #region Methods
            [TestCase]
            public void ReturnsArgumentNullException()
            {
                var mediator = new MessageMediator();

                Assert.Throws<ArgumentNullException>(() => mediator.IsMessageRegistered(null));
            }


            [TestCase]
            public void ReturnsFalseWhenNotRegistered()
            {
                var mediator = new MessageMediator();

                Assert.IsFalse(mediator.IsMessageRegistered(typeof(string)));
            }

            [TestCase]
            public void ReturnsTrueWhenRegistered()
            {
                var mediator = new MessageMediator();
                var recipient = new MessageRecipient();

                mediator.Register<string>(recipient, recipient.OnMessage);

                Assert.IsTrue(mediator.IsMessageRegistered(typeof(string)));
            }

            [TestCase]
            public void ReturnsFalseWhenNotRegisteredWithTag()
            {
                var mediator = new MessageMediator();
                var recipient = new MessageRecipient();

                mediator.Register<string>(recipient, recipient.OnMessage);

                Assert.IsFalse(mediator.IsMessageRegistered(typeof(string), "myTag"));
            }

            [TestCase]
            public void ReturnsTrueWhenRegisteredWithTag()
            {
                var mediator = new MessageMediator();
                var recipient = new MessageRecipient();

                mediator.Register<string>(recipient, recipient.OnMessage, "myTag");

                Assert.IsTrue(mediator.IsMessageRegistered(typeof(string), "myTag"));
            }

            [TestCase]
            public void ReturnsFalseWhenRegisteredWithTagButNotProvided()
            {
                var mediator = new MessageMediator();
                var recipient = new MessageRecipient();

                mediator.Register<string>(recipient, recipient.OnMessage, "myTag");

                Assert.IsFalse(mediator.IsMessageRegistered(typeof(string)));
            }

            [TestCase]
            public void ReturnsTrueWhenRegisteredWithAndWithoutTag()
            {
                var mediator = new MessageMediator();
                var recipient = new MessageRecipient();

                mediator.Register<string>(recipient, recipient.OnMessage, "myTag");
                mediator.Register<string>(recipient, recipient.AnotherOnMessage);

                Assert.IsTrue(mediator.IsMessageRegistered(typeof(string)));
            }
            #endregion
        }
        #endregion

        #region Nested type: TheRegisterMethod
        [TestFixture]
        public class TheRegisterMethod
        {
            #region Methods
            [TestCase]
            public void RegistersHandler()
            {
                var mediator = new MessageMediator();
                var recipient = new MessageRecipient();

                Assert.IsFalse(mediator.IsRegistered<string>(recipient, recipient.OnMessage));

                mediator.Register<string>(recipient, recipient.OnMessage);

                Assert.IsTrue(mediator.IsRegistered<string>(recipient, recipient.OnMessage));
            }

            [TestCase]
            public void RegistersHandlerWithTag()
            {
                var mediator = new MessageMediator();
                var recipient = new MessageRecipient();

                Assert.IsFalse(mediator.IsRegistered<string>(recipient, recipient.OnMessage, "myTag"));

                mediator.Register<string>(recipient, recipient.OnMessage, "myTag");

                Assert.IsTrue(mediator.IsRegistered<string>(recipient, recipient.OnMessage, "myTag"));
                Assert.IsFalse(mediator.IsRegistered<string>(recipient, recipient.OnMessage, "anotherTag"));
            }

            [TestCase]
            public void ReturnsFalsForDoubleRegistration()
            {
                var mediator = new MessageMediator();
                var recipient = new MessageRecipient();

                Assert.IsTrue(mediator.Register<string>(recipient, recipient.OnMessage));
                Assert.IsFalse(mediator.Register<string>(recipient, recipient.OnMessage));
            }
            #endregion
        }
        #endregion

        #region Nested type: TheSendMessageMethod
        [TestFixture]
        public class TheSendMessageMethod
        {
            #region Methods
            [TestCase]
            public void ReturnsFalseForUnregisteredHandlers()
            {
                var mediator = new MessageMediator();
                var sender = new MessageSender();

                Assert.IsFalse(sender.SendMessage(mediator, "test"));
            }

            [TestCase]
            public void ReturnsFalseForUnregisteredHandlersWithTag()
            {
                var mediator = new MessageMediator();
                var sender = new MessageSender();
                var recipient = new MessageRecipient();

                mediator.Register<string>(recipient, recipient.OnMessage);

                Assert.IsFalse(sender.SendMessage(mediator, "test", "myTag"));
            }

            [TestCase]
            public void ReturnsTrueForRegisteredHandlers()
            {
                var mediator = new MessageMediator();
                var sender = new MessageSender();
                var recipient = new MessageRecipient();

                mediator.Register<string>(recipient, recipient.OnMessage);

                Assert.IsTrue(sender.SendMessage(mediator, "test"));
                Assert.AreEqual(1, recipient.MessagesReceived);
            }

            [TestCase]
            public void ReturnsTrueForRegisteredHandlersWithTag()
            {
                var mediator = new MessageMediator();
                var sender = new MessageSender();
                var recipient = new MessageRecipient();

                // Double registration with separate tags is possible
                mediator.Register<string>(recipient, recipient.OnMessage, "myTag");
                mediator.Register<string>(recipient, recipient.OnMessage, "anotherTag");

                Assert.IsTrue(sender.SendMessage(mediator, "test", "myTag"));
                Assert.AreEqual(1, recipient.MessagesReceived);
            }

            [TestCase, Explicit]
            public void ReturnsFalseForHandlersClearedByGarbageCollector()
            {
                var mediator = new MessageMediator();
                var sender = new MessageSender();
                var recipient = new MessageRecipient();

                mediator.Register<string>(recipient, recipient.OnMessage);

                recipient = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();

                Assert.IsFalse(sender.SendMessage(mediator, "test"));
                Assert.AreEqual(0, mediator.GetRegisteredHandlers<string>().Count, "SendMessage should auto cleanup");
            }
            #endregion
        }
        #endregion

        #region Nested type: TheUnregisterMethod
        [TestFixture]
        public class TheUnregisterMethod
        {
            #region Methods
            [TestCase]
            public void UnregistersRegisteredHandler()
            {
                var mediator = new MessageMediator();
                var recipient = new MessageRecipient();

                mediator.Register<string>(recipient, recipient.OnMessage);

                Assert.IsTrue(mediator.IsRegistered<string>(recipient, recipient.OnMessage));
                Assert.IsTrue(mediator.Unregister<string>(recipient, recipient.OnMessage));
                Assert.IsFalse(mediator.IsRegistered<string>(recipient, recipient.OnMessage));
            }

            [TestCase]
            public void UnregistersRegisteredHandlerWithTag()
            {
                var mediator = new MessageMediator();
                var recipient = new MessageRecipient();

                mediator.Register<string>(recipient, recipient.OnMessage, "myTag");

                Assert.IsTrue(mediator.IsRegistered<string>(recipient, recipient.OnMessage, "myTag"));
                Assert.IsTrue(mediator.Unregister<string>(recipient, recipient.OnMessage, "myTag"));
                Assert.IsFalse(mediator.IsRegistered<string>(recipient, recipient.OnMessage, "myTag"));
            }

            [TestCase]
            public void ReturnsFalseForUnregisteredHandler()
            {
                var mediator = new MessageMediator();
                var recipient = new MessageRecipient();

                Assert.IsFalse(mediator.IsRegistered<string>(recipient, recipient.OnMessage));
                Assert.IsFalse(mediator.Unregister<string>(recipient, recipient.OnMessage));
            }

            [TestCase]
            public void UnregistersAllMethodsOfRecipient()
            {
                var mediator = new MessageMediator();
                var recipient = new MessageRecipient();

                mediator.Register<string>(recipient, recipient.OnMessage);
                mediator.Register<string>(recipient, recipient.AnotherOnMessage);
                mediator.Register<string>(recipient, recipient.YetAnotherOnMessage);

                Assert.IsTrue(mediator.IsRegistered<string>(recipient, recipient.OnMessage));
                Assert.IsTrue(mediator.IsRegistered<string>(recipient, recipient.AnotherOnMessage));
                Assert.IsTrue(mediator.IsRegistered<string>(recipient, recipient.YetAnotherOnMessage));

                mediator.UnregisterRecipient(recipient);

                Assert.IsFalse(mediator.IsRegistered<string>(recipient, recipient.OnMessage));
                Assert.IsFalse(mediator.IsRegistered<string>(recipient, recipient.AnotherOnMessage));
                Assert.IsFalse(mediator.IsRegistered<string>(recipient, recipient.YetAnotherOnMessage));
            }
            #endregion
        }
        #endregion


        [TestFixture]
        public class TheIsRegisteredMethod
        {

            [Test]
            public void ReturnsTrueAfterRegistration()
            {
                var recipient = new MessageRecipient();
                var messageMediator = new MessageMediator();
                messageMediator.Register<string>(recipient, recipient.OnMessage);
                Assert.IsTrue(messageMediator.IsRegistered<string>(recipient, recipient.OnMessage));
            }

            [Test]
            public void ReturnsFalseAfterGarbageCollected()
            {
                var recipient = new MessageRecipient();
                var messageMediator = new MessageMediator();
                messageMediator.Register<string>(recipient, recipient.OnMessage);

                recipient = null;

                GC.Collect();

                recipient = new MessageRecipient();
                Assert.IsFalse(messageMediator.IsRegistered<string>(recipient, recipient.OnMessage));
            }
        }
    }

    public class Message
    {
        public string Text { get; set; }
    }

    public class ReceiverA
    {
        public string Received { get; private set; }
        public void OnMessageReceived(Message msg)
        {
            Received = msg.Text;
        }
    }

    public class ReceiverB
    {
        public string Received { get; private set; }
        public void OnMessageReceived(Message msg)
        {
            Received = msg.Text;
        }
    }

    [TestFixture]
    public class TestRegistrationOfMethodsWithSameName
    {
        [Test]
        public void SendMessage()
        {
            var a = new ReceiverA();
            var b = new ReceiverB();

            var m = new MessageMediator();

            m.Register<Message>(a, a.OnMessageReceived);
            m.Register<Message>(b, b.OnMessageReceived);
            m.Unregister<Message>(b, b.OnMessageReceived); // this actually unregisters a's handler, not b's handler.

            m.SendMessage(new Message { Text = "hello" });
            Assert.That(a.Received, Is.EqualTo("hello"));
        }
    }
}
