using PROGPOE1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace PROGPOE1.Controllers
{
    public class HomeController : Controller
    {
        private static List<Claim> _claims = new List<Claim>
        {
            new Claim { Id = 1, User = "Lecturer1", Month = "2025-01", Hours = 20, Amount = 1000, Status = "Submitted", Documents = new List<string>() },
            new Claim { Id = 2, User = "Lecturer1", Month = "2025-02", Hours = 15, Amount = 750, Status = "Approved by Coordinator", Documents = new List<string> { "timesheet.pdf" } }
        };
        private const double HourlyRate = 50; // Mock rate
        private static int _nextId = 3;

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserRole") == null)
            {
                return View("Login");
            }
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public IActionResult Login(string role, string name)
        {
            if (!string.IsNullOrEmpty(role) && !string.IsNullOrEmpty(name))
            {
                HttpContext.Session.SetString("UserRole", role);
                HttpContext.Session.SetString("UserName", name);
                return RedirectToAction("Dashboard");
            }
            return View("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == null) return RedirectToAction("Index");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserRole = role;
            ViewBag.CurrentDate = "September 16, 2025";

            var userClaims = role == "lecturer"
                ? _claims.Where(c => c.User == ViewBag.UserName).ToList()
                : _claims;

            return View(userClaims);
        }

        [HttpGet]
        public IActionResult SubmitClaim()
        {
            if (HttpContext.Session.GetString("UserRole") != "lecturer") return RedirectToAction("Dashboard");
            return View();
        }

        [HttpPost]
        public IActionResult SubmitClaim(string month, double hours, IFormFileCollection documents)
        {
            if (hours <= 0 || string.IsNullOrEmpty(month))
            {
                ModelState.AddModelError("", "Invalid input.");
                return View();
            }

            var claim = new Claim
            {
                Id = _nextId++,
                User = HttpContext.Session.GetString("UserName") ?? "Unknown",
                Month = month,
                Hours = hours,
                Amount = hours * HourlyRate,
                Documents = documents.Select(f => f.FileName).ToList()
            };
            _claims.Add(claim);
            TempData["Message"] = "Claim Submitted!";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public IActionResult ViewClaim(int id)
        {
            var claim = _claims.FirstOrDefault(c => c.Id == id);
            if (claim == null) return NotFound();
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
            return View(claim);
        }

        [HttpPost]
        public IActionResult UpdateStatus(int id, string status, string notes)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "lecturer") return Unauthorized();

            var claim = _claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = role == "coordinator" && status == "Approved" ? "Approved by Coordinator" : status;
                claim.Notes = notes;
                TempData["Message"] = $"{status}!";
            }
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public IActionResult AddDocuments(int id, IFormFileCollection documents)
        {
            var claim = _claims.FirstOrDefault(c => c.Id == id);
            if (claim != null && HttpContext.Session.GetString("UserRole") == "lecturer" && claim.Status == "Submitted")
            {
                claim.Documents.AddRange(documents.Select(f => f.FileName));
                TempData["Message"] = "Documents Added!";
            }
            return RedirectToAction("ViewClaim", new { id });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}