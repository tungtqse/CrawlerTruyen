using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class Story : BaseEntity
    {
        public string Name { get; set; }
        public Guid AuthorId { get; set; }
        public string ProgressStatus { get; set; }
        public int TotalChapter { get; set; }
        public Guid? AttachmentFileId { get; set; }
        public string Link { get; set; }
        public virtual ICollection<Chapter> Chapters { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
    }
}
