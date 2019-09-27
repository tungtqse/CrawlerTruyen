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

        public async Task Excute()
        {
            logger.Info("Getting stories from db...");

            var stories = new List<StoryModel>();

            try
            {
                #region Excute Crawler

                using (var scope = _scopeFactory.Create())
                {
                    var context = scope.DbContexts.Get<MainContext>();

                    stories = context.Set<Story>().Where(f => f.StatusId && (f.ProgressStatus == Constants.StoryProgressStatus.Processing
                                                            || f.ProgressStatus == Constants.StoryProgressStatus.Pending)
                                                            && !string.IsNullOrEmpty(f.Link))
                                                      .Select(s => new StoryModel()
                                                      {
                                                          Id = s.Id,
                                                          Name = s.Name,
                                                          Link = s.Link,
                                                          TotalChapter = s.TotalChapter,
                                                          Source = s.Source
                                                      }).ToList();                    
                }

                if (stories != null && stories.Count > 0)
                {
                    logger.Info($"Get total: {stories.Count} stories");

                    // ***Create a query that, when executed, returns a collection of tasks.
                    IEnumerable<Task> crawlerTasksQuery = from story in stories select CrawlerChapter(story);

                    // ***Use ToList to execute the query and start the tasks.
                    List<Task> crawlerTasks = crawlerTasksQuery.ToList();

                    // Await the completion of all the running tasks.
                    await Task.WhenAll(crawlerTasks);


                    // ***Add a loop to process the tasks one at a time until none remain.
                    //while (crawlerTasks.Count > 0)
                    //{
                    //    // Identify the first task that completes.
                    //    Task firstFinishedTask = await Task.WhenAny(crawlerTasks);

                    //    // ***Remove the selected task from the list so that you don't
                    //    // process it more than once.
                    //    crawlerTasks.Remove(firstFinishedTask);

                    //    // Await the completed task.
                    //    await firstFinishedTask;                        
                    //}

                    //foreach (var story in stories)
                    //{
                    //    chapter.Excute(story);
                    //}
                }
                else
                {
                    logger.Info("Not found valid story.");
                }              

                logger.Info("Finish Crawler.");

                #endregion
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }            
        }

        private async Task CrawlerChapter(StoryModel story)
        {
            ChapterProcess chapter = new ChapterProcess(_scopeFactory);

            await chapter.Excute(story);
        }
    }
}
