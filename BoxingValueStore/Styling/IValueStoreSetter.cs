using Avalonia.PropertyStore;

namespace Avalonia.Styling
{
    internal interface IValueStoreSetter : ISetter
    {
        IValue Instance(StyleInstance instance, IStyleable target);
    }
}
