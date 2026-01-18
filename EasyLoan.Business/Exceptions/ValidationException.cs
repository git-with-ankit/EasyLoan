namespace EasyLoan.Business.Exceptions
{
    public sealed class ValidationException : EasyLoanException
    {
        public ValidationException(string message) : base(message) { }
    }
}
