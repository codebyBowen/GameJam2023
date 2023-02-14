using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public class ValueModification<T> : IComparable<ValueModification<T>> {
  static public readonly ParameterExpression PlaceHolder = Expression.Parameter(typeof(T));
  static private readonly List<ParameterExpression> __Params = new List<ParameterExpression>{ PlaceHolder };

  public readonly Expression Operation;
  public int priority;

  internal readonly Delegate __Delegate;

  public ValueModification(Expression Operation, int priority) {
    this.Operation = Operation;
    this.priority = priority;

    this.__Delegate = Expression.Lambda(Operation, __Params).Compile();
  }

  public int CompareTo(ValueModification<T> other) {
    // Decending sort
    return -this.priority.CompareTo(other.priority);
  }

  public ValueModification<T> Clone() {
    // FIXME: Assuming all instance attributes are immutable
    return (ValueModification<T>)this.MemberwiseClone();
  }
}
