using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Monads;

using AutoMapper;

namespace NuGetFeed.Infrastructure.AutoMapper
{
    public static class AutoMapperExtensions
    {
        public static List<TResult> MapTo<TResult>(this IEnumerable self)
        {
            self.CheckNull("self");

            return (List<TResult>)Mapper.Map(self, self.GetType(), typeof(List<TResult>));
        }

        public static List<TResult> MapToDynamic<TResult>(this IEnumerable self)
        {
            self.CheckNull("self");

            var result = (from object i in self select i.MapToDynamic<TResult>()).ToList();

            return result;
        }

        public static TResult MapTo<TResult>(this object self)
        {
            self.CheckNull("self");

            return (TResult)Mapper.Map(self, self.GetType(), typeof(TResult));
        }

        public static TResult MapToDynamic<TResult>(this object self)
        {
            self.CheckNull("self");

            return (TResult)Mapper.DynamicMap(self, self.GetType(), typeof(TResult));
        }
    }
}