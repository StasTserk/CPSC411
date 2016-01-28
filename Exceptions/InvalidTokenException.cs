using System;

namespace CPSC411.Exceptions
{
    /// <summary>
    /// Simple exception for declaring an invalid token error
    /// </summary>
    public class InvalidTokenException : Exception
    {
        public InvalidTokenException(string message) : base(message) { }
    }
}
