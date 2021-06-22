using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Models
{
    [Keyless]
    public class UseClass
    {
        
        public string firstname { get; set; }

        public string lastname { get; set; }

        public string username { get; set; }

        public string password { get; set; }

        public string email { get; set; }

        public string number { get; set; }

        public string dob { get; set; }
    }
}
