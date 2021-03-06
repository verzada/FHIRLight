﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using System.Xml.Linq;
using Hl7.Fhir.Model;

namespace FhirStarter.Bonfire.Filters
{
    public class ExceptionFilter: AbstractExceptionFilter
    {
        public ExceptionFilter()
        {
        }

        protected override Resource GetOperationOutCome(Exception exception)
        {
            LogError(exception);

            var operationOutCome = new OperationOutcome {Issue = new List<OperationOutcome.IssueComponent>()};
            var issue = new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Fatal,
                Code = OperationOutcome.IssueType.NotFound,
                Details = new CodeableConcept("StandardException", exception.GetType().ToString(), exception.Message),
            };
            if (exception.InnerException != null)
            {
                issue.Diagnostics = exception.InnerException.Message;
            }

            var responseIssue = CheckForHttpResponseException(exception);
            if(responseIssue != null)
                 operationOutCome.Issue.Add(responseIssue);

            operationOutCome.Issue.Add(issue);
            return operationOutCome;
        }

        private static OperationOutcome.IssueComponent CheckForHttpResponseException(Exception exception)
        {
            OperationOutcome.IssueComponent responseIssue = null;
            if (exception.GetType().ToString().Contains(nameof(HttpResponseException)))
            {
                var responseException = (HttpResponseException) exception;

                if (responseException.Response != null)
                {
                    responseIssue = new OperationOutcome.IssueComponent
                    {
                        Severity = OperationOutcome.IssueSeverity.Fatal,
                        Code = OperationOutcome.IssueType.Exception,
                        Details =
                            new CodeableConcept("Response", exception.GetType().ToString(),
                                responseException.Response.ReasonPhrase)
                    };
                }
            }
            return responseIssue;
        }

        // todo add support for post (needs examples for it)
        private void LogError(Exception exception)
        {
            var url = GetAbsolutePathRequest();
            // var post = GetPost();
            // var exceptionXml = XmlConverter.CreateXmlDocumentFromObject(exception);
            if (!string.IsNullOrEmpty(url))
            {
                var newXDoc = new XDocument();
                var root = new XElement("Error");

                var contentElement = new XElement("RequestUrl", url);
                root.Add(contentElement);
                newXDoc.Add(root);

            }
        }

        private static string GetAbsolutePathRequest()
        {
            var httpRequest = HttpContext.Current;
            var absolutePath = string.Empty;
            if (httpRequest != null)
            {
                absolutePath = httpRequest.Request.Url.ToString();
            }
            return absolutePath;
        }

        protected override Type GetExceptionType()
        {
            return typeof (Exception);
        }
    }
}