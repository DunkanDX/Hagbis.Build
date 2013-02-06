using System;
using System.Collections.Generic;
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
                    preparedVariables.Add(new Tuple<string, string>(string.Concat("%", variable.Name, "%"), variable.Value));
                }
            }
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
                    processintItem.Key = ProcessString(processintItem.Key);
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

        string ProcessString(string str) {
            string result = str;
            foreach(var variable in preparedVariables) {
                result = result.Replace(variable.Item1, variable.Item2);
            }
            return result;
        }
    }
}
