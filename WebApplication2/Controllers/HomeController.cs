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
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Logging;
using WebApplication2.Models;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationUserClass _auc;
        private readonly ApplicationResClass _db;


        public HomeController(ILogger<HomeController> logger, ApplicationUserClass auc, ApplicationResClass db )
        {
            _logger = logger;
            _auc = auc;
            _db = db;
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

                string link = "https://localhost:44344/home/resetpassword?guidcode=" + guidid;
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

        //https://localhost:44344/home/resetpassword?guidcode=95c575ab-e4c5-4308-b650-e46e5f92a229
        public IActionResult resetpassword(string change_password)
        {
            string str_paramenter = HttpContext.Request.QueryString.Value;
            string str_guid = str_paramenter.Substring(10);
            UserClass.static_guid = str_guid;
            TempData["Guidid"] = str_guid;
            return View();
        }

        public IActionResult restpasswordfunction(string change_password)
        {
            string savedguid = UserClass.static_guid;
            //Runs the SQL Procedure Command
            SqlConnection conn = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            SqlCommand cmd = new SqlCommand("change_pass", conn);
            //finds stored procedure to add to database
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            //linking database variables with form variables
            cmd.Parameters.AddWithValue("@password", change_password);
            cmd.Parameters.AddWithValue("@db_guid", savedguid);
            conn.Open();
            var loginResult = cmd.ExecuteScalar();
            conn.Close(); ;
            return View("Index");
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
        public async Task<IActionResult> Validate(string username, string password, string returnUrl, AdminClass ac)
        {
            //Check if returnUrl is empty. If yes, set to Homepage so users don't get a null value error
            if (returnUrl == null)
            {
                returnUrl = "Home";
            }
            
            //Set up connection to Admin table to check Password to Hash
            SqlConnection conn = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            string sfu = "passwordget";
            SqlCommand com = new SqlCommand(sfu, conn);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@username", username.ToString());
            com.Parameters.AddWithValue("@password", password.ToString());
            com.Parameters.Add("@hashedpass", SqlDbType.VarChar, 64).Direction = ParameterDirection.Output;
            
            //Connect to the database to execute stored procedure
            conn.Open();
            com.ExecuteNonQuery();
            
            //Save the result of the query to a variable and convert the value to a string
            var hashedpass = com.Parameters["@hashedpass"].Value.ToString();
            
            //close connection
            conn.Close();
            
            // Run Entity Framework on the Database to check user exists
            var AdloginResult = _auc.admintb.Count(u => u.username.Equals(ac.username));
            
            //Check if the stored procedure and Entity framework both found the user in Admintb
            if (AdloginResult != 0 && hashedpass.Length > 0)
            {
                var claims = new List<Claim>();
                claims.Add(new Claim("username", username));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, username));
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
                return Redirect(returnUrl);
            }
            //Didn't find user in Admintb? Run checks on the normal usertable

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
            
            if (loginResult == 1 && AdloginResult == 0)
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
        

        [Authorize(Roles = "Admin")]
        
        public IActionResult Admin()
        {
            //Initialize the list
            var model = new List<ResdbClass>();
            //Setup connection to the database
            SqlConnection conn = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            SqlCommand cmd = new SqlCommand("SELECT * FROM res");
            cmd.Connection = conn;
            //Open the connection
            conn.Open();
            //Read the data in the database
            SqlDataReader rdr = cmd.ExecuteReader();
            //Add data to the page table
            while (rdr.Read())
            {
                var Restb = new ResdbClass();
                Restb.udate = rdr["udate"].ToString();
                Restb.utime = rdr["utime"].ToString();
                Restb.partysize = rdr["partysize"].ToString();
                Restb.occasion = rdr["occasion"].ToString();
                Restb.username = rdr["username"].ToString();
                model.Add(Restb);
            }
            //Close the connection
            conn.Close();

            
            //Show the view
            return View(model);
        }

        [HttpPost]
        public IActionResult Admin(string partysize, string occasion, string username, string updatepartysize)
        {
            //Setup connection to the database
            SqlConnection conn = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            string spr = "res_update";
            //If updatepartysize field is left empty or is not a number, set spr to the delete procedure
            if (updatepartysize == null || int.TryParse(updatepartysize, out int testsize) == false)
            {
                spr = "res_delete";
            }
            SqlCommand cmd = new SqlCommand(spr, conn);
            //Check the partysize and updatepartysize fields are numbers. If all conditions are met, continue with connection
            if (int.TryParse(partysize, out int size) != false || int.TryParse(updatepartysize, out int upsize) != false && spr == "res_update")
            {


                
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@partysize", int.Parse(partysize));
                cmd.Parameters.AddWithValue("@occasion", occasion.ToString());
                cmd.Parameters.AddWithValue("@username", username.ToString());
                //If spr is res_update, add the updatepartysize parameter
                if (spr == "res_update")
                {
                    cmd.Parameters.AddWithValue("@updatepartysize", int.Parse(updatepartysize));
                }
                //Open the connection
                conn.Open();
                //Execute stored procedure
                cmd.ExecuteNonQuery();
                //Close the connection
                conn.Close();
            }
            //Refresh the list and reload the page
            var model = new List<ResdbClass>();
            conn = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            cmd = new SqlCommand("SELECT * FROM res");
            cmd.Connection = conn;
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var Restb = new ResdbClass();
                Restb.udate = rdr["udate"].ToString();
                Restb.utime = rdr["utime"].ToString();
                Restb.partysize = rdr["partysize"].ToString();
                Restb.occasion = rdr["occasion"].ToString();
                Restb.username = rdr["username"].ToString();
                model.Add(Restb);
            }
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminUSR()
        {
            //Initialize the list
            var usrmodel = new List<UseClass>();
            //Setup connection to the database
            SqlConnection conn = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            SqlCommand cmd = new SqlCommand("SELECT * FROM hashtb");
            cmd.Connection = conn;
            //Open the connection
            conn.Open();
            //Read the data in the database
            SqlDataReader rdr = cmd.ExecuteReader();
            //Add the data to the page table
            while (rdr.Read())
            {
                var Usetb = new UseClass();
                Usetb.firstname = rdr["firstname"].ToString();
                Usetb.lastname = rdr["lastname"].ToString();
                Usetb.username = rdr["username"].ToString();
                Usetb.password = rdr["password"].ToString();
                Usetb.email = rdr["email"].ToString();
                Usetb.number = rdr["number"].ToString();
                Usetb.dob = rdr["dob"].ToString();
                usrmodel.Add(Usetb);
            }

            
            //Show the view
            return View(usrmodel);
        }

        [HttpPost]
        public IActionResult AdminUsr(string username, string email, string newname)
        {
            //Setup connection to the database
            SqlConnection conn = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            string spr = "usr_delete";
            //Set spr to usr_update if newname is not null
            if (newname != null)
            {
                spr = "usr_update";
            }    
            SqlCommand cmd = new SqlCommand(spr, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@username", username.ToString());
            cmd.Parameters.AddWithValue("@email", email.ToString());
            //Add the newname parameter if the spr is usr_update
            if (spr == "usr_update")
            {
                cmd.Parameters.AddWithValue("@newname", newname.ToString());
            }
            //Open the connection
            conn.Open();
            //Execute stored procedure
            cmd.ExecuteNonQuery();
            //Close the connection
            conn.Close();
            //Refresh the list and reload the page
            var usrmodel = new List<UseClass>();
            conn = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            cmd = new SqlCommand("SELECT * FROM hashtb");
            {
                cmd.Connection = conn;
                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var Usetb = new UseClass();
                    Usetb.firstname = rdr["firstname"].ToString();
                    Usetb.lastname = rdr["lastname"].ToString();
                    Usetb.username = rdr["username"].ToString();
                    Usetb.password = rdr["password"].ToString();
                    Usetb.email = rdr["email"].ToString();
                    Usetb.number = rdr["number"].ToString();
                    Usetb.dob = rdr["dob"].ToString();
                    usrmodel.Add(Usetb);
                }
                conn.Close();
                return View(usrmodel);
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
        public  IActionResult Register(UserClass uc)
        {
            //Check if the fields are Valid
            if (ModelState.IsValid)
            {
                //Check the user's birthday to ensure they are within a valid range
                if (uc.dob.Year > 1900 && uc.dob.Year < 2003)
                {
                    //Check if Username exists
                    SqlConnection conm = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
                    string sfum = "usr_usercheck";
                    SqlCommand comn = new SqlCommand(sfum, conm);
                    comn.CommandType = System.Data.CommandType.StoredProcedure;
                    comn.Parameters.AddWithValue("@username", uc.username.ToString());

                    //open connection
                    conm.Open();

                    int userResult = Convert.ToInt32(comn.ExecuteScalar());

                    //close connection
                    conm.Close();

                    //Check if Email exists
                    SqlConnection cnm = new SqlConnection(@"Data Source=chapter.database.windows.net;Initial Catalog=chapterdb;User ID=chapter;Password=Usepassword1;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
                    string sfm = "usr_emailcheck";
                    SqlCommand cmn = new SqlCommand(sfm, cnm);
                    cmn.CommandType = System.Data.CommandType.StoredProcedure;
                    cmn.Parameters.AddWithValue("@email", uc.email.ToString());

                    //open connection
                    cnm.Open();

                    int emailResult = Convert.ToInt32(cmn.ExecuteScalar());

                    //close connection
                    cnm.Close();
                    
                    //Check results and display relevent error if conditions are met
                    if (userResult != 0 || emailResult != 0)
                    {
                        TempData["Error"] = "Error. Username and Email are Taken. Please choose a different Username and Email";
                    }
                    else if (userResult != 0)
                    {
                        TempData["Error"] = "Error. Username Taken. Please choose another Username.";
                        return View();
                    }
                    else if (emailResult != 0)
                    {
                        TempData["Error"] = "Error. Email Taken. Please choose another Email.";
                        return View();
                    }
                    else
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
                else
                {
                    //Return user to register page if DOB is out of range
                    TempData["Error"] = "Error. Date of birth must be between 1901 and 2002";
                    return View();
                }
                

            }
            //Return user to register page if validation fails
            TempData["Error"] = "Error. Validation failed";
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
        public IActionResult ReservationBooking(string EmailID, DateTime aDate, DateTime aTime, string party_size, string occasion, string resname)
        {
            string formatted_Date = aDate.ToString("dd/MM/yyyy");
            string formatted_Time = aTime.ToString("hh:mm tt");
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
            cmd.Parameters.AddWithValue("@p_username", EmailID);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            //send email
            string to = EmailID; //To address    
            string from = "thechapterrestaurant@gmail.com"; //From address    
            MailMessage message = new MailMessage(from, to);

            string mailbody = "Thank you for creating a booking at The Chapter. \n Your Booking is confirmed for - " + formatted_Date + " at " + formatted_Time + "\n";
            message.Subject = "Booking Confirmation";
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

            //resets the form
            return Redirect("Secured");
        }
    }
}
