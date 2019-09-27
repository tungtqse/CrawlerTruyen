using Core;
using CrawlerTruyenCV.Models;
using DataAccess;
using DataAccess.Models;
using EntityFramework.DbContextScope.Interfaces;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerTruyenCV.Business
{
    public class ChapterProcess
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IDbContextScopeFactory _scopeFactory;      

        public ChapterProcess(IDbContextScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task Excute(StoryModel story)
        {
            var list = new List<ChapterModel>();

            switch (story.Source)
            {
                case Constants.Source.WikiDich:
                    {
                        CrawlerFromWiki crWiki = new CrawlerFromWiki(_scopeFactory);

                        list = crWiki.ExcuteCrawler(story);

                        break;
                    }
                case Constants.Source.TruyenYY:                  
                case Constants.Source.TangThuVien:
                case Constants.Source.TruyenCV:
                    {
                        CrawlerFromTruyenCV crCV = new CrawlerFromTruyenCV(_scopeFactory);

                        list = crCV.ExcuteCrawler(story);

                        break;
                    }
                default: break;
            }            

            if(list != null && list.Count > 0)
            {
                var currentChapter = (story.TotalChapter != 0) ? story.TotalChapter : 0;

                UpdateToDB(story.Id, list, currentChapter);
            }

            logger.Info("Finish get chapter.");
        }

        private void UpdateToDB(Guid storyId, List<ChapterModel> list, int chapterNumber = 0)
        {
            try
            {
                logger.Info("Updating to db...");
                logger.Info($"Get total: {list.Count} chapters");

                using (var scope = _scopeFactory.Create())
                {
                    var context = scope.DbContexts.Get<MainContext>();

                    var story = context.Set<Story>().Where(f => f.Id == storyId).FirstOrDefault();

                    var chapters = new List<Chapter>();

                    foreach (var item in list)
                    {
                        chapterNumber += 1;

                        chapters.Add(new Chapter()
                        {
                            Id = Guid.NewGuid(),
                            StatusId = true,
                            Content = item.Content,
                            Link = item.Link,
                            StoryId = story.Id,
                            Title = item.Title,
                            NumberChapter = chapterNumber
                        });
                    }

                    context.Set<Chapter>().AddRange(chapters);

                    story.TotalChapter = chapterNumber;

                    scope.SaveChanges();

                    logger.Info("Finish update to db.");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}
