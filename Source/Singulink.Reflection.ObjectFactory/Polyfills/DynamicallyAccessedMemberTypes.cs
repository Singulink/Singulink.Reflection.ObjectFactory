namespace System.Diagnostics.CodeAnalysis;

#if NETSTANDARD

internal enum DynamicallyAccessedMemberTypes
{
    None = 0,
    PublicParameterlessConstructor = 1,
    PublicConstructors = 2,
    NonPublicConstructors = 4,
}

#endif