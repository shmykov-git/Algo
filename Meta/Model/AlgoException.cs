using System;

namespace Meta.Model
{
    public class AlgorithmException : Exception
    {
        public AlgorithmException(string? message = null) : base(message)
        {
        }
    }
}
