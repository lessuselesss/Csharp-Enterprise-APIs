using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using static CircularEnterpriseApis.Utils;
using System;

namespace CircularEnterpriseApis.UnitTests
{
    /// <summary>
    /// Unit tests for Utils class
    /// Maps to Go: pkg/utils/utils_test.go
    /// </summary>
    public class UtilsTests
    {
        [Fact]
        public void StringToHex_EmptyString_ReturnsEmpty()
        {
            string result = StringToHex("");
            result.Should().Be("");
        }

        [Fact]
        public void StringToHex_SimpleString_ReturnsCorrectHex()
        {
            string result = StringToHex("hello");
            result.Should().Be("68656C6C6F");
        }

        [Fact]
        public void StringToHex_UnicodeString_ReturnsCorrectHex()
        {
            string result = StringToHex("Hello World!");
            result.Should().Be("48656C6C6F20576F726C6421");
        }

        [Fact]
        public void HexToString_EmptyHex_ReturnsEmpty()
        {
            string result = HexToString("");
            result.Should().Be("");
        }

        [Fact]
        public void HexToString_ValidHex_ReturnsCorrectString()
        {
            string result = HexToString("68656c6c6f");
            result.Should().Be("hello");
        }

        [Fact]
        public void HexToString_HexWithPrefix_ReturnsCorrectString()
        {
            string result = HexToString("0x48656c6c6f20576f726c6421");
            result.Should().Be("Hello World!");
        }

        [Fact]
        public void HexToString_InvalidHex_ReturnsEmpty()
        {
            string result = HexToString("invalid");
            result.Should().Be("");
        }

        [Fact]
        public void HexToString_OddLengthHex_ReturnsEmpty()
        {
            string result = HexToString("abc");
            result.Should().Be("");
        }

        [Fact]
        public void HexFix_EmptyString_ReturnsEmpty()
        {
            string result = HexFix("");
            result.Should().Be("");
        }

        [Fact]
        public void HexFix_WithPrefix_RemovesPrefix()
        {
            string result = HexFix("0xABCD");
            result.Should().Be("abcd");
        }

        [Fact]
        public void HexFix_UpperCase_ReturnsLowerCase()
        {
            string result = HexFix("ABCDEF");
            result.Should().Be("abcdef");
        }

        [Fact]
        public void PadNumber_SingleDigit_PadsCorrectly()
        {
            string result = PadNumber(5);
            result.Should().Be("05");
        }

        [Fact]
        public void PadNumber_DoubleDigit_NoChange()
        {
            string result = PadNumber(15);
            result.Should().Be("15");
        }

        [Fact]
        public void PadNumber_Zero_PadsCorrectly()
        {
            string result = PadNumber(0);
            result.Should().Be("00");
        }

        [Fact]
        public void GetFormattedTimestamp_ReturnsCorrectFormat()
        {
            string result = GetFormattedTimestamp();

            // Should match format: YYYY:MM:DD-HH:MM:SS
            result.Should().MatchRegex(@"^\d{4}:\d{2}:\d{2}-\d{2}:\d{2}:\d{2}$");

            // Should be a valid timestamp close to now
            DateTime.TryParseExact(result, "yyyy:MM:dd-HH:mm:ss", null,
                System.Globalization.DateTimeStyles.None, out DateTime parsed).Should().BeTrue();

            // Should be within last minute (allowing for test execution time)
            var timeDiff = DateTime.UtcNow - parsed;
            timeDiff.TotalMinutes.Should().BeLessThan(1);
        }

        [Theory]
        [InlineData("hello", "68656C6C6F")]
        [InlineData("world", "776F726C64")]
        [InlineData("test", "74657374")]
        public void StringToHex_HexToString_RoundTrip(string original, string expectedHex)
        {
            // Test round trip conversion
            string hex = StringToHex(original);
            hex.Should().Be(expectedHex);

            string restored = HexToString(hex);
            restored.Should().Be(original);
        }
    }
}