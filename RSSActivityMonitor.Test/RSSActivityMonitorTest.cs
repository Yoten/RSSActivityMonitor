using System;
using System.Resources;

using NUnit.Framework;

namespace RSSActivityMonitor.Test
{
    [TestFixture]
    public class RSSActivityMonitorTest
    {
        public ResourceManager rm;
        public RSSActivityMonitor monitor;

        [OneTimeSetUp]
        public void SetUp()
        {
            // Access the main assembly's resx file so we can compare our outputs.
            var asm = System.Reflection.Assembly.Load("RSSActivityMonitor");
            rm = new ResourceManager("RSSActivityMonitor.Resources.Resources", asm);

            monitor = new RSSActivityMonitor();
        }
        
        [Test]
        public void ShouldDisplayHelpMessageWithNoArgs()
        {
            var args = new string[0];
            var output = monitor.Start(args);
            
            Assert.IsTrue(AppReturnsMessage(output, "HelpMessage"));
        }

        [Test]
        public void ShouldDisplayHelpMessageWithWrongNumberOfArgs()
        {
            var args = new string[] { "./TestData/NoInactiveCompanies.txt", "5", "something" };
            var output = monitor.Start(args);
            
            Assert.IsTrue(AppReturnsMessage(output, "HelpMessage"));
        }

        [Test]
        public void ShouldDisplayFileNotFoundMessageWithMissingFile()
        {
            var args = new string[] { "./TestData/iDoNotExist.txt", "5" };
            var output = monitor.Start(args);

            Assert.IsTrue(AppReturnsMessage(output, "FileNotFound"));
        }

        [Test]
        public void ShouldDisplayHelpMessageWithInvalidDayCount()
        {
            var args = new string[] { "./TestData/NoInactiveCompanies.txt", "apple" };
            var output = monitor.Start(args);

            Assert.IsTrue(AppReturnsMessage(output, "InvalidDayCount"));

            args = new string[] { "./TestData/NoInactiveCompanies.txt", "-3" };
            output = monitor.Start(args);

            Assert.IsTrue(AppReturnsMessage(output, "InvalidDayCount"));
        }

        [Test]
        public void ShouldDisplayNoInactiveResults()
        {
            var args = new string[] { "./TestData/NoInactiveCompanies.txt", "5" };
            var output = monitor.Start(args);

            Assert.IsTrue(AppReturnsMessage(output, "NoResults"));
        }

        [Test]
        public void ShouldDisplayOneInactiveResult()
        {
            var args = new string[] { "./TestData/OneInactiveCompany.txt", "5" };
            var output = monitor.Start(args);

            Assert.IsTrue(AppReturnsMessage(output, "YesResults"));
            Assert.IsTrue(output.Contains("dr. mcninja"));
        }

        [Test]
        public void ShouldDisplayMultipleInactiveResult()
        {
            var args = new string[] { "./TestData/MultipleInactiveCompanies.txt", "5" };
            var output = monitor.Start(args);

            Assert.IsTrue(AppReturnsMessage(output, "YesResults"));
            Assert.IsTrue(output.Contains("dr. mcninja"));
            Assert.IsTrue(output.Contains("inactive company name"));
        }

        [Test]
        public void ShouldCountCompaniesAsActiveIfAtLeastOneFeedIsActive()
        {
            var args = new string[] { "./TestData/MixedActivity.txt", "5" };
            var output = monitor.Start(args);

            Assert.IsTrue(AppReturnsMessage(output, "NoResults"));
        }

        [Test]
        public void ShouldThrowErrorOnUnreadableFeed()
        {
            var args = new string[] { "./TestData/UnreadableFeed.txt", "5" };

            var ex = Assert.Throws<Exception>(() => { monitor.Start(args); });
            Assert.That(AppReturnsMessage(ex.Message, "CannotLoadFeed"));
        }

        /// <summary>
        /// Compares the program's output to the given string in the assembly's resource file.
        /// Because this demo's strings are all unique, only the first 5 characters are compared.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="resourceStringName"></param>
        /// <returns></returns>
        private bool AppReturnsMessage(string output, string resourceStringName)
        {
            return output.Substring(0, 5) == rm.GetString(resourceStringName).Substring(0, 5);
        }
    }
}
