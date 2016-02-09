using System;

namespace CPSC411.Exceptions
{
    public class UnexpectedTokenException : Exception
    {
        public UnexpectedTokenException(string message) : base(message) { }
    }
}