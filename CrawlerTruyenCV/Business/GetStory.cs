using Core;
using CrawlerTruyenCV.Models;
using DataAccess;
using DataAccess.Models;
using EntityFramework.DbContextScope.Interfaces;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerTruyenCV.Business
{
    public class GetStory
    {
        private readonly IDbContextScopeFactory _scopeFactory;

        public GetStory(IDbContextScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void GetStoryContent(string story)
        {
            Console.WriteLine("Getting chapter...");

            HtmlWeb htmlWeb = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8  //Set UTF8 để hiển thị tiếng Việt            
            };

            var url = Constants.Url + "/truyen/tay-du-to-lon-giai-tri-gia-W1mDAlS4CBRxQwES#!/";

            HtmlDocument document = htmlWeb.Load(url);

            var firstChapter = document.DocumentNode.QuerySelectorAll("li.chapter-name > a.truncate").FirstOrDefault();

            var titleFirst = firstChapter.InnerText;
            var linkFirst = firstChapter.Attributes["href"].Value;

            var list = new List<ChapterModel>();
            list.Add(new ChapterModel()
            {
                Title = titleFirst,
                Link = Constants.Url + linkFirst
            });

            ProcessChapters(list);

            if(list != null && list.Count > 0)
            {
                Console.WriteLine($"Total chapters:  {list.Count}");

                Console.WriteLine("Inserting to db...");

                ProcessDB(list);

                Console.WriteLine("Finish");
            }
        }

        private void ProcessChapters(List<ChapterModel> list)
        {
            var firstItem = list.FirstOrDefault();        

            HtmlWeb htmlWeb = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8  //Set UTF8 để hiển thị tiếng Việt            
            };
            
            HtmlDocument document = htmlWeb.Load(firstItem.Link);

            var thread = document.DocumentNode.QuerySelectorAll("div#bookContentBody").FirstOrDefault();
            var content = thread.InnerHtml;

            firstItem.Content = content;

            var nextChapter = document.DocumentNode.QuerySelectorAll("a#btnNextChapter").FirstOrDefault();
            var linkNextChapter = nextChapter.Attributes["href"].Value;

            if (!string.IsNullOrEmpty(linkNextChapter))
            {
                var url = Constants.Url + linkNextChapter;

                var link = GetChapterContent(list, url);

                while (!string.IsNullOrEmpty(link))
                {
                    link = GetChapterContent(list, link);
                }
            }

        }

        private string GetChapterContent(List<ChapterModel> list, string link)
        {
            HtmlWeb htmlWeb = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8  //Set UTF8 để hiển thị tiếng Việt            
            };           

            HtmlDocument document = htmlWeb.Load(link);

            try
            {
                var headers = document.DocumentNode.QuerySelectorAll("div#bookContent p").ToArray();

                var chapterTitle = headers[1].InnerText;

                var thread = document.DocumentNode.QuerySelectorAll("div#bookContentBody").FirstOrDefault();
                var content = thread.InnerHtml;

                var nextChapter = document.DocumentNode.QuerySelectorAll("a#btnNextChapter").FirstOrDefault();
                var linkNextChapter = nextChapter.Attributes["href"].Value;

                var item = new ChapterModel();
                item.Title = chapterTitle;
                item.Content = content;

                list.Add(item);

                if (!string.IsNullOrEmpty(linkNextChapter))
                {
                    return (Constants.Url + linkNextChapter);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }           

            return string.Empty;
        }

        private void ProcessDB(List<ChapterModel> list)
        {
            using (var scope = _scopeFactory.Create())
            {
                var context = scope.DbContexts.Get<MainContext>();

                var listChapter = new List<Chapter>();
                var number = 1;

                foreach(var item in list)
                {
                    listChapter.Add(new Chapter()
                    {
                        Id = Guid.NewGuid(), 
                        Title = item.Title,
                        Content = item.Content,
                        StatusId = true,
                        StoryId = new Guid("CEB70306-11A6-4151-8A26-990CB704427B"),
                        NumberChapter = number
                    });

                    number += 1;
                }

                context.Set<Chapter>().AddRange(listChapter);

                var story = context.Set<Story>().FirstOrDefault(f => f.Id == new Guid("CEB70306-11A6-4151-8A26-990CB704427B"));

                if(story != null)
                {
                    story.TotalChapter = listChapter.Count;
                }

                scope.SaveChanges();
            }
        }
    }
}
