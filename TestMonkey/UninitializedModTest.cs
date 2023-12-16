using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UIXDialogBuilder
{
    [TestClass]
    public class UninitializedModTest
    {
        [TestMethod]
        public void TestModHasNonEmptyName()
        {
            var mod = UIXDialogBuilderModMonkey.Instance;
            Assert.IsNotNull(mod.Name);
            Assert.AreNotEqual("", mod.Name);
        }
    }
}
