using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using ChatCommands.Abstractions;

namespace ChatCommands.Services
{
    public class ExpressionCache<T> : IExpressionCache<T>
    {
        public Dictionary<string, IMemberExpressionCache<T>> Cache { get; }

        public ExpressionCache()
        {
            Cache = new Dictionary<string, IMemberExpressionCache<T>>();
        }

        public MemberExpressionCache<T, TProperty> Get<TProperty>(Expression<Func<T, TProperty>> getter)
        {
            if (getter.Body is MemberExpression {Member.Name: { } propertyName} && Cache.ContainsKey(propertyName) &&
                Cache[propertyName] is MemberExpressionCache<T, TProperty> me)
                return me;
            var memberExpr = new MemberExpressionCache<T, TProperty>(getter);
            Cache.Add(memberExpr.Name, memberExpr);
            return memberExpr;
        }

        public MemberExpressionCache<T, TProperty> Get2<TProperty>(string propertyName)
        {
            if (Cache.ContainsKey(propertyName))
                return (MemberExpressionCache<T, TProperty>)Cache[propertyName];
            var memberExpr = new MemberExpressionCache<T, TProperty>(propertyName);
            Cache.Add(memberExpr.Name, memberExpr);
            return memberExpr;
        }
    }
}
