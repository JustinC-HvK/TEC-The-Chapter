using System;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApplication2.Models;
using System.Data.SqlClient;


namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationUserClass _auc;

        public HomeController(ILogger<HomeController> logger, ApplicationUserClass auc )
        {
            _logger = logger;
            _auc = auc;
        }

        public IActionResult Aboutus()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secured()
        {
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("login")]
        public IActionResult Login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Validate(string username, string password, string returnUrl)
        {
            //setting the variables/strings/connections
            SqlConnection conn = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            string sfu = "usr_login";
            SqlCommand com = new SqlCommand(sfu, conn);
            com.CommandType = System.Data.CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@username", username.ToString());
            com.Parameters.AddWithValue("@password", password.ToString());

            //open connection
            conn.Open();

            int loginResult = Convert.ToInt32(com.ExecuteScalar());
            
            //close connection
            conn.Close();

            //if loginresult=one thats in the db
            if (loginResult == 1)
            {
                var claims = new List<Claim>();
                claims.Add(new Claim("username", username));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, username));
                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
                return Redirect(returnUrl);
            }

            //if it doesnt
            else
            {
                TempData["Error"] = "Error. Username or Password is invalid";
                return View("login");
            }

        }


        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }
        public IActionResult Menu()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(UserClass uc)
        {
            if (ModelState.IsValid)
            {
                if (uc.dob.Year > 1900 && uc.dob.Year < 2003)
                {
                    //uc.password =
                    _auc.Add(uc);
                    _auc.SaveChanges();
                    ViewBag.message = "Registration of user" + uc.username + " Is complete";
                    return RedirectToAction("Index");
                }

            }
            return View();
        }

        public IActionResult Reservations()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult ReservationBooking(DateTime aDate, DateTime aTime, string party_size, string occasion)
        {
            
            //Runs the SQL Procedure Command
            SqlConnection conn = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            SqlCommand cmd = new SqlCommand("reservationpro", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            
            cmd.Parameters.AddWithValue("@p_date", aDate);
            cmd.Parameters.AddWithValue("@p_time", aTime);
            cmd.Parameters.AddWithValue("@p_partysize", party_size);
            cmd.Parameters.AddWithValue("@p_occasion", occasion);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            return Redirect("Secured");
        }
    }
}
