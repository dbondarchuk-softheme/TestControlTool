using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using TestControlTool.Core;

namespace TestControlTool.Web.Models
{
  public class TestSuiteModel
    {
        public string Name { get; set; }

        public IEnumerable<object> Tests { get; set; }

        public TestSuiteType Type { get; set; }

        public Guid Machine { get; set; }

        public TestSuiteModel()
        {
            Name = string.Empty;
            Tests = new List<object>();
            Type = TestSuiteType.UITrunk;
            Machine = Guid.Empty;
        }

        public static TestSuiteModel GetFromXmlFile(string file, TestSuiteType type = TestSuiteType.UITrunk, Guid? machineId = null)
        {
            file = ConfigurationManager.AppSettings["TasksFolder"] + "\\" + file;

            var split = file.Split('.');
            var name = split.ElementAt(split.Length - 2);

            var suiteType = GetTestPerformerType("Suite", type);
            var testType = GetTestPerformerType("test", type);

            var suite = suiteType.DeserializeFromFile(file, TestSuiteTypesHelper.GetOnlyScriptsTypes(type).Union(new[] { suiteType, testType }).Where(x => !(x.IsAbstract && x.IsSealed) && !x.IsInterface));

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(file);

            var node = xmlDocument.SelectSingleNode("/Suite/Machine");
            var machine = machineId ?? (node != null ? new Guid(node.Attributes["id"].Value) : Guid.Empty);

            var model = new TestSuiteModel
                {
                    Name = name,
                    Tests = (IEnumerable<object>)(suite.GetType().GetProperty("Tests").GetValue(suite)),
                    Type = type,
                    Machine = machine
                };

            return model;
        }

        public void SaveToFile(string file)
        {
            file = ConfigurationManager.AppSettings["TasksFolder"] + "\\" + file;

            var suiteType = GetTestPerformerType("Suite", Type);
            var testType = GetTestPerformerType("Test", Type);

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

            suite.SerializeToFile(file, TestSuiteTypesHelper.GetOnlyScriptsTypes(Type).Union(new[] {suiteType, testType}).Where(x => !(x.IsAbstract && x.IsSealed) && !x.IsInterface));

            var content = File.ReadAllText(file, new UnicodeEncoding());

            var insertString = "<Machine id=\"MACHINE_ID\" address=\"{$MACHINE_ID/Address}\" username=\"{$MACHINE_ID/UserName}\" password=\"{$MACHINE_ID/Password}\" share=\"{$MACHINE_ID/Share}\" />"
                .Replace("MACHINE_ID", Machine.ToString());

            content = content.Insert(content.IndexOf("<Tests", System.StringComparison.OrdinalIgnoreCase), insertString);

            File.WriteAllText(file, content, new UnicodeEncoding());

        }

        public string GetJson()
        {
            var json = "[";

            foreach (var test in Tests)
            {
                var serializer = new DataContractJsonSerializer(test.GetType(), TestSuiteTypesHelper.GetScriptsTypes(Type));

                using (var stream = new MemoryStream())
                {
                    serializer.WriteObject(stream, test);
                    json += " { \"type\" : \"" + test.GetType().FullName + "\", \"object\": " +  Encoding.Default.GetString(stream.ToArray()) + "},";
                }
            }

            json = json.TrimEnd(',') + " ]";

            return json;
        }

        private static Type GetTestPerformerType(string type, TestSuiteType suiteType)
        {
            var types = TestSuiteTypesHelper.GetTestPerformerTypes(suiteType);

            return types.SingleOrDefault(x => x.Name.ToUpperInvariant() == type.ToUpperInvariant());
        }
    }
}