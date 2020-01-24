using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RFI.MicroserviceFramework._Helpers
{
    public static class HReflection
    {
        public static object GetPropValue(this object obj, string name)
        {
            foreach(var part in name.Split('.'))
            {
                if(obj.IsNull()) return null;

                var type = obj.GetType();
                var info = type.GetProperty(part);
                if(info.IsNull()) return null;

                obj = info.GetValue(obj, null);
            }

            return obj;
        }

        /*public static T GetPropValue<T>(this object obj, string name)
        {
            var retval = GetPropValue(obj, name);
            if(retval.IsNull()) return default;
            return (T)retval;
        }*/

        public static IEnumerable<PropertyInfo> GetAttributeProperties(this object obj, Type type)
        {
            return obj.GetType().GetProperties().Where(p => p.CustomAttributes.Any(a => a.AttributeType == type));
        }
    }
}