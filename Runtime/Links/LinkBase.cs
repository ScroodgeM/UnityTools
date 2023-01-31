
using System.IO;

namespace UnityTools.Runtime.Links
{
    public abstract class LinkBase
    {
        public const string EmptyLinkKeyword = "null";

        public abstract string LinkedObjectId { get; }

        private int cachedHashCode;

        public static bool HasValue(LinkBase link)
        {
            return
                link is LinkBase
                &&
                string.IsNullOrEmpty(link.LinkedObjectId) == false
                &&
                link.LinkedObjectId != EmptyLinkKeyword;
        }

        public static bool operator ==(LinkBase a, LinkBase b)
        {
            return HasValue(a)
                && HasValue(b)
                && a.GetType() == b.GetType()
                && string.Equals(a.LinkedObjectId, b.LinkedObjectId);
        }

        public static bool operator !=(LinkBase a, LinkBase b) => (a == b) == false;

        public override bool Equals(object obj) => obj is LinkBase linkBase && this == linkBase;

        public override int GetHashCode()
        {
            if (cachedHashCode == 0)
            {
                if (HasValue(this) == false)
                {
                    return 0;
                }

                cachedHashCode = LinkedObjectId.GetHashCode();
            }

            return cachedHashCode;
        }

        public override string ToString() { return $"{GetType()}: {LinkedObjectId}"; }

        public static string GetResourcesPathForAsset<T>() => Path.Combine("Resources", GetPathForAssetInsideResources<T>());

        public static string GetPathForAssetInsideResources<T>() => Path.Combine("LinkTargets", typeof(T).Name).Replace(@"\", @"/");
    }
}
