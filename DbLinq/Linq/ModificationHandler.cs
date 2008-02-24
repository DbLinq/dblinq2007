using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DbLinq.Linq
{
    /// <summary>
    /// ModificationHandler class handles entities in two ways:
    /// 1. if entity implements IModifed, uses the interface and its IsModifed flag property
    /// 2. otherwise, the handler keeps a dictionary of raw data per entity
    /// </summary>
    public class ModificationHandler
    {
        private readonly IDictionary<object, IDictionary<string, object>> entities = new Dictionary<object, IDictionary<string, object>>();

        /// <summary>
        /// Adds simple (value) properties of an object to a given dictionary
        /// and recurses if a property contains complex data
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="rawData"></param>
        /// <param name="prefix"></param>
        protected void AddRawData(object entity, IDictionary<string, object> rawData, string prefix)
        {
            foreach (PropertyInfo propertyInfo in entity.GetType().GetProperties())
            {
                // we consider only properties marked as [Column] since these are the only ones going to DB
                if (propertyInfo.GetCustomAttributes(typeof (ColumnAttribute), true).Length > 0)
                {
                    object propertyValue = propertyInfo.GetGetMethod().Invoke(entity, null);
                    // if it is a value, it can be stored directly
                    if (propertyInfo.PropertyType.IsValueType)
                    {
                        rawData[prefix + propertyInfo.Name] = propertyValue;
                    }
                    else // otherwise, we recurse, and prefix the current property name to sub properties to avoid conflicts
                    {
                        AddRawData(propertyValue, rawData, propertyInfo.Name + ".");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a "flat view" from a composite object
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>a pair of {property name, property value}</returns>
        protected IDictionary<string, object> GetEntityRawData(object entity)
        {
            Dictionary<string, object> rawData = new Dictionary<string, object>();
            AddRawData(entity, rawData, string.Empty);
            return rawData;
        }

        /// <summary>
        /// Start to watch an entity. From here, changes will make IsModified() return true
        /// </summary>
        /// <param name="entity"></param>
        public void Register(object entity)
        {
            // self handling
            if (entity is IModified)
                return;
            if (!entities.ContainsKey(entity))
                entities[entity] = GetEntityRawData(entity);
        }

        /// <summary>
        /// Returns if the entity was modified since it has been Register() for the first time
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Marks the entity as not dirty.
        /// </summary>
        /// <param name="entity"></param>
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

        /// <summary>
        /// Marks the entity as dirty (apparently unused)
        /// </summary>
        /// <param name="entity"></param>
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
    }
}
