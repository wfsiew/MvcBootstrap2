using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MvcBootstrap2.Helper;

namespace MvcBootstrap2.Models
{
    public class Department
    {
        private const string COLLECTION_NAME = "departments";

        public ObjectId Id { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal Budget { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Administrator")]
        public ObjectId PersonId { get; set; }

        public List<ObjectId> CourseIdList { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public Instructor Administrator
        {
            get
            {
                var q = Query<Instructor>.EQ(x => x.Id, PersonId);
                var a = Instructor.GetCollection();
                Instructor o = a.FindOne(q);
                return o;
            }
        }
        
        public IEnumerable<Course> Courses
        {
            get
            {
                if (CourseIdList == null)
                    return new List<Course>();

                var q = Query<Course>.Where(x => CourseIdList.Contains(x.Id));
                var a = Course.GetCollection();
                var b = a.Find(q);
                List<Course> l = b.ToList();
                return l;
            }
        }

        public static MongoCollection<Department> GetCollection()
        {
            MongoCollection<Department> a = DbHelper.Db.GetCollection<Department>(COLLECTION_NAME);
            return a;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class DepartmentModel
    {
        public string Id { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal Budget { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Administrator")]
        public string PersonId { get; set; }

        public List<string> CourseIdList { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}