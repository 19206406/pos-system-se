using Shared.Constants;

namespace Shared.Exceptions.Common
{
    public abstract class BaseException : Exception
    {
        public ErrorType ErrorType { get; }

        protected BaseException(ErrorType errorType, string message) : base(message)
        {
            ErrorType = errorType; 
        }
    }
}
