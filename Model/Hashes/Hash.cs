using System.Collections.Generic;
using System.Linq;

namespace Model.Hashes;

public static class Hash
{
    public static int Get(int v1, int v2)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + v1;
            hash = hash * 31 + v2;
            return hash;
        }
    }

    public static int Get(this IEnumerable<int> list)
    {
        return list.Aggregate(Get);
    }
}