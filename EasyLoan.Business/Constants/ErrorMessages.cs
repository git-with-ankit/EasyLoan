namespace EasyLoan.Business.Constants
{
    public static class ErrorMessages
    {
        // Authentication / Authorization
        public const string InvalidCredentials = "Invalid email or password.";
        public const string AccessDenied = "You do not have access to this resource.";
        public const string InvalidToken = "The token is invalid.";

        // Customer
        public const string CustomerNotFound = "Customer not found.";
        public const string EmailAlreadyExists = "Email already exists.";
        public const string PanAlreadyExists = "PAN number already exists.";

        // Loan Application
        public const string LoanApplicationNotFound = "Loan application not found.";
        public const string CreditScoreTooLow = "Credit score is too low to apply for loan.";
        public const string ManagersNotAvailable = "No managers available to approve loan. Please try after some time.";
        public const string LoanApplicationAlreadyReviewed = "Loan application has already been reviewed.";
        public const string ApprovedAmountCannotExceedRequestedAmount = "Approved amount cannot exceed requested amount.";
        public const string BelowMinimumLoanAmount = "Approved amount cannot be less than the minimum amount for this loan type.";

        // Loan
        public const string LoanNotFound = "Loan not found.";
        public const string LoanNotActive = "Loan is not active.";

        //LoanPayment
        public const string LoanPaymentsNotFound = "Loan payments not found.";

        //Loan Type 
        public const string ExceededMaxTenure = "The tenure selected cannot exceed max tenure.";
        public const string LoanTypeNotFound = "The loan type is not found.";
        public const string AmountLessThanMinAmount = "Amount cannot be less than the minimum amount for this loan type.";

        // Generic
        public const string UnexpectedError = "An unexpected error occurred.";
    }
}
