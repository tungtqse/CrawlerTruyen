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
        private HtmlWeb htmlWeb;

        public ChapterProcess(IDbContextScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        private HtmlWeb GetHtmlContext()
        {
            HtmlWeb htmlWeb = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8  //Set UTF8 để hiển thị tiếng Việt            
            };

            return htmlWeb;
        }

        public void Excute(StoryModel story)
        {
            Console.WriteLine("Getting chapter...");
            logger.Info("Getting chapter...");

            htmlWeb = GetHtmlContext();

            if (story.TotalChapter == 0)
            {
                // Get All Chapters
                GetNewChapter(story);
            }
            else
            {
                // Get All New Chapters
                GetCurrentChapter(story);
            }

            Console.WriteLine("Finish get chapter.");
            logger.Info("Finish get chapter.");
        }

        private void GetNewChapter(StoryModel story)
        {
            var list = new List<ChapterModel>();

            try
            {
                HtmlDocument document = htmlWeb.Load(story.Link);

                var firstChapter = document.DocumentNode.QuerySelectorAll("li.chapter-name > a.truncate").FirstOrDefault();

                if (firstChapter != null)
                {
                    var titleFirst = firstChapter.InnerText;
                    var linkFirst = firstChapter.Attributes["href"].Value;

                    if (!string.IsNullOrEmpty(linkFirst))
                    {
                        linkFirst = Constants.Url + linkFirst;

                        HtmlDocument chapterDocument = htmlWeb.Load(linkFirst);

                        var thread = chapterDocument.DocumentNode.QuerySelectorAll("div#bookContentBody").FirstOrDefault();
                        var content = thread.InnerHtml;

                        var nextChapter = chapterDocument.DocumentNode.QuerySelectorAll("a#btnNextChapter").FirstOrDefault();
                        var linkNextChapter = nextChapter.Attributes["href"].Value;

                        list.Add(new ChapterModel()
                        {
                            Title = titleFirst,
                            Link = linkFirst,
                            Content = content
                        });

                        if (!string.IsNullOrEmpty(linkNextChapter))
                        {
                            var url = Constants.Url + linkNextChapter;
                            ProcessChapters(list, url);
                        }
                    }
                    else
                    {
                        logger.Error("Cannot get first chapter - link error");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                logger.Error(ex);
            }

            if (list != null && list.Count > 0)
            {
                UpdateToDB(story.Id, list, 0);
            }

        }

        private void GetCurrentChapter(StoryModel story)
        {
            var list = new List<ChapterModel>();
            var currentChapterNumber = 0;

            using (var scope = _scopeFactory.CreateReadOnly())
            {
                var context = scope.DbContexts.Get<MainContext>();

                var currentChapter = context.Set<Chapter>().Where(f => f.StoryId == story.Id).OrderByDescending(o => o.NumberChapter).FirstOrDefault();

                if (currentChapter != null)
                {
                    currentChapterNumber = currentChapter.NumberChapter.Value;

                    HtmlDocument document = htmlWeb.Load(currentChapter.Link);

                    var nextChapter = document.DocumentNode.QuerySelectorAll("a#btnNextChapter").FirstOrDefault();

                    if (nextChapter != null)
                    {
                        var linkNextChapter = nextChapter.Attributes["href"].Value;

                        if (!string.IsNullOrEmpty(linkNextChapter))
                        {
                            var url = Constants.Url + linkNextChapter;

                            ProcessChapters(list, url);
                        }
                    }
                    else
                    {
                        Console.WriteLine("This chapter is latest");
                        logger.Info("This chapter is latest");
                    }
                }
            }

            if (list != null && list.Count > 0)
            {
                UpdateToDB(story.Id, list, currentChapterNumber);
            }
        }

        private void ProcessChapters(List<ChapterModel> list, string url)
        {
            var count = 0;

            var link = GetChapterContent(list, url);

            while (!string.IsNullOrEmpty(link) && count < 100)
            {
                link = GetChapterContent(list, link);
                count += 1;
            }
        }

        private string GetChapterContent(List<ChapterModel> list, string link)
        {
            try
            {
                HtmlDocument document = htmlWeb.Load(link);

                var headers = document.DocumentNode.QuerySelectorAll("div#bookContent p").ToArray();
                var chapterTitle = headers[1].InnerText;

                var thread = document.DocumentNode.QuerySelectorAll("div#bookContentBody").FirstOrDefault();
                var content = thread.InnerHtml;

                list.Add(new ChapterModel
                {
                    Title = chapterTitle,
                    Content = content,
                    Link = link
                });

                var nextChapter = document.DocumentNode.QuerySelectorAll("a#btnNextChapter").FirstOrDefault();

                if (nextChapter != null)
                {
                    var linkNextChapter = nextChapter.Attributes["href"].Value;

                    if (!string.IsNullOrEmpty(linkNextChapter))
                    {
                        return (Constants.Url + linkNextChapter);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                logger.Error(ex, "Error when get chapter content");
            }

            return string.Empty;
        }

        private void UpdateToDB(Guid storyId, List<ChapterModel> list, int chapterNumber = 0)
        {
            try
            {
                Console.WriteLine("Updating to db...");
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

                    Console.WriteLine("Finish update to db.");
                    logger.Info("Finish update to db.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                logger.Error(ex);
            }

        }
    }
}
