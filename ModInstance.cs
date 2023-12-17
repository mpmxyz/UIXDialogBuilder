using System;

namespace UIXDialogBuilder
{
    public static class ModInstance
    {
        private static IUIXDialogBuilderMod current;

        public static IUIXDialogBuilderMod Current
        {
            get
            {
                if (current == null)
                {
                    throw new InvalidOperationException("Mod has not been instantiated yet!");
                }
                return current;
            }

            internal set
            {
                current = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
    }
}
