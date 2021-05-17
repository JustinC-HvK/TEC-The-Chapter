using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Models
{
    public class ResClass
    {
        [Required]
        public DateTime Date { get; set; }
        
    }
}
