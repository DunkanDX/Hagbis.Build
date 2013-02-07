using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace Hagbis.Build {
    public class TaskExecutor : ITaskProcessor { 
        readonly Project project;
        readonly static char[] separatorChars = new char[] { ';' };
        readonly static string separatorString = ";";
        readonly Dictionary<string, string[]> processedFilesDict = new Dictionary<string, string[]>();
        readonly Dictionary<string, string[]> processedDirectoriesDict = new Dictionary<string, string[]>();
        public TaskExecutor(Project project) {
            this.project = project;
        }
        public object ProcessTasks() {
            foreach(Task task in project.Tasks) {
                if(task == null) continue;
                object result = task.Accept(this);
                if(result == null) continue;
                Console.WriteLine("Error: {0}", result);
                return result;
            }
            return null;
        }
        object ITaskProcessor.Process(BCBBuildTask task) {
            if(task == null) return null;
            string project = task.ProjectPath;            
            string projectDir = Path.GetDirectoryName(project);
            if(!File.Exists(CommonSettings.BprToMakExe)) {
                return new FileNotFoundException(string.Format("File not found {0}", CommonSettings.BprToMakExe));                
            }
            if(!File.Exists(CommonSettings.MakeExe)) {
                return new FileNotFoundException(string.Format("File not found {0}", CommonSettings.MakeExe));     
            }
            string currentDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(projectDir);
            bool removeProjectFile = PrepareProject(ref project, task);
            string projectName = Path.GetFileNameWithoutExtension(project);
            try {
                BprToMak(project, projectName);
                Make(project, projectName);
            }catch(Exception ex){
                return ex;
            } finally {
                Directory.SetCurrentDirectory(currentDir);
                if(removeProjectFile && File.Exists(project))
                    File.Delete(project);
            }
            return null;
        }

        const string DefinesTag = "USERDEFINES";

        static bool PrepareProject(ref string project, BCBBuildTask task) {
            if(string.IsNullOrEmpty(task.AddDefines) && (task.ProcessingItems == null || task.ProcessingItems.Length == 0))
                return false;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(project);
            if(!string.IsNullOrEmpty(task.AddDefines)) {
                XmlNodeList definesNodeList = xmlDoc.GetElementsByTagName(DefinesTag);
                foreach(XmlNode node in definesNodeList) {
                    node.Attributes["value"].Value = string.Concat(node.Attributes["value"].Value, separatorString, string.Join(separatorString, task.AddDefines.Split(separatorChars, StringSplitOptions.RemoveEmptyEntries)), separatorString);
                }
            }
            if(task.ProcessingItems != null) {
                foreach(BCBProjectProcessingItem item in task.ProcessingItems) {
                    if(string.IsNullOrEmpty(item.Element)) continue;
                    XmlNodeList nodeList = xmlDoc.GetElementsByTagName(item.Element);
                    string[] removeList = string.IsNullOrEmpty(item.ToRemove) ? null : item.ToRemove.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach(XmlNode node in nodeList) {
                        if(string.IsNullOrEmpty(item.Attribute)) continue;
                        XmlAttribute attribute = node.Attributes[item.Attribute];
                        if(attribute == null) {
                            attribute = xmlDoc.CreateAttribute(item.Attribute); 
                            node.Attributes.Append(attribute);
                        }
                        if(item.Value != null) {
                            attribute.Value = item.Value;
                            continue;
                        }
                        string value = attribute.Value;                        
                        if(removeList != null) {
                            foreach(var remove in removeList) {
                                value = value.Replace(remove, "");
                            }
                        }
                        if(!string.IsNullOrEmpty(item.ToAdd)) {
                            value = value + item.ToAdd;
                        }
                        attribute.Value = value;
                    }
                }
            }
            string dir = Path.GetDirectoryName(project);
            string projectName = Path.GetFileNameWithoutExtension(project);
            project = Path.Combine(dir, string.Concat(projectName, Guid.NewGuid().ToString("N"), ".bpr"));
            xmlDoc.Save(project);
            string contect = File.ReadAllText(project);
            contect.Replace("&#xD;&#xA;", Environment.NewLine);
            File.WriteAllText(project, contect);
            return true;
        }
        static void BprToMak(string project, string projectName) {
            Console.WriteLine("Start bpr2mak for {0}...", project);
            ProcessStartInfo psi = new ProcessStartInfo(CommonSettings.BprToMakExe, string.Format("-o{0}.mak {0}.bpr", projectName));
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            Process process = Process.Start(psi);
            string results = process.StandardOutput.ReadToEnd();
            Console.WriteLine(results);
            if(process.ExitCode == 1) {
                throw new InvalidOperationException(string.Format("Erros while bpr2mak for {0}", project));
            }
        }

        static void Make(string project, string projectName) {
            Console.WriteLine("Start make for {0}...", project);
            ProcessStartInfo psi = new ProcessStartInfo(CommonSettings.MakeExe, string.Format("-B -f{0}.mak", projectName));
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            Process process = Process.Start(psi);
            string results = process.StandardOutput.ReadToEnd();
            Console.WriteLine(results);
            if(process.ExitCode == 1) {
                throw new InvalidOperationException(string.Format("Erros while make for {0}", project));
            }
        }

        object ITaskProcessor.Process(CopyTask task) {
            if(task == null) return null;
            try {
                if(File.Exists(task.SourcePath)) {
                    File.Copy(task.SourcePath, task.DestPath, true);
                    return null;
                }
                if(Directory.Exists(task.SourcePath)) {
                    if((!string.IsNullOrEmpty(task.Exclude) && !string.IsNullOrEmpty(task.Include)) ||
                       (!string.IsNullOrEmpty(project.CopyOptionExclude) && !string.IsNullOrEmpty(task.Include)) ||
                        (!string.IsNullOrEmpty(task.Exclude) && !string.IsNullOrEmpty(project.CopyOptionInclude))) {
                            return new InvalidOperationException("Wrong include/exclude copy parameters.");
                    }                    
                    bool include = false;
                    HashSet<string> filterSet = new HashSet<string>();
                    if(!string.IsNullOrEmpty(task.Exclude) || !string.IsNullOrEmpty(project.CopyOptionExclude)) {
                        if(task.Exclude != null){
                            foreach(var exItem in task.Exclude.Split(separatorChars, StringSplitOptions.RemoveEmptyEntries)) {
                                filterSet.Add(exItem);
                            }
                        }
                        if(project.CopyOptionExclude != null) {
                            foreach(var exItem in project.CopyOptionExclude.Split(separatorChars, StringSplitOptions.RemoveEmptyEntries)) {
                                filterSet.Add(exItem);
                            }
                        }
                    }else if(!string.IsNullOrEmpty(task.Include) || !string.IsNullOrEmpty(project.CopyOptionInclude)) {
                        include = true;
                        if(task.Include != null) {
                            foreach(var inItem in task.Include.Split(separatorChars, StringSplitOptions.RemoveEmptyEntries)) {
                                filterSet.Add(inItem);
                            }
                        }
                        if(project.CopyOptionInclude != null) {
                            foreach(var inItem in project.CopyOptionInclude.Split(separatorChars, StringSplitOptions.RemoveEmptyEntries)) {
                                filterSet.Add(inItem);
                            }
                        }
                    }
                    List<string> sourceDirectories = new List<string>();
                    string[] sourceFiles;
                    sourceDirectories.Add(task.SourcePath);
                    if(!task.Recursive) {
                        sourceFiles = Directory.GetFiles(task.SourcePath, "*", SearchOption.TopDirectoryOnly);
                    } else {
                        sourceDirectories.AddRange(Directory.GetDirectories(task.SourcePath, "*", SearchOption.AllDirectories));
                        sourceFiles = Directory.GetFiles(task.SourcePath, "*", SearchOption.AllDirectories);
                    }
                    int sourcePathLength = task.SourcePath.Length;
                    List<string> directoriesToDelete = new List<string>(sourceDirectories.Count);
                    foreach(string dir in sourceDirectories) {
                        string partDir = dir.Remove(0, sourcePathLength).TrimStart('\\');
                        string destWDir = Path.Combine(task.DestPath, partDir);
                        directoriesToDelete.Add(destWDir);
                        if(!Directory.Exists(destWDir)) {
                            Directory.CreateDirectory(destWDir);
                        }
                    }
                    Console.WriteLine();
                    Console.WriteLine("Coping: {0}...", task.SourcePath);
                    int lastLeft = Console.CursorLeft;
                    int lastTop = Console.CursorTop;
                    int fileIndex = 0;
                    int fileCount = sourceFiles.Length;
                    List<string> filesToDelete = new List<string>(sourceFiles.Length);
                    foreach(string file in sourceFiles) {
                        bool foundInFilterSet = filterSet.Contains(Path.GetFileName(file)) || filterSet.Contains(Path.GetExtension(file));
                        if((!foundInFilterSet && include) || (foundInFilterSet && !include))
                            continue;
                        string partFile = file.Remove(0, sourcePathLength).TrimStart('\\');
                        string destFile = Path.Combine(task.DestPath, partFile);
                        File.Copy(file, destFile, true);
                        if(fileIndex > 0 && (fileIndex % 15) == 0) {
                            Console.SetCursorPosition(lastLeft, lastTop);
                            Console.Write("\t{0} %             ", fileIndex * 100 / fileCount);
                        }
                        fileIndex++;
                        filesToDelete.Add(destFile);
                    }
                    if(!string.IsNullOrEmpty(task.Id)) {
                        processedDirectoriesDict[task.Id] = directoriesToDelete.ToArray();
                        processedFilesDict[task.Id] = filesToDelete.ToArray();
                    }
                    return null;
                }
            } catch(Exception ex) {
                return ex;
            }
            return new FileNotFoundException(task.SourcePath);
        }
        object ITaskProcessor.Process(ExecTask task) {
            if(task == null) return null;
            try {
                ProcessStartInfo psi = task.Parameters == null ? new ProcessStartInfo(task.ExecPath) : new ProcessStartInfo(task.ExecPath, string.Join(" ", task.Parameters));
                Process process = Process.Start(psi);
                process.WaitForExit();
                if(process.ExitCode == 1) {
                    return new InvalidOperationException(string.Format("Erros while execute: {0} {1}", task.ExecPath, string.Join(" ", task.Parameters)));
                }
                return null;
            } catch(Exception ex) {
                return ex;
            }           
        }

        object ITaskProcessor.Process(DeleteTask task) {
            if(task == null) return null;
            List<string> files = new List<string>();
            List<string> directories = new List<string>();
            try {
                if(!string.IsNullOrEmpty(task.RelatedTaskId)) {
                    if(!processedFilesDict.ContainsKey(task.RelatedTaskId))
                        return new InvalidOperationException(string.Format("Task id not found: {0}", task.RelatedTaskId));
                    if(!task.OnlyFiles)
                        directories.AddRange(processedDirectoriesDict[task.RelatedTaskId]);
                    files.AddRange(processedFilesDict[task.RelatedTaskId]);
                }
                if(!string.IsNullOrEmpty(task.PathToDelete)) {
                    if(File.Exists(task.PathToDelete)) {
                        files.Add(task.PathToDelete);
                    } else if(Directory.Exists(task.PathToDelete)) {
                        if(!task.OnlyFiles) {
                            directories.Add(task.PathToDelete);
                            directories.AddRange(Directory.GetDirectories(task.PathToDelete, "*", task.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                        }
                        files.AddRange(Directory.GetFiles(task.PathToDelete, "*", task.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                    } else {
                        return new FileNotFoundException(task.PathToDelete);
                    }
                }
                foreach(var file in files) {
                    try {
                        if(File.Exists(file)) {
                            File.SetAttributes(file, FileAttributes.Normal);
                            File.Delete(file);
                        }
                    } catch(Exception ex) {
                        Console.WriteLine("Delete error skipped: {0}", ex.ToString());
                    }
                }
                for(int i = directories.Count - 1; i >= 0; i--) {
                    var dir = directories[i];
                    try {
                        if(Directory.Exists(dir)) {
                            Directory.Delete(dir);
                        }
                    } catch(Exception ex) {
                        Console.WriteLine("Delete error skipped: {0}", ex.ToString());
                    }
                }
                return null;
            } catch(Exception ex) {
                return ex;
            }
        }
        object ITaskProcessor.Process(SleepTask task) {
            if(task == null) return null;
            Console.WriteLine("Sleeps: {0}", task.TimeToSleep);
            Thread.Sleep(task.TimeToSleep);
            return null;
        }
    }
}
