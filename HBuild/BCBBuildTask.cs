using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Hagbis.Build {
    public class BCBBuildTask : Task {
        BCBProjectProcessingItem[] processingItems;
        string addDefines;
        string projectPath;
        [XmlAttribute("projectPath")]
        public string ProjectPath {
            get { return projectPath; }
            set { projectPath = value; }
        }
        [XmlAttribute("addDefines")]
        public string AddDefines {
            get { return addDefines; }
            set { addDefines = value; }
        }
        [XmlElement("ProcessingItem")]
        public BCBProjectProcessingItem[] ProcessingItems {
            get { return processingItems; }
            set { processingItems = value; }
        }
        public override object Accept(ITaskProcessor processor) {
            if(processor == null) return null;
            return processor.Process(this);
        }
    }
    public class BCBProjectProcessingItem {
        string val;
        string attribute;
        string toAdd;
        string toRemove;
        string element;
        [XmlAttribute("element")]
        public string Element {
            get { return element; }
            set { element = value; }
        }
        [XmlAttribute("attribute")]
        public string Attribute {
            get { return attribute; }
            set { attribute = value; }
        }
        [XmlAttribute("toRemove")]
        public string ToRemove {
            get { return toRemove; }
            set { toRemove = value; }
        }
        [XmlAttribute("toAdd")]
        public string ToAdd {
            get { return toAdd; }
            set { toAdd = value; }
        }
        [XmlAttribute("value")]
        public string Value {
            get { return val; }
            set { val = value; }
        }
    }
}
