using Core;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerTruyenCV.Business
{
    public class GetAuthor
    {
        public void GetData()
        {
            HtmlWeb htmlWeb = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8  //Set UTF8 để hiển thị tiếng Việt            
            };

            var url = Constants.UrlWiki;

            HtmlDocument document = htmlWeb.Load(Constants.UrlWiki);

            var thread = document.DocumentNode.QuerySelectorAll("div#content").FirstOrDefault();
            var content = thread.QuerySelector("blockquote.postcontent");
        }
        
    }
}
