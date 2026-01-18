namespace EasyLoan.Business.Exceptions
{
    public abstract class EasyLoanException : Exception
    {
        protected EasyLoanException(string message) : base(message) { }
    }
}
