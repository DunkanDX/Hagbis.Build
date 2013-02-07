using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Hagbis.Build {
    class Program {
        static int Main(string[] args) {
            if(args.Length != 1) {
                Console.WriteLine("Please specify project file name");
                return 1;
            }
            if(!File.Exists(args[0])) {
                Console.WriteLine("File not found: {0}", args[0]);
                return 1;
            }
            try{
                XmlSerializer serializer = new XmlSerializer(typeof(Project));
                Project project;
                using(FileStream fs = new FileStream(args[0], FileMode.Open, FileAccess.Read)){
                    project = serializer.Deserialize(fs) as Project;
                }
                if(project == null) {
                    Console.WriteLine("Error: project didn't deserialized correctly");
                    return 1;
                }
                project.ProcessVariables();
                TaskExecutor executor = new TaskExecutor(project);
                object result = executor.ProcessTasks();
                if(result == null) {
                    Console.ReadKey();
                    return 0;
                }
                Console.WriteLine("Error: {0}", result.ToString());
                Console.ReadKey();
                return 1;
            }catch(Exception ex){
                Console.WriteLine("Error: {0}", ex.ToString());
                Console.ReadKey();
                return 1;
            }            
        }
    }
}
