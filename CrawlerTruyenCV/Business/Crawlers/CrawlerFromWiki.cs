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
    public class CrawlerFromWiki
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IDbContextScopeFactory _scopeFactory;
        private HtmlWeb htmlWeb;

        public CrawlerFromWiki(IDbContextScopeFactory scopeFactory)
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

        private void GetNewChapter(StoryModel story, List<ChapterModel> list)
        {
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
                        linkFirst = Constants.UrlWiki + linkFirst;

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
                            var url = Constants.UrlWiki + linkNextChapter;
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
                logger.Error(ex);
            }
        }

        private void GetCurrentChapter(StoryModel story, List<ChapterModel> list)
        {           
            using (var scope = _scopeFactory.CreateReadOnly())
            {
                var context = scope.DbContexts.Get<MainContext>();

                var currentChapter = context.Set<Chapter>().Where(f => f.StoryId == story.Id).OrderByDescending(o => o.NumberChapter).FirstOrDefault();

                if (currentChapter != null)
                {

                    HtmlDocument document = htmlWeb.Load(currentChapter.Link);

                    var nextChapter = document.DocumentNode.QuerySelectorAll("a#btnNextChapter").FirstOrDefault();

                    if (nextChapter != null)
                    {
                        var linkNextChapter = nextChapter.Attributes["href"].Value;

                        if (!string.IsNullOrEmpty(linkNextChapter))
                        {
                            var url = Constants.UrlWiki + linkNextChapter;

                            ProcessChapters(list, url);
                        }
                    }
                    else
                    {                        
                        logger.Info("This chapter is latest");
                    }
                }
            }           
        }

        private void ProcessChapters(List<ChapterModel> list, string url)
        {
            var count = 0;

            var link = GetChapterContent(list, url);

            while (!string.IsNullOrEmpty(link) && count < Constants.MaxChapter)
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
                        return (Constants.UrlWiki + linkNextChapter);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error when get chapter content");
            }

            return string.Empty;
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
