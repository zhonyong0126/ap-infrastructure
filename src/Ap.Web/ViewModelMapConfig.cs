using AutoMapper;
using System.Linq;
using System.Collections.Generic;
using Ap.Infrastructure;
using System;

namespace Ap.Web
{
    /// <summary>
    /// ViewModel的AutoMapper映配置
    /// </summary>
    static class ViewModelMapConfig
    {
        // public static string ToLongDateTime(this DateTime? dt)
        // {
        //     if (!dt.HasValue)
        //     {
        //         return "-";
        //     }

        //     return dt.Value.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
        // }

        // public static string ToShortDateTime(this DateTime? dt)
        // {
        //     if (!dt.HasValue)
        //     {
        //         return "-";
        //     }

        //     return dt.Value.AddHours(8).ToString("yyyy-MM-dd");
        // }

        // public static DateTime? WithBeginTime(this DateTime? dt)
        // {
        //     if (dt == null)
        //     {
        //         return dt;
        //     }

        //     return dt.Value;
        // }

        // public static DateTime? WithEndTime(this DateTime? dt)
        // {
        //     if (dt == null)
        //     {
        //         return dt;
        //     }

        //     return dt.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
        // }
        

        public static TDto ToDto<TDto>(this IRequest request) where TDto : IDto, new()
        {
            if (null == request)
            {
                return new TDto();
            }
            return Mapper.Map<IRequest, TDto>(request);
        }

        public static TDto ToDto<TDto>(this IRequest request, IOperatorProvider operatorProvider) where TDto : IDto, IHavingOperator, new()
        {
            if (null == request)
            {
                return new TDto();
            }
            var dto = Mapper.Map<IRequest, TDto>(request);
            dto.Operator = operatorProvider.GetOperator();
            return dto;
        }

        public static TResponse ToResponse<TResponse>(this object obj) where TResponse : class, IResponse, new()
        {
            if (obj == null)
            {
                return null;
            }
            return Mapper.Map<TResponse>(obj);
        }

        public static TResponse[] ToResponses<TSource, TResponse>(this IEnumerable<TSource> sources) where TResponse : class, IResponse, new()
        {
            return sources.Select(_ => Mapper.Map<TResponse>(_)).ToArray();
        }
    }
}