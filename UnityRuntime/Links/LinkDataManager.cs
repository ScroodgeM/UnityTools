//this empty line for UTF-8 BOM header

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityTools.Runtime.Links;

namespace UnityTools.UnityRuntime.Links
{
    public class LinkDataManager<TL, TD> where TL : LinkBase where TD : UnityEngine.Object
    {
        private readonly Dictionary<TL, TD> cache = new Dictionary<TL, TD>();

        public TD GetByLink(TL link)
        {
            if (cache.TryGetValue(link, out TD cachedObject) == false)
            {
                cachedObject = Resources.Load<TD>(Path.Combine(LinkBase.GetPathForAssetInsideResources<TL>(), link.LinkedObjectId));
                cache.Add(link, cachedObject);
            }

            return cachedObject;
        }
    }
}
