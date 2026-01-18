namespace EasyLoan.Business.Exceptions
{
    public sealed class BusinessRuleViolationException : EasyLoanException
    {
        public BusinessRuleViolationException(string message) : base(message) { }
    }
}
