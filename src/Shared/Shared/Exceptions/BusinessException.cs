using Shared.Exceptions.Common;

namespace Shared.Exceptions
{
    public sealed class BusinessException : BaseException
    {
        public BusinessException(string message) : base("Business", message) { }
    }
}
