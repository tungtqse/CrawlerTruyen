using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class Constants
    {
        public const string Url = "https://wikidich.com";
        public const string ConnectionString = "MainContext";

        public class BaseProperty
        {
            public const string Id = "Id";
            public const string CreatedDate = "CreatedDate";
            public const string CreatedBy = "CreatedBy";
            public const string ModifiedDate = "ModifiedDate";
            public const string ModifiedBy = "ModifiedBy";
            public const string StatusId = "StatusId";
        }

        public class AuditTrailProperty : BaseProperty
        {
            public const string ItemId = "ItemId";
            public const string TableName = "TableName";
            public const string TrackChange = "TrackChange";
            public const string TransactionId = "TransactionId";
        }

        public class StoryProgressStatus
        {
            public const string Completed = "Hoàn thành";
            public const string Pending = "Tạm dừng";
            public const string Dropped = "Bỏ dở";
            public const string Processing = "Đang ra";
        }
    }
}
