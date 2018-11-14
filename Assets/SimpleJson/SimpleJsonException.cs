using System;
using System.Collections.Generic;

public class SimpleJsonException : Exception
{
    public SimpleJsonException(List<string> warnings)
        : base(FormatMessage(warnings))
    {
    }

    private static string FormatMessage(List<string> warnings)
    {
        return "Failed to fully parse JSON:\n" + string.Join("\n", warnings.ToArray());
    }
}
