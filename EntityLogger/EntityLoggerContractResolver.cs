using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace EntityLogger
{
    internal class EntityLoggerContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (IsPrimitive(property.PropertyType))
            {
                property.ShouldSerialize = instance => true;
                return property;
            }

            property.ShouldSerialize = instance => false;
            return property;
        }

        private bool IsPrimitive(Type type)
        {
            return type != null &&
               (
                   type.IsPrimitive ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   IsPrimitive(Nullable.GetUnderlyingType(type))
               );
        }
    }
}
