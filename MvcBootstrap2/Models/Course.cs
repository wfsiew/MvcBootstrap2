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
    public class Course
    {
        private const string COLLECTION_NAME = "courses";

        public ObjectId Id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Number")]
        public int CourseID { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string Title { get; set; }

        [Range(0, 5)]
        public int Credits { get; set; }

        [Display(Name = "Department")]
        public ObjectId DepartmentId { get; set; }

        public List<ObjectId> InstructorIdList { get; set; }
        public List<ObjectId> EnrollmentIdList { get; set; }

        public Department Department
        {
            get
            {
                var q = Query<Department>.EQ(x => x.Id, DepartmentId);
                var a = Department.GetCollection();
                Department o = a.FindOne(q);
                return o;
            }
        }

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

        public IEnumerable<Instructor> Instructors
        {
            get
            {
                var q = Query<Instructor>.Where(x => InstructorIdList.Contains(x.Id));
                var a = Instructor.GetCollection();
                var b = a.Find(q);
                List<Instructor> l = b.ToList();
                return l;
            }
        }

        public static MongoCollection<Course> GetCollection()
        {
            MongoCollection<Course> a = DbHelper.Db.GetCollection<Course>(COLLECTION_NAME);
            //a.CreateIndex(IndexKeys<Course>.Ascending(x => x.CourseID), IndexOptions.SetUnique(true));
            return a;
        }
    }

    public class CourseModel
    {
        public string Id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Number")]
        public int CourseID { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string Title { get; set; }

        [Range(0, 5)]
        public int Credits { get; set; }

        [Display(Name = "Department")]
        public string DepartmentId { get; set; }

        public List<string> InstructorIdList { get; set; }
        public List<string> EnrollmentIdList { get; set; }
    }
}