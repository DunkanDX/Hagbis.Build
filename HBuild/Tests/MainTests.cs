#if DEBUGTEST
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using NUnit.Framework;


namespace Hagbis.Build.Tests {
    [TestFixture]
    public class MainTests {
        public Stream GetTestXml1() {
            return GetType().Assembly.GetManifestResourceStream("Hagbis.Build.Tests.ProjectForTest.xml");
        }
        [Test]
        public void LoadConfig() {
            Stream testStream = GetTestXml1();
            Assert.IsNotNull(testStream);
            XmlSerializer serializer = new XmlSerializer(typeof(Project));
            Project project = serializer.Deserialize(testStream) as Project;
            Assert.IsNotNull(project);
            Assert.AreEqual("TestProject", project.Name);
            Assert.IsNotNull(project.CopyOption);
            Assert.AreEqual("exe;com", project.CopyOption.Include);
            Assert.AreEqual("txt;doc", project.CopyOption.Exclude);
            Assert.IsNotNull(project.Tasks);
            Assert.AreEqual(3, project.Tasks.Length);
            Task task = project.Tasks[0];
            Assert.IsNotNull(task);
            Assert.AreEqual(1, task.TextProcessingList.Length);
            Assert.AreEqual("pt", task.TextProcessingList[0].Pattern);
            Assert.AreEqual("res", task.TextProcessingList[0].Result);
            Assert.AreEqual(TextProcessingType.Replace, task.TextProcessingList[0].ProcessingType);
            BCBBuildTask bcbBuildTask = task as BCBBuildTask;
            Assert.IsNotNull(bcbBuildTask);
            Assert.AreEqual("%SP%\\Super.bpr", bcbBuildTask.ProjectPath);
            Assert.AreEqual("TRIPL_ONE", bcbBuildTask.AddDefines);
            Assert.AreEqual(2, bcbBuildTask.ProcessingItems.Length);
            Assert.AreEqual("key1", bcbBuildTask.ProcessingItems[0].Key);
            Assert.AreEqual("addOne", bcbBuildTask.ProcessingItems[0].ToAdd);
            Assert.AreEqual("removeTwo", bcbBuildTask.ProcessingItems[0].ToRemove);
            Assert.AreEqual("key2", bcbBuildTask.ProcessingItems[1].Key);
            Assert.AreEqual("addOne2", bcbBuildTask.ProcessingItems[1].ToAdd);
            Assert.AreEqual("removeTwo2", bcbBuildTask.ProcessingItems[1].ToRemove);

            CopyTask copyTask = project.Tasks[1] as CopyTask;
            Assert.IsNotNull(copyTask);
            Assert.AreEqual("sourcePath", copyTask.SourcePath);
            Assert.AreEqual("destPath", copyTask.DestPath);
            Assert.AreEqual("exe;com", copyTask.Include);
            Assert.AreEqual("txt;doc", copyTask.Exclude);
        }
    }
}
#endif