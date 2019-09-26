using Core;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{  

    public partial class MainContext : AuditDbContext
    {
        public MainContext(string connectionStringName) : base(connectionStringName) { }

        public MainContext() : base(Constants.ConnectionString) { }

        public virtual DbSet<AuditTrail> AuditTrails { get; set; }
        public virtual DbSet<AttachmentFile> AttachmentFiles { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Story> Stories { get; set; }
        public virtual DbSet<Author> Authors { get; set; }
        public virtual DbSet<Chapter> Chapters { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();             

            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new CategoryConfiguration());
            modelBuilder.Configurations.Add(new AuthorConfiguration());
            modelBuilder.Configurations.Add(new StoryConfiguration());
            modelBuilder.Configurations.Add(new ChapterConfiguration());
        }
    }
}
