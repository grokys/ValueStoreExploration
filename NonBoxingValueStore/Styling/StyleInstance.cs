using System.Collections.Generic;
using Avalonia.Data;
using Avalonia.PropertyStore;

namespace Avalonia.Styling
{
    internal class StyleInstance : ValueFrameBase
    {
        public StyleInstance(BindingPriority priority, IReadOnlyList<ISetterInstance> setters)
        {
            Priority = priority;

            for (var i = 0; i < setters.Count; ++i)
            {
                if (setters[i] is IValue value)
                    Add(value);
            }

            IsActive = true;
        }

        public override bool IsActive { get; }
        public override BindingPriority Priority { get; }
    }
}
