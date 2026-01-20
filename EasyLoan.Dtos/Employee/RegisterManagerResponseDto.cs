using EasyLoan.Models.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.Employee
{
    public class RegisterManagerResponseDto
    {
        public string Name { get; set; }
       
        public string Email { get; set; }
        
        public string PhoneNumber { get; set; }
        
        public EmployeeRole Role { get; set; }
    }
}
