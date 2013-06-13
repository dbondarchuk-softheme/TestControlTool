using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Web;
using TestControlTool.Management;
using WebGuiAutomation.Scripts;
using WebGuiAutomation.TestPerformerCore.Testing;

namespace TestControlTool.Web.Models
{
    public class TestSuiteModel
    {
        public string Name { get; set; }

        public IEnumerable<object> Tests { get; set; }

        public TestSuiteModel()
        {
            Name = string.Empty;
            Tests = new List<object>();
        }

        public static TestSuiteModel GetFromXmlFile(string file)
        {
            file = ConfigurationManager.AppSettings["TasksFolder"] + "\\" + file;

            var split = file.Split('.');
            var name = split.ElementAt(split.Length - 2);

            var suite = Extensions.DeserializeFromFile<Suite>(file, TestControlToolApplication.AvailableTests);

            var model = new TestSuiteModel
                {
                    Name = name,
                    Tests = suite.Tests
                };

            return model;
        }

        public void SaveToFile(string file)
        {
            file = ConfigurationManager.AppSettings["TasksFolder"] + "\\" + file;

            var suite = new Suite
                {
                    Name = Name,
                    Tests = new Collection<Test>(Tests.Cast<Test>().ToList())
                };

            suite.SerializeToFile(file, TestControlToolApplication.AvailableTests);
        }
    }
}