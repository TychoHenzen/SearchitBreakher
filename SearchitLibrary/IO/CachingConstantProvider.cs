// CachingConstantProvider.cs
using SearchitLibrary;
using SearchitLibrary.Abstractions;
using System;

namespace SearchitLibrary.IO
{
    public class CachingConstantProvider : IConstantProvider
    {
        private readonly IConstantProvider _innerProvider;
        private Constants _cached;

        public CachingConstantProvider(IConstantProvider innerProvider)
        {
            _innerProvider = innerProvider;
            _cached = _innerProvider.Get();
        }

        public Constants Get() => _cached;

        public void Save(Constants constants)
        {
            if (AreEqual(_cached, constants)) return;
            
            _cached = constants;
            _innerProvider.Save(constants);
        }

        private static bool AreEqual(Constants a, Constants b)
        {
            return Math.Abs(a.LookSpeed - b.LookSpeed) < 0.0001f &&
                   Math.Abs(a.MoveSpeed - b.MoveSpeed) < 0.0001f;
        }
    }
}