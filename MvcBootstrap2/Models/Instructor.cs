﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MvcBootstrap2.Helper;

namespace MvcBootstrap2.Models
{
    public class Instructor : Person
    {
        private const string COLLECTION_NAME = "instructors";

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Hire Date")]
        public DateTime HireDate { get; set; }

        public List<ObjectId> CourseIdList { get; set; }

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

        public OfficeAssignment OfficeAssignment
        {
            get
            {
                var q = Query<OfficeAssignment>.EQ(x => x.PersonId, Id);
                var a = OfficeAssignment.GetCollection();
                OfficeAssignment o = a.FindOne(q);
                return o;
            }
        }

        public static MongoCollection<Instructor> GetCollection()
        {
            MongoCollection<Instructor> a = DbHelper.Db.GetCollection<Instructor>(COLLECTION_NAME);
            return a;
        }

        public string GetCourses(bool ishtml)
        {
            IEnumerable<Course> l = Courses;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < l.Count(); i++)
            {
                Course o = l.ElementAt(i);
                string s = string.Format("{0} {1}", o.CourseID, o.Title);
                
                if (i < l.Count() - 1)
                {
                    if (ishtml)
                        sb.Append(s + "<br/>");

                    else
                        sb.AppendLine(s);
                }

                else
                    sb.Append(s);
            }

            return sb.ToString();
        }
    }

    public class InstructorModel : PersonModel
    {
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Hire Date")]
        public DateTime HireDate { get; set; }

        public List<string> CourseIdList { get; set; }
        public OfficeAssignment OfficeAssignment { get; set; }
    }
}