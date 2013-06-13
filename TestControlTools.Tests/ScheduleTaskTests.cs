using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TestControlTool.Core.Implementations;

namespace TestControlTools.Tests
{
    [TestFixture]
    public class ScheduleTaskTests
    {
        private ScheduleTask _task;
        private DateTime _time;

        [SetUp]
        public void SetUp()
        {
            _task = new ScheduleTask
                {
                    IsEnabled = true,
                    StartTime = new DateTime(1934, 3, 5, 3, 4, 5),
                    EndTime = new DateTime(2015, 3, 5, 3, 4, 5),
                    Frequency = "2 3 * *"
                };

            _time = new DateTime(2013, 3, 2, 3, 4, 0); ;
        }

        [Test]
        public void IsTimeToStartTest()
        {
            Assert.True(_task.IsTimeToStart(_time));
        }

        [Test]
        public void IsNotTimeToStartTest()
        {
            var time = new DateTime(2013, 3, 2, 3, 6, 0);

            Assert.False(_task.IsTimeToStart(time));
        }

        [Test]
        public void IsNotTimeToStartTimeLessThenStartTest()
        {
            var time = new DateTime(1913, 3, 2, 3, 6, 0);

            Assert.False(_task.IsTimeToStart(time));
        }

        [Test]
        public void IsNotTimeToStartTimeGreaterThenEndTest()
        {
            var time = new DateTime(2015, 3, 5, 3, 6, 0);

            Assert.False(_task.IsTimeToStart(time));
        }

        [Test]
        public void IsNotTimeToStartTaskIsNotEnabledTest()
        {
            _task.IsEnabled = false;

            Assert.False(_task.IsTimeToStart(_time));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void IsTimeToStartExceptionLeftGreaterTest()
        {
            _task.Frequency = "2 3 5-4 *";

            _task.IsTimeToStart(_time);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void IsTimeToStartExceptionGreaterMaximalTest()
        {
            _task.Frequency = "122 3 * *";
            
            _task.IsTimeToStart(_time);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void IsNotCronTimeFormatTest()
        {
            _task.Frequency = "* *ds * *";
            
            _task.IsTimeToStart(_time);
        }
    }
}
