using System.Reflection;

public static class GoldScrapPrivateAccesser
{
    //Method copied from https://www.meziantou.net/access-private-members-of-a-class-using-reflection.htm
    public static T GetPrivateField<T>(this object obj, string name)
    {
        FieldInfo field = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
        {
            return default(T);
        }
        return (T)field.GetValue(obj);
    }

    public static FieldInfo SetPrivateValue<T>(this object obj, string name)
    {
        FieldInfo field = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
        {
            return default(FieldInfo);
        }
        return field;
    }
}