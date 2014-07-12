using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcBootstrap2.Models;
using MvcBootstrap2.Helper;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using PagedList;
using DoddleReport;
using DoddleReport.Web;

namespace MvcBootstrap2.Controllers
{
    public class CourseController : Controller
    {
        public const string MENU = "Course";

        //
        // GET: /Course/

        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.menu = MENU;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.TitleSortParm = string.IsNullOrEmpty(sortOrder) ? "Title_desc" : "";
            ViewBag.DeptSortParm = sortOrder == "Dept" ? "Dept_desc" : "Dept";
            ViewBag.CourseIDSortParm = sortOrder == "CourseID" ? "CourseID_desc" : "CourseID";
            ViewBag.CreditsSortParm = sortOrder == "Credits" ? "Credits_desc" : "Credits";

            if (searchString != null)
                page = 1;

            else
                searchString = currentFilter;

            string keyword = string.IsNullOrEmpty(searchString) ? null : searchString.ToUpper();

            ViewBag.CurrentFilter = searchString;
            MongoCursor<Course> c = null;
            IOrderedEnumerable<Course> el = null;
            var courses = Course.GetCollection();

            if (!string.IsNullOrEmpty(keyword))
            {
                var qd = Query<Department>.Where(x => x.Name.Contains(keyword));
                var departments = DbHelper.Db.GetCollection<Department>("departments");
                var d = departments.Find(qd);
                var did = d.Select(x => x.Id);

                var q = Query<Course>.Where(x => x.Title.Contains(keyword) ||
                    did.Contains(x.DepartmentId));
                c = courses.Find(q);
            }

            else
                c = courses.FindAll();

            switch (sortOrder)
            {
                case "Title_desc":
                    el = c.OrderByDescending(x => x.Title);
                    break;

                case "Dept":
                    el = c.OrderBy(x => x.Department.Name);
                    break;

                case "Dept_desc":
                    el = c.OrderByDescending(x => x.Department.Name);
                    break;

                case "CourseID":
                    el = c.OrderBy(x => x.CourseID);
                    break;

                case "CourseID_desc":
                    el = c.OrderByDescending(x => x.CourseID);
                    break;

                case "Credits":
                    el = c.OrderBy(x => x.Credits);
                    break;

                case "Credits_desc":
                    el = c.OrderByDescending(x => x.Credits);
                    break;

                default:
                    el = c.OrderBy(x => x.Title);
                    break;
            }

            int pageSize = Constants.PAGE_SIZE;
            int pageNumber = (page ?? 1);

            return View(el.ToPagedList(pageNumber, pageSize));
        }

        //
        // GET: /Course/Details/5

        public ActionResult Details(string id)
        {
            ViewBag.menu = MENU;
            var courses = Course.GetCollection();
            var q = Query<Course>.EQ(x => x.Id, new ObjectId(id));
            Course course = courses.FindOne(q);
            if (course == null)
            {
                return HttpNotFound();
            }

            return View(course);
        }

        //
        // GET: /Course/Create

        public ActionResult Create()
        {
            ViewBag.menu = MENU;
            PopulateDepartmentsDropDownList();
            return View();
        }

        //
        // POST: /Course/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CourseID,Title,Credits,DepartmentId")] CourseModel course)
        {
            ViewBag.menu = MENU;
            try
            {
                if (ModelState.IsValid)
                {
                    Course o = new Course();
                    o.CourseID = course.CourseID;
                    o.Title = course.Title;
                    o.Credits = course.Credits;
                    o.DepartmentId = new ObjectId(course.DepartmentId);
                    var courses = Course.GetCollection();
                    courses.Insert(o);
                    TempData["message"] = string.Format("{0} has been saved", course.Title);
                    return RedirectToAction("Index");
                }
            }

            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            PopulateDepartmentsDropDownList(course.DepartmentId);
            return View(course);
        }

        //
        // GET: /Course/Edit/5

        public ActionResult Edit(string id)
        {
            ViewBag.menu = MENU;
            var courses = Course.GetCollection();
            var q = Query<Course>.EQ(x => x.Id, new ObjectId(id));
            Course course = courses.FindOne(q);
            if (course == null)
            {
                return HttpNotFound();
            }

            CourseModel o = new CourseModel();
            o.CourseID = course.CourseID;
            o.Credits = course.Credits;
            o.DepartmentId = course.DepartmentId.ToString();
            o.Id = course.Id.ToString();
            o.Title = course.Title;

            PopulateDepartmentsDropDownList(course.DepartmentId);
            return View(o);
        }

        //
        // POST: /Course/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CourseModel course)
        {
            ViewBag.menu = MENU;
            if (ModelState.IsValid)
            {
                var courses = Course.GetCollection();
                var q = Query<Course>.EQ(x => x.Id, new ObjectId(course.Id));
                Course o = courses.FindOne(q);
                o.CourseID = course.CourseID;
                o.Credits = course.Credits;
                o.DepartmentId = new ObjectId(course.DepartmentId);
                o.Title = course.Title;
                courses.Save(o);
                TempData["message"] = string.Format("{0} has been saved", course.Title);
                return RedirectToAction("Index");
            }

            PopulateDepartmentsDropDownList(course.DepartmentId);
            return View(course);
        }

        //
        // GET: /Course/Delete/5

        public ActionResult Delete(string id)
        {
            ViewBag.menu = MENU;
            var courses = Course.GetCollection();
            var q = Query<Course>.EQ(x => x.Id, new ObjectId(id));
            Course course = courses.FindOne(q);
            if (course == null)
            {
                return HttpNotFound();
            }

            return View(course);
        }

        //
        // POST: /Course/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ViewBag.menu = MENU;
            var courses = Course.GetCollection();
            var q = Query<Course>.EQ(x => x.Id, new ObjectId(id));
            courses.Remove(q);
            TempData["message"] = "Course was deleted";
            return RedirectToAction("Index");
        }

        public ActionResult HtmlReport(string sortOrder, string currentFilter, string searchString)
        {
            Report report = GetReport(sortOrder, currentFilter, searchString);
            return new ReportResult(report);
        }

        public ActionResult PdfReport(string sortOrder, string currentFilter, string searchString)
        {
            Report report = GetReport(sortOrder, currentFilter, searchString);
            return new ReportResult(report, new DoddleReport.iTextSharp.PdfReportWriter(), "application/pdf");
        }

        public ActionResult ExcelReport(string sortOrder, string currentFilter, string searchString)
        {
            var report = GetReport(sortOrder, currentFilter, searchString);
            return new ReportResult(report, new DoddleReport.OpenXml.ExcelReportWriter(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        public ActionResult CsvReport(string sortOrder, string currentFilter, string searchString)
        {
            var report = GetReport(sortOrder, currentFilter, searchString);
            return new ReportResult(report, new DoddleReport.Writers.DelimitedTextReportWriter(), "text/plain;charset=UTF-8");
        }

        private void PopulateDepartmentsDropDownList(object selectedDepartment = null)
        {
            var departments = Department.GetCollection();
            var l = departments.FindAll().OrderBy(x => x.Name);
            ViewBag.DepartmentID_ = new SelectList(l, "Id", "Name", selectedDepartment);
        }

        private Report GetReport(string sortOrder, string currentFilter, string searchString)
        {
            ViewBag.menu = MENU;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.TitleSortParm = string.IsNullOrEmpty(sortOrder) ? "Title_desc" : "";
            ViewBag.DeptSortParm = sortOrder == "Dept" ? "Dept_desc" : "Dept";
            ViewBag.CourseIDSortParm = sortOrder == "CourseID" ? "CourseID_desc" : "CourseID";
            ViewBag.CreditsSortParm = sortOrder == "Credits" ? "Credits_desc" : "Credits";

            if (searchString == null)
                searchString = currentFilter;

            string keyword = string.IsNullOrEmpty(searchString) ? null : searchString.ToUpper();

            ViewBag.CurrentFilter = searchString;
            MongoCursor<Course> c = null;
            IOrderedEnumerable<Course> el = null;
            var courses = Course.GetCollection();

            if (!string.IsNullOrEmpty(keyword))
            {
                var qd = Query<Department>.Where(x => x.Name.Contains(keyword));
                var departments = DbHelper.Db.GetCollection<Department>("departments");
                var d = departments.Find(qd);
                var did = d.Select(x => x.Id);

                var q = Query<Course>.Where(x => x.Title.Contains(keyword) ||
                    did.Contains(x.DepartmentId));
                c = courses.Find(q);
            }

            else
                c = courses.FindAll();

            switch (sortOrder)
            {
                case "Title_desc":
                    el = c.OrderByDescending(x => x.Title);
                    break;

                case "Dept":
                    el = c.OrderBy(x => x.Department.Name);
                    break;

                case "Dept_desc":
                    el = c.OrderByDescending(x => x.Department.Name);
                    break;

                case "CourseID":
                    el = c.OrderBy(x => x.CourseID);
                    break;

                case "CourseID_desc":
                    el = c.OrderByDescending(x => x.CourseID);
                    break;

                case "Credits":
                    el = c.OrderBy(x => x.Credits);
                    break;

                case "Credits_desc":
                    el = c.OrderByDescending(x => x.Credits);
                    break;

                default:
                    el = c.OrderBy(x => x.Title);
                    break;
            }

            var lr = el.Select(x => new
            { 
                Number = x.CourseID, 
                Title = x.Title, 
                Credits = x.Credits, 
                Department = x.Department
            });
            Report report = new Report(lr.ToReportSource());
            report.TextFields.Title = "Courses Report";
            report.TextFields.Header = string.Format(@"Report Generated: {0} Total Courses: {1}", DateTime.Now, c.Count());

            report.RenderHints.FreezeRows = 4;

            report.RenderingRow += report_RenderingRow;

            return report;
        }

        private void report_RenderingRow(object sender, ReportRowEventArgs e)
        {
        }
	}
}