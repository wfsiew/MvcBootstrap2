using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcBootstrap2.Models;
using MvcBootstrap2.Helper;
using MongoDB.Bson;
using MongoDB.Driver;

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

            return null;
        }
	}
}