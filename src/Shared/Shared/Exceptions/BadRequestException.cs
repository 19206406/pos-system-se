using Shared.Constants;
using Shared.Exceptions.Common;

namespace Shared.Exceptions
{
    public sealed class BadRequestException : BaseException
    {
        public BadRequestException(string message) : base(ErrorType.BadRequest, message) { }
    }
}
