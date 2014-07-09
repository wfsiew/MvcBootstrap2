using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using MongoDB.Driver;

namespace MvcBootstrap2.Helper
{
    public class DbHelper
    {
        private static MongoDatabase db = null;

        public static MongoDatabase Db
        {
            get
            {
                if (db == null)
                {
                    var connectionString = ConfigurationManager.AppSettings.Get("MONGOLAB_URI");
                    var url = new MongoUrl(connectionString);
                    var client = new MongoClient(url);
                    var server = client.GetServer();
                    db = server.GetDatabase("appdb");
                }

                return db;
            }
        }
    }
}