using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var db = server.GetDatabase("test");

            List<Student> l = Builder<Student>.CreateListOfSize(500).Build().ToList();
            var q = db.GetCollection<Student>("students");
            q.InsertBatch(l);

            Console.WriteLine("done");
            Console.ReadKey();
        }
    }

    public class Student
    {
        public ObjectId Id { get; set; }

        public string LastName { get; set; }

        public string FirstMidName { get; set; }

        public DateTime EnrollmentDate { get; set; }

        public string FullName
        {
            get
            {
                return LastName + ", " + FirstMidName;
            }
        }
    }
}
