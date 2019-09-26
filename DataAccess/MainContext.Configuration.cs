using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public partial class MainContext
    {
        public class CategoryConfiguration : EntityTypeConfiguration<Category>
        {
            public CategoryConfiguration()
            {
                this.Property(p => p.Name).HasMaxLength(50).IsRequired();
            }
        }

        public class AuthorConfiguration : EntityTypeConfiguration<Author>
        {
            public AuthorConfiguration()
            {
                this.Property(p => p.Name).HasMaxLength(200).IsRequired();
            }
        }

        public class StoryConfiguration : EntityTypeConfiguration<Story>
        {
            public StoryConfiguration()
            {
                this.Property(p => p.Name).HasMaxLength(500).IsRequired();
                this.Property(p => p.ProgressStatus).HasMaxLength(100).IsRequired();
            }
        }

        public class ChapterConfiguration : EntityTypeConfiguration<Chapter>
        {
            public ChapterConfiguration()
            {
                this.Property(p => p.Title).HasMaxLength(500).IsRequired();
            }
        }
    }
}
