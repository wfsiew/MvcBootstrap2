using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;

namespace MvcBootstrap2.ViewModels
{
    public class AssignedCourseData
    {
        public ObjectId CourseID { get; set; }
        public string Title { get; set; }
        public bool Assigned { get; set; }
    }
}