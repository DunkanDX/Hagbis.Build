using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Hagbis.Build {
    public class Project {
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
        [XmlArray("Tasks")]
        [XmlArrayItem("BCBBuildTask", typeof(BCBBuildTask))]
        [XmlArrayItem("CopyTask", typeof(CopyTask))]
        [XmlArrayItem("ExecTask", typeof(ExecTask))]
        public Task[] Tasks {
            get { return tasks; }
            set { tasks = value; }
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
}
