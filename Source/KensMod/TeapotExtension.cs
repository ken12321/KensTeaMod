using Verse;

namespace KensMod
{
    // Per-def teapot settings, so fancier teapots can hold more later.
    public class TeapotExtension : DefModExtension
    {
        public int capacity = 50;

        // Haulers top the pot up once at least this many cups are missing.
        public int refillWhenMissing = 10;
    }
}
