using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}