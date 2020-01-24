using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using RFI.MicroserviceFramework._Environment;
using RFI.MicroserviceFramework._Helpers;
using RFI.MicroserviceFramework._Loggers;

namespace RFI.MicroserviceFramework._Api
{
    [AttributeUsage(AttributeTargets.All)]
    public class DBIncludeParam : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.All)]
    public class DBIgnoreParam : Attribute
    {
    }

    public static class ApiDbController
    {
        private const string PackageNameProduction = "API";
        private const string PackageNameDevelopment = "API_DEVELOPMENT";
        private const string PackageNameDebug = "API_DEBUG";
        public static readonly string PackageName = PackageNameProduction;

        static ApiDbController()
        {
            if(SEnv.IsDevelopment) PackageName = PackageNameDevelopment;
            if(SEnv.IsDebug) PackageName = PackageNameDebug;
        }

        public static List<T> ExecSelect<T>(string tableName, bool usePackage, object request) where T : new()
        {
            var parametersString = GetParams(request).Cast<object>().Aggregate("", (current, value) => current + value switch
            {
                null => "'', ",
                string _ => $"'{value}', ",
                int _ => $"{value},",
                long _ => $"{value},",
                decimal _ => $"{value},",
                bool b => $"{(b ? 1 : 0)},",
                DateTime _ => $"to_date('{value:dd/MM/yyyy HH:mm:ss}', 'dd/mm/yyyy hh24:mi:ss'), ",
                _ => throw new ApiException("Unknown param type")
            });

            var resultList = new List<T>();

            using(var connection = new OracleConnection(SEnv.OracleCS))
            {
                connection.Open();

                using(var cmd = connection.CreateCommand())
                {
                    try
                    {
                        var prms = parametersString.IsNullOrEmpty() ? "" : $"({parametersString.Trim().TrimEnd(',')})";
                        var select = usePackage ? $"select * from table({PackageName}.{tableName}{prms})" : $"select * from {tableName}{prms}";

                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = select;

                        using var reader = cmd.ExecuteReader();
                        while(reader.Read())
                        {
                            var outParams = new T();

                            foreach(var propertyInfo in outParams.GetType().GetProperties())
                            {
                                var propertyTypeName = propertyInfo.PropertyType.Name == "Nullable`1" ? propertyInfo.PropertyType.FullName.Regex("(?<=System.Nullable`1\\[\\[System.).*?(?=,)") : propertyInfo.PropertyType.Name;

                                var value = reader.GetValue(reader.GetOrdinal(propertyInfo.Name.ToUpper())).ToString();

                                /*switch(propertyTypeName)
                                {
                                    case "String":
                                        obj = value;
                                        break;
                                    case "Boolean":
                                        bool.TryParse(value, out var resultBool);
                                        obj = resultBool;
                                        break;
                                    case "Int32":
                                    case "Int64":
                                        int.TryParse(value, out var resultInt);
                                        obj = resultInt;
                                        break;
                                    case "Long":
                                        long.TryParse(value, out var resultLong);
                                        obj = resultLong;
                                        break;
                                    case "Decimal":
                                        decimal.TryParse(value.Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var resultDecimal);
                                        obj = resultDecimal;
                                        break;
                                    case "DateTime":
                                        DateTime.TryParse(value, out var resultDateTime);
                                        obj = resultDateTime;
                                        break;
                                    default:
                                        throw new ApiException("Unknown param type");
                                }*/

                                var obj = propertyTypeName switch
                                {
                                    "String" => (object)value,
                                    "Boolean" => bool.Parse(value),
                                    "Int32" => int.Parse(value),
                                    "Int64" => int.Parse(value),
                                    "Long" => long.Parse(value),
                                    "Decimal" => decimal.Parse(value.Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                                    "DateTime" => DateTime.Parse(value),
                                    _ => throw new ApiException("Unknown param type")
                                };

                                propertyInfo.SetValue(outParams, obj);
                            }

                            resultList.Add(outParams);
                        }
                    }
                    catch(OracleException ex)
                    {
                        ProcessOracleException(ex);
                    }

                    cmd.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }


            return resultList;
        }


        public static T ExecStoredProcedure<T>(string procedureName, object request) where T : new()
        {
            using var connection = new OracleConnection(SEnv.OracleCS);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = PackageName + "." + procedureName;

            var response = new T();

            // in parameters
            foreach(var (_, value) in GetParams(request)) cmd.AddParamIn(value);

            // out parameters
            foreach(var (name, value) in GetParams(response)) cmd.AddParamOut(name, value);

            try
            {
                //ChainLogger.Step($"{callerMemberName} => {cmd.CommandText}");
                cmd.ExecuteNonQuery();
            }
            catch(OracleException ex)
            {
                ProcessOracleException(ex);
            }

            // read out params
            foreach(var propertyInfo in response.GetType().GetProperties().Where(p => p.CustomAttributes.All(a => a.AttributeType != typeof(DBIncludeParam) && a.AttributeType != typeof(DBIgnoreParam))))
            {
                var prmStr = cmd.Parameters[propertyInfo.Name].Value.ToString();

                var obj = response.GetPropValue(propertyInfo.Name) switch
                {
                    null => (object)(prmStr == "null" ? "" : prmStr),
                    string _ => (prmStr == "null" ? "" : prmStr),
                    int _ => int.Parse(prmStr),
                    long _ => long.Parse(prmStr),
                    decimal _ => decimal.Parse(prmStr.Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                    DateTime _ => DateTime.Parse(prmStr),
                    _ => throw new ApiException("Unknown param type")
                };

                propertyInfo.SetValue(response, obj);
            }

            connection.Close();
            connection.Dispose();
            cmd.Dispose();

            return response;
        }

        public static void ExecStoredProcedure(string procedureName, object request)
        {
            using var connection = new OracleConnection(SEnv.OracleCS);

            connection.Open();

            using(var cmd = connection.CreateCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = PackageName + "." + procedureName;

                // in parameters
                foreach(var (_, value) in GetParams(request)) cmd.AddParamIn(value);

                try
                {
                    //ChainLogger.Step($"{callerMemberName} => {cmd.CommandText}");

                    cmd.ExecuteNonQuery();
                }
                catch(OracleException ex)
                {
                    ProcessOracleException(ex);
                }
            }

            connection.Close();
            connection.Dispose();
        }


        private static void AddParamIn(this OracleCommand cmd, object obj)
        {
            cmd.Parameters.Add(new OracleParameter
            {
                OracleDbType = GetOracleDbType(obj),
                Direction = ParameterDirection.Input,
                Value = obj
            });
        }

        private static void AddParamOut(this OracleCommand cmd, string name, object obj)
        {
            cmd.Parameters.Add(new OracleParameter
            {
                ParameterName = name,
                OracleDbType = GetOracleDbType(obj),
                Size = int.MaxValue,
                Direction = ParameterDirection.Output
            });
        }


        private static void ProcessOracleException(OracleException ex)
        {
            ex.Log();

            if(ex.Number > 20100 && ex.Number < 20200) throw new ApiException((CodeStatus)(ex.Number - 20100));

            if(ex.Number > 20200 && ex.Number < 20300) throw new ApiException((CodeStatus)(ex.Number - 20200), ex.Message?.Regex("(?<=ORA-\\d+:).*?(?=\\n)"));

            if(ex.Number == 1403) throw new ApiException(CodeStatus.DataNotFound);

            throw new ApiException(CodeStatus.UnhandledException);
        }


        private static OracleDbType GetOracleDbType(object obj) => obj switch
        {
            null => OracleDbType.Varchar2,
            string _ => OracleDbType.Varchar2,
            int _ => OracleDbType.Int32,
            long _ => OracleDbType.Long,
            decimal _ => OracleDbType.Decimal,
            DateTime _ => OracleDbType.Date,
            _ => throw new ApiException("Unknown param type")
        };


        private static Dictionary<string, object> GetParams<T>(T request)
        {
            var prms = new Dictionary<string, object>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(var propertyInfo in request.GetType().GetProperties().Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(DBIncludeParam))))
            {
                prms.Add(propertyInfo.Name, request.GetPropValue(propertyInfo.Name));
            }

            foreach(var propertyInfo in request.GetType().GetProperties().Where(p => p.CustomAttributes.All(a => a.AttributeType != typeof(DBIncludeParam) && a.AttributeType != typeof(DBIgnoreParam))))
            {
                prms.Add(propertyInfo.Name, request.GetPropValue(propertyInfo.Name));
            }

            return prms;
        }


        public static void ClearCaches()
        {
            try
            {
                ExecStoredProcedure("api_clear_caches", new object());
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }
    }
}