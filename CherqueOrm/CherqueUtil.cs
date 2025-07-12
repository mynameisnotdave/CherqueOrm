namespace CherqueOrm;

internal sealed class CherqueUtil
{
    internal static bool IsPrimitiveOrSimpleType(Type typeToTest)
    {
        // Get the underlying type if it's a nullable type (e.g., int? -> int)
        Type actualType = Nullable.GetUnderlyingType(typeToTest) ?? typeToTest;

        // The logic for identifying "primitive or simple mappable" types
        bool isPrimitiveOrSimple = actualType.IsPrimitive ||          // Covers bool, int, float, etc.
                                   actualType == typeof(string) ||    // string (a reference type, but simple mappable)
                                   actualType.IsEnum ||               // Any enum type
                                   actualType == typeof(decimal) ||   // decimal
                                   actualType == typeof(DateTime) ||  // DateTime
                                   actualType == typeof(Guid) ||      // Guid
                                   actualType == typeof(TimeSpan) ||  // TimeSpan
                                   actualType == typeof(DateTimeOffset); // DateTimeOffset

        return isPrimitiveOrSimple;
    }
}