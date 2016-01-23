using System;
using System.Reflection;

namespace DreamAmazon
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MinLengthAttribute : ValidationAttribute
    {
        public const string LENGTH_FIELD = "Field length must be greater than ";

        private readonly int _length;

        public MinLengthAttribute(int length)
        {
            _length = length;
        }

        protected override void Validate(object target, object rawValue, Notification notification)
        {
            var value = rawValue as string;
            if (value == null) return;
            var s = value;
            if (s.Length < _length)
            {
                LogMessage(notification, LENGTH_FIELD + _length);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class LengthAttribute : ValidationAttribute
    {
        public const string LENGTH_FIELD = "Field length must be equal to ";

        private readonly int _length;

        public LengthAttribute(int length)
        {
            _length = length;
        }

        protected override void Validate(object target, object rawValue, Notification notification)
        {
            var value = rawValue as string;
            if (value == null) return;
            var s = value;
            if (s.Length != _length)
            {
                LogMessage(notification, LENGTH_FIELD + _length);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredAttribute : ValidationAttribute
    {
        protected override void Validate(object target, object rawValue, Notification notification)
        {
            if (rawValue == null)
            {
                LogMessage(notification, Notification.REQUIRED_FIELD);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ValidationAttribute : Attribute
    {
        private PropertyInfo _property;

        public void Validate(object target, Notification notification)
        {
            object rawValue = _property.GetValue(target, null);
            Validate(target, rawValue, notification);
        }

        protected void LogMessage(Notification notification, string message)
        {
            notification.RegisterMessage(Property.Name, message);
        }

        protected abstract void Validate(object target, object rawValue, Notification notification);

        public PropertyInfo Property
        {
            get { return _property; }
            set { _property = value; }
        }

        public string PropertyName
        {
            get
            {
                return _property.Name;
            }
        }
    }
}