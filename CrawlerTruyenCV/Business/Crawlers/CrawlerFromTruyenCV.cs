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
    public class CrawlerFromTruyenCV
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IDbContextScopeFactory _scopeFactory;
        private HtmlWeb htmlWeb;

        public CrawlerFromTruyenCV(IDbContextScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public List<ChapterModel> ExcuteCrawler(StoryModel story)
        {
            logger.Info($"Getting chapter for story '{story.Name}' from '{story.Source}'...");

            htmlWeb = GetHtmlContext();

            var list = new List<ChapterModel>();

            if (story.TotalChapter == 0)
            {
                // Get All Chapters
                GetNewChapter(story, list);
            }
            else
            {
                // Get All New Chapters
                GetCurrentChapter(story, list);
            }

            return list;
        }

        private void GetCurrentChapter(StoryModel story, List<ChapterModel> list)
        {       
            var linkNextChapter = string.Empty;

            using (var scope = _scopeFactory.CreateReadOnly())
            {
                var context = scope.DbContexts.Get<MainContext>();

                var currentChapter = context.Set<Chapter>().Where(f => f.StoryId == story.Id).OrderByDescending(o => o.NumberChapter).FirstOrDefault();

                if (currentChapter != null)
                {

                    HtmlDocument document = htmlWeb.Load(currentChapter.Link);

                    var chapterQuery = document.DocumentNode.QuerySelectorAll("main.truyencv-main").FirstOrDefault();

                    if (chapterQuery == null)
                    {
                        Task.Delay(30000);

                        chapterQuery = document.DocumentNode.QuerySelectorAll("main.truyencv-main").FirstOrDefault();

                        if (chapterQuery != null)
                        {
                            var linkNextQuery = chapterQuery.QuerySelector("div.truyencv-read-navigation a[title='Chương sau']");

                            if (linkNextQuery != null)
                            {
                                linkNextChapter = linkNextQuery.Attributes["href"].Value;
                            }
                        }
                        else
                        {
                            logger.Error("Cannot get chapter - link error");
                        }
                    }
                    else
                    {
                        var linkNextQuery = chapterQuery.QuerySelector("div.truyencv-read-navigation a[title='Chương sau']");

                        if (linkNextQuery != null)
                        {
                            linkNextChapter = linkNextQuery.Attributes["href"].Value;
                        }
                    }

                    if (!string.IsNullOrEmpty(linkNextChapter))
                    {    
                        ProcessChapters(list, linkNextChapter);
                    }
                    else
                    {
                        logger.Info("This chapter is latest");
                    }
                }
            }
        }

        private void GetNewChapter(StoryModel story, List<ChapterModel> list)
        {
            try
            {            
                var link = story.Link + "/chuong-1";
                var linkNextChapter = string.Empty;

                linkNextChapter = GetChapter(list, link);            

                if (!string.IsNullOrEmpty(linkNextChapter))
                {
                    var firstItem = list.FirstOrDefault();
                    firstItem.Link = link;

                    ProcessChapters(list, linkNextChapter);
                }
                else
                {
                    logger.Error("Cannot get first chapter - link error");
                }                
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }        

        private void ProcessChapters(List<ChapterModel> list, string url)
        {
            var count = 0;

            var link = GetChapter(list, url);

            while (!string.IsNullOrEmpty(link) && count < Constants.MaxChapter)
            {
                link = GetChapter(list, link);
                count += 1;
            }
        }

        private string GetChapter(List<ChapterModel> list, string link)
        {
            var linkNextChapter = string.Empty;

            try
            {
                HtmlDocument document = htmlWeb.Load(link);

                var chapterQuery = document.DocumentNode.QuerySelectorAll("main.truyencv-main").FirstOrDefault();

                if (chapterQuery == null)
                {
                    Task.Delay(30000);

                    chapterQuery = document.DocumentNode.QuerySelectorAll("main.truyencv-main").FirstOrDefault();

                    if (chapterQuery != null)
                    {
                        linkNextChapter = GetChapterContent(list, chapterQuery, link);
                    }
                    else
                    {
                        logger.Error("Cannot get chapter - link error");
                    }
                }
                else
                {
                    linkNextChapter = GetChapterContent(list, chapterQuery, link);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error when get chapter content");
            }

            return linkNextChapter;
        }

        private string GetChapterContent(List<ChapterModel> list, HtmlNode chapterQuery, string link)
        {
            var model = new ChapterModel();
            model.Link = link;

            var linkNextChapter = string.Empty;

            var titleQuery = chapterQuery.QuerySelectorAll("div.header h2.title").FirstOrDefault();

            if (titleQuery != null)
            {
                model.Title = titleQuery.InnerText;
            }

            var linkNextQuery = chapterQuery.QuerySelector("div.truyencv-read-navigation a[title='Chương sau']");

            if (linkNextQuery != null)
            {
                linkNextChapter = linkNextQuery.Attributes["href"].Value;
            }

            var contentQuery = chapterQuery.QuerySelector("div#js-truyencv-content");

            if (contentQuery != null)
            {
                model.Content = contentQuery.InnerHtml;
            }

            if (!string.IsNullOrEmpty(model.Content))
            {
                list.Add(model);
            }
            else
            {
                linkNextChapter = string.Empty;
            }

            return linkNextChapter;
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
    }
}
