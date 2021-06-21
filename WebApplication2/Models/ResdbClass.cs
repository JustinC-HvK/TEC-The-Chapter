using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Models
{
    [Keyless]
    public class ResdbClass
    {
        public string udate { get; set; }
        public string utime { get; set; }
        public string partysize { get; set; }
        public string occasion { get; set; }
        public string username { get; set; }
    }
}
