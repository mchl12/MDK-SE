using System;

namespace Malware.MDKUtilities
{
    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Field
        | AttributeTargets.Method | AttributeTargets.Property)]
    public class NoRenameAttribute : Attribute { }
}
