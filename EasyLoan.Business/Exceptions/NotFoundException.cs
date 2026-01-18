namespace EasyLoan.Business.Exceptions
{
    public sealed class NotFoundException : EasyLoanException
    {
        public NotFoundException(string message) : base(message) { }
    }
}