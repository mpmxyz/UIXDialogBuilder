using Elements.Core;
using FrooxEngine;
using System.Collections.Generic;

namespace UIXDialogBuilder
{

    /// <summary>
    /// This class demonstrates how to use the library.
    /// </summary>
    internal class TestDialogState : IDialogState
    {
        private class ListMapper : IReversibleMapper<List<string>, string>
        {
            public ListMapper() { }

            public bool TryMap(List<string> value, out string mapped)
            {
                if (value == null)
                {
                    mapped = null;
                }
                else
                {
                    List<string> valueWithSeps = new List<string>();
                    bool first = true;
                    foreach (string str in value)
                    {
                        if (!first)
                        {
                            valueWithSeps.Add(",");
                        }
                        valueWithSeps.Add(str);
                        first = false;
                    }
                    mapped = string.Concat(valueWithSeps);
                }
                return true;
            }

            public bool TryUnmap(string value, out List<string> unmapped)
            {
                unmapped = value != null ? new List<string>(value.Split(',')) : null;
                return true;
            }
        }

#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0649 // Never assigned
        [DialogOption("List", toOutsideWorldMapper: typeof(ListMapper))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "<Pending>")]
        List<string> list = new List<string>(new string[] { "A", "B", "C" });

        [DialogOption("Output")]
        IField<string> output;

        [DialogOption("A Matrix", secret: true)]
        float4x4 matrix;
        [DialogOption("Some Text")]
        string text;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore CS0649 // Never assigned

        public void Bind(Dialog dialog)
        {
            UniLog.Log("Bind");
            FrooxEngineBootstrap.LogStream.Flush();
        }

        [DialogAction("Left")]
        public void OnLeft()
        {
            UniLog.Log("OnLeft");
            output?.World.RunSynchronously(() => output.Value = "OnLeft");
            FrooxEngineBootstrap.LogStream.Flush();
        }

        [DialogAction("Middle", onlyValidating: new object[0])]
        public void OnMiddle()
        {
            UniLog.Log("OnMiddle");
            output?.World.RunSynchronously(() => output.Value = "OnMiddle");
            FrooxEngineBootstrap.LogStream.Flush();
        }

        [DialogAction("Right", onlyValidating: new object[] { "text" })]
        public void OnRight()
        {
            UniLog.Log("OnRight");
            output?.World.RunSynchronously(() => output.Value = "OnRight");
            FrooxEngineBootstrap.LogStream.Flush();
        }

        public void Dispose()
        {
            UniLog.Log("OnDestroy");
            output?.World.RunSynchronously(() => output.Value = "OnDestroy");
            FrooxEngineBootstrap.LogStream.Flush();
        }

        public IDictionary<object, string> UpdateAndValidate()
        {
            var errors = new Dictionary<object, string>();
            UniLog.Log($"Validate {matrix} {text} {output}");
            if (list != null) {
                UniLog.Log($"List with {list.Count} items:");
                foreach (var item in list) {
                    UniLog.Log($" {item}");
                    if (item.Length == 0)
                    {
                        errors.Add(nameof(list), "Items must be non-empty!");
                    }
                }
            }
            else
            {
                UniLog.Log("No list");
            }
            if (matrix.Determinant == 0.0)
            {
                errors.Add(nameof(matrix), "Determinant == 0");
            }
            else if (matrix.Determinant == 1.0)
            {
                errors.Add(nameof(matrix), "Determinant == 1");
            }
#pragma warning disable CA1308 // Normalize strings to uppercase
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
            if (text == null || text.Length == 0)
            {
                errors.Add(nameof(text), "Missing text");
            }
            else if (text.ToLowerInvariant() == text && matrix.m00 == 0.0)
            {
                errors.Add("special", "lowercase text and matrix_0_0 == 0");
            }
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
#pragma warning restore CA1308 // Normalize strings to uppercase
            output?.World.RunSynchronously(() => output.Value = $"{matrix} {text}");
            UniLog.Log($"Validated: {errors}");
            FrooxEngineBootstrap.LogStream.Flush();
            return errors;
        }
    }
}
