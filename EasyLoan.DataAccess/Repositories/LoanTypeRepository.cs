using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyLoan.DataAccess.Repositories
{
    public class LoanTypeRepository : GenericRepository<LoanType>, ILoanTypeRepository
    {
        public LoanTypeRepository(EasyLoanDbContext context) : base(context) { }

    }
}
