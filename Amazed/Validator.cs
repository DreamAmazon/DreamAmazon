using System;
using System.Collections.Generic;
using System.Reflection;
using DreamAmazon.Interfaces;

namespace DreamAmazon
{
    public class Validator
    {
        private static readonly Dictionary<Type, List<ValidationAttribute>> Cache = new Dictionary<Type, List<ValidationAttribute>>();

        public static List<ValidationAttribute> FindAttributes(Type type)
        {
            if (IsCachePresented(type))
                return GetFromCache(type);

            List<ValidationAttribute> atts = new List<ValidationAttribute>();

            foreach (PropertyInfo property in type.GetProperties())
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(property, typeof(ValidationAttribute));
                foreach (ValidationAttribute attribute in attributes)
                {
                    attribute.Property = property;
                    atts.Add(attribute);
                }
            }

            CashIt(type, atts);
            return atts;
        }

        private static List<ValidationAttribute> GetFromCache(Type type)
        {
            return Cache[type];
        }

        private static bool IsCachePresented(Type type)
        {
            return Cache.ContainsKey(type);
        }

        private static void CashIt(Type type, List<ValidationAttribute> atts)
        {
            Cache.Add(type, atts);
        }

        public static Notification ValidateObject(object target)
        {
            List<ValidationAttribute> atts = FindAttributes(target.GetType());
            Notification notification = new Notification();

            foreach (ValidationAttribute att in atts)
            {
                att.Validate(target, notification);
            }

            if (target is IValidated)
            {
                ((IValidated)target).Validate(notification);
            }
            return notification;
        }
    }
}