using Shared.Exceptions.Common;

namespace Shared.Exceptions
{
    public sealed class NotFoundException : BaseException
    {
        public NotFoundException(string message) : base("NotFound", message) { }

        public NotFoundException(string entity, string key) : base("NotFound", $"{entity} with identifier '{key}' was not found") { }
    }
}
