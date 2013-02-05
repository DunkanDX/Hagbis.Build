using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Hagbis.Build {
    public class Task {
        TextProcessing[] textProcessingList;
        [XmlElement("TextProcessing")]
        public TextProcessing[] TextProcessingList {
            get { return textProcessingList; }
            set { textProcessingList = value; }
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
