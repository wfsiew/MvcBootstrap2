using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcBootstrap2.Models;
using MvcBootstrap2.ViewModels;
using MvcBootstrap2.Helper;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace MvcBootstrap2.Controllers
{
    public class HomeController : Controller
    {
        public const string MENU = "Home";
        private MongoCollection<Student> students;

        public HomeController()
        {
            students = DbHelper.Db.GetCollection<Student>("students");
        }

        //
        // GET: /Home/
        public ActionResult Index()
        {
            ViewBag.menu = MENU;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.menu = "About";
            var students = Student.GetCollection();
            List<EnrollmentDateGroup> v = new List<EnrollmentDateGroup>();
            var l = students.Distinct("EnrollmentDate").OrderBy(x => x.ToUniversalTime());
            foreach (var k in l)
            {
                EnrollmentDateGroup o = new EnrollmentDateGroup();
                o.EnrollmentDate = k.ToUniversalTime();
                var q = Query<Student>.Where(x => x.EnrollmentDate == o.EnrollmentDate);
                o.StudentCount = students.Count(q);
                v.Add(o);
            }

            return View(v);
        }
	}
}