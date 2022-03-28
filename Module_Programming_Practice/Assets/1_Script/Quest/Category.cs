using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Quest/Category", fileName = "Category ")]
public class Category : ScriptableObject, IEquatable<Category>
{
    [SerializeField] string codeName;
    public string CodeName => codeName;
    [SerializeField] string displayName;
    public string DisplayName => displayName;

    #region Operator
    public bool Equals(Category _other)
    {
        if (_other == null) return false;
        if (ReferenceEquals(_other, this)) return true;
        if (GetType() != _other.GetType()) return false;
        return codeName == _other.CodeName;
    }

    public override int GetHashCode() => (CodeName, DisplayName).GetHashCode();
    public override bool Equals(object other) => base.Equals(other);

    public static bool operator == (Category lhs, string rhs)
    {
        if (lhs is null) return ReferenceEquals(lhs, null);
        return lhs.CodeName == rhs || lhs.DisplayName == rhs;
    }
    public static bool operator != (Category lhs, string rhs) => !(lhs == rhs);
    #endregion
}
