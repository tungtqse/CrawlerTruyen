using EntityFramework.DbContextScope.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerTruyenCV.Business
{
    public class MainProcess
    {
        private readonly IDbContextScopeFactory _scopeFactory;

        public MainProcess(IDbContextScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
    }
}
