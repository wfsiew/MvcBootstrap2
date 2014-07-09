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
    public class Student : Person
    {
        private const string COLLECTION_NAME = "students";

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Enrollment Date")]
        public DateTime EnrollmentDate { get; set; }

        public List<ObjectId> EnrollmentIdList { get; set; }

        public IEnumerable<Enrollment> Enrollments
        {
            get
            {
                var q = Query<Enrollment>.Where(x => EnrollmentIdList.Contains(x.Id));
                var a = Enrollment.GetCollection();
                var b = a.Find(q);
                List<Enrollment> l = b.ToList();
                return l;
            }
        }

        public static MongoCollection<Student> GetCollection()
        {
            MongoCollection<Student> a = DbHelper.Db.GetCollection<Student>(COLLECTION_NAME);
            return a;
        }
    }

    public class StudentModel : PersonModel
    {
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Enrollment Date")]
        public DateTime EnrollmentDate { get; set; }
    }
}