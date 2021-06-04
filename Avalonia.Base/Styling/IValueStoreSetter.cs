using Avalonia.PropertyStore;

namespace Avalonia.Styling
{
    internal interface IValueStoreSetter : ISetter
    {
        IValueEntry Instance(StyleInstance instance, IStyleable target);
    }
}
