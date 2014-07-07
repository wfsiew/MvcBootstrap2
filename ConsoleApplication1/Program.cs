using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

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

            var q = db.GetCollection<Student>("students");
            var k = db.GetCollection<Course>("courses");

            var f = Query<Student>.

            //Student o = new Student();
            //o.FirstMidName = "ben";
            //o.LastName = "jun";
            //q.Insert(o);

            //Course g = new Course();
            //g.Code = "fgT";
            //k.Insert(g);

            //o.Course = g.Id;
            //q.Save(o);

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

        public ObjectId Course { get; set; }
    }

    public class Course
    {
        public ObjectId Id { get; set; }
        public string Code { get; set; }
    }
}
