﻿namespace Catel.Tests.MVVM.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Catel.MVVM;
    using Catel.Threading;

    using NUnit.Framework;

    [TestFixture]
    public class TaskCommandFacts
    {
        #region Constants
        private static readonly TimeSpan TaskDelay = TimeSpan.FromSeconds(5);
        #endregion

        #region Methods
        [TestCase]
        public void TestCommandCancellation()
        {
            var taskCommand = new TaskCommand(TestExecuteAsync);

            Assert.IsFalse(taskCommand.IsExecuting);
            Assert.IsFalse(taskCommand.IsCancellationRequested);

            taskCommand.Execute();

            Assert.IsTrue(taskCommand.IsExecuting);

            ThreadHelper.Sleep(1000);

            taskCommand.Cancel();

            ThreadHelper.Sleep(1000);

            Assert.IsFalse(taskCommand.IsExecuting);
            Assert.IsFalse(taskCommand.IsCancellationRequested);
        }

        [TestCase]
        public async Task TestCommandExceptions_SwallowExceptionsAsync()
        {
            var taskCommand = new TaskCommand(TestExecuteWithExceptionAsync)
            {
                SwallowExceptions = true
            };

            Assert.IsFalse(taskCommand.IsExecuting);
            Assert.IsFalse(taskCommand.IsCancellationRequested);

            try
            {
                taskCommand.Execute();

                Assert.IsTrue(taskCommand.IsExecuting);

                await taskCommand.Task;
            }
            catch (Exception ex)
            {
                Assert.Fail($"No exception expected, should be swallowed, but got '{ex}'");
            }

            Assert.IsFalse(taskCommand.IsExecuting);
        }

        [TestCase, Explicit]
        public async Task TestCommandExceptions_DontSwallowExceptionsAsync()
        {
            var taskCommand = new TaskCommand(TestExecuteWithExceptionAsync)
            {
                SwallowExceptions = false
            };

            Assert.IsFalse(taskCommand.IsExecuting);
            Assert.IsFalse(taskCommand.IsCancellationRequested);

            try
            {
                taskCommand.Execute();

                await taskCommand.Task;

                Assert.Fail("Expected exception");
            }
            catch (Exception)
            {
            }

            Assert.IsFalse(taskCommand.IsExecuting, "Command should not be executing");
        }

        //[TestCase]
        //public async Task TestCommandExceptions_DontSwallowExceptionsWithoutAwaitAsync()
        //{
        //    var taskCommand = new TaskCommand(TestExecuteWithExceptionAsync)
        //    {
        //        SwallowExceptions = false
        //    };

        //    Assert.IsFalse(taskCommand.IsExecuting);
        //    Assert.IsFalse(taskCommand.IsCancellationRequested);

        //    try
        //    {
        //        taskCommand.Execute();

        //        await Task.Delay(1500);

        //        Assert.Fail("Expected exception");
        //    }
        //    catch (Exception)
        //    {
        //    }

        //    Assert.IsFalse(taskCommand.IsExecuting, "Command should not be executing");
        //}

        private static async Task TestExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(TaskDelay, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
        }

        private static async Task TestExecuteWithExceptionAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(500, cancellationToken);

            throw new Exception("This is an expected exception");
        }
        #endregion
    }

    public class PercentProgress : ITaskProgressReport
    {
        public PercentProgress(int percents, string status = "")
        {
            Percents = percents;
            Status = status;
        }

        public int Percents { get; private set; }

        public string Status { get; private set; }
    }
}
