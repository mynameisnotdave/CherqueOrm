using NUnit.Framework;
using System;
using System.Collections.Generic;
using AwesomeAssertions;

namespace CherqueOrmTests;

[TestFixture]
public class TypeIdentificationTests
{
    // The test method now directly accepts a Type object, avoiding generic inference issues with 'null' values.
    [TestCaseSource(nameof(GetTypesToTest))]
    public void PrimitiveAndSimpleTypeChecksReturnCorrectly(Type typeToTest)
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

        isPrimitiveOrSimple.Should().BeTrue($"Type {typeToTest.Name} should be identified as primitive or simple mappable.");
    }

    // This method provides the Type objects to be tested.
    private static IEnumerable<TestCaseData> GetTypesToTest()
    {
        // 1. Primitive Types (from Type.IsPrimitive)
        yield return new TestCaseData(typeof(bool));
        yield return new TestCaseData(typeof(byte));
        yield return new TestCaseData(typeof(sbyte));
        yield return new TestCaseData(typeof(short));
        yield return new TestCaseData(typeof(ushort));
        yield return new TestCaseData(typeof(int));
        yield return new TestCaseData(typeof(uint));
        yield return new TestCaseData(typeof(long));
        yield return new TestCaseData(typeof(ulong));
        yield return new TestCaseData(typeof(char));
        yield return new TestCaseData(typeof(double));
        yield return new TestCaseData(typeof(float));
        yield return new TestCaseData(typeof(IntPtr));
        yield return new TestCaseData(typeof(UIntPtr));

        // 2. Nullable Primitive Types
        yield return new TestCaseData(typeof(bool?));
        yield return new TestCaseData(typeof(byte?));
        yield return new TestCaseData(typeof(sbyte?));
        yield return new TestCaseData(typeof(short?));
        yield return new TestCaseData(typeof(ushort?));
        yield return new TestCaseData(typeof(int?));
        yield return new TestCaseData(typeof(uint?));
        yield return new TestCaseData(typeof(long?));
        yield return new TestCaseData(typeof(ulong?));
        yield return new TestCaseData(typeof(char?));
        yield return new TestCaseData(typeof(double?));
        yield return new TestCaseData(typeof(float?));
        yield return new TestCaseData(typeof(IntPtr?));
        yield return new TestCaseData(typeof(UIntPtr?));

        // 3. Other Common "Simple Mappable" Types (not strictly primitive)
        yield return new TestCaseData(typeof(string)); // String is a reference type, but simple mappable
        yield return new TestCaseData(typeof(decimal));
        yield return new TestCaseData(typeof(DateTime));
        yield return new TestCaseData(typeof(Guid));
        yield return new TestCaseData(typeof(TimeSpan));
        yield return new TestCaseData(typeof(DateTimeOffset));

        // 4. Nullable Other Common "Simple Mappable" Types
        yield return new TestCaseData(typeof(decimal?));
        yield return new TestCaseData(typeof(DateTime?));
        yield return new TestCaseData(typeof(Guid?));
        yield return new TestCaseData(typeof(TimeSpan?));
        yield return new TestCaseData(typeof(DateTimeOffset?));

        // 5. Enums (example)
        yield return new TestCaseData(typeof(DayOfWeek)); // System.DayOfWeek enum
        yield return new TestCaseData(typeof(DayOfWeek?)); // Nullable enum

        // If you want to test complex types (which should return false)
        // yield return new TestCaseData(typeof(object)).Returns(false); // Example of a negative test case
        // yield return new TestCaseData(typeof(List<int>)).Returns(false);
    }
}