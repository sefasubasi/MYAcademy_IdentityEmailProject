using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;
using System.Security.Cryptography;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public LoginController(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;

        }
        private void SetCodeExpiration(AppUser user)
        {
            ViewBag.CodeExpiration = user.ValidCodeExpiration.HasValue
                ? DateTime.SpecifyKind(
                    user.ValidCodeExpiration.Value,
                    DateTimeKind.Utc
                  ).ToString("o")
                : null;
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(UserLoginViewModel userLoginViewModel)
        {
            var user = await _userManager.FindByNameAsync(userLoginViewModel.Username);

            if(user == null)
            {
                ModelState.AddModelError("", "Kullanici adi veya sifre hatali!");
                return View(userLoginViewModel);
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, userLoginViewModel.Password);

            if (!passwordValid)
            {
                ModelState.AddModelError("", "Kullanici adi veya sifre hatali!");
                return View(userLoginViewModel);
            }

            if (user.EmailConfirmed)
            {
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Inbox", "Message");
            }

            HttpContext.Session.SetString("PendingUserId", user.Id);

            // Yeni doğrulama kodu üret.
            user.ValidCode = RandomNumberGenerator.GetInt32(100000, 1000000);
            user.ValidCodeExpiration = DateTime.UtcNow.AddMinutes(1);
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                ModelState.AddModelError("", "Doğrulama kodu oluşturulamadı.");
                return View(userLoginViewModel);
            }

            await SendVerificationEmail(user);

            return RedirectToAction("VerifyEmail");


        }
        [HttpGet]
        public async Task<IActionResult> VerifyEmail()
        {

            var userId = HttpContext.Session.GetString("PendingUserId");


            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Oturumunuzun süresi doldu. Tekrar giriş yapın.";
                return RedirectToAction(nameof(SignIn));
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || user.EmailConfirmed)
                return RedirectToAction(nameof(SignIn));

            SetCodeExpiration(user);

            var model = new UserLoginViewModel
            {
                Name = user.Name,
                Surname = user.Surname,
                Username = user.UserName,
                Email = user.Email
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(UserLoginViewModel model)
        {
            var userId = HttpContext.Session.GetString("PendingUserId");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Oturumunuzun süresi doldu. Tekrar giriş yapın.";
                return RedirectToAction(nameof(SignIn));
            }

            var user = await _userManager.FindByIdAsync(userId);
            SetCodeExpiration(user);

            if (user == null || user.EmailConfirmed)
                return RedirectToAction(nameof(SignIn));

            // Kod formatını kontrol et.
            if (string.IsNullOrWhiteSpace(model.Code) ||
                model.Code.Length != 6 ||
                !model.Code.All(char.IsDigit))
            {
                ModelState.AddModelError("Code", "6 haneli geçerli bir kod giriniz.");

                model.Name = user.Name;
                model.Surname = user.Surname;
                model.Username = user.UserName;
                model.Email = user.Email;

                return View(model);
            }

            // Kodun doğruluğunu ve süresini kontrol et.
            if (!int.TryParse(model.Code, out int enteredCode) ||enteredCode != user.ValidCode ||user.ValidCodeExpiration == null ||user.ValidCodeExpiration < DateTime.UtcNow)
            {
                ModelState.AddModelError("Code","Doğrulama kodu hatalı veya süresi dolmuş.");

                model.Name = user.Name;
                model.Surname = user.Surname;
                model.Username = user.UserName;
                model.Email = user.Email;

                return View(model);
            }

            // Doğrulama başarılı.
            user.EmailConfirmed = true;
            user.ValidCode = 0;
            user.ValidCodeExpiration = null;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Doğrulama işlemi tamamlanamadı.");

                model.Name = user.Name;
                model.Surname = user.Surname;
                model.Username = user.UserName;
                model.Email = user.Email;

                return View(model);
            }

            HttpContext.Session.Remove("PendingUserId");

            await _signInManager.SignInAsync(user, false);

            return RedirectToAction("Inbox", "Message");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateNewCode()
        {
            // Session üzerinden bekleyen kullanıcıyı bul.
            var userId = HttpContext.Session.GetString("PendingUserId");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Oturumunuzun süresi doldu. Tekrar giriş yapın.";
                return RedirectToAction(nameof(SignIn));
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || user.EmailConfirmed)
            {
                HttpContext.Session.Remove("PendingUserId");
                return RedirectToAction(nameof(SignIn));
            }

            // Yeni doğrulama kodu oluştur.
            user.ValidCode = RandomNumberGenerator.GetInt32(100000, 1000000);

            // Yeni kodun geçerlilik süresi.
            user.ValidCodeExpiration = DateTime.UtcNow.AddMinutes(1);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["Error"] = "Yeni doğrulama kodu oluşturulamadı.";
                return RedirectToAction(nameof(VerifyEmail));
            }

            // Yeni kodu e-posta ile gönder.
            await SendVerificationEmail(user);

            TempData["Success"] = "Yeni doğrulama kodunuz e-posta adresinize gönderildi.";

            return RedirectToAction(nameof(VerifyEmail));
        }
        [HttpGet]
        private async Task SendVerificationEmail(AppUser user)
        {
            var senderName = _configuration["EmailSettings:SenderName"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var host = _configuration["EmailSettings:Host"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);
            var password = _configuration["EmailSettings:Password"];

            MimeMessage mimeMessage = new MimeMessage();

            MailboxAddress mailboxAddressFrom = new MailboxAddress(senderName, senderEmail);
            mimeMessage.From.Add(mailboxAddressFrom);

            MailboxAddress mailboxAddressTo = new MailboxAddress(user.Name, user.Email);
            mimeMessage.To.Add(mailboxAddressTo);

            mimeMessage.Subject = "Email Doğrulama Kodu";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody =
            $"Merhaba {user.Name},\n\n" +
            $"Giriş işleminizi tamamlamak için doğrulama kodunuz: {user.ValidCode}\n\n" +
            "Bu kodu kimseyle paylaşmayınız.";
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Connect(host, port, false);
            smtpClient.Authenticate(senderEmail, password);
            smtpClient.Send(mimeMessage);
            smtpClient.Disconnect(true);
        }
    }
}
