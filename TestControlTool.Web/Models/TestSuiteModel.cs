using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using TestControlTool.Core;

namespace TestControlTool.Web.Models
{
    public class TestSuiteModel
    {
        public string Name { get; set; }

        public IEnumerable<object> Tests { get; set; }

        public bool IsTrunk { get; set; }

        public Guid Machine { get; set; }

        public TestSuiteModel()
        {
            Name = string.Empty;
            Tests = new List<object>();
            IsTrunk = true;
            Machine = Guid.Empty;
        }

        public static TestSuiteModel GetFromXmlFile(string file, bool isTrunk = true, Guid? machineId = null)
        {
            file = ConfigurationManager.AppSettings["TasksFolder"] + "\\" + file;

            var split = file.Split('.');
            var name = split.ElementAt(split.Length - 2);

            var suiteType = GetTestPerformerType("Suite", isTrunk);
            var testType = GetTestPerformerType("test", isTrunk);
            
            var suite = suiteType.DeserializeFromFile(file, (isTrunk ? TestControlToolApplication.TypesHelper.ScriptsTrunkTypes
                : TestControlToolApplication.TypesHelper.ScriptsReleaseTypes).Union(new[] { suiteType, testType }).Where(x => !(x.IsAbstract && x.IsSealed)));

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(file);

            var node = xmlDocument.SelectSingleNode("/Suite/Machine");
            var machine = machineId ?? (node != null ? new Guid(node.Attributes["id"].Value) : Guid.Empty);

            var model = new TestSuiteModel
                {
                    Name = name,
                    Tests = (IEnumerable<object>)(suite.GetType().GetProperty("Tests").GetValue(suite)),
                    IsTrunk = isTrunk,
                    Machine = machine
                };

            return model;
        }

        public void SaveToFile(string file)
        {
            file = ConfigurationManager.AppSettings["TasksFolder"] + "\\" + file;

            var suiteType = GetTestPerformerType("Suite", IsTrunk);
            var testType = GetTestPerformerType("Test", IsTrunk);

            var suite = Activator.CreateInstance(suiteType);

            suite.GetType().GetProperty("Name").SetValue(suite, Name);

            var collectionType = typeof (Collection<>).MakeGenericType(testType);
            var addMethod = collectionType.GetMethod("Add");

            var collection = Activator.CreateInstance(collectionType);

            foreach (var test in Tests)
            {
                addMethod.Invoke(collection, new[] {test});
            }

            suite.GetType().GetProperty("Tests").SetValue(suite, collection);

            suite.SerializeToFile(file, (IsTrunk ? TestControlToolApplication.TypesHelper.ScriptsTrunkTypes
                : TestControlToolApplication.TypesHelper.ScriptsReleaseTypes).Union(new[] {suiteType, testType}).Where(x => !(x.IsAbstract && x.IsSealed)));

            var content = File.ReadAllText(file, new UnicodeEncoding());

            var insertString = "<Machine id=\"MACHINE_ID\" address=\"{$MACHINE_ID/Address}\" username=\"{$MACHINE_ID/UserName}\" password=\"{$MACHINE_ID/Password}\" share=\"{$MACHINE_ID/Share}\" />"
                .Replace("MACHINE_ID", Machine.ToString());

            content = content.Insert(content.IndexOf("<Tests", System.StringComparison.OrdinalIgnoreCase), insertString);

            File.WriteAllText(file, content, new UnicodeEncoding());

        }

        private static Type GetTestPerformerType(string type, bool isTrunk)
        {
            var types = isTrunk ? TestControlToolApplication.TypesHelper.TestPerformerTrunkTypes : TestControlToolApplication.TypesHelper.TestPerformerReleaseTypes;

            return types.SingleOrDefault(x => x.Name.ToUpperInvariant() == type.ToUpperInvariant());
        }
    }
}