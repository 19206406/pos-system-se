using Shared.Constants;
using Shared.Exceptions.Common;

namespace Shared.Exceptions
{
    public sealed class BusinessException : BaseException
    {
        public BusinessException(string message) : base(ErrorType.Business, message) { }
    }
}
