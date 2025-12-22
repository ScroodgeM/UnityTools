using System;
using System.Collections.Generic;
using System.IO;
using UnityTools.Runtime.Links;

namespace UnityTools.UnityRuntime.Links
{
    public class LinkDataManager<TL, TD> where TL : LinkBase where TD : UnityEngine.Object
    {
        private readonly bool useCache;
        private readonly Dictionary<string, TD> cache = new Dictionary<string, TD>();

        public LinkDataManager(bool useCache = true)
        {
            this.useCache = useCache;
        }

        public TD GetByLink(TL link)
        {
            if (LinkBase.HasValue(link) == false)
            {
                throw new ArgumentNullException($"Attempt to load asset by link with no linked object id. Link type: {typeof(TL).Name}");
            }

            if (useCache == false)
            {
                return LoadFromResources(link);
            }

            if (cache.TryGetValue(link.LinkedObjectId, out TD cachedObject) == false)
            {
                cachedObject = LoadFromResources(link);
                cache.Add(link.LinkedObjectId, cachedObject);
            }

            return cachedObject;
        }

        public void ClearCachedObjects()
        {
            cache.Clear();
        }

        private TD LoadFromResources(TL link)
        {
            TD asset = UnityEngine.Resources.Load<TD>(Path.Combine(LinkBase.GetPathForAssetInsideResources<TD>(), link.LinkedObjectId));

            if (asset == null)
            {
                throw new ArgumentException($"Attempt to load missing asset. Link type: {typeof(TL).Name}. Linked object: {link.LinkedObjectId}");
            }

            return asset;
        }
    }
}
