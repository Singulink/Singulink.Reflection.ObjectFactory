using System;

namespace Singulink.Reflection.Tests;

public class PrivateDefaultConstructor
{
    public bool InitializerCalled { get; } = true;

    private PrivateDefaultConstructor()
    {
    }
}
