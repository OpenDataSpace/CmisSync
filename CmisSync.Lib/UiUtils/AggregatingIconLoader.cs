
namespace CmisSync.Lib.UiUtils {
    using System;
    public class AggregatingIconLoader : IIconLoader {
        private IIconLoader primaryLoader;
        private IIconLoader fallbackLoader;
        public AggregatingIconLoader(IIconLoader primaryLoader, IIconLoader fallbackLoader, params IIconLoader[] moreFallBackLoader) {
            if (primaryLoader == null) {
                throw new ArgumentNullException("Given primary loader is null");
            }

            if (fallbackLoader == null) {
                throw new ArgumentNullException("Given fallback loader is null");
            }

            this.primaryLoader = primaryLoader;

            if (moreFallBackLoader.Length > 0) {
                IIconLoader[] moreLoader = new IIconLoader[moreFallBackLoader.Length - 1];
                Array.Copy(moreFallBackLoader, moreLoader, moreLoader.Length);
                this.fallbackLoader = new AggregatingIconLoader(fallbackLoader, moreFallBackLoader[0], moreLoader);
            } else {
                this.fallbackLoader = fallbackLoader;
            }
        }

        public string GetPathOf(Icons icon) {
            return this.primaryLoader.GetPathOf(icon) ?? this.fallbackLoader.GetPathOf(icon);
        }
    }
}