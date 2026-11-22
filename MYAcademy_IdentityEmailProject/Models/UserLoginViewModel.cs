
using System.ComponentModel.DataAnnotations;

namespace MYAcademy_IdentityEmailProject.Models
{
    public class UserLoginViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }

        // Kullanıcı bilgileri
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }

        // E-posta doğrulama kodu
        [Required(ErrorMessage = "Doğrulama kodunu giriniz.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Kod 6 haneli olmalıdır.")]
        [RegularExpression(@"^\d{6}$",ErrorMessage = "Kod yalnızca 6 rakamdan oluşmalıdır.")]
        public string Code { get; set; }
    }
}
