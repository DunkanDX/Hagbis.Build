using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Hagbis.Build {
    public class CopyTask : Task {
        bool recursive;
        string exclude;
        string include;
        string destPath;
        string sourcePath;
        [XmlAttribute("sourcePath")]
        public string SourcePath {
            get { return sourcePath; }
            set { sourcePath = value; }
        }
        [XmlAttribute("destPath")]
        public string DestPath {
            get { return destPath; }
            set { destPath = value; }
        }
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
        [XmlAttribute("recursive")]
        public bool Recursive {
            get { return recursive; }
            set { recursive = value; }
        }
        public override object Accept(ITaskProcessor processor) {
            if(processor == null) return null;
            return processor.Process(this);
        }
    }
}
