// CachingConstantProvider.cs

using SearchitLibrary.Abstractions;

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
            _cached = constants;
            _innerProvider.Save(constants);
        }
    }
}