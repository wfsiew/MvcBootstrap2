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
using DoddleReport.iTextSharp;
using DoddleReport.OpenXml;

namespace MvcBootstrap2.Controllers
{
    public class StudentController : Controller
    {
        public const string MENU = "Student";

        //
        // GET: /Student/

        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.menu = MENU;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "Name_desc" : "";
            ViewBag.FirstNameSortParm = sortOrder == "FirstName" ? "FirstName_desc" : "FirstName";
            ViewBag.DateSortParm = sortOrder == "Date" ? "Date_desc" : "Date";

            if (searchString != null)
                page = 1;

            else
                searchString = currentFilter;

            string keyword = string.IsNullOrEmpty(searchString) ? null : searchString.ToUpper();

            ViewBag.CurrentFilter = searchString;
            MongoCursor<Student> c = null;
            IOrderedEnumerable<Student> el = null;
            var students = Student.GetCollection();

            if (!string.IsNullOrEmpty(keyword))
            {
                var q = Query<Student>.Where(x => x.LastName.ToUpper().Contains(keyword) ||
                    x.FirstMidName.ToUpper().Contains(keyword));
                c = students.Find(q);
            }

            else
                c = students.FindAll();

            switch (sortOrder)
            {
                case "Name_desc":
                    el = c.OrderByDescending(x => x.LastName);
                    break;

                case "FirstName":
                    el = c.OrderBy(x => x.FirstMidName);
                    break;

                case "FirstName_desc":
                    el = c.OrderByDescending(x => x.FirstMidName);
                    break;

                case "Date":
                    el = c.OrderBy(x => x.EnrollmentDate);
                    break;

                case "Date_desc":
                    el = c.OrderByDescending(x => x.EnrollmentDate);
                    break;

                default:
                    el = c.OrderBy(x => x.LastName);
                    break;
            }

            int pageSize = Constants.PAGE_SIZE;
            int pageNumber = (page ?? 1);

            return View(el.ToPagedList(pageNumber, pageSize));
        }

        //
        // GET: /Student/Details/5

        public ActionResult Details(string id)
        {
            ViewBag.menu = MENU;
            var students = Student.GetCollection();
            var q = Query<Student>.EQ(x => x.Id, new ObjectId(id));
            Student student = students.FindOne(q);
            if (student == null)
            {
                return HttpNotFound();
            }

            return View(student);
        }

        //
        // GET: /Student/Create

        public ActionResult Create()
        {
            ViewBag.menu = MENU;
            return View();
        }

        //
        // POST: /Student/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "LastName, FirstMidName, EnrollmentDate")] StudentModel student)
        {
            ViewBag.menu = MENU;
            try
            {
                if (ModelState.IsValid)
                {
                    Student o = new Student();
                    o.EnrollmentDate = student.EnrollmentDate;
                    o.FirstMidName = student.FirstMidName;
                    o.LastName = student.LastName;
                    var students = Student.GetCollection();
                    students.Insert(o);
                    TempData["message"] = string.Format("{0} has been saved", o.FullName);
                    return RedirectToAction("Index");
                }
            }

            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(student);
        }

        //
        // GET: /Student/Edit/5

        public ActionResult Edit(string id)
        {
            ViewBag.menu = MENU;
            var students = Student.GetCollection();
            var q = Query<Student>.EQ(x => x.Id, new ObjectId(id));
            Student student = students.FindOne(q);
            if (student == null)
            {
                return HttpNotFound();
            }

            StudentModel o = new StudentModel();
            o.Id = student.Id.ToString();
            o.EnrollmentDate = student.EnrollmentDate;
            o.FirstMidName = student.FirstMidName;
            o.LastName = student.LastName;
            return View(o);
        }

        //
        // POST: /Student/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StudentModel student)
        {
            ViewBag.menu = MENU;
            if (ModelState.IsValid)
            {
                var students = Student.GetCollection();
                var q = Query<Student>.EQ(x => x.Id, new ObjectId(student.Id));
                Student o = students.FindOne(q);
                o.EnrollmentDate = student.EnrollmentDate;
                o.FirstMidName = student.FirstMidName;
                o.LastName = student.LastName;
                students.Save(o);
                TempData["message"] = string.Format("{0} has been saved", o.FullName);
                return RedirectToAction("Index");
            }

            return View(student);
        }

        //
        // GET: /Student/Delete/5

        public ActionResult Delete(bool? saveChangesError = false, string id = null)
        {
            ViewBag.menu = MENU;
            if (saveChangesError.GetValueOrDefault())
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";

            var students = Student.GetCollection();
            var q = Query<Student>.EQ(x => x.Id, new ObjectId(id));
            Student student = students.FindOne(q);
            if (student == null)
            {
                return HttpNotFound();
            }

            return View(student);
        }

        //
        // POST: /Student/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            ViewBag.menu = MENU;
            try
            {
                var students = Student.GetCollection();
                var q = Query<Student>.EQ(x => x.Id, new ObjectId(id));
                students.Remove(q);
                TempData["message"] = "Student was deleted";
            }

            catch (Exception)
            {
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }

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

        private Report GetReport(string sortOrder, string currentFilter, string searchString)
        {
            ViewBag.menu = MENU;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "Name_desc" : "";
            ViewBag.FirstNameSortParm = sortOrder == "FirstName" ? "FirstName_desc" : "FirstName";
            ViewBag.DateSortParm = sortOrder == "Date" ? "Date_desc" : "Date";

            if (searchString == null)
                searchString = currentFilter;

            string keyword = string.IsNullOrEmpty(searchString) ? null : searchString.ToUpper();

            ViewBag.CurrentFilter = searchString;
            MongoCursor<Student> c = null;
            IOrderedEnumerable<Student> el = null;
            var students = Student.GetCollection();

            if (!string.IsNullOrEmpty(keyword))
            {
                var q = Query<Student>.Where(x => x.LastName.ToUpper().Contains(keyword) ||
                    x.FirstMidName.ToUpper().Contains(keyword));
                c = students.Find(q);
            }

            else
                c = students.FindAll();

            switch (sortOrder)
            {
                case "Name_desc":
                    el = c.OrderByDescending(x => x.LastName);
                    break;

                case "FirstName":
                    el = c.OrderBy(x => x.FirstMidName);
                    break;

                case "FirstName_desc":
                    el = c.OrderByDescending(x => x.FirstMidName);
                    break;

                case "Date":
                    el = c.OrderBy(x => x.EnrollmentDate);
                    break;

                case "Date_desc":
                    el = c.OrderByDescending(x => x.EnrollmentDate);
                    break;

                default:
                    el = c.OrderBy(x => x.LastName);
                    break;
            }

            var lr = el.Select(x => new
            {
                LastName = x.LastName, 
                FirstName = x.FirstMidName,
                EnrollmentDate = x.EnrollmentDate
            });
            Report report = new Report(lr.ToReportSource());
            report.TextFields.Title = "Students Report";
            report.TextFields.Header = string.Format(@"Report Generated: {0} Total Students: {1}", DateTime.Now, c.Count());

            report.DataFields["EnrollmentDate"].DataFormatString = "{0:yyyy-MM-dd}";
            //report.DataFields["Id"].Hidden = true;
            //report.DataFields["Enrollments"].Hidden = true;
            //report.DataFields["FullName"].Hidden = true;

            report.RenderHints.FreezeRows = 4;

            report.RenderingRow += report_RenderingRow;

            return report;
        }

        private void report_RenderingRow(object sender, ReportRowEventArgs e)
        {
        }
	}
}