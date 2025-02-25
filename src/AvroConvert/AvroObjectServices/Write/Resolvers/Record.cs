﻿#region license
/**Copyright (c) 2022 Adrian Strugala
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SolTechnology.Avro.Features.Serialize;
using SolTechnology.Avro.AvroObjectServices.Schemas;
namespace SolTechnology.Avro.AvroObjectServices.Write.Resolvers
{
    internal class Record
    {
        private readonly ConcurrentDictionary<int, Func<object, string, object>> gettersDictionary = new ConcurrentDictionary<int, Func<object, string, object>>();

        internal Encoder.WriteItem Resolve(RecordSchema recordSchema)
        {
            WriteStep[] writeSteps = new WriteStep[recordSchema.Fields.Count];

            int index = 0;
            foreach (RecordFieldSchema field in recordSchema.Fields)
            {
                var record = new WriteStep
                {
                    WriteField = WriteResolver.ResolveWriter(field.TypeSchema),
                    FiledName = field.Aliases.FirstOrDefault() ?? field.Name,
                };
                writeSteps[index++] = record;
            }

            void RecordResolver(object v, IWriter e)
            {
                WriteRecordFields(v, writeSteps, e);
            }

            return RecordResolver;
        }

        private void WriteRecordFields(object recordObj, WriteStep[] writers, IWriter encoder)
        {
            if (recordObj is ExpandoObject expando)
            {
                HandleExpando(writers, encoder, expando);
                return;
            }

            var type = recordObj.GetType();
            var typeHash = type.GetHashCode();

            Func<object, string, object> getters;
            if (gettersDictionary.ContainsKey(typeHash))
            {
                getters = gettersDictionary[typeHash];
            }
            else
            {
                getters = GenerateGetValue(type);
                gettersDictionary.AddOrUpdate(typeHash, getters, (key, existingVal) => existingVal);
            }

            foreach (var writer in writers)
            {
                var value = getters.Invoke(recordObj, writer.FiledName);
                if (value == null)
                {
                    value = type.GetField(writer.FiledName)?.GetValue(recordObj);
                }

                writer.WriteField(value, encoder);
            }
        }

        private static void HandleExpando(WriteStep[] writers, IWriter encoder, ExpandoObject expando)
        {
            var expandoDictionary = expando.ToDictionary(x => x.Key, y => y.Value, StringComparer.InvariantCultureIgnoreCase);

            foreach (var writer in writers)
            {
                expandoDictionary.TryGetValue(writer.FiledName, out var value);
                writer.WriteField(value, encoder);
            }
        }

        private static Func<object, string, object> GenerateGetValue(Type type)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash,
                Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();
            foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy))
            {
                var property = Expression.Property(Expression.Convert(instance, type), propertyInfo.Name);
                var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));

                cases.Add(Expression.SwitchCase(Expression.Convert(property, typeof(object)), propertyHash));
            }

            var switchEx = Expression.Switch(nameHash, Expression.Constant(null), cases.ToArray());
            var methodBody = Expression.Block(typeof(object), new[] { nameHash }, calHash, switchEx);

            return Expression.Lambda<Func<object, string, object>>(methodBody, instance, memberName).Compile();

        }
    }
}