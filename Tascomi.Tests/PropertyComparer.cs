using System;
using System.Collections.Generic;
using System.Reflection;
using Dynamitey;
using System.Diagnostics;

namespace Tascomi.Tests
{
    public class PropertyComparer<T> : IEqualityComparer<T>
    {
        private string _PropertyName;

        /// <summary>
        /// Creates a new instance of PropertyComparer.
        /// </summary>
        /// <param name="propertyName">The name of the property on type T 
        /// to perform the comparison on.</param>
        public PropertyComparer(string propertyName)
        {
            _PropertyName = propertyName;

            if (String.IsNullOrWhiteSpace(_PropertyName))
            {
                throw new ArgumentException("You must specify a property name to compare");
            }
        }

        public bool Equals(T x, T y)
        {
            var xValue = Dynamic.InvokeGet(x, _PropertyName);
            var yValue = Dynamic.InvokeGet(y, _PropertyName);

            if (xValue == null)
                return yValue == null;

            var test = xValue.Equals(yValue);

            if (!test) { Debugger.Break(); }

            return xValue.Equals(yValue);
        }

        public int GetHashCode(T obj)
        {
            object propertyValue = Dynamic.InvokeGet(obj, _PropertyName);

            if (propertyValue == null)
                return 0;
            else
                return propertyValue.GetHashCode();
        }
    }
}
