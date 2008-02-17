using System;
using System.Collections.Specialized;
using System.Linq;
using System.Configuration;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace SqlMetal
{
    public class Parameters
    {
        public readonly IList<string> Extra = new List<string>();

        private bool IsParameter(string arg, string switchPrefix, out string parameterName, out string parameterValue)
        {
            bool isParameter;
            if (arg.StartsWith(switchPrefix))
            {
                isParameter = true;
                string nameValue = arg.Substring(switchPrefix.Length);
                int separator = nameValue.IndexOfAny(new char[] { ':', '=' });
                if (separator >= 0)
                {
                    parameterName = nameValue.Substring(0, separator);
                    parameterValue = nameValue.Substring(separator + 1).Trim('\"');
                }
                else
                {
                    parameterName = nameValue;
                    parameterValue = null;
                }
            }
            else
            {
                isParameter = false;
                parameterName = null;
                parameterValue = null;
            }
            return isParameter; 
        }

        protected bool IsParameter(string arg, out string parameterName, out string parameterValue)
        {
            return IsParameter(arg, "--", out parameterName, out parameterValue)
                || IsParameter(arg, "-", out parameterName, out parameterValue)
                || IsParameter(arg, "/", out parameterName, out parameterValue);
        }

        protected delegate void InjectorDelegate(object typedValue);

        protected void InjectValue(InjectorDelegate injector, string value, Type targetType)
        {
            object typedValue;
            if (typeof(bool).IsAssignableFrom(targetType))
            {
                if (value == null || value == "+")
                    typedValue = true;
                else if (value == "-")
                    typedValue = false;
                else
                    typedValue = Convert.ToBoolean(value);
            }
            else
            {
                typedValue = Convert.ChangeType(value, targetType);
            }
            injector(typedValue);
        }

        protected void SetParameter(string name, string value)
        {
            Type thisType=GetType();
            BindingFlags flags=BindingFlags.IgnoreCase|BindingFlags.FlattenHierarchy|BindingFlags.Instance|BindingFlags.Public;
            FieldInfo fieldInfo = thisType.GetField(name, flags);
            if (fieldInfo != null)
                InjectValue(delegate(object typedValue) { fieldInfo.SetValue(this, typedValue); }, value, fieldInfo.FieldType); 
            else
            {
                PropertyInfo propertyInfo = thisType.GetProperty(name, flags);
                if (propertyInfo != null)
                    InjectValue(delegate(object typedValue) { propertyInfo.GetSetMethod().Invoke(this, new object[] { typedValue }); }, value, propertyInfo.PropertyType);
                else
                    throw new ArgumentException(string.Format("Parameter {0} does not exist", name));
            }
        }

        public Parameters(string[] args)
        {
            NameValueCollection configurationParameters = (NameValueCollection)ConfigurationManager.GetSection("parameters");
            foreach (string key in configurationParameters.AllKeys)
            {
                SetParameter(key, configurationParameters[key]);
            }
            foreach (string arg in args)
            {
                string key, value;
                if (IsParameter(arg, out key, out value))
                    SetParameter(key, value);
                else
                    Extra.Add(arg);
            }
        }
    }
}
