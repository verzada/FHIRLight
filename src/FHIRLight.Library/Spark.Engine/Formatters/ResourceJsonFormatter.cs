﻿using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace FHIRLight.Library.Spark.Engine.Formatters
{
    public class ResourceJsonFormatter : MediaTypeFormatter
    {
        public ResourceJsonFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json+fhir"));
        }
        public override bool CanReadType(Type type)
        {
            return type == typeof(Resource);
        }
        public override bool CanWriteType(Type type)
        {
            return false;
        }
        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return Task.Factory.StartNew(() =>
            {
                return (object)null;
            });
        }
    }
}