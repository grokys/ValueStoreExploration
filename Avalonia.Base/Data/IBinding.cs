namespace Avalonia.Data
{
    public interface IBinding
    {
        InstancedBinding Initiate(IAvaloniaObject target,  AvaloniaProperty targetProperty);
    }
}
