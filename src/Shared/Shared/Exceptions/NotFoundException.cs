using Shared.Constants;
using Shared.Exceptions.Common;

namespace Shared.Exceptions
{
    public sealed class NotFoundException : BaseException
    {
        public NotFoundException(string message) : base(ErrorType.NotFound, message) { }

        public NotFoundException(string entity, string key) : base(ErrorType.NotFound, $"{entity} with identifier '{key}' was not found") { }
    }
}
