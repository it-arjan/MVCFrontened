using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data;
using Data.Models;
namespace NancyApi.Modules
{
    public class PostbackDataModule : Nancy.NancyModule
    {
        public PostbackDataModule() : base("api/nancy/postbacks")
        {
            Get["/"] = _ => Hello(_.SessionId);
            Get["/{SessionId}"]=  _ => GetPostbacks(_.SessionId);
        }
        private List<PostbackData> GetPostbacks(string SessionId)
        {
            var db = Data.DbFactory.Db();
            return db.GetEtfdb().Postbacks.ToList();
        }
        private string Hello(string SessionId)
        {
            return "Helloooo :)";
        }
    }
}