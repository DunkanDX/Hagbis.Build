using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hagbis.Build {
    public class VariablesProcessor : ITaskProcessor {
        Project project;
        List<Tuple<string, string>> preparedVariables = new List<Tuple<string, string>>();
        public VariablesProcessor(Project project) {
            if(project == null) new ArgumentNullException("project");
            this.project = project;
            if(project.Variables != null) {
                foreach(var variable in project.Variables) {
                    preparedVariables.Add(new Tuple<string, string>(string.Concat("%", variable.Name.ToUpper(), "%"), variable.Value));
                }
            }
            DateTime now = DateTime.Now;
            preparedVariables.Add(new Tuple<string, string>("%DATETIME%", now.ToString("yyyyMMdd_HHmmss")));
            preparedVariables.Add(new Tuple<string, string>("%DATE%", now.ToString("yyyyMMdd")));
            preparedVariables.Add(new Tuple<string, string>("%TIME%", now.ToString("HHmmss")));
            preparedVariables.Add(new Tuple<string, string>("%DTHASH%", now.ToBinary().ToString()));
            preparedVariables.Add(new Tuple<string, string>("%PROGRAM%", Path.GetDirectoryName(typeof(Program).Assembly.Location)));
            preparedVariables.Add(new Tuple<string, string>("%7ZEXE%", Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "7za.exe")));
            preparedVariables.Add(new Tuple<string, string>("%7ZSFX%", Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "7z.sfx")));
        }
        public List<object> Process() {
            if(project.Tasks == null) return null;
            List<object> result = new List<object>();
            foreach(Task task in project.Tasks) {
                if(task == null) {
                    result.Add(null);
                    continue;
                }
                result.Add(task.Accept(this));
            }
            return result;
        }

        object ProcessBaseTask(Task task) {
            if(task.TextProcessingList != null) {
                foreach(var tpItem in task.TextProcessingList) {
                    tpItem.Pattern = ProcessString(tpItem.Pattern);
                    tpItem.Result = ProcessString(tpItem.Result);
                }
            }
            return null;
        }

        object ITaskProcessor.Process(BCBBuildTask task) {
            ProcessBaseTask(task);
            task.ProjectPath = ProcessString(task.ProjectPath);
            task.AddDefines = ProcessString(task.AddDefines);
            if(task.ProcessingItems != null) {
                foreach(var processintItem in task.ProcessingItems) {
                    processintItem.Element = ProcessString(processintItem.Element);
                    processintItem.Attribute = ProcessString(processintItem.Attribute);
                    processintItem.ToAdd = ProcessString(processintItem.ToAdd);
                    processintItem.ToRemove = ProcessString(processintItem.ToRemove);
                }
            }
            return null;
        }

        object ITaskProcessor.Process(CopyTask task) {
            ProcessBaseTask(task);
            task.SourcePath = ProcessString(task.SourcePath);
            task.DestPath = ProcessString(task.DestPath);
            task.Include = ProcessString(task.Include);
            task.Exclude = ProcessString(task.Exclude);
            return null;
        }
        object ITaskProcessor.Process(DeleteTask task) {
            ProcessBaseTask(task);
            task.PathToDelete = ProcessString(task.PathToDelete);
            return null;
        }

        object ITaskProcessor.Process(ExecTask task) {
            ProcessBaseTask(task);
            task.ExecPath = ProcessString(task.ExecPath);
            if(task.Parameters != null) {
                for(int i = 0; i < task.Parameters.Length; i++) {
                    task.Parameters[i] = ProcessString(task.Parameters[i]);
                }
            }
            return null;
        }
        object ITaskProcessor.Process(SleepTask task) {
            return ProcessBaseTask(task);
        }

        string ProcessString(string str) {
            if(string.IsNullOrEmpty(str)) return str;
            string result = str;
            foreach(var variable in preparedVariables) {
                result = result.Replace(variable.Item1, variable.Item2);
            }
            return result;
        }
    }
}
