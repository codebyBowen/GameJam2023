using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable] 
public class ModableValue<T> : ISerializationCallbackReceiver {

  public T InitVal;
  public List<ValueModification<T>> Modifications = new List<ValueModification<T>>();

  public ModableValue(T InitVal) {
    this.InitVal = InitVal;
  }

  public void OnBeforeSerialize() {
  }

  public void OnAfterDeserialize() {
    Modifications = new List<ValueModification<T>>();
  }

  public T Get() => Modifications.OrderBy(x=>x).Aggregate(InitVal, (val, mod) => (T)mod.__Delegate.DynamicInvoke(val));

  public ModableValue<T> Clone() {
    ModableValue<T> obj = (ModableValue<T>)this.MemberwiseClone();
    obj.Modifications = this.Modifications.ConvertAll(x => x.Clone());
    return obj;
  }
}
