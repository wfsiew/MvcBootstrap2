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

namespace MvcBootstrap2.Controllers
{
    public class SeedController : Controller
    {
        //
        // GET: /Seed/
        public ActionResult Index()
        {
            //ClearAll();

            var students = new List<Student>
            {
                new Student { FirstMidName = "Carson",   LastName = "Alexander", 
                    EnrollmentDate = DateTime.Parse("2010-09-01"),
                    EnrollmentIdList = new List<ObjectId>() },
                new Student { FirstMidName = "Meredith", LastName = "Alonso",    
                    EnrollmentDate = DateTime.Parse("2012-09-01"),
                    EnrollmentIdList = new List<ObjectId>() },
                new Student { FirstMidName = "Arturo",   LastName = "Anand",     
                    EnrollmentDate = DateTime.Parse("2013-09-01"),
                    EnrollmentIdList = new List<ObjectId>() },
                new Student { FirstMidName = "Gytis",    LastName = "Barzdukas", 
                    EnrollmentDate = DateTime.Parse("2012-09-01"),
                    EnrollmentIdList = new List<ObjectId>() },
                new Student { FirstMidName = "Yan",      LastName = "Li",        
                    EnrollmentDate = DateTime.Parse("2012-09-01"),
                    EnrollmentIdList = new List<ObjectId>() },
                new Student { FirstMidName = "Peggy",    LastName = "Justice",   
                    EnrollmentDate = DateTime.Parse("2011-09-01"),
                    EnrollmentIdList = new List<ObjectId>() },
                new Student { FirstMidName = "Laura",    LastName = "Norman",    
                    EnrollmentDate = DateTime.Parse("2013-09-01"),
                    EnrollmentIdList = new List<ObjectId>() },
                new Student { FirstMidName = "Nino",     LastName = "Olivetto",  
                    EnrollmentDate = DateTime.Parse("2005-09-01"),
                    EnrollmentIdList = new List<ObjectId>() }
            };

            var sc = Student.GetCollection();
            sc.InsertBatch(students);

            var instructors = new List<Instructor>
            {
                new Instructor { FirstMidName = "Kim",     LastName = "Abercrombie", 
                    HireDate = DateTime.Parse("1995-03-11"),
                    CourseIdList = new List<ObjectId>() },
                new Instructor { FirstMidName = "Fadi",    LastName = "Fakhouri",    
                    HireDate = DateTime.Parse("2002-07-06"),
                    CourseIdList = new List<ObjectId>() },
                new Instructor { FirstMidName = "Roger",   LastName = "Harui",       
                    HireDate = DateTime.Parse("1998-07-01"),
                    CourseIdList = new List<ObjectId>() },
                new Instructor { FirstMidName = "Candace", LastName = "Kapoor",      
                    HireDate = DateTime.Parse("2001-01-15"),
                    CourseIdList = new List<ObjectId>() },
                new Instructor { FirstMidName = "Roger",   LastName = "Zheng",      
                    HireDate = DateTime.Parse("2004-02-12"),
                    CourseIdList = new List<ObjectId>() }
            };

            var ic = Instructor.GetCollection();
            ic.InsertBatch(instructors);

            var departments = new List<Department>
            {
                new Department { Name = "English",     Budget = 350000, 
                    StartDate = DateTime.Parse("2007-09-01"), 
                    PersonId  = instructors.Single( i => i.LastName == "Abercrombie").Id,
                    CourseIdList = new List<ObjectId>() },
                new Department { Name = "Mathematics", Budget = 100000, 
                    StartDate = DateTime.Parse("2007-09-01"), 
                    PersonId  = instructors.Single( i => i.LastName == "Fakhouri").Id,
                    CourseIdList = new List<ObjectId>() },
                new Department { Name = "Engineering", Budget = 350000, 
                    StartDate = DateTime.Parse("2007-09-01"), 
                    PersonId  = instructors.Single( i => i.LastName == "Harui").Id,
                    CourseIdList = new List<ObjectId>() },
                new Department { Name = "Economics",   Budget = 100000, 
                    StartDate = DateTime.Parse("2007-09-01"), 
                    PersonId  = instructors.Single( i => i.LastName == "Kapoor").Id,
                    CourseIdList = new List<ObjectId>() }
            };

            var dc = Department.GetCollection();
            dc.InsertBatch(departments);

            var courses = new List<Course>
            {
                new Course {CourseID = 1050, Title = "Chemistry",      Credits = 3,
                  DepartmentId = departments.Single( s => s.Name == "Engineering").Id,
                  InstructorIdList = new List<ObjectId>(),
                  EnrollmentIdList = new List<ObjectId>()
                },
                new Course {CourseID = 4022, Title = "Microeconomics", Credits = 3,
                  DepartmentId = departments.Single( s => s.Name == "Economics").Id,
                  InstructorIdList = new List<ObjectId>(),
                  EnrollmentIdList = new List<ObjectId>()
                },
                new Course {CourseID = 4041, Title = "Macroeconomics", Credits = 3,
                  DepartmentId = departments.Single( s => s.Name == "Economics").Id,
                  InstructorIdList = new List<ObjectId>(),
                  EnrollmentIdList = new List<ObjectId>()
                },
                new Course {CourseID = 1045, Title = "Calculus",       Credits = 4,
                  DepartmentId = departments.Single( s => s.Name == "Mathematics").Id,
                  InstructorIdList = new List<ObjectId>(),
                  EnrollmentIdList = new List<ObjectId>()
                },
                new Course {CourseID = 3141, Title = "Trigonometry",   Credits = 4,
                  DepartmentId = departments.Single( s => s.Name == "Mathematics").Id,
                  InstructorIdList = new List<ObjectId>(),
                  EnrollmentIdList = new List<ObjectId>()
                },
                new Course {CourseID = 2021, Title = "Composition",    Credits = 3,
                  DepartmentId = departments.Single( s => s.Name == "English").Id,
                  InstructorIdList = new List<ObjectId>(),
                  EnrollmentIdList = new List<ObjectId>()
                },
                new Course {CourseID = 2042, Title = "Literature",     Credits = 4,
                  DepartmentId = departments.Single( s => s.Name == "English").Id,
                  InstructorIdList = new List<ObjectId>(),
                  EnrollmentIdList = new List<ObjectId>()
                },
            };

            var cc = Course.GetCollection();
            cc.InsertBatch(courses);

            UpdateDepartment();

            var officeAssignments = new List<OfficeAssignment>
            {
                new OfficeAssignment { 
                    PersonId = instructors.Single( i => i.LastName == "Fakhouri").Id, 
                    Location = "Smith 17" },
                new OfficeAssignment { 
                    PersonId = instructors.Single( i => i.LastName == "Harui").Id, 
                    Location = "Gowan 27" },
                new OfficeAssignment { 
                    PersonId = instructors.Single( i => i.LastName == "Kapoor").Id, 
                    Location = "Thompson 304" },
            };

            var oc = OfficeAssignment.GetCollection();
            oc.InsertBatch(officeAssignments);

            AddOrUpdateInstructor("Chemistry", "Kapoor");
            AddOrUpdateInstructor("Chemistry", "Harui");
            AddOrUpdateInstructor("Microeconomics", "Zheng");
            AddOrUpdateInstructor("Macroeconomics", "Zheng");

            AddOrUpdateInstructor("Calculus", "Fakhouri");
            AddOrUpdateInstructor("Trigonometry", "Harui");
            AddOrUpdateInstructor("Composition", "Abercrombie");
            AddOrUpdateInstructor("Literature", "Abercrombie");

            var enrollments = new List<Enrollment>
            {
                new Enrollment { 
                    PersonId = students.Single(s => s.LastName == "Alexander").Id, 
                    CourseId = courses.Single(c => c.Title == "Chemistry" ).Id, 
                    Grade = Grade.A 
                },
                 new Enrollment { 
                    PersonId = students.Single(s => s.LastName == "Alexander").Id,
                    CourseId = courses.Single(c => c.Title == "Microeconomics" ).Id, 
                    Grade = Grade.C 
                 },                            
                 new Enrollment { 
                    PersonId = students.Single(s => s.LastName == "Alexander").Id,
                    CourseId = courses.Single(c => c.Title == "Macroeconomics" ).Id, 
                    Grade = Grade.B
                 },
                 new Enrollment { 
                     PersonId = students.Single(s => s.LastName == "Alonso").Id,
                    CourseId = courses.Single(c => c.Title == "Calculus" ).Id, 
                    Grade = Grade.B 
                 },
                 new Enrollment { 
                     PersonId = students.Single(s => s.LastName == "Alonso").Id,
                    CourseId = courses.Single(c => c.Title == "Trigonometry" ).Id, 
                    Grade = Grade.B 
                 },
                 new Enrollment {
                    PersonId = students.Single(s => s.LastName == "Alonso").Id,
                    CourseId = courses.Single(c => c.Title == "Composition" ).Id, 
                    Grade = Grade.B 
                 },
                 new Enrollment { 
                    PersonId = students.Single(s => s.LastName == "Anand").Id,
                    CourseId = courses.Single(c => c.Title == "Chemistry" ).Id
                 },
                 new Enrollment { 
                    PersonId = students.Single(s => s.LastName == "Anand").Id,
                    CourseId = courses.Single(c => c.Title == "Microeconomics").Id,
                    Grade = Grade.B         
                 },
                new Enrollment { 
                    PersonId = students.Single(s => s.LastName == "Barzdukas").Id,
                    CourseId = courses.Single(c => c.Title == "Chemistry").Id,
                    Grade = Grade.B         
                 },
                 new Enrollment { 
                    PersonId = students.Single(s => s.LastName == "Li").Id,
                    CourseId = courses.Single(c => c.Title == "Composition").Id,
                    Grade = Grade.B         
                 },
                 new Enrollment { 
                    PersonId = students.Single(s => s.LastName == "Justice").Id,
                    CourseId = courses.Single(c => c.Title == "Literature").Id,
                    Grade = Grade.B         
                 }
            };

            var ec = Enrollment.GetCollection();

            foreach (Enrollment e in enrollments)
            {
                var qe = Query<Enrollment>.Where(x => x.PersonId == e.PersonId &&
                    x.CourseId == e.CourseId);
                var enrollmentInDataBase = ec.FindOne(qe);

                if (enrollmentInDataBase == null)
                {
                    ec.Insert(e);
                    var qs = Query<Student>.EQ(x => x.Id, e.PersonId);
                    var student = sc.FindOne(qs);
                    student.EnrollmentIdList.Add(e.Id);
                    var qc = Query<Course>.EQ(x => x.Id, e.CourseId);
                    var course = cc.FindOne(qc);
                    course.EnrollmentIdList.Add(e.Id);
                    sc.Save(student);
                    cc.Save(course);
                }
            }

            return View();
        }

