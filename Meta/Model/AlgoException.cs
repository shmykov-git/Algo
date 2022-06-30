using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meta.Model
{
    public class AlgorithmException : Exception
    {
        public AlgorithmException(string message) : base(message)
        {
        }
    }
}
