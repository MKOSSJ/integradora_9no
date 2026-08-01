using System;

namespace Plandi.Dto.Common
{
    public class AppException : Exception
    {
        public AppException(string message) : base(message)
        {
        }
    }
}
