namespace Haxbot.Extensions;

public static class TypeExtensions
{
    public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
    {
        bool IsAssignableTo(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == genericType;

        if (IsAssignableTo(givenType) || givenType.GetInterfaces().Any(IsAssignableTo))
            return true;

        return givenType.BaseType is not null && IsAssignableToGenericType(givenType.BaseType, genericType);
    }
}