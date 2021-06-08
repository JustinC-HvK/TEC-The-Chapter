using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Models
{
    [Keyless]
    public class AdminClass
    {
       

        [Required(ErrorMessage = "Please enter your username")]
        [Display(Name = "Username")]
        public string username { get; set; }

        [Required(ErrorMessage = "Please enter your Password")]
        [Display(Name = "Password")]
        public string password { get; set; }
    }
}
