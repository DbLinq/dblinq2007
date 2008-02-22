using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DbLinq.Linq
{
    public class ModificationHandler
    {
        private IDictionary<object, IDictionary<string, object>> entities = new Dictionary<object, IDictionary<string, object>>();

        protected void AddRawData(object entity, IDictionary<string, object> rawData, string prefix)
        {
            foreach (PropertyInfo propertyInfo in entity.GetType().GetProperties())
            {
                if (propertyInfo.GetCustomAttributes(typeof (ColumnAttribute), true).Length > 0)
                {
                    object propertyValue = propertyInfo.GetGetMethod().Invoke(entity, null);
                    if (propertyInfo.PropertyType.IsValueType)
                    {
                        rawData[prefix + propertyInfo.Name] = propertyValue;
                    }
                    else
                    {
                        AddRawData(propertyValue, rawData, propertyInfo.Name + ".");
                    }
                }
            }
        }

        protected IDictionary<string, object> GetEntityRawData(object entity)
        {
            Dictionary<string, object> rawData = new Dictionary<string, object>();
            AddRawData(entity, rawData, string.Empty);
            return rawData;
        }

        public void Register(object entity)
        {
            // self handling
            if (entity is IModified)
                return;
            if (!entities.ContainsKey(entity))
                entities[entity] = GetEntityRawData(entity);
        }

        public void Clean(object entity)
        {
            var modified = entity as IModified;
            if (modified != null)
            {
                modified.IsModified = false;
                return;
            }
            entities[entity] = GetEntityRawData(entity);
        }

        // is this really useful?
        public void Dirty(object entity)
        {
            var modified = entity as IModified;
            if (modified != null)
            {
                modified.IsModified = true;
                return;
            }
            entities.Remove(entity);
        }

        public bool IsModified(object entity)
        {
            var modified = entity as IModified;
            if (modified != null)
            {
                return modified.IsModified;
            }

            // if not present, maybe it was inserted (or set to dirty)
            if (!entities.ContainsKey(entity))
                return true;

            IDictionary<string, object> originalData = entities[entity];
            IDictionary<string, object> currentData = GetEntityRawData(entity);

            foreach (string key in originalData.Keys)
            {
                object originalValue = originalData[key];
                object currentValue = currentData[key];
                if (!originalValue.Equals(currentValue))
                    return true;
            }

            return false;
        }
    }
}
