using Shared.Exceptions.Common;

namespace Shared.Exceptions
{
    public sealed class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message) : base("Unauthorized", message) { }
    }
}