        private void AddOrUpdateInstructor(string courseTitle, string instructorName)
        {
            var instructors = Instructor.GetCollection();
            var qi = Query<Instructor>.EQ(x => x.LastName, instructorName);
            var ri = instructors.FindOne(qi);

            var courses = Course.GetCollection();
            var qc = Query<Course>.Where(x => x.Title == courseTitle && x.InstructorIdList.Contains(ri.Id));
            var rc = courses.FindOne(qc);

            var qa = Query<Course>.Where(x => x.Title == courseTitle);
            var ra = courses.FindOne(qa);

            var qb = Query<Instructor>.Where(x => x.LastName == instructorName && x.CourseIdList.Contains(ra.Id));
            var rb = instructors.FindOne(qb);

            if (rc == null)
            {
                ra.InstructorIdList.Add(ri.Id);
                courses.Save(ra);
            }

            if (rb == null)
            {
                ri.CourseIdList.Add(ra.Id);
                instructors.Save(ri);
            }
        }

        private void UpdateDepartment()
        {
            var departments = Department.GetCollection();
            var courses = Course.GetCollection();
            var l = courses.FindAll().ToList();

            foreach (Course c in l)
            {
                var q = Query<Department>.EQ(x => x.Id, c.DepartmentId);
                var d = departments.FindOne(q);
                if (!d.CourseIdList.Contains(c.Id))
                {
                    d.CourseIdList.Add(c.Id);
                    departments.Save(d);
                }
            }
        }

        private void ClearAll()
        {
            Student.GetCollection().RemoveAll();
            Instructor.GetCollection().RemoveAll();
            Course.GetCollection().RemoveAll();
            Department.GetCollection().RemoveAll();
            Enrollment.GetCollection().RemoveAll();
        }
	}
}