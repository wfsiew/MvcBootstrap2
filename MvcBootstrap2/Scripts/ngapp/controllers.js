﻿'use strict';

/* Controllers */

function PageCtrl($scope, Page, Menu) {
    $scope.Page = Page;
    $scope.Menu = Menu;
}

function IndexCtrl($scope, Page, Menu) {
    Page.setTitle('Index');
    Menu.setMenu('home');
}

function AboutCtrl($scope, $http, Page, Menu) {
    Page.setTitle('Student Body Statistics');
    Menu.setMenu('about');

    $http.get('/Ng/About').success(function (data) {
        $scope.model = data;
    });
}

// region students
function StudentCtrl($scope, $http, Page, Menu) {
    Page.setTitle('Students');
    Menu.setMenu('students');

    $scope.selected = {
        all: false,
        count: 0,
        message: function () {
            return this.count + " item" + (this.count > 1 ? 's' : '') + " selected";
        },
        reset: function () {
            this.all = false;
            this.count = 0;
        }
    };

    if (Page.message().show) {
        $scope.message = _.clone(Page.message());
        Page.resetMessage();
    }

    $scope.find = function () {
        $scope.gotoPage(1);
    }

    $scope.gotoPage = function (page) {
        var params = {
            SearchString: $scope.SearchString,
            sortOrder: $scope.currentSort,
            page: page
        };
        $http.get('/Ng/Student/Index', { params: params }).success(function (data) {
            $scope.pager = data.pager;
            $scope.model = data.model;
        });
    }

    $scope.sort = function (a) {
        if (a == 'Name') {
            if ($scope.currentSort == null || $scope.currentSort == '')
                $scope.currentSort = 'Name_desc';

            else
                $scope.currentSort = '';
        }

        else if (a == 'FirstName') {
            if ($scope.currentSort == 'FirstName')
                $scope.currentSort = 'FirstName_desc';

            else
                $scope.currentSort = 'FirstName';
        }

        else if (a == 'Date') {
            if ($scope.currentSort == 'Date')
                $scope.currentSort = 'Date_desc';

            else
                $scope.currentSort = 'Date';
        }

        $scope.gotoPage($scope.pager.PageNum);
    }

    $scope.getSortCss = function (a) {
        var up = 'icon-chevron-up icon-white';
        var down = 'icon-chevron-down icon-white';

        if (($scope.currentSort == null || $scope.currentSort == '') && a == 'Name')
            return up;

        if ($scope.currentSort.indexOf(a) == 0) {
            if ($scope.currentSort.indexOf('desc') > 0)
                return down;

            else
                return up;
        }

        return null;
    }

    $scope.selectRow = function ($event, o) {
        $event.stopPropagation();

        if (o.selected)
            ++$scope.selected.count;

        else
            --$scope.selected.count;
    }

    $scope.selectAll = function ($event) {
        $event.stopPropagation();

        var list = null;
        var n = 0;

        if ($scope.model != null)
            list = $scope.model;

        if (list != null)
            n = list.length;

        for (var i = 0; i < n; i++) {
            var o = list[i];
            o.selected = $scope.selected.all;
        }

        if ($scope.selected.all)
            $scope.selected.count = n;

        else
            $scope.selected.count = 0;
    }

    $scope.removeItems = function () {
        if ($scope.selected.count < 1)
            return;

        var list = $scope.model;
        var lx = _.where(list, { selected: true });
        var ids = _.map(lx, function (o) {
            return o.PersonID;
        });
        $http.post('/Ng/Student/Delete', { ids: ids }).success(function (data) {
            if (data.success == 1) {
                Page.setMessage(data.message);
                $scope.message = _.clone(Page.message());
                Page.resetMessage();
                $scope.selected.reset();
                $scope.gotoPage($scope.pager.PageNum);
            }

            else if (data.error == 1) {
                $scope.error = true;
                $scope.errorText = data.message;
            }
        });
    }

    $scope.removeItem = function (o) {
        var ids = [o.PersonID];
        $http.post('/Ng/Student/Delete', { ids: ids }).success(function (data) {
            if (data.success == 1) {
                Page.setMessage(data.message);
                $scope.message = _.clone(Page.message());
                Page.resetMessage();
                $scope.selected.reset();
                $scope.gotoPage($scope.pager.PageNum);
            }

            else if (data.error == 1) {
                $scope.error = true;
                $scope.errorText = data.message;
            }
        });
    }

    $scope.dismissAlert = function () {
        $scope.error = false;
    }

    $scope.gotoPage(1);
}

function StudentCreateCtrl($scope, $http, $timeout, Page, Menu) {
    Page.setTitle('Create');
    Menu.setMenu('students');

    $scope.title = 'Create';
    $scope.action = 'Create';

    $scope.save = function () {
        var o = {
            LastName: $scope.model.LastName,
            FirstMidName: $scope.model.FirstMidName,
            EnrollmentDate: utils.getDateStr($scope.model.EnrollmentDate)
        };

        $http.post('/Ng/Student/Create', o).success(function (data) {
            if (data.success == 1) {
                Page.setMessage(data.message);
                window.location.href = '#/students';
            }

            else if (data.error == 1) {
                $scope.error = true;
                $scope.errorText = data.message;
            }
        });
    }

    $scope.open = function () {
        $timeout(function () {
            $scope.opened = true;
        });
    }

    $scope.dismissAlert = function () {
        $scope.error = false;
    }
}

function StudentEditCtrl($scope, $http, $routeParams, $timeout, Page, Menu) {
    Page.setTitle('Edit');
    Menu.setMenu('students');

    $scope.title = 'Edit';
    $scope.action = 'Save';

    $http.get('/Ng/Student/Edit/' + $routeParams.id).success(function (data) {
        $scope.model = data;
        $scope.model.EnrollmentDate = utils.getDate(data.EnrollmentDate);
    });

    $scope.save = function () {
        var o = {
            Id: $scope.model.Id,
            LastName: $scope.model.LastName,
            FirstMidName: $scope.model.FirstMidName,
            EnrollmentDate: utils.getDateStr($scope.model.EnrollmentDate)
        };

        $http.post('/Ng/Student/Edit', o).success(function (data) {
            if (data.success == 1) {
                Page.setMessage(data.message);
                window.location.href = '#/students';
            }

            else if (data.error == 1) {
                $scope.error = true;
                $scope.errorText = data.message;
            }
        });
    }

    $scope.open = function () {
        $timeout(function () {
            $scope.opened = true;
        });
    }

    $scope.dismissAlert = function () {
        $scope.error = false;
    }
}

function StudentDetailsCtrl($scope, $http, $routeParams, Page, Menu) {
    Page.setTitle('Details');
    Menu.setMenu('students');

    $http.get('/Ng/Student/Details/' + $routeParams.id).success(function (data) {
        $scope.model = data.model;
        $scope.enrollments = data.enrollments;
    });
}
// endregion students

