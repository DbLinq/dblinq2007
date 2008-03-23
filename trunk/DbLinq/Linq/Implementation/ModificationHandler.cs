#region MIT license
////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Jiri George Moudry
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DbLinq.Linq.Implementation
{
    /// <summary>
    /// ModificationHandler class handles entities in two ways:
    /// 1. if entity implements IModifed, uses the interface and its IsModifed flag property
    /// 2. otherwise, the handler keeps a dictionary of raw data per entity
    /// </summary>
    public class ModificationHandler : IModificationHandler
    {
        private readonly IDictionary<object, IDictionary<string, object>> rawDataEntities = new Dictionary<object, IDictionary<string, object>>();
        private readonly IDictionary<object, IDictionary<string, PropertyInfo>> modifiedProperties = new Dictionary<object, IDictionary<string, PropertyInfo>>();

        protected IEnumerable<PropertyInfo> GetColumnProperties(Type entityType)
        {
            foreach (PropertyInfo propertyInfo in entityType.GetProperties())
            {
                // we consider only properties marked as [Column] since these are the only ones going to DB
                if (propertyInfo.GetCustomAttributes(typeof(ColumnAttribute), true).Length > 0)
                {
                    yield return propertyInfo;
                }
            }
        }

        protected bool IsPrimitiveType(Type t)
        {
            if (t.IsValueType)
                return true;
            if (t == typeof(string))
                return true;
            return false;
        }

        /// <summary>
        /// Adds simple (value) properties of an object to a given dictionary
        /// and recurses if a property contains complex data
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="rawData"></param>
        /// <param name="prefix"></param>
        protected void AddRawData(object entity, IDictionary<string, object> rawData, string prefix)
        {
            if (entity == null)
                return;
            foreach (PropertyInfo propertyInfo in GetColumnProperties(entity.GetType()))
            {
                object propertyValue = propertyInfo.GetGetMethod().Invoke(entity, null);
                // if it is a value, it can be stored directly
                if (IsPrimitiveType(propertyInfo.PropertyType))
                {
                    rawData[prefix + propertyInfo.Name] = propertyValue;
                }
                else // otherwise, we recurse, and prefix the current property name to sub properties to avoid conflicts
                {
                    AddRawData(propertyValue, rawData, propertyInfo.Name + ".");
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
        /// Tells if the object is self declaring (holds its own change state)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private bool IsSelfDeclaring(object entity)
        {
            //return false;
            return entity is IModified;
        }

        /// <summary>
        /// Tells if the object notifies a change
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private bool IsNotifying(object entity)
        {
            return entity is INotifyPropertyChanged
                   || entity is INotifyPropertyChanging;
        }

        /// <summary>
        /// Start to watch an entity. From here, changes will make IsModified() return true
        /// </summary>
        /// <param name="entity"></param>
        public void Register(object entity)
        {
            // self handling, no need to do anything
            if (IsSelfDeclaring(entity))
            {
                return;
            }
            // notifying, we need to wait for changes
            else if (IsNotifying(entity))
            {
                RegisterNotification(entity);
            }
            // raw data, we keep a snapshot of the current state
            else
            {
                if (!rawDataEntities.ContainsKey(entity))
                    rawDataEntities[entity] = GetEntityRawData(entity);
            }
        }

        private void RegisterNotification(object entity)
        {
            modifiedProperties[entity] = new Dictionary<string, PropertyInfo>();
            if (entity is INotifyPropertyChanging)
            {
                ((INotifyPropertyChanging)entity).PropertyChanging += delegate(object sender, PropertyChangingEventArgs e)
                                                                           {
                                                                               SetPropertyChanged(sender, e.PropertyName);
                                                                           };
            }
            else if (entity is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged)entity).PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
                                                                         {
                                                                             SetPropertyChanged(sender, e.PropertyName);
                                                                         };
            }
        }

        /// <summary>
        /// This method is called when a notifying object sends an event because of a property change
        /// We may keep track of the precise change in the future
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        private void SetPropertyChanged(object entity, string propertyName)
        {
            modifiedProperties[entity][propertyName] = GetProperty(entity, propertyName);
        }

        /// <summary>
        /// Returns if the entity was modified since it has been Register()ed for the first time
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool IsModified(object entity)
        {
            // 1. self declaring case (IModified)
            if (IsSelfDeclaring(entity))
                return IsSelfDeclaringModified(entity);

            // 2. event notifying case (INotify*)
            if (IsNotifying(entity))
                return IsNotifyingModified(entity);

            // 3. raw data
            return IsRawModified(entity);
        }

        private bool IsSelfDeclaringModified(object entity)
        {
            var modified = entity as IModified;
            if (modified != null)
            {
                return modified.IsModified;
            }
            throw new ArgumentException("object does not implement a known self declaring interface");
        }

        private bool IsNotifyingModified(object entity)
        {
            return !modifiedProperties.ContainsKey(entity) || modifiedProperties[entity].Count > 0;
        }

        private bool IsPropertyModified(object p1, object p2)
        {
            return !object.Equals(p1, p2);
        }

        private bool IsRawModified(object entity)
        {
            // if not present, maybe it was inserted (or set to dirty)
            if (!rawDataEntities.ContainsKey(entity))
                return true;

            IDictionary<string, object> originalData = rawDataEntities[entity];
            IDictionary<string, object> currentData = GetEntityRawData(entity);

            foreach (string key in originalData.Keys)
            {
                object originalValue = originalData[key];
                object currentValue = currentData[key];
                if (IsPropertyModified(originalValue, currentValue))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a list of all modified properties since last Register/ClearModified
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IList<PropertyInfo> GetModifiedProperties(object entity)
        {
            if (IsSelfDeclaring(entity))
                return GetSelfDeclaringModifiedProperties(entity);

            if (IsNotifying(entity))
                return GetNotifyingModifiedProperties(entity);

            return GetRawModifiedProperties(entity);
        }

        protected IList<PropertyInfo> GetAllColumnProperties(object entity)
        {
            if (entity == null)
                throw new ArgumentNullException("GetAllColumnProperties(): entity must not be null");
            IList<PropertyInfo> properties = new List<PropertyInfo>(GetColumnProperties(entity.GetType()));
            return properties;
        }

        protected IList<PropertyInfo> GetSelfDeclaringModifiedProperties(object entity)
        {
            return GetAllColumnProperties(entity);
        }

        protected IList<PropertyInfo> GetNotifyingModifiedProperties(object entity)
        {
            IDictionary<string, PropertyInfo> properties;
            // if we don't have it, it is fully dirty
            if (!modifiedProperties.TryGetValue(entity, out properties))
                return GetAllColumnProperties(entity);
            return new List<PropertyInfo>(properties.Values);
        }

        protected IList<PropertyInfo> GetRawModifiedProperties(object entity)
        {
            IList<PropertyInfo> properties = new List<PropertyInfo>();

            IDictionary<string, object> originalData;
            if (!rawDataEntities.TryGetValue(entity, out originalData))
                return GetAllColumnProperties(entity);
            IDictionary<string, object> currentData = GetEntityRawData(entity);

            foreach (string key in currentData.Keys)
            {
                object currentValue = currentData[key];
                object originalValue = originalData[key];
                if (IsPropertyModified(originalValue, currentValue))
                    properties.Add(GetProperty(entity, key));
            }

            return properties;
        }

        public void ClearModified(object entity)
        {
            if (IsSelfDeclaring(entity))
                ClearSelfDeclaringModified(entity);
            else if (IsNotifying(entity))
                ClearNotifyingModified(entity);
            else
                ClearRawModified(entity);
        }

        private void ClearSelfDeclaringModified(object entity)
        {
            var modified = entity as IModified;
            if (modified != null)
            {
                modified.IsModified = false;
                return;
            }
            throw new ArgumentException("object does not implement a known self declaring interface");
        }

        private void ClearNotifyingModified(object entity)
        {
            modifiedProperties[entity] = new Dictionary<string, PropertyInfo>();
        }

        private void ClearRawModified(object entity)
        {
            rawDataEntities[entity] = GetEntityRawData(entity);
        }

        private PropertyInfo GetProperty(object entity, string propertyName)
        {
            return entity.GetType().GetProperty(propertyName);
        }
    }
}
