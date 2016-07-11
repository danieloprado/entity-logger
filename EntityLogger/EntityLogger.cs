using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace EntityLogger
{
    public class EntityLogger
    {
        private readonly DbContext _context;
        private readonly JsonSerializerSettings _jsonSettings;

        public EntityLogger(DbContext context)
        {
            _context = context;
            _jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new EntityLoggerContractResolver()
            };
        }

        public string GenerateLog()
        {
            _context.ChangeTracker.DetectChanges();
            var builder = new StringBuilder();

            foreach (var data in GetTackerData(_context))
            {
                builder.AppendLine($"{data.Item1}|{data.Item2}|{CleanValue(data.Item3)}|{CleanValue(data.Item4)}");
            }

            return builder.ToString();
        }

        private IEnumerable<Tuple<EntityState, string, object, object>> GetTackerData(DbContext context)
        {
            var manager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
            var modifiedEntities = manager.GetObjectStateEntries(EntityState.Modified | EntityState.Added | EntityState.Deleted);

            foreach (var entry in modifiedEntities)
            {
                if (entry.IsRelationship)
                {
                    var relationshipResult = ResolveRelationship(entry);
                    if (relationshipResult == null)
                    {
                        continue;
                    }

                    yield return relationshipResult;
                }


                if (entry.State != EntityState.Modified)
                {
                    yield return Tuple.Create(entry.State, "Entity", (object)entry.EntitySet.Name, entry.Entity);
                    continue;
                }

                var modifications = GetModified(entry);
                if (modifications.Any())
                {
                    yield return Tuple.Create(entry.State, "Entity", (object)entry.EntitySet.Name, entry.Entity);
                    foreach (var item in modifications)
                    {
                        yield return item;
                    }
                }
            }
        }

        private Tuple<EntityState, string, object, object> ResolveRelationship(ObjectStateEntry entry)
        {
            var values = new Dictionary<string, string>();
            var records = entry.State == EntityState.Added ? entry.CurrentValues : entry.OriginalValues;

            var recordValues = new object[records.FieldCount];
            records.GetValues(recordValues);

            foreach (EntityKey value in recordValues)
            {
                if (value.EntityKeyValues == null)
                {
                    return null;
                }

                foreach (var keyValue in value.EntityKeyValues)
                {
                    values.Add(value.EntitySetName + keyValue.Key, keyValue.Value.ToString());
                }
            }

            return Tuple.Create(entry.State, "Entity", (object)entry.EntitySet.Name, (object)values);
        }

        private IEnumerable<Tuple<EntityState, string, object, object>> GetModified(ObjectStateEntry entry)
        {
            var modifiedProps = entry.GetModifiedProperties();

            foreach (var propName in modifiedProps)
            {
                var original = entry.OriginalValues[propName];
                var current = entry.CurrentValues[propName];

                if (propName == "UpdatedDate" || !IsModified(original, current))
                {
                    continue;
                }

                yield return Tuple.Create(entry.State, propName, original, current);
            }
        }

        private string CleanValue(object value)
        {
            return JsonConvert.SerializeObject(value, _jsonSettings).Replace("|", "&#124;").Trim('"');
        }

        private bool IsModified(object originalValue, object newValue)
        {
            return (originalValue != null && !originalValue.Equals(newValue))
                || (originalValue == null && newValue != null);
        }
    }


}
