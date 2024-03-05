
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.Utilities
{
    public static class LinqUtils
    {
        public static IQueryable<TEntity> DynamicFilter<TEntity>(this IQueryable<TEntity> source, TEntity entity)
        {
            foreach (PropertyInfo property in entity.GetType().GetProperties())
            {
                if (!(entity.GetType().GetProperty(property.Name) == null))
                {
                    object data = entity.GetType().GetProperty(property.Name)
                        ?.GetValue((object)entity, (object[])null);
                    if (data != null &&
                        !property.CustomAttributes.Any(
                            (Func<CustomAttributeData, bool>)(a => a.AttributeType == typeof(SkipAttribute))))
                    {
                        if (property.CustomAttributes.Any(
                                (Func<CustomAttributeData, bool>)(a => a.AttributeType == typeof(StringAttribute))))
                        {
                            source = source.Where<TEntity>(property.Name + ".ToLower().Contains(@0)",
                                (object)data.ToString().ToLower());
                        }
                        else if (property.CustomAttributes.Any(
                                (Func<CustomAttributeData, bool>)(a => a.AttributeType == typeof(IntAttribute))))
                        {
                            source = source.Where<TEntity>(property.Name + " == @0",
                                (object)data);
                        }
                        else if (property.CustomAttributes.Any(a => a.AttributeType == typeof(BooleanAttribute)))
                        {
                            source = source.Where<TEntity>(property.Name + "== @0", (object)data);
                        }
                        else if (property.CustomAttributes.Any(
                                     (Func<CustomAttributeData, bool>)(a =>
                                        a.AttributeType == typeof(ChildAttribute))))
                        {
                            foreach (PropertyInfo propertyChild in property.PropertyType.GetProperties())
                            {
                                object dataChild = data.GetType().GetProperty(propertyChild.Name)
                                    ?.GetValue(data, (object[])null);
                                if (dataChild != null)
                                {
                                    source = source.Where<TEntity>(string.Format("{0}.{1}=\"{2}\"", property.Name,
                                        propertyChild.Name, dataChild));
                                }
                            }
                        }
                        else if (property.CustomAttributes.Any(
                                     (Func<CustomAttributeData, bool>)(a =>
                                        a.AttributeType == typeof(DateRangeAttribute))))
                        {
                            DateTime date = (DateTime)data;
                            string predicate = property.Name.Equals("StartDate")
                                ? $"{property.Name} >= @0"
                                : $"{property.Name} <= @0";

                            object[] dateRange = property.Name.Equals("StartDate")
                                ? new object[] { date.Date }
                                : new object[] { date.Date };

                            source = source.Where<TEntity>(predicate, dateRange);

                        }                      
                    }
                }
            }

            return source;
        }
    }
}
