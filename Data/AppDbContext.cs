using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyPdfService.Models;

namespace MyPdfService.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<FileModel> Files { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}