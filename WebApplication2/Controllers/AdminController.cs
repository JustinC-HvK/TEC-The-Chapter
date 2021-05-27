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
using System.Security.Cryptography;
using System.Text;

namespace WebApplication2.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationUserClass _auc;

        public AdminController(ApplicationUserClass auc)
        {
            _auc = auc;
        }

        [HttpGet("adlogin")]
        public IActionResult Adlogin(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("adlogin")]
        public async Task<IActionResult> Validate(string username, string password, string returnUrl)
        {
            //setting the variables/strings/connections
            SqlConnection conn = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            string sfu = "usr_adminlogin";
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
                return View();
            }

        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        [Authorize]
        public IActionResult Admin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserClass uc)
        {
            _auc.Add(uc);
            _auc.SaveChanges();
            ViewBag.message = "Registration of user" + uc.username + " Is complete";
            return View();
        }
    }
}
