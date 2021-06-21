using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace WebApplication2.Models
{
    public class ApplicationResClass: DbContext
    {
        public ApplicationResClass(DbContextOptions<ApplicationResClass> options):base(options)
        {

        }

        public DbSet<ResdbClass> res{ get; set; }
    }
}
