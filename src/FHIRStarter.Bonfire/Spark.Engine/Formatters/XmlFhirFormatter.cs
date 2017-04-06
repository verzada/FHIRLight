﻿/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FhirStarter.Bonfire.Spark.Engine.Auxiliary;
using FhirStarter.Bonfire.Spark.Engine.Core;
using FhirStarter.Bonfire.Spark.Engine.Extensions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;

namespace FhirStarter.Bonfire.Spark.Engine.Formatters
{
    public class XmlFhirFormatter : FhirMediaTypeFormatter
    {
        public XmlFhirFormatter()
        {
            foreach (var mediaType in ContentType.XML_CONTENT_HEADERS)
            {
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
            }
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.ContentType = FhirMediaType.GetMediaTypeHeaderValue(type, ResourceFormat.Xml);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return Task.Factory.StartNew<object>( () => 
            {
                try
                {
                    var body = ReadBodyFromStream(readStream, content);

                    if (type == typeof(Bundle))
                    {
                        if (XmlSignatureHelper.IsSigned(body))
                        {
                            if (!XmlSignatureHelper.VerifySignature(body))
                                throw Error.BadRequest("Digital signature in body failed verification");
                        }
                    }

                    if (!typeof(Resource).IsAssignableFrom(type))
                        throw Error.Internal("The type {0} expected by the controller can not be deserialized",
                            type.Name);

                    //var fhirparser = new FhirJsonParser();
                    //var resource = fhirparser.Parse(body, type);
                    var fhirXmlParser = new FhirXmlParser();
                    var resource = fhirXmlParser.Parse(body, type);
                    return resource;
                }
                catch (FormatException exc)
                {
                    throw Error.BadRequest("Body parsing failed: " + exc.Message);
                }
            });
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            
            return Task.Factory.StartNew(() =>
            {
                XmlWriter writer = new XmlTextWriter(writeStream, new UTF8Encoding(false));
                var summary = RequestMessage.RequestSummary();

                if (type == typeof(OperationOutcome)) 
                {
                    var resource = (Resource)value;
                    FhirSerializer.SerializeResource(resource, writer, summary);
                }
                else if (typeof(Resource).IsAssignableFrom(type))
                {
                    var resource = (Resource)value;
                    FhirSerializer.SerializeResource(resource, writer, summary);
                }
                else if (type == typeof(FhirResponse))
                {
                    var response = (value as FhirResponse);
                    if (response != null && response.HasBody)
                    FhirSerializer.SerializeResource(response.Resource, writer, summary);
                }
                
                writer.Flush();
            });
        }
    }
}
