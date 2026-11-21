
using System.ComponentModel.DataAnnotations;

namespace MYAcademy_IdentityEmailProject.Models
{
    public class UserRegisterViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-30 karakter olmalıdır.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Şifreler birbiriyle eşleşmiyor.")]
        public string PasswordConfirm { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "KVKK ve kullanıcı sözleşmesini kabul etmelisiniz.")]
        public bool AcceptTerms { get; set; }
    }
}
