using System;
using System.Linq.Expressions;
using System.Reflection;
using ChatCommands.Abstractions;

namespace ChatCommands.Services;

public class MemberExpressionCache<T, TProperty> : IMemberExpressionCache<T>
{
    protected readonly Lazy<Func<T, TProperty>> Getter;
    protected readonly Lazy<Action<T, TProperty>> Setter;

    public string Name { get; }

    public Type InstanceType { get; }

    public Type PropertyType { get; }

    public TProperty GetValue(T instance) => Getter.Value.Invoke(instance);

    public void SetValue(T instance, TProperty value) => Setter.Value.Invoke(instance, value);

    public MemberExpressionCache(string propertyName)
    {
        Name = propertyName;
        InstanceType = typeof(T);
        PropertyType = typeof(TProperty);

        Getter = new Lazy<Func<T, TProperty>>(CreateGetter);
        Setter = new Lazy<Action<T, TProperty>>(CreateSetter);
    }

    public MemberExpressionCache(Expression<Func<T, TProperty>> getter)
    {
        Getter = new Lazy<Func<T, TProperty>>(getter.Compile);

        if (getter.Body is MemberExpression {Member.Name: {} propertyName})
        {
            Name = propertyName;
            Setter = new Lazy<Action<T, TProperty>>(CreateSetter);
        }
    }

    protected virtual Func<T, TProperty> CreateGetter()
    {
        var instance = Expression.Parameter(InstanceType);
        var propertyAccessor = Expression.PropertyOrField(instance, Name);
        var action = Expression.Lambda<Func<T, TProperty>>(propertyAccessor, instance);
        return action.Compile();
    }

    protected virtual Action<T, TProperty> CreateSetter()
    {
        var instance = Expression.Parameter(InstanceType);
        var value = Expression.Parameter(PropertyType);
        var propertyAccessor = Expression.PropertyOrField(instance, Name);
        var assignment = Expression.Assign(propertyAccessor, value);
        var action = Expression.Lambda<Action<T, TProperty>>(assignment, instance, value);
        return action.Compile();
    }
}

public class MemberExpressionCache<T> : MemberExpressionCache<T, object>
{
    public MemberExpressionCache(string propertyName) : base(propertyName)
    {
    }

    protected override Func<T, object> CreateGetter()
    {
        var instance = Expression.Parameter(InstanceType);
        var propertyAccessor = Expression.PropertyOrField(instance, Name);
        var cast = Expression.Convert(propertyAccessor, typeof(object));
        var action = Expression.Lambda<Func<T, object>>(cast, instance);
        return action.Compile();
    }

    protected override Action<T, object> CreateSetter()
    {
        var instance = Expression.Parameter(InstanceType);
        var value = Expression.Parameter(typeof(object));
        var propertyAccessor = Expression.PropertyOrField(instance, Name);
        var convert = Expression.Convert(value, ((FieldInfo) propertyAccessor.Member).FieldType);
        var assignment = Expression.Assign(propertyAccessor, convert);
        var action = Expression.Lambda<Action<T, object>>(assignment, instance, value);
        return action.Compile();
    }
}