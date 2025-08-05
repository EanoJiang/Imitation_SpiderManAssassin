using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations
{
    public class Tags : MonoBehaviour, ITag
    {
        public List<Tag> tags = new List<Tag>();
        protected Dictionary<int, Tag> tags_Dic;

        private void Start()
        {
            tags_Dic = new Dictionary<int, Tag>();

            foreach (var tag in tags)
            {
                if (tag == null) continue;

                if (!tags_Dic.ContainsValue(tag))
                {
                    tags_Dic.Add(tag.ID, tag);
                }
            }

            tags = new List<Tag>();

            foreach (var item in tags_Dic)
            {
                tags.Add(item.Value);
            }
        }

        public bool HasTag(Tag tag)
        {
            return tags_Dic.ContainsValue(tag);
        }

        public bool HasTag(int key)
        {
            return tags_Dic.ContainsKey(key);
        }
    }
}