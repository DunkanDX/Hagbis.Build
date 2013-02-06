using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hagbis.Build {
    public interface ITaskProcessor {
        object Process(BCBBuildTask task);
        object Process(CopyTask task);
        object Process(ExecTask task);
    }
}
