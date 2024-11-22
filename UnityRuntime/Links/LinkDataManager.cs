//this empty line for UTF-8 BOM header

using System.Collections.Generic;
using System.IO;
using UnityTools.Runtime.Links;

namespace UnityTools.UnityRuntime.Links
{
    public class LinkDataManager<TL, TD> where TL : LinkBase where TD : UnityEngine.Object
    {
        private readonly bool useCache;
        private readonly Dictionary<TL, TD> cache = new Dictionary<TL, TD>();

        public LinkDataManager(bool useCache = true)
        {
            this.useCache = useCache;
        }

        public TD GetByLink(TL link)
        {
            if (useCache == false)
            {
                return LoadFromResources(link);
            }

            if (cache.TryGetValue(link, out TD cachedObject) == false)
            {
                cachedObject = LoadFromResources(link);
                cache.Add(link, cachedObject);
            }

            return cachedObject;
        }

        public void ClearCachedObjects()
        {
            cache.Clear();
        }

        private TD LoadFromResources(TL link)
        {
            return UnityEngine.Resources.Load<TD>(Path.Combine(LinkBase.GetPathForAssetInsideResources<TD>(), link.LinkedObjectId));
        }
    }
}
