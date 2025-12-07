using System;
using UnityEngine;
using UnityTools.Runtime.Links;

namespace UnityTools.UnityRuntime.Links
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LinksDisplayFilterForInspectorAttribute : PropertyAttribute
    {
        public readonly Func<string, bool> filter;

        public LinksDisplayFilterForInspectorAttribute(Func<string, bool> filter) => this.filter = filter;
    }
}
