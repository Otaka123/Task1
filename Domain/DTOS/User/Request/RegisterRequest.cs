using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.User.Request
{
    public class RegisterRequest
    {
        [Required(ErrorMessage ="UserNameRequired")]
        public string UserName { get; set; }

        [Required( ErrorMessage = "EmailRequired")]
        [EmailAddress(ErrorMessage = "EmailInvalid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "FirstNameRequired")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastNameRequired")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "PasswordRequired")]
        [StringLength(100, ErrorMessage = "PasswordLength")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "ConfirmPasswordRequired")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "PasswordsNotMatch")]
        public string ConfirmPassword { get; set; }
    
    }
}
