using System;

namespace Singulink.Reflection;

#if NETSTANDARD

internal class DynamicallyAccessedMembers : Attribute
{
    public DynamicallyAccessedMemberTypes MemberTypes { get; }

    public DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes memberTypes)
    {
        MemberTypes = memberTypes;
    }
}

#endif