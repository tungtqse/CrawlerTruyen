using Core;
using CrawlerTruyenCV.Models;
using DataAccess;
using DataAccess.Models;
using EntityFramework.DbContextScope.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerTruyenCV.Business
{
    public class StoryProcess
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IDbContextScopeFactory _scopeFactory;

        public StoryProcess(IDbContextScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void Excute()
        {
            Console.WriteLine("Getting stories from db...");
            logger.Info("Getting stories from db...");

            using (var scope = _scopeFactory.Create())
            {
                var context = scope.DbContexts.Get<MainContext>();

                var stories = context.Set<Story>().Where(f => f.StatusId && (f.ProgressStatus == Constants.StoryProgressStatus.Processing
                                                        || f.ProgressStatus == Constants.StoryProgressStatus.Pending)
                                                        && !string.IsNullOrEmpty(f.Link))
                                                  .Select(s => new StoryModel()
                                                  {
                                                      Id = s.Id,
                                                      Name = s.Name,
                                                      Link = s.Link,
                                                      TotalChapter = s.TotalChapter
                                                  }).ToList();

                if (stories != null && stories.Count > 0)
                {
                    logger.Info($"Get total: {stories.Count} stories");

                    ChapterProcess chapter = new ChapterProcess(_scopeFactory);

                    foreach (var story in stories)
                    {
                        chapter.Excute(story);
                    }
                }
                else
                {
                    logger.Info("Not found valid story.");
                }

                scope.SaveChanges();
            }
        }
    }
}
