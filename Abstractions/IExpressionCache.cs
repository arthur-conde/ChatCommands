using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using ChatCommands.Services;

namespace ChatCommands.Abstractions
{
    public interface IExpressionCache<T>
    {
        MemberExpressionCache<T, TProperty> Get<TProperty>(Expression<Func<T, TProperty>> getter);

        MemberExpressionCache<T, TProperty> Get2<TProperty>(string propertyName);
    }
}
