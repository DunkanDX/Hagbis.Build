using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Hagbis.Build {
    public class ExecTask : Task {
        string[] parameters;
        string execPath;
        [XmlElement]
        public string ExecPath {
            get { return execPath; }
            set { execPath = value; }
        }
        [XmlElement("Parameter")]
        public string[] Parameters {
            get { return parameters; }
            set { parameters = value; }
        }
        public override object Accept(ITaskProcessor processor) {
            if(processor == null) return null;
            return processor.Process(this);
        }
    }
}
