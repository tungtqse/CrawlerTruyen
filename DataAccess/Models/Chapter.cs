using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class Chapter : BaseEntity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public Guid StoryId { get; set; }
        public int? NumberChapter { get; set; }
        public string Link { get; set; }
    }
}
