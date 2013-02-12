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
        Stream GetTestXml1() {
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
            Assert.AreEqual(4, project.Tasks.Length);
            Task task = project.Tasks[0];
            Assert.IsNotNull(task);
            Assert.AreEqual(1, task.TextProcessingList.Length);
            Assert.AreEqual("pt", task.TextProcessingList[0].Pattern);
            Assert.AreEqual("res", task.TextProcessingList[0].Result);
            Assert.AreEqual(TextProcessingType.Replace, task.TextProcessingList[0].ProcessingType);
            BCBBuildTask bcbBuildTask = task as BCBBuildTask;
            Assert.IsNotNull(bcbBuildTask);
            Assert.AreEqual(@"%SP%\Super.bpr", bcbBuildTask.ProjectPath);
            Assert.AreEqual("TRIPL_ONE", bcbBuildTask.AddDefines);
            Assert.AreEqual(2, bcbBuildTask.ProcessingItems.Length);
            Assert.AreEqual("key1", bcbBuildTask.ProcessingItems[0].Element);
            Assert.AreEqual("a1", bcbBuildTask.ProcessingItems[0].Attribute);
            Assert.AreEqual("addOne", bcbBuildTask.ProcessingItems[0].ToAdd);
            Assert.AreEqual("removeTwo", bcbBuildTask.ProcessingItems[0].ToRemove);
            Assert.IsNull(bcbBuildTask.ProcessingItems[0].Value);
            Assert.AreEqual("key2", bcbBuildTask.ProcessingItems[1].Element);
            Assert.AreEqual("a2", bcbBuildTask.ProcessingItems[1].Attribute);
            Assert.AreEqual("addOne2", bcbBuildTask.ProcessingItems[1].ToAdd);
            Assert.AreEqual("11", bcbBuildTask.ProcessingItems[1].Value);

            CopyTask copyTask = project.Tasks[1] as CopyTask;
            Assert.IsNotNull(copyTask);
            Assert.AreEqual("sourcePath", copyTask.SourcePath);
            Assert.AreEqual("destPath", copyTask.DestPath);
            Assert.AreEqual("exe;com", copyTask.Include);
            Assert.AreEqual("txt;doc", copyTask.Exclude);
            Assert.AreEqual(true, copyTask.Recursive);

            ExecTask execTask = project.Tasks[2] as ExecTask;
            Assert.IsNotNull(execTask);
            Assert.IsTrue(execTask.ShellStart);
            Assert.AreEqual(@"C:\sds\sds.exe", execTask.ExecPath);
            Assert.AreEqual(3, execTask.Parameters.Length);
            Assert.AreEqual("-s", execTask.Parameters[0]);
            Assert.AreEqual("-t", execTask.Parameters[1]);
            Assert.AreEqual("%SOURCE%", execTask.Parameters[2]);

            Assert.AreEqual(2, project.Variables.Length);
            Assert.AreEqual("SP", project.Variables[0].Name);
            Assert.AreEqual(@"C:\Tests", project.Variables[0].Value);
            Assert.AreEqual("SOURCE", project.Variables[1].Name);
            Assert.AreEqual(@"C:\Source", project.Variables[1].Value);

            DeleteTask deleteTask = project.Tasks[3] as DeleteTask;
            Assert.IsNotNull(deleteTask);
            Assert.AreEqual("copyTask1", deleteTask.RelatedTaskId);
            Assert.AreEqual(@"%SP%\333", deleteTask.PathToDelete);
            Assert.AreEqual(true, deleteTask.Recursive);
            Assert.AreEqual(false, deleteTask.OnlyFiles);

            project.ProcessVariables();

            Assert.AreEqual(@"C:\Tests\Super.bpr", bcbBuildTask.ProjectPath);
            Assert.AreEqual(@"C:\Source", execTask.Parameters[2]);
            Assert.AreEqual(@"C:\Tests\333", deleteTask.PathToDelete);
        }
    }
}
#endif