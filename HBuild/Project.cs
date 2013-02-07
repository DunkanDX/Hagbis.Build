using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Hagbis.Build {
    public class Project {
        VariablesProcessor variablesProcessor;
        Variable[] variables;
        CopyOption copyOption;
        Task[] tasks;
        string name;
        public string Name {
            get { return name; }
            set { name = value; }
        }
        [XmlElement]
        public CopyOption CopyOption {
            get { return copyOption; }
            set { copyOption = value; }
        }
        public string CopyOptionInclude {
            get { return copyOption == null ? null : copyOption.Include; }
        }
        public string CopyOptionExclude {
            get { return copyOption == null ? null : copyOption.Exclude; }
        }
        [XmlElement("Variable")]
        public Variable[] Variables {
            get { return variables; }
            set { variables = value; }
        }
        [XmlArray("Tasks")]
        [XmlArrayItem("BCBBuild", typeof(BCBBuildTask))]
        [XmlArrayItem("Copy", typeof(CopyTask))]
        [XmlArrayItem("Exec", typeof(ExecTask))]
        [XmlArrayItem("Delete", typeof(DeleteTask))]
        [XmlArrayItem("Sleep", typeof(SleepTask))]
        public Task[] Tasks {
            get { return tasks; }
            set { tasks = value; }
        }
        public void ProcessVariables() {
            if(variablesProcessor != null) return;
            variablesProcessor = new VariablesProcessor(this);
            variablesProcessor.Process();
        }
    }
    public class CopyOption {
        string exclude;
        string include;
        [XmlAttribute("include")]
        public string Include {
            get { return include; }
            set { include = value; }
        }
        [XmlAttribute("exclude")]
        public string Exclude {
            get { return exclude; }
            set { exclude = value; }
        }
    }
    public class Variable {
        string val;
        string name;
        [XmlAttribute("name")]
        public string Name {
            get { return name; }
            set { name = value; }
        }
        [XmlAttribute("value")]
        public string Value {
            get { return val; }
            set { val = value; }
        }
    }
}
