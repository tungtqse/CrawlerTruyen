using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerTruyenCV.Models
{
    public class StoryModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ProgressStatus { get; set; }
        public int TotalChapter { get; set; }
        public string Link { get; set; }
        public bool StatusId { get; set; }
    }
}
