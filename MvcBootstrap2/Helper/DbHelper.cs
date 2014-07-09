using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
                    var connectionString = "mongodb://localhost";
                    var client = new MongoClient(connectionString);
                    var server = client.GetServer();
                    db = server.GetDatabase("appdb");
                }

                return db;
            }
        }
    }
}