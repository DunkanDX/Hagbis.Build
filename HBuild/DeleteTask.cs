using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Hagbis.Build {
    public class DeleteTask : Task {
        bool onlyFiles;
        bool recursive;
        string pathToDelete;
        string relatedTask;
        [XmlAttribute("relatedTaskId")]
        public string RelatedTaskId {
            get { return relatedTask; }
            set { relatedTask = value; }
        }
        [XmlAttribute("pathToDelete")]
        public string PathToDelete {
            get { return pathToDelete; }
            set { pathToDelete = value; }
        }
        [XmlAttribute("recursive")]
        public bool Recursive {
            get { return recursive; }
            set { recursive = value; }
        }
        [XmlAttribute("onlyFiles")]
        public bool OnlyFiles {
            get { return onlyFiles; }
            set { onlyFiles = value; }
        }
        public override object Accept(ITaskProcessor processor) {
            if(processor == null) return null;
            return processor.Process(this);
        }
    }
}
