﻿#region license
/**Copyright (c) 2020 Adrian Strugała
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* https://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
#endregion

using System;
using SolTechnology.Avro.Infrastructure.Exceptions;

namespace SolTechnology.Avro.AvroObjectServices.Write.Resolvers
{
    internal class Long
    {
        internal void Resolve(object value, IWriter encoder)
        {
            if (value is not long converted)
            {
                try //Resolve ulong
                {
                    encoder.WriteLong(Convert.ToInt64(value));
                    return;
                }
                catch
                {
                    throw new AvroTypeMismatchException("[Long] required to write against [Long] schema but found " + value.GetType());
                }
            }

            encoder.WriteLong(converted);
        }
    }
}
