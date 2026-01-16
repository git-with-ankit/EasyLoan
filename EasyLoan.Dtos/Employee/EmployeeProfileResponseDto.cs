using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.Employee
{
    public class EmployeeProfileResponseDto//TODO : Generate a service and controller for this.
    {
        public Guid EmployeeId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
