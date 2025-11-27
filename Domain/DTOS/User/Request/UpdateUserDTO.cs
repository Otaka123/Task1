using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.User.Request
{
    public class UpdateUserDTO
    {
        // معلومات الحساب الأساسية
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
       

        // كلمة المرور الجديدة (اختيارية - إزالة Required)
        [StringLength(100,ErrorMessage = "PasswordLengthValidation", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        // تأكيد كلمة المرور الجديدة
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passworddontmatch")]
        public string? ConfirmPassword { get; set; }

        public string? SelectedRole { get; set; }
    }
}