// region courses
function CourseCtrl($scope, $http, Page, Menu) {
    Page.setTitle('Courses');
    Menu.setMenu('courses');

    $scope.selected = {
        all: false,
        count: 0,
        message: function () {
            return this.count + " item" + (this.count > 1 ? 's' : '') + " selected";
        },
        reset: function () {
            this.all = false;
            this.count = 0;
        }
    };

    if (Page.message().show) {
        $scope.message = _.clone(Page.message());
        Page.resetMessage();
    }

    $scope.find = function () {
        $scope.gotoPage(1);
    }

    $scope.gotoPage = function (page) {
        var params = {
            SearchString: $scope.SearchString,
            sortOrder: $scope.currentSort,
            page: page
        };
        $http.get('/Ng/Course/Index', { params: params }).success(function (data) {
            $scope.pager = data.pager;
            $scope.model = data.model;
        });
    }

    $scope.sort = function (a) {
        if (a == 'Title') {
            if ($scope.currentSort == null || $scope.currentSort == '')
                $scope.currentSort = 'Title_desc';

            else
                $scope.currentSort = '';
        }

        else if (a == 'Dept') {
            if ($scope.currentSort == 'Dept')
                $scope.currentSort = 'Dept_desc';

            else
                $scope.currentSort = 'Dept';
        }

        else if (a == 'CourseID') {
            if ($scope.currentSort == 'CourseID')
                $scope.currentSort = 'CourseID_desc';

            else
                $scope.currentSort = 'CourseID';
        }

        else if (a == 'Credits') {
            if ($scope.currentSort == 'Credits')
                $scope.currentSort = 'Credits_desc';

            else
                $scope.currentSort = 'Credits';
        }

        $scope.gotoPage($scope.pager.PageNum);
    }

    $scope.getSortCss = function (a) {
        var up = 'icon-chevron-up icon-white';
        var down = 'icon-chevron-down icon-white';

        if (($scope.currentSort == null || $scope.currentSort == '') && a == 'Name')
            return up;

        if ($scope.currentSort.indexOf(a) == 0) {
            if ($scope.currentSort.indexOf('desc') > 0)
                return down;

            else
                return up;
        }

        return null;
    }

    $scope.selectRow = function ($event, o) {
        $event.stopPropagation();

        if (o.selected)
            ++$scope.selected.count;

        else
            --$scope.selected.count;
    }

    $scope.selectAll = function ($event) {
        $event.stopPropagation();

        var list = null;
        var n = 0;

        if ($scope.model != null)
            list = $scope.model;

        if (list != null)
            n = list.length;

        for (var i = 0; i < n; i++) {
            var o = list[i];
            o.selected = $scope.selected.all;
        }

        if ($scope.selected.all)
            $scope.selected.count = n;

        else
            $scope.selected.count = 0;
    }

    $scope.removeItems = function () {
        if ($scope.selected.count < 1)
            return;

        var list = $scope.model;
        var lx = _.where(list, { selected: true });
        var ids = _.map(lx, function (o) {
            return o.Id;
        });
        $http.post('/Ng/Course/Delete', { ids: ids }).success(function (data) {
            if (data.success == 1) {
                Page.setMessage(data.message);
                $scope.message = _.clone(Page.message());
                Page.resetMessage();
                $scope.selected.reset();
                $scope.gotoPage($scope.pager.PageNum);
            }

            else if (data.error == 1) {
                $scope.error = true;
                $scope.errorText = data.message;
            }
        });
    }

    $scope.removeItem = function (o) {
        var ids = [o.Id];
        $http.post('/Ng/Course/Delete', { ids: ids }).success(function (data) {
            if (data.success == 1) {
                Page.setMessage(data.message);
                $scope.message = _.clone(Page.message());
                Page.resetMessage();
                $scope.selected.reset();
                $scope.gotoPage($scope.pager.PageNum);
            }

            else if (data.error == 1) {
                $scope.error = true;
                $scope.errorText = data.message;
            }
        });
    }

    $scope.dismissAlert = function () {
        $scope.error = false;
    }

    $scope.gotoPage(1);
}

function CourseCreateCtrl($scope, $http, Page, Menu) {
    Page.setTitle('Create');
    Menu.setMenu('courses');

    $scope.title = 'Create';
    $scope.action = 'Create';

    $scope.save = function () {
        var o = {
            CourseID: $scope.model.CourseID,
            Title: $scope.model.Title,
            Credits: $scope.model.Credits,
            DepartmentId: $scope.model.DepartmentId
        };

        $http.post('/Ng/Course/Create', o).success(function (data) {
            if (data.success == 1) {
                Page.setMessage(data.message);
                window.location.href = '#/courses';
            }

            else if (data.error == 1) {
                $scope.error = true;
                $scope.errorText = data.message;
            }
        });
    }

    $scope.dismissAlert = function () {
        $scope.error = false;
    }

    $http.get('/Ng/Course/Departments').success(function (data) {
        $scope.DepartmentIdList = data;
    });
}

function CourseEditCtrl($scope, $http, $routeParams, $timeout, Page, Menu) {
    Page.setTitle('Edit');
    Menu.setMenu('courses');

    $scope.title = 'Edit';
    $scope.action = 'Save';

    $http.get('/Ng/Course/Edit/' + $routeParams.id).success(function (data) {
        $scope.model = data;
        $scope.model.StartDate = utils.getDate(data.StartDate);
        $scope.DepartmentIdList = data.DepartmentIdList;
    });

    $scope.save = function () {
        var o = {
            CourseID: $scope.model.CourseID,
            Title: $scope.model.Title,
            Credits: $scope.model.Credits,
            DepartmentId: $scope.model.DepartmentId
        };

        $http.post('/Ng/Course/Edit', o).success(function (data) {
            if (data.success == 1) {
                Page.setMessage(data.message);
                window.location.href = '#/courses';
            }

            else if (data.error == 1) {
                $scope.error = true;
                $scope.errorText = data.message;
            }
        });
    }

    $scope.dismissAlert = function () {
        $scope.error = false;
    }
}

function CourseDetailsCtrl($scope, $http, $routeParams, Page, Menu) {
    Page.setTitle('Details');
    Menu.setMenu('courses');

    $http.get('/Ng/Course/Details/' + $routeParams.id).success(function (data) {
        $scope.model = data.model;
    });
}
// endregion courses

// region instructors
function InstructorCtrl($scope, $http, $timeout, Page, Menu) {
    Page.setTitle('Instructors');
    Menu.setMenu('instructors');

    $scope.selected = {
        all: false,
        count: 0,
        message: function () {
            return this.count + " item" + (this.count > 1 ? 's' : '') + " selected";
        },
        reset: function () {
            this.all = false;
            this.count = 0;
        }
    };

    $scope.instructor = null;
    $scope.course = null;

    if (Page.message().show) {
        $scope.message = _.clone(Page.message());
        Page.resetMessage();
    }

    $scope.find = function () {
        $scope.gotoPage(1);
    }

    $scope.gotoPage = function (page) {
        var params = {
            SearchString: $scope.SearchString,
            sortOrder: $scope.currentSort,
            page: page
        };
        $http.get('/Ng/Instructor/Index', { params: params }).success(function (data) {
            $scope.pager = data.pager;
            $scope.model = data.model;
        });
    }

    $scope.sort = function (a) {
        if (a == 'Name') {
            if ($scope.currentSort == null || $scope.currentSort == '')
                $scope.currentSort = 'Name_desc';

            else
                $scope.currentSort = '';
        }

        else if (a == 'FirstName') {
            if ($scope.currentSort == 'FirstName')
                $scope.currentSort = 'FirstName_desc';

            else
                $scope.currentSort = 'FirstName';
        }

        else if (a == 'Date') {
            if ($scope.currentSort == 'Date')
                $scope.currentSort = 'Date_desc';

            else
                $scope.currentSort = 'Date';
        }

        else if (a == 'Loc') {
            if ($scope.currentSort == 'Loc')
                $scope.currentSort = 'Loc_desc';

            else
                $scope.currentSort = 'Loc';
        }

        $scope.gotoPage($scope.pager.PageNum);
    }

    $scope.getSortCss = function (a) {
        var up = 'icon-chevron-up icon-white';
        var down = 'icon-chevron-down icon-white';

        if (($scope.currentSort == null || $scope.currentSort == '') && a == 'Name')
            return up;

        if ($scope.currentSort.indexOf(a) == 0) {
            if ($scope.currentSort.indexOf('desc') > 0)
                return down;

            else
                return up;
        }

        return null;
    }

    $scope.selectRow = function ($event, o) {
        $event.stopPropagation();

        if (o.selected)
            ++$scope.selected.count;

        else
            --$scope.selected.count;
    }

    $scope.selectAll = function ($event) {
        $event.stopPropagation();

        var list = null;
        var n = 0;

        if ($scope.model != null)
            list = $scope.model;

        if (list != null)
            n = list.length;

        for (var i = 0; i < n; i++) {
            var o = list[i];
            o.selected = $scope.selected.all;
        }

        if ($scope.selected.all)
            $scope.selected.count = n;

        else
            $scope.selected.count = 0;
    }

    $scope.removeItems = function () {
        if ($scope.selected.count < 1)
            return;

        var list = $scope.model;
        var lx = _.where(list, { selected: true });
        var ids = _.map(lx, function (o) {
            return o.Id;
        });
        $http.post('/Ng/Instructor/Delete', { ids: ids }).success(function (data) {
            if (data.success == 1) {
                Page.setMessage(data.message);
                $scope.message = _.clone(Page.message());
                Page.resetMessage();
                $scope.selected.reset();
                $scope.gotoPage($scope.pager.PageNum);
            }

            else if (data.error == 1) {
                $scope.error = true;
                $scope.errorText = data.message;
            }
        });
    }

    $scope.removeItem = function (o) {
        var ids = [o.Id];
        $http.post('/Ng/Instructor/Delete', { ids: ids }).success(function (data) {
            if (data.success == 1) {
                Page.setMessage(data.message);
                $scope.message = _.clone(Page.message());
                Page.resetMessage();
                $scope.selected.reset();
                $scope.gotoPage($scope.pager.PageNum);
            }

            else if (data.error == 1) {
                $scope.error = true;
                $scope.errorText = data.message;
            }
        });
    }

    $scope.selectInstructor = function (o) {
        $http.get('/Ng/Instructor/Courses', { params: { id: o.Id } }).success(function (data) {
            $scope.enrollments = null;
            if ($scope.instructor != null)
                $scope.instructor.select = false;

            o.select = true;
            $scope.instructor = o;
            $scope.courses = data;
        });
    }

    $scope.selectCourse = function (x) {
        $http.get('/Ng/Instructor/Enrollments', { params: { id: x.Id, courseID: x.Id } }).success(function(data) {
            if ($scope.course != null)
                $scope.course.select = false;

            x.select = true;
            $scope.course = x;
            $scope.enrollments = data;
        });
    }

    $scope.dismissAlert = function () {
        $scope.error = false;
    }

    $scope.gotoPage(1);
}

function InstructorCreateCtrl($scope, $http, Page, Menu) {
    Page.setTitle('Create');
    Menu.setMenu('instructors');

    $scope.title = 'Create';
    $scope.action = 'Create';

    $scope.save = function () {
        var _selectedCourses = _.where($scope.Courses, { Assigned: true });
        var selectedCourses = _.map(_selectedCourses, function (o) {
            return o.CourseId;
        });
        var o = {
            LastName: $scope.model.LastName,
            FirstMidName: $scope.model.FirstMidName,
            HireDate: utils.getDateStr($scope.model.HireDate),
            'OfficeAssignment.Location': $scope.model.OfficeAssignment.Location,
            selectedCourses: selectedCourses
        };

        $http.post('/Ng/Instructor/Create', o).success(function (data) {
            if (data.success == 1) {
                Page.setMessage(data.message);
                window.location.href = '#/instructors';
            }

            else if (data.error == 1) {
                $scope.error = true;
                $scope.errorText = data.message;
            }
        });
    }

    $scope.open = function () {
        $timeout(function () {
            $scope.opened = true;
        });
    }

    $scope.dismissAlert = function () {
        $scope.error = false;
    }

    $http.get('/Ng/Instructor/AllCourses').success(function (data) {
        $scope.Courses = data;
    });
}

function InstructorEditCtrl($scope, $http, $routeParams, $timeout, Page, Menu) {
    Page.setTitle('Edit');
    Menu.setMenu('instructors');

    $scope.title = 'Edit';
    $scope.action = 'Save';

    $http.get('/Ng/Instructor/Edit/' + $routeParams.id).success(function (data) {
        $scope.model = data;
        $scope.model.HireDate = utils.getDate(data.HireDate);
        $scope.Courses = data.Courses;
    });

    $scope.save = function () {
        var _selectedCourses = _.where($scope.Courses, { Assigned: true });
        var selectedCourses = _.map(_selectedCourses, function (o) {
            return o.CourseId;
        });
        var o = {
            LastName: $scope.model.LastName,
            FirstMidName: $scope.model.FirstMidName,
            HireDate: utils.getDateStr($scope.model.HireDate),
            'OfficeAssignment.Location': $scope.model.OfficeAssignment.Location,
            selectedCourses: selectedCourses,
            Id: $scope.model.Id
        };

        $http.post('/Ng/Instructor/Edit/' + $routeParams.id, o).success(function (data) {
            if (data.success == 1) {
                Page.setMessage(data.message);
                window.location.href = '#/instructors';
            }

            else if (data.error == 1) {
                $scope.error = true;
                $scope.errorText = data.message;
            }
        });
    }

    $scope.open = function () {
        $timeout(function () {
            $scope.opened = true;
        });
    }

    $scope.dismissAlert = function () {
        $scope.error = false;
    }
}

function InstructorDetailsCtrl($scope, $http, $routeParams, Page, Menu) {
    Page.setTitle('Details');
    Menu.setMenu('instructors');

    $http.get('/Ng/Instructor/Details/' + $routeParams.id).success(function (data) {
        $scope.model = data.model;
    });
}
// endregion instructors

// region departments
function DepartmentCtrl($scope, $http, Page, Menu) {
    Page.setTitle('Departments');
    Menu.setMenu('departments');

    $scope.selected = {
        all: false,
        count: 0,
        message: function () {
            return this.count + " item" + (this.count > 1 ? 's' : '') + " selected";
        },
        reset: function () {
            this.all = false;
            this.count = 0;
        }
    };

    if (Page.message().show) {
        $scope.message = _.clone(Page.message());
        Page.resetMessage();
    }

    $scope.find = function () {
        $scope.gotoPage(1);
    }

    $scope.gotoPage = function (page) {
        var params = {
            SearchString: $scope.SearchString,
            sortOrder: $scope.currentSort,
            page: page
        };
        $http.get('/Ng/Department/Index', { params: params }).success(function (data) {
            $scope.pager = data.pager;
            $scope.model = data.model;
        });
    }

    $scope.sort = function (a) {
        if (a == 'Name') {
            if ($scope.currentSort == null || $scope.currentSort == '')
                $scope.currentSort = 'Name_desc';

            else
                $scope.currentSort = '';
        }

        else if (a == 'Budget') {
            if ($scope.currentSort == 'Budget')
                $scope.currentSort = 'Budget_desc';

            else
                $scope.currentSort = 'Budget';
        }

        else if (a == 'Date') {
            if ($scope.currentSort == 'Date')
                $scope.currentSort = 'Date_desc';

            else
                $scope.currentSort = 'Date';
        }

        else if (a == 'Admin') {
            if ($scope.currentSort == 'Admin')
                $scope.currentSort = 'Admin_desc';

            else
                $scope.currentSort = 'Admin';
        }

        $scope.gotoPage($scope.pager.PageNum);
    }

    $scope.getSortCss = function (a) {
        var up = 'icon-chevron-up icon-white';
        var down = 'icon-chevron-down icon-white';

        if (($scope.currentSort == null || $scope.currentSort == '') && a == 'Name')
            return up;

        if ($scope.currentSort.indexOf(a) == 0) {
            if ($scope.currentSort.indexOf('desc') > 0)
                return down;

            else
                return up;
        }

        return null;
    }

    $scope.selectRow = function ($event, o) {
        $event.stopPropagation();

        if (o.selected)
            ++$scope.selected.count;

        else
            --$scope.selected.count;
    }

    $scope.selectAll = function ($event) {
        $event.stopPropagation();

        var list = null;
        var n = 0;

        if ($scope.model != null)
            list = $scope.model;

        if (list != null)
            n = list.length;

        for (var i = 0; i < n; i++) {
            var o = list[i];
            o.selected = $scope.selected.all;
        }

        if ($scope.selected.all)
            $scope.selected.count = n;

        else
            $scope.selected.count = 0;
    }

    $scope.removeItems = function () {
        if ($scope.selected.count < 1)
            return;

        var list = $scope.model;
        var lx = _.where(list, { selected: true });
        var departments = _.map(lx, function (o) {
            return { DepartmentId: o.DepartmentId, RowVersion: o.RowVersion };
        });
        $http.post('/Ng/Department/Delete', { departments: departments }).success(function (data) {
            if (data.success == 1) {
                Page.setMessage(data.message);
                $scope.message = _.clone(Page.message());
                Page.resetMessage();
                $scope.selected.reset();
                $scope.gotoPage($scope.pager.PageNum);
            }

            else if (data.error == 1) {
                $scope.error = true;
                $scope.errorText = data.message;
            }
        });
    }

    $scope.removeItem = function (o) {
        var departments = [{ DepartmentId: o.DepartmentId, RowVersion: o.RowVersion }];
        $http.post('/Ng/Department/Delete', { departments: departments }).success(function (data) {
            if (data.success == 1) {
                Page.setMessage(data.message);
                $scope.message = _.clone(Page.message());
                Page.resetMessage();
                $scope.selected.reset();
                $scope.gotoPage($scope.pager.PageNum);
            }

            else if (data.error == 1) {
                $scope.error = true;
                $scope.errorText = data.message;
            }
        });
    }

    $scope.dismissAlert = function () {
        $scope.error = false;
    }

    $scope.gotoPage(1);
}

function DepartmentCreateCtrl($scope, $http, $timeout, Page, Menu) {
    Page.setTitle('Create');
    Menu.setMenu('departments');

    $scope.title = 'Create';
    $scope.action = 'Create';

    $scope.save = function () {
        var o = {
            Name: $scope.model.Name,
            Budget: $scope.model.Budget,
            StartDate: utils.getDateStr($scope.model.StartDate),
            PersonID: $scope.model.PersonID
        };

        $http.post('/Ng/Department/Create', o).success(function (data) {
            if (data.success == 1) {
                Page.setMessage(data.message);
                window.location.href = '#/departments';
            }

            else if (data.error == 1) {
                $scope.error = true;
                $scope.errorText = data.message;
            }
        });
    }

    $scope.open = function () {
        $timeout(function () {
            $scope.opened = true;
        });
    }

    $scope.dismissAlert = function () {
        $scope.error = false;
    }

    $http.get('/Ng/Department/Instructors').success(function (data) {
        $scope.PersonIDList = data;
    });
}

function DepartmentEditCtrl($scope, $http, $routeParams, $timeout, Page, Menu) {
    Page.setTitle('Edit');
    Menu.setMenu('departments');

    $scope.title = 'Edit';
    $scope.action = 'Save';

    $http.get('/Ng/Department/Edit/' + $routeParams.id).success(function (data) {
        $scope.model = data;
        $scope.model.StartDate = utils.getDate(data.StartDate);
        $scope.PersonIDList = data.PersonIDList;
    });

    $scope.save = function () {
        var o = {
            DepartmentID: $scope.model.DepartmentID,
            RowVersion: $scope.model.RowVersion,
            Name: $scope.model.Name,
            Budget: $scope.model.Budget,
            StartDate: utils.getDateStr($scope.model.StartDate),
            PersonID: $scope.model.PersonID
        };

        $http.post('/Ng/Department/Edit', o).success(function (data) {
            if (data.success == 1) {
                Page.setMessage(data.message);
                window.location.href = '#/departments';
            }

            else if (data.error == 1) {
                $scope.error = true;
                $scope.errorText = data.message;
            }
        });
    }

    $scope.open = function () {
        $timeout(function () {
            $scope.opened = true;
        });
    }

    $scope.dismissAlert = function () {
        $scope.error = false;
    }
}

function DepartmentDetailsCtrl($scope, $http, $routeParams, Page, Menu) {
    Page.setTitle('Details');
    Menu.setMenu('departments');

    $http.get('/Ng/Department/Details/' + $routeParams.id).success(function (data) {
        $scope.model = data.model;
    });
}
