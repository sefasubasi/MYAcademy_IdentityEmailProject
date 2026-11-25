using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MessageController(EmailContext context,UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Inbox()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("SignIn", "Login");
            }

            var values = _context.Messages
                .Where(x => x.ReceiverEmail == user.Email)
                .OrderByDescending(x => x.SendDate)
                .ToList();

            return View(values);
        }
        [HttpGet]
        public IActionResult CreateMessage()
        {
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "CategoryName");
            return View();
        }
        [HttpPost]
        public IActionResult CreateMessage(Message message)
        {
            return RedirectToAction("Sendbox");
        }
        [Authorize]
        public async Task<IActionResult> SendBox()
        {
            var user = await _userManager.GetUserAsync(User);
            var values = _context.Messages.Where(x => x.SenderEmail == user.Email).OrderByDescending(x => x.SendDate).ToList();

            return View(values);
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("SignIn", "Login");

            var message = _context.Messages.FirstOrDefault(x =>
                x.MessageId == id &&
                (x.ReceiverEmail == user.Email || x.SenderEmail == user.Email));

            if (message == null)
                return NotFound();

            if (message.ReceiverEmail == user.Email && !message.IsRead)
            {
                message.IsRead = true;
                _context.SaveChanges();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Detail", message);
            }

            return View(message);
        }
    }
}
