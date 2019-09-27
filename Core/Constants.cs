using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class Constants
    {
        public const string UrlWiki = "https://wikidich.com";
        public const string UrlTruyenCV = "http://truyencv.com";
        public const string UrlTruyenYY = "https://truyenyy.com";
        public const string UrlTTV = "https://truyen.tangthuvien.vn";

        public const string ConnectionString = "MainContext";
        public const int MaxChapter = 100;

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
            public const string Completed = "Completed";
            public const string Pending = "Pending";
            public const string Dropped = "Dropped";
            public const string Processing = "Processing";
        }

        public class Source
        {
            public const string WikiDich = "WikiDich";
            public const string TruyenCV = "TruyenCV";
            public const string TruyenYY = "TruyenYY";
            public const string TangThuVien = "TangThuVien";
        }
    }
}
