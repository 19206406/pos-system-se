using Shared.Exceptions.Common;

namespace Shared.Exceptions
{
    public sealed class ConflictException : BaseException
    {
        public ConflictException(string message) : base("Conflict", message) { }
    }
}
