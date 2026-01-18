namespace EasyLoan.Business.Exceptions
{
    public sealed class ForbiddenException : EasyLoanException
    {
        public ForbiddenException(string message) : base(message) { }
    }
}
