using System;
using System.Linq.Expressions;
using Brite.App.Win.Helpers;
using ReactiveUI;

namespace Brite.App.Win.Extensions
{
    public static class ReactiveObjectExtensions
    {
        public static void RaisePropertyChanged<TSender, T>(this TSender obj, Expression<Func<T>> expression)
            where TSender : IReactiveObject
        {
            obj.RaisePropertyChanged(ExpressionHelper.Name(expression));
        }
    }
}
