using System;
using System.Collections;
using NUnit.Framework;

namespace CS.Tests
{
    public class SimpleJsonTests
    {
        [Test]
        public void TestForUnexpectedCharacters()
        {
            AssertFails("invalid {key1: \"value1\"}", "Unexpected character: found 'i', was expecting '{' or '[' (at line 1, column 1)");
            AssertFails("\"string\"}", "Unexpected character: found '\"', was expecting '{' or '[' (at line 1, column 1)");
            AssertSucceeds("\t\r\n {key1: \"value1\"}", new Hashtable() { {"key1", "value1"}});
        }
        [Test]
        public void TestForMissingComma()
        {
            AssertFails("{key1: {key11: \"value11\", key12: \"value12\" key13: \"value13\"}}", "Unexpected character in hashtable: found 'k', was expecting ',' (at line 1, column 44)");
            AssertFails("{key1: [\"value11\", \"value12\" \"value13\"]}", "Unexpected character in array list: found '\"', was expecting ',' (at line 1, column 30)");
        }

        [Test]
        public void TestForMissingEnd()
        {
            AssertFails("{key1: \"value1\"", "Missing closing character for '{' (at line 1, column 16)");
            AssertFails("{key1: {key12: \"value12\"}", "Missing closing character for '{' (at line 1, column 26)");
            AssertFails("[\"value1\", \"value2\"", "Missing closing character for '[' (at line 1, column 20)");
        }

        [Test]
        public void TestForMismatchedCharacters()
        {
            AssertFails("{key1: \"value1\"]", "Invalid closing character: found ']', was expecting '}' (at line 1, column 16)");
            AssertFails("[value1}", "Invalid closing character: found '}', was expecting ']' (at line 1, column 8)");
            AssertSucceeds("{key1: \"value1]\"}", new Hashtable() { {"key1", "value1]"}});
            AssertSucceeds("[\"value1}\"]", new Hashtable() { { "SimpleJSON", new ArrayList() { "value1}" }}});
        }

        [Test]
        public void TestForInvalidHashtableKeys()
        {
            AssertFails("{key 1: \"value1\"}", "Unexpected character in hashtable key: found '1', was expecting ':' (at line 1, column 6)");
            AssertFails("{key\"1\": \"value1\"}", "Found '\"' in unquoted string (at line 1, column 5)\nFound '\"' in unquoted string (at line 1, column 7)");
            AssertFails("{: \"value1\"}", "Empty unquoted string (at line 1, column 1)\nEmpty hashtable key (at line 1, column 2)");
            AssertSucceeds("{key%1: \"value1\"}", new Hashtable() { {"key%1", "value1"}});
            AssertSucceeds("{ \t\r\n key_1 \t\r\n : \"value1\"}", new Hashtable() { {"key_1", "value1"}});
            AssertSucceeds("{ \t\r\n \"key_1\" \t\r\n : \"value1\"}", new Hashtable() { {"key_1", "value1"}});
        }

        [Test]
        public void TestForInvalidHashtableValues()
        {
            AssertFails("{key1: }", "Missing value for hashtable key: 'key1' (at line 1, column 7)");
            AssertFails("{key1: ,}", "Empty unquoted string (at line 1, column 7)");
            AssertSucceeds("{key1: value1}", new Hashtable() { { "key1", "value1" } });
            AssertSucceeds("{key1:   \t\r\n?}", new Hashtable() { { "key1", "?" } });
            AssertSucceeds("{key1: \t\r\n\"\"}", new Hashtable() { { "key1", "" } });
        }

        [Test]
        public void TestForInvalidArrayListValues()
        {
            AssertFails("[,]", "Empty unquoted string (at line 1, column 1)");
            AssertSucceeds("[]", null);
            AssertSucceeds("[\"value1\", value2]", new Hashtable() { { "SimpleJSON", new ArrayList() { "value1", "value2" }}});
            AssertSucceeds("[[\"value1\"]]", new Hashtable() { { "SimpleJSON", new ArrayList() { new ArrayList() { "value1" }}}});
            AssertSucceeds(
                "[{\"key1\": \"value1\"}]",
                new Hashtable() { { "SimpleJSON", new ArrayList() { new Hashtable() { {"key1", "value1" }}}}});
        }

        private static void AssertFails(string testCase, string message)
        {
            Assert.That(
                () => SimpleJsonImporter.Import(testCase),
                Throws.TypeOf<SimpleJsonException>().With.Message.EqualTo("Failed to fully parse JSON:\n" + message),
                "Failed to validate " + testCase);
        }

        private static void AssertSucceeds(string testCase, Hashtable expected)
        {
            Assert.That(
                () => SimpleJsonImporter.Import(testCase),
                Throws.Nothing,
                "Failed to validate " + testCase);
            Assert.That(
                SimpleJsonImporter.Import(testCase),
                Is.EqualTo(expected),
                "Failed to validate " + testCase);
        }
    }
}
