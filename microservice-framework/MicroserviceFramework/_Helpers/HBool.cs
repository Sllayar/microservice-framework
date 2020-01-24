using System;

// ReSharper disable All

namespace RFI.MicroserviceFramework._Helpers
{
    public static class HBool
    {
        public static bool IsNull(this object obj) => obj == null;


        public static bool NotNull(this object obj) => obj != null;


        public static bool ParseBool(this object obj, bool def = false) => bool.TryParse(obj.ToString(), out var tryResult) ? tryResult : def;


        // Simple True/False tests

        public static bool IsTrue(this bool obj) => obj;

        public static bool IsFalse(this bool obj) => obj == false;

        public static bool NotTrue(this bool obj) => obj == false;

        public static bool NotFalse(this bool obj) => obj;

        public static bool Not(this bool obj) => !obj;


        // Actions on true, false, null, notNull

        public static void IfTrue(this bool value, Action action)
        {
            if(value) action();
        }

        public static void IfFalse(this bool value, Action action)
        {
            if(value.Not()) action();
        }

        public static void If(this bool value, Action actionIfTrue, Action actionIfFalse)
        {
            if(value) actionIfTrue();
            else actionIfFalse();
        }


        // Null classes

        public static void IfNull<TObj>(this TObj value, Action action) where TObj : class
        {
            if(value.IsNull()) action();
        }

        public static void IfNotNull<TObj>(this TObj value, Action action) where TObj : class
        {
            if(value.NotNull()) action();
        }

        public static void IfNull<TObj>(this TObj value, Action actionIfNull, Action actionIfNotNull) where TObj : class
        {
            if(value.IsNull())
            {
                actionIfNull();
            }
            else
            {
                actionIfNotNull();
            }
        }

        public static void IfNotNull<TObj>(this TObj value, Action actionIfNotNull, Action actionIfNull) where TObj : class
        {
            if(value.NotNull())
            {
                actionIfNotNull();
            }
            else
            {
                actionIfNull();
            }
        }


        //Null nullables

        public static void IfNull<TObj>(this TObj? value, Action action) where TObj : struct
        {
            if(value.IsNull()) action();
        }

        public static void IfNotNull<TObj>(this TObj? value, Action action) where TObj : struct
        {
            if(value.NotNull()) action();
        }

        public static void IfNull<TObj>(this TObj? value, Action actionIfNull, Action actionIfNotNull) where TObj : struct
        {
            if(value.IsNull())
            {
                actionIfNull();
            }
            else
            {
                actionIfNotNull();
            }
        }

        public static void IfNotNull<TObj>(this TObj? value, Action actionIfNotNull, Action actionIfNull) where TObj : struct
        {
            if(value.NotNull())
            {
                actionIfNotNull();
            }
            else
            {
                actionIfNull();
            }
        }


        // Funcs on true, false, null, notNull

        public static T IfTrue<T>(this bool value, Func<T> action) => value ? action() : default;

        public static T IfFalse<T>(this bool value, Func<T> action) => value == false ? action() : default;

        public static T If<T>(this bool value, Func<T> actionIfTrue, Func<T> actionIfFalse) => value ? actionIfTrue() : actionIfFalse();


        // Null classes

        public static T IfNull<TObj, T>(this TObj value, Func<T> action) where TObj : class => value.IsNull() ? action() : default;

        public static T IfNotNull<TObj, T>(this TObj value, Func<T> action) where TObj : class
        {
            if(value.NotNull()) action();
            return default;
        }

        public static T IfNull<TObj, T>(this TObj value, Func<T> actionIfNull, Func<T> actionIfNotNull) where TObj : class => value.IsNull() ? actionIfNull() : actionIfNotNull();

        public static T IfNotNull<TObj, T>(this TObj value, Func<T> actionIfNotNull, Func<T> actionIfNull) where TObj : class => value.NotNull() ? actionIfNotNull() : actionIfNull();


        // Null Nullables

        public static T IfNull<TObj, T>(this TObj? value, Func<T> action) where TObj : struct => value.IsNull() ? action() : default;

        public static T IfNotNull<TObj, T>(this TObj? value, Func<T> action) where TObj : struct
        {
            if(value.NotNull()) action();
            return default;
        }

        public static T IfNull<TObj, T>(this TObj? value, Func<T> actionIfNull, Func<T> actionIfNotNull) where TObj : struct => value.IsNull() ? actionIfNull() : actionIfNotNull();

        public static T IfNotNull<TObj, T>(this TObj? value, Func<T> actionIfNotNull, Func<T> actionIfNull) where TObj : struct => value.NotNull() ? actionIfNotNull() : actionIfNull();
    }
}