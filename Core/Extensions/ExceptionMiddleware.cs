﻿using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public class ExceptionMiddleware
    {
        private RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception e)
            {
                await HandleExceptionAsync(httpContext, e);
            }
        }

        private Task HandleExceptionAsync(HttpContext httpContext, Exception e)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            string message = "Internal Server Error";
            IEnumerable<ValidationFailure> errors;//hatalarımızı IEnumerable şeklinde validationfailure tipinde tutuyoruz error details sayfasından bulduk
            if (e.GetType() == typeof(ValidationException)) //bu kod bizim validation hatası varsa çalışır biz bunları angularda nasıl göstermek isstiyorsak öyle ayaralyacağız
            {
                message = e.Message;
                errors = ((ValidationException)e).Errors;
                httpContext.Response.StatusCode = 400;
                return httpContext.Response.WriteAsync(new ValidationErrorDetails
                {
                    StatusCode = 400,
                    Message = message,
                    Errors = errors
                }.ToString());

            }

            return httpContext.Response.WriteAsync(new ErrorDetails   //bu kod sistemsel bir hata varsa çalışır
            {
                StatusCode = httpContext.Response.StatusCode,
                Message = message
            }.ToString());
        }
    }
}
