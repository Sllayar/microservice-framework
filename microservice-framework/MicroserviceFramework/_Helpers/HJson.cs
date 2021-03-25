using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

// ReSharper disable All

namespace RFI.MicroserviceFramework._Helpers
{
    public class MaskAttributes
    {
        [AttributeUsage(AttributeTargets.All)]
        public class MaskedParam : Attribute
        {
            public MaskedParam(int countLeft = 0, bool showCenter = false, int countRight = 0)
            {
            }
        }
    }

    public static class HJson
    {
        public static T JsonDeserialize<T>(this string source) => JsonConvert.DeserializeObject<T>(source);

        public static string JsonSerialize<T>(this T source, bool indented = false) => JsonConvert.SerializeObject(source, indented ? Formatting.Indented : Formatting.None);

        public static string JsonSerializeAsSimpleString<T>(this T source, int maxCharsCount = 500) => JsonAsSimpleString(source.JsonSerialize(), maxCharsCount);

        public static void PopulateFromJson<T>(this T source, string json) => JsonConvert.PopulateObject(json, source);

        public static void PopulateFromJson<T>(this T source, string json, JsonSerializerSettings settings) => JsonConvert.PopulateObject(json, source, settings);


        public static string ToMaskedJson<T>(this T obj)
        {
            if(obj.IsNull()) return null;

            var replaces = new List<(string, string)>();

            foreach(var propertyInfo in obj.GetAttributeProperties(typeof(MaskAttributes.MaskedParam)))
            {
                var attribute = propertyInfo.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(MaskAttributes.MaskedParam));

                if(attribute.IsNull()) continue;

                var countLeft = (int)attribute.ConstructorArguments[0].Value;
                var showCenter = (bool)attribute.ConstructorArguments[1].Value;
                var countRight = (int)attribute.ConstructorArguments[2].Value;

                var prmObject = obj.GetPropValue(propertyInfo.Name);

                if(prmObject.IsNull()) continue;

                var prm = prmObject.ToString();

                if(propertyInfo.PropertyType == typeof(decimal) && (decimal)obj.GetPropValue(propertyInfo.Name) % 1 == 0) prm += ".0";

                /*todo <<< LISTS AND CLASSES >>> if(propertyInfo.PropertyType == typeof(IList)) { prm = obj.GetPropValue(propertyInfo.Name).ToJson(); }*/

                string maskedPrm;

                if(countLeft == -1)
                {
                    maskedPrm = "*";
                }
                else if(countLeft == 0 && countRight == 0 || prm.Length <= countLeft + countRight)
                {
                    maskedPrm = prm.Mask();
                }
                else
                {
                    var strStart = prm.Substring(0, countLeft);
                    var strCenter = prm.Substring(countLeft, prm.Length - countRight - countLeft);
                    var strEnd = prm.Substring(prm.Length - countRight, countRight);

                    maskedPrm = showCenter ? strStart.Mask() + strCenter + strEnd.Mask() : strStart + strCenter.Mask() + strEnd;
                }

                var q = propertyInfo.PropertyType == typeof(string) || propertyInfo.PropertyType == typeof(DateTime) ? "\"" : "";

                replaces.Add(($"\"{propertyInfo.Name}\":{q}{prm}{q}", $"\"{propertyInfo.Name}\":{q}{maskedPrm}{q}"));
            }

            return obj.JsonSerialize()?.Replace(replaces.OrderByDescending(p => p.Item1.Length).ToArray());
        }

        private static string Mask(this object prmObj) => new string('*', prmObj.ToString().Length);


        public static string JsonAsSimpleString(string json, int maxCharsCount = 500)
        {
            try
            {
                if(json.IsNull()) return "null";
                if(json.IsNullOrEmpty(true)) return "empty";
                var str = json.Replace((": ", ":"), ("{", "{ "), ("}", " }"), (",", ", "), ("\r", " "), ("\n", " ")).Remove("\"").RegexReplace("\\ +", " ").Trim();
                return str.Length > maxCharsCount ? str.Substring(0, maxCharsCount - 4) + "..." : str;
            }
            catch
            {
                return "";
            }
        }
    }
}