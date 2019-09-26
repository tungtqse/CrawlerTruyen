using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class Author : BaseEntity
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public virtual ICollection<Story> Stories { get; set; }
    }
}
