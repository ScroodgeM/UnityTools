using System;
using UnityEngine;

namespace UnityTools.UnityRuntime.Links
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LinksDisplayFilterForInspectorAttribute : PropertyAttribute
    {
        public readonly string filterMethodName;

        /// <summary>
        /// expected signature for this method is:
        /// static bool [FilterMethodName](T linkTarget)
        /// where T is LinkTarget type, not link itself
        /// </summary>
        /// <param name="filterMethodName"></param>
        public LinksDisplayFilterForInspectorAttribute(string filterMethodName) => this.filterMethodName = filterMethodName;
    }
}
