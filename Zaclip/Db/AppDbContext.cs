using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Zaclip.Models;

namespace Zaclip.Db
{
    public class AppDbContext : DbContext
    {
        public DbSet<ClipboardItem> ClipItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=app.db");
    }
}
