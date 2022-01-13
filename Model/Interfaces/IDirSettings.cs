using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Interfaces
{
    public interface IDirSettings
    {
        string OutputDirectory { get; }
        string InputDirectory { get; }
        string GetContentFileName(string fileName);
    }
}
