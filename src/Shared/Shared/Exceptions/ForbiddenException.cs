using Shared.Exceptions.Common;

namespace Shared.Exceptions
{
    public sealed class ForbiddenException : BaseException
    {
        public ForbiddenException(string message) : base("Forbidden", message) { }
    }
}
