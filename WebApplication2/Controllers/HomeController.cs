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
using System.IO;
using System.Net;
using System.Net.Mail;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationUserClass _auc;
        public int adbit = 0;

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
        [HttpGet("passwordpage")]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost("passwordpage")]
        public IActionResult ForgotPass(string emailID)
        {
            //Runs the SQL Procedure Command
            SqlConnection conn = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            SqlCommand cmd = new SqlCommand("getEmailID", conn);
            //finds stored procedure to add to database
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            //linking database variables with form variables
            cmd.Parameters.AddWithValue("@email", emailID);
            conn.Open();
            var loginResult = cmd.ExecuteScalar();
            conn.Close();;
            
            //resets the form
            if (loginResult != null)
            {
                //unique userid store
                Console.WriteLine(loginResult.ToString());
                string guidid = loginResult.ToString();
                TempData["Guidid"] = guidid;

                //reset password token, creates random number to store into database
                string resetPScode = Guid.NewGuid().ToString();
                //finds stored procedure to add to database
                SqlCommand cmd2 = new SqlCommand("addresetcode", conn);
                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //linking database variables with form variables
                cmd2.Parameters.AddWithValue("@Resetcode", resetPScode);
                cmd2.Parameters.AddWithValue("@email", emailID);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                
                //send email
                string to = emailID; //To address    
                string from = "thechapterrestaurant@gmail.com"; //From address    
                MailMessage message = new MailMessage(from, to);

                string link = "https://localhost:44344/home/resetpassword?guid=" + guidid;
                string mailbody = "To reset your password please click the link below.\n" + link;
                message.Subject = "Reset Password";
                message.Body = mailbody;
                message.BodyEncoding = Encoding.UTF8;
                message.IsBodyHtml = true;

                SmtpClient client = new SmtpClient("smtp.gmail.com", 587); //Gmail smtp    
                System.Net.NetworkCredential basicCredential1 = new
                System.Net.NetworkCredential("thechapterrestaurant@gmail.com", "thechapter123");
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = basicCredential1;
                try
                {
                    client.Send(message);
                }

                catch (Exception ex)
                {
                    throw ex;
                }

                return View("ForgotPassword");
            }
            else
            {
                TempData["Error"] = "Error. Email incorrect";
                return View("ForgotPassword");
            }
        }

        public IActionResult resetpassword()
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
            //Set up connection to Admin table
            SqlConnection conn = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            string sfu = "usr_adminlogin";
            SqlCommand com = new SqlCommand(sfu, conn);
            com.CommandType = System.Data.CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@username", username.ToString());
            com.Parameters.AddWithValue("@password", password.ToString());

            //open connection
            conn.Open();

            int AdloginResult = Convert.ToInt32(com.ExecuteScalar());

            //close connection
            conn.Close();
            
            //setting the variables/strings/connections for full user table
            SqlConnection conm = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            string sfum = "usr_hashlogin";
            SqlCommand comn = new SqlCommand(sfum, conm);
            comn.CommandType = System.Data.CommandType.StoredProcedure;
            comn.Parameters.AddWithValue("@username", username.ToString());
            comn.Parameters.AddWithValue("@password", password.ToString());

            //open connection
            conm.Open();

            int loginResult = Convert.ToInt32(comn.ExecuteScalar());
            
            //close connection
            conm.Close();
            
            //if Adloginresult=one thats in the db
            if (AdloginResult == 1)
            {
                var claims = new List<Claim>();
                claims.Add(new Claim("username", username));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, username));
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
                adbit = 1;
                return Redirect(returnUrl);
            }
            else if (loginResult == 1 && AdloginResult == 0)
            {
                var claims = new List<Claim>();
                claims.Add(new Claim("username", username));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, username));
                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
                adbit = 0;
                return Redirect(returnUrl);
            }

            //if it doesnt
            else
            {
                TempData["Error"] = "Error. Username or Password is invalid";
                return View("login");
            }

        }

        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return View();
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


        


        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public  IActionResult Register(UserClass uc)
        {
            //Check if the fields are Valid
            if (ModelState.IsValid)
            {
                //Check the user's birthday to ensure they are within a valid range
                if (uc.dob.Year > 1900 && uc.dob.Year < 2003)
                {
                    //Add the user values and save them to the database if validation passes
                    SqlConnection connect = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
                    string spo = "user_reg";
                    SqlCommand comm = new SqlCommand(spo, connect);
                    comm.CommandType = System.Data.CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@firstname", uc.firstname.ToString());
                    comm.Parameters.AddWithValue("@lastname", uc.lastname.ToString());
                    comm.Parameters.AddWithValue("@username", uc.username.ToString());
                    comm.Parameters.AddWithValue("@password", uc.password.ToString());
                    comm.Parameters.AddWithValue("@email", uc.email.ToString());
                    comm.Parameters.AddWithValue("@number", uc.number);
                    comm.Parameters.AddWithValue("@dob", uc.dob);
                    connect.Open();

                    comm.ExecuteReader();

                    connect.Close();

                    
                    //Return the user to the Index
                    return RedirectToAction("Index");
                }

            }
            //Return user to register page if validation fails
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
        //adding reservations into database
        public IActionResult ReservationBooking(DateTime aDate, DateTime aTime, string party_size, string occasion, string resname)
        {
            
            //Runs the SQL Procedure Command , added user
            SqlConnection conn = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            SqlCommand cmd = new SqlCommand("reservationpro", conn);
            //finds stored procedure to add to database
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            //linking database variables with form variables
            cmd.Parameters.AddWithValue("@p_date", aDate);
            cmd.Parameters.AddWithValue("@p_time", aTime);
            cmd.Parameters.AddWithValue("@p_partysize", party_size);
            cmd.Parameters.AddWithValue("@p_occasion", occasion);
            cmd.Parameters.AddWithValue("@p_username", resname);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            //resets the form
            return Redirect("Secured");
        }
    }
}
