using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Hagbis.Build {
    public abstract class Task {
        string id;
        TextProcessing[] textProcessingList;
        [XmlAttribute("id")]
        public string Id {
            get { return id; }
            set { id = value; }
        }
        [XmlElement("TextProcessing")]
        public TextProcessing[] TextProcessingList {
            get { return textProcessingList; }
            set { textProcessingList = value; }
        }
        public abstract object Accept(ITaskProcessor processor);
    }
    public class SleepTask : Task {
        int timeToSleep;
        [XmlAttribute("timeToSleep")]
        public int TimeToSleep {
            get { return timeToSleep; }
            set { timeToSleep = value; }
        }
        public override object Accept(ITaskProcessor processor) {
            if(processor == null) return null;
            return processor.Process(this);
        }
    }
    public class TextProcessing {
        TextProcessingType processingType;
        string result;
        string pattern;
        [XmlAttribute("pattern")]
        public string Pattern {
            get { return pattern; }
            set { pattern = value; }
        }
        [XmlAttribute("result")]
        public string Result {
            get { return result; }
            set { result = value; }
        }

        [XmlAttribute("type")]
        public TextProcessingType ProcessingType {
            get { return processingType; }
            set { processingType = value; }
        }
    }
    public enum TextProcessingType {
        Replace,
        Remove,
        Add,
    }
}
