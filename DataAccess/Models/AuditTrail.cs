using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class AuditTrail : BaseEntity
    {
        public string TableName { get; set; }
        public Guid ItemId { get; set; }
        public string TrackChange { get; set; }
        public string TransactionId { get; set; }
    }
}
