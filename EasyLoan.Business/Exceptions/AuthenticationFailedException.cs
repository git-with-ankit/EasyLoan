namespace EasyLoan.Business.Exceptions
{
    public sealed class AuthenticationFailedException : EasyLoanException
    {
        public AuthenticationFailedException(string message) : base(message) { }
    }
}
