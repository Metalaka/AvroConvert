// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License.  You may obtain a copy
// of the License at http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED
// WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
// 
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

/** Modifications copyright(C) 2020 Adrian Struga�a **/

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SolTechnology.Avro.AvroObjectServices.Schemas.Abstract;
using SolTechnology.Avro.Infrastructure.Extensions;

namespace SolTechnology.Avro.AvroObjectServices.Schemas
{
    /// <summary>
    ///     Represents a map.
    ///     For more details please see <a href="http://avro.apache.org/docs/current/spec.html#Maps">the specification</a>.
    /// </summary>
    internal sealed class MapSchema : TypeSchema
    {
        private readonly TypeSchema valueSchema;
        private readonly TypeSchema keySchema;

        internal MapSchema(TypeSchema keySchema, TypeSchema valueSchema, Type runtimeType)
            : this(keySchema, valueSchema, runtimeType, new Dictionary<string, string>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapSchema" /> class.
        /// </summary>
        /// <param name="keySchema">The key schema.</param>
        /// <param name="valueSchema">The value schema.</param>
        /// <param name="runtimeType">Type of the runtime.</param>
        /// <param name="attributes">The attributes.</param>
        internal MapSchema(
            TypeSchema keySchema,
            TypeSchema valueSchema,
            Type runtimeType,
            Dictionary<string, string> attributes)
            : base(runtimeType, attributes)
        {
            if (keySchema == null)
            {
                throw new ArgumentNullException("keySchema");
            }
            if (valueSchema == null)
            {
                throw new ArgumentNullException("valueSchema");
            }

            this.valueSchema = valueSchema;
            this.keySchema = keySchema;
        }

        /// <summary>
        ///     Gets the value schema.
        /// </summary>
        internal TypeSchema ValueSchema
        {
            get { return this.valueSchema; }
        }

        /// <summary>
        /// Gets the key schema.
        /// </summary>
        internal TypeSchema KeySchema
        {
            get { return this.keySchema; }
        }

        /// <summary>
        ///     Converts current not to JSON according to the avro specification.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="seenSchemas">The seen schemas.</param>
        internal override void ToJsonSafe(JsonTextWriter writer, HashSet<NamedSchema> seenSchemas)
        {
            writer.WriteStartObject();
            writer.WriteProperty("type", "map");
            writer.WritePropertyName("values");
            ValueSchema.ToJson(writer, seenSchemas);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Gets the type of the schema as string.
        /// </summary>
        internal override AvroType Type => AvroType.Map;
    }
}
