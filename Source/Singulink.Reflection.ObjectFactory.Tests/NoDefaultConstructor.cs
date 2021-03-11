using System;

namespace Singulink.Reflection.Tests
{
    public class NoDefaultConstructor
    {
        public bool InitializerCalled { get; } = true;

        public string ArgValue { get; }

        private NoDefaultConstructor(string argValue)
        {
            ArgValue = argValue;
        }
    }
}
