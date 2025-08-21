using System.Collections.Generic;
using UnityEngine;

namespace FS_ThirdPerson
{
    public static class ListExtension
    {
        public static T GetRandom<T>(this List<T> list)
        {
            return list.Count > 0 ? list[Random.Range(0, list.Count)] : default(T);
        }
    }
}