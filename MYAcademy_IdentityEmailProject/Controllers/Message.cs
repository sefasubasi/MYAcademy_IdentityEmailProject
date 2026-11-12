using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Context;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    [Authorize]
    public class Message : Controller
    {
        private readonly EmailContext _context;

        public Message(EmailContext context)
        {
            _context = context;
        }

        public IActionResult Inbox()
        {
            var values = _context.Messages.Where(x => x.ReceiverEmail == "aaa").ToList();
            return View(values);
        }
    }
}
