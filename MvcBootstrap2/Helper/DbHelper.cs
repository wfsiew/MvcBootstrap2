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
                    var connectionString = "mongodb://appharbor_803b7e5d-de7d-474f-96a7-1f5d651c3ef0:o8o4usrnrb2d6rtihpd168oj54@ds061298.mongolab.com:61298/appharbor_803b7e5d-de7d-474f-96a7-1f5d651c3ef0";
                    var client = new MongoClient(connectionString);
                    var server = client.GetServer();
                    db = server.GetDatabase("appdb");
                }

                return db;
            }
        }
    }
}