
using System;
using System.Collections.Generic;

public static class SharpUtils {
    public static void ForEachWithIndex<T>(this IEnumerable<T> ie, Action<T, int> action) {
        var i = 0;
        foreach (var e in ie) action(e, i++);
    }
}
