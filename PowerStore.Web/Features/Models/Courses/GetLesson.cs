﻿using PowerStore.Domain.Courses;
using PowerStore.Domain.Customers;
using PowerStore.Domain.Localization;
using PowerStore.Web.Models.Course;
using MediatR;

namespace PowerStore.Web.Features.Models.Courses
{
    public class GetLesson : IRequest<LessonModel>
    {
        public Customer Customer { get; set; }
        public Language Language { get; set; }
        public Course Course { get; set; }
        public CourseLesson Lesson { get; set; }
    }
}
