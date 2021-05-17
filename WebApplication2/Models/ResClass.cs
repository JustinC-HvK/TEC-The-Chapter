using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Models
{
    public class ResClass
    {
        [Required(ErrorMessage ="Please Select a valid Date")]
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
    }
    public class Timepick
    {
        [Required]
        public DateTime Time { get; set; }
    }
}
