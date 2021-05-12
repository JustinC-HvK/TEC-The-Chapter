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


namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
        public async Task<IActionResult> Validate(string username, string password , string returnUrl)
        {
            SqlConnection conn = new SqlConnection(@"Server=tcp:chapterdb.database.windows.net,1433;Initial Catalog=CHAPTERDB;Persist Security Info=False;User ID=TECADMIN;Password=Thepasswordispassword1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            conn.Open();
            string sfu = "select username, password from userinfo";
            SqlCommand com = new SqlCommand(sfu , conn);
            SqlDataReader dr = com.ExecuteReader();

            //only reading first row, need to add in a loop
            if (dr.Read())
            {
                if (username.Equals(dr["username"].ToString()) && password.Equals(dr["password"].ToString()))
                {
                    var claims = new List<Claim>();
                    claims.Add(new Claim("username", username));
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, username));
                    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                    var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(claimsPrincipal);
                    return Redirect(returnUrl);
                }

                else
                {
                    TempData["Error"] = "Error. Username or Password is invalid";
                    return View("login");
                }

                conn.Close();
            }

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

        public IActionResult Reservations()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
