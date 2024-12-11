using System;
using System.Reflection;

public static class PrivateAccesser
{
    //Method copied from https://www.meziantou.net/access-private-members-of-a-class-using-reflection.htm
    public static T GetPrivateField<T>(this object obj, string name)
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = obj.GetType();
        FieldInfo field = type.GetField(name, flags);
        return (T)field.GetValue(obj);
    }

    public static FieldInfo SetPrivateValue<T>(this object obj, string name)
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = obj.GetType();
        FieldInfo field = type.GetField(name, flags);
        return field;
    }
}