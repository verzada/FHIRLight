﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using FhirStarter.Bonfire.Filters;
using FhirStarter.Bonfire.Interface;
using FhirStarter.Bonfire.Spark.Engine.Core;
using FhirStarter.Bonfire.Spark.Engine.Infrastructure;
using FhirStarter.Bonfire.Spark.Engine.Extensions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace FhirStarter.Bonfire.Controllers
{
    [RoutePrefix("fhir"), EnableCors("*", "*", "*", "*")]
    [RouteDataValuesOnly]
    [ExceptionFilter]
    public class FhirController : ApiController
    {
        private readonly ICollection<IFhirLightService> _lightServices;
        private readonly ServiceHandler _handler = new ServiceHandler();

        public FhirController(ICollection<IFhirLightService> services) {
        	_lightServices = services;
        }

        [HttpGet, Route("{type}/{id}"), Route("{type}/identifier/{id}")]
        public HttpResponseMessage Read(string type, string id)
        {
            var service = _handler.FindServiceFromList(_lightServices, type);
            var result = service.Read(id);
            
            return SendResponse(result);
        }

        [HttpGet, Route("{type}")]
        public HttpResponseMessage Read(string type)
        {
            var service = _handler.FindServiceFromList(_lightServices, type);
            var parameters = Request.GetSearchParams();
            if (!(parameters.Parameters.Count > 0)) return new HttpResponseMessage(HttpStatusCode.ExpectationFailed);
            var results = service.Read(parameters);
            return SendResponse(results);
        }

        [HttpGet, Route("")]
        // ReSharper disable once InconsistentNaming
        public HttpResponseMessage Query(string _query)
        {
            var searchParams = Request.GetSearchParams();
            var service = _handler.FindServiceFromList(_lightServices, searchParams.Query);
            var result = service.Read(searchParams);

            return SendResponse(result);
        }

        [HttpPost, Route("{type}")]
        public HttpResponseMessage Create(string type, Resource resource)
        {
            var service = _handler.FindServiceFromList(_lightServices, type);
            return _handler.ResourceCreate(type, resource, service);
        }

        [HttpPut, Route("{type}/{id}")]
        public HttpResponseMessage Update(string type, string id, Resource resource)
        {
            var service = _handler.FindServiceFromList(_lightServices, type);
            return _handler.ResourceUpdate(type, id, resource, service);
        }

        [HttpDelete, Route("{type}/{id}")]
        public HttpResponseMessage Delete(string type, string id)
        {
            var service = _handler.FindServiceFromList(_lightServices, type);
            return _handler.ResourceDelete(type, Key.Create(type, id), service);
        }

        private HttpResponseMessage SendResponse(Base resource)
        {
            //var validationResult = _validator.Validate(resource);
            //if (validationResult.Issue.Count != 0)
            //{
            //    resource = validationResult;
            //}
            var headers = Request.Headers;
            var accept = headers.Accept;
            var returnJson = ReturnJson(accept);

            StringContent httpContent;
            if (!returnJson)
            {
                var xml = FhirSerializer.SerializeToXml(resource);
                httpContent =
                    new StringContent(xml, Encoding.UTF8,
                     FhirMediaType.XmlResource);
            }
            else
            {
                httpContent =
                    new StringContent(FhirSerializer.SerializeToJson(resource), Encoding.UTF8,
                     FhirMediaType.JsonResource);
            }
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = httpContent };
            return response;
        }

        private static bool ReturnJson(HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> accept)
        {
            var jsonHeaders = Hl7.Fhir.Rest.ContentType.JSON_CONTENT_HEADERS;
            var returnJson = false;
            foreach (var x in accept)
            {
                foreach (var y in jsonHeaders)
                {
                    if (!x.MediaType.Contains(y)) continue;
                    returnJson = true;
                    break;
                }
            }
            return returnJson;
        }

        [HttpGet, Route("metadata")]
        public HttpResponseMessage MetaData()
        {
            var headers = Request.Headers;
            var accept = headers.Accept;
            var returnJson = accept.Any(x => x.MediaType.Contains(FhirMediaType.HeaderTypeJson));

            StringContent httpContent;
            var metaData = _handler.CreateMetadata(_lightServices);
            if (!returnJson)
            {
                var xml = FhirSerializer.SerializeToXml(metaData);
                httpContent =
                    new StringContent(xml, Encoding.UTF8,
                        "application/xml");
            }
            else
            {
                httpContent =
                    new StringContent(FhirSerializer.SerializeToJson(metaData), Encoding.UTF8,
                        "application/json");
            }
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = httpContent };
            return response;
        }
    }
}
