using Shared.Constants;
using Shared.Exceptions.Common;

namespace Shared.Exceptions
{
    public sealed class InvalidTokenException : BaseException
    {
        public InvalidTokenException(string message) : base(ErrorType.InvalidToken, message) { }
    }
}
