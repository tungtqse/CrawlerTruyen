using CrawlerTruyenCV.Business;
using EntityFramework.DbContextScope;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerTruyenCV
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        static async Task MainAsync(string[] args)
        {
            logger.Info("Starting program...");

            var dbContextScopeFactory = new DbContextScopeFactory();

            Program p = new Program();

            //GetStory story = new GetStory(dbContextScopeFactory);
            //story.GetStoryContent("");

            StoryProcess story = new StoryProcess(dbContextScopeFactory);
            await story.Excute();

            logger.Info("Finish program.");

            LogManager.Flush();

            //Console.ReadLine();
        }
    }
}
