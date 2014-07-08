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

            //var f = Query<Course>.Where(i => i.Code.Contains("jj"));
            //var g = k.Find(f);
            //var h = g.Select(i => i.Id);

            //var r = Query<Student>.Where(i => h.Contains(i.Course));
            //var s = q.Find(r);
            //var t = s.ToList();

            //foreach (var a in t)
            //{
            //    Console.WriteLine(a.FullName);
            //}

            var a = q.FindAll();

            var e = a.OrderBy(j => j.Course.Code);

            foreach (var x in e)
            {
                Console.WriteLine(x.FirstMidName + ", " + x.Course.Code);
            }

            //Student o = new Student();
            //o.FirstMidName = "ben";
            //o.LastName = "jun";
            //q.Insert(o);

            //Course g = new Course();
            //g.Code = "asd";
            //k.Insert(g);

            //o.CourseId = g.Id;
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

        public ObjectId CourseId { get; set; }
        public Course Course {
            get
            {
                var connectionString = "mongodb://localhost";
                var client = new MongoClient(connectionString);
                var server = client.GetServer();
                var db = server.GetDatabase("test");

                var q = Query<Course>.EQ(o => o.Id, CourseId);
                var i = db.GetCollection<Course>("courses");
                var x = i.FindOne(q);
                return x;
            }
        }
    }

    public class Course
    {
        public ObjectId Id { get; set; }
        public string Code { get; set; }
    }
}
