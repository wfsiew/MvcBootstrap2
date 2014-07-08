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
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Number")]
        public ObjectId Id { get; set; }

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
                var a = DbHelper.Db.GetCollection<Department>("departments");
                Department o = a.FindOne(q);
                return o;
            }
        }

        public IEnumerable<Enrollment> Enrollments
        {
            get
            {
                var q = Query<Enrollment>.Where(x => EnrollmentIdList.Contains(x.Id));
                var a = DbHelper.Db.GetCollection<Enrollment>("enrollments");
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
                var a = DbHelper.Db.GetCollection<Instructor>("instructors");
                var b = a.Find(q);
                List<Instructor> l = b.ToList();
                return l;
            }
        }
    }

    public class CourseModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Number")]
        public string Id { get; set; }

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