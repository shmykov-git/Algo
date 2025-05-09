using System.Collections.Generic;
using System.Linq;

namespace meta.Tools;

public static class LongHash
{
    public static long Get(long v1, long v2)
    {
        unchecked
        {
            long hash = 17;
            hash = hash * 31 + v1;
            hash = hash * 31 + v2;
            return hash;
        }
    }

    public static ulong GetU(ulong v1, ulong v2) => (ulong)Get((long)v1, (long)v2);

    public static long Get(this IEnumerable<long> list)
    {
        return list.Aggregate(37L, Get);
    }

    public static ulong GetU(this IEnumerable<ulong> list)
    {
        return list.Aggregate(37ul, GetU);
    }

    public static long Get(string s)
    {
        return s.Select(c => (long)c).Get();
    }
    public static ulong GetU(string s)
    {
        return s.Select(c => (ulong)c).GetU();
    }

    public static long Get(this byte[] bytes)
    {
        return bytes.Select(c => (long)c).Get();
    }

    public static string GetLongHashId(this string s)
    {
        return s.GetULongHash().ToString();
    }

    public static ulong GetULongHash(this string s)
    {
        return GetU(s);
    }

    public static ulong GetULongHash(this IEnumerable<string> ss)
    {
        return ss.Select(GetULongHash).Aggregate(37ul, GetU);
    }

    public static ulong GetULongHash(this IEnumerable<IEnumerable<string>> sss)
    {
        return sss.Select(GetULongHash).Aggregate(37ul, GetU);
    }
}