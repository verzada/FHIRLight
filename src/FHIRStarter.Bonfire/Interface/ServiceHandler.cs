﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Configuration;
using FhirStarter.Bonfire.Service;
using FhirStarter.Bonfire.Spark.Engine.Core;
using Hl7.Fhir.Model;

namespace FhirStarter.Bonfire.Interface
{
    public class ServiceHandler
    {
        public HttpResponseMessage ResourceCreate(string type, Resource resource, IFhirLightService service)
        {
            if (service != null && !string.IsNullOrEmpty(type) && resource != null)
            {
                var key = Key.Create(type);
                var result = service.Create(key, resource);
                if (result != null)
                    return result;
            }

            return new HttpResponseMessage(HttpStatusCode.Ambiguous);
        }

        public HttpResponseMessage ResourceUpdate(string type, string id, Resource resource, IFhirLightService service)
        {
            if (service != null && !string.IsNullOrEmpty(type) && resource != null && !string.IsNullOrEmpty(id))
            {
                var key = Key.Create(type, id);
                var result = service.Update(key, resource);
                if (result != null)
                    return result;
            }
            throw new ArgumentException("Service is null, cannot update resource of type " + type);
        }

        public HttpResponseMessage ResourceDelete(string type, Key key, IFhirLightService service)
        {
           if (service != null)
            {
                return service.Delete(key);
            }

            throw new ArgumentException("Service is null, cannot update resource of type " + type);
        }


        public  IFhirLightService FindServiceFromList(ICollection<IFhirLightService> services, string type)
        {
            if (services.Any())
            {
                foreach (var service in services)
                {
                    if (service.GetAlias().Equals(type))
                    {
                        return service;
                    }
                }   
            }
            if (services.Count > 1)
            {
                throw new ArgumentException("The resource type " + type + " is not supported by the available services.");
            }
            throw new ArgumentException("The resource type " + type + " is not supported by the available service.");
        }

        public Conformance CreateMetadata(ICollection<IFhirLightService> services)
        {
            if (services.Any())
            {
                var serviceName = MetaDataName(services);

                var fhirVersion = ModelInfo.Version;

                var fhirPublisher = GetFhirPublisher();
                var fhirDescription = GetFhirDescription();

                var conformance = ConformanceBuilderFhirLight.CreateServer(serviceName, fhirPublisher, fhirVersion);

                conformance.AddUsedResources(services, false, false,
                    Conformance.ResourceVersionPolicy.VersionedUpdate);

                conformance.AddSearchSetInteraction().AddSearchTypeInteractionForResources();
                conformance = conformance.AddCoreSearchParamsAllResources(services);
                conformance = conformance.AddOperationDefintion(services);

                conformance.AcceptUnknown = Conformance.UnknownContentCode.Both;
                conformance.Experimental = true;
                conformance.Format = new[] {"xml", "json"};
                conformance.Description = fhirDescription;

                return conformance;
            }
            return new Conformance();
        }

        private static string GetFhirDescription()
        {
            var fhirDescription = WebConfigurationManager.AppSettings["FhirDescription"];
            if (string.IsNullOrEmpty(fhirDescription))
            {
                fhirDescription = "Add FhirDescription key with the description of the service to appSettings in web.config.";
            }
            return fhirDescription;
        }

        private static string GetFhirPublisher()
        {
            var fhirPublisher = WebConfigurationManager.AppSettings["FhirPublisher"];
            if (string.IsNullOrEmpty(fhirPublisher))
            {
                fhirPublisher = "Add FhirPublisher key with the name of the publisher to appSettings in web.config.";
            }
            return fhirPublisher;
        }


        private string MetaDataName(ICollection<IFhirLightService> services)
        {
            var serviceName = services.Count > 1 ? "The following services are available: " : "The following service is available: ";

            var servicesAsArray = services.ToArray();
            for (var i = 0; i < services.Count; i++)
            {
                serviceName += servicesAsArray[i].GetAlias();
                if (i < services.Count - 1)
                {
                    serviceName += " ";
                }
            }
            return serviceName;
        }
    }
}
