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
    public enum Grade
    {
        A, B, C, D, F
    }

    public class Enrollment
    {
        public const string NO_GRADE = "No grade";

        public ObjectId Id { get; set; }
        public ObjectId CourseId { get; set; }
        public ObjectId PersonId { get; set; }

        [DisplayFormat(NullDisplayText = NO_GRADE)]
        public Grade? Grade { get; set; }

        public Course Course
        {
            get
            {
                var q = Query<Course>.EQ(x => x.Id, CourseId);
                var a = DbHelper.Db.GetCollection<Course>("courses");
                Course o = a.FindOne(q);
                return o;
            }
        }

        public Student Student
        {
            get
            {
                var q = Query<Student>.EQ(x => x.Id, PersonId);
                var a = DbHelper.Db.GetCollection<Student>("students");
                Student o = a.FindOne(q);
                return o;
            }
        }
    }
}