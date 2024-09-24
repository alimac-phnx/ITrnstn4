using System.ComponentModel.DataAnnotations;

namespace ITrnstn4.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(20)]
        public string Nickname { get; set; }

        [Required]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "The email must math this form: *@*.*")]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{6,255}$", ErrorMessage = "The password must contain at least one uppercase letter, one lowercase letter and one digit.")]
        public string Password { get; set; }

        public DateTime RegistrationDate { get; set; }

        public DateTime LastLoginDate { get; set; }

        public string Status { get; set; }

        public User()
        {
            Status = "Active";
        }
    }
}
