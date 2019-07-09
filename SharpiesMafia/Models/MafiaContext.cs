using System;
using Microsoft.EntityFrameworkCore;

namespace SharpiesMafia.Models
{
    public class MafiaContext : DbContext
    {
        public MafiaContext(DbContextOptions<MafiaContext> options) : base (options) { }
            public DbSet<User> Users { get; set; }
    }
}
