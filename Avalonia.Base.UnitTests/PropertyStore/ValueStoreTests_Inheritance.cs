using System.Collections.Generic;
using System.Linq;
using Avalonia.Data;
using Avalonia.PropertyStore;
using Xunit;

namespace Avalonia.Tests.PropertyStore
{
    public class ValueStoreTests_Inheritance
    {
        [Fact]
        public void InheritanceFrame_Is_Initially_Null()
        {
            var (_, target) = CreateTarget();

            Assert.Null(target.InheritanceFrame);
        }

        [Fact]
        public void InheritanceFrame_Is_Null_After_Setting_Inheriting_Value()
        {
            var (o, target) = CreateTarget();

            o.Foo = "foo";

            Assert.Null(target.InheritanceFrame);
        }

        [Fact]
        public void Parent_InheritanceFrame_Is_Created_After_Adding_Child()
        {
            var (parent, parentStore) = CreateTarget();
            var (child, childStore) = CreateTarget(parent);

            var parentFrame = parentStore.InheritanceFrame;
            var childFrame = childStore.InheritanceFrame;

            Assert.NotNull(parentFrame);
            Assert.Same(parentFrame, childFrame);
            Assert.Same(parentStore, parentFrame!.Owner);
            Assert.Null(parentFrame.Parent);
        }

        [Fact]
        public void Parent_And_Child_InheritanceFrames_Are_Created_After_Adding_Child_With_Inheriting_Value_Set()
        {
            var (parent, parentStore) = CreateTarget();
            var (child, childStore) = CreateTarget(parent);

            child.Foo = "foo";

            var parentFrame = parentStore.InheritanceFrame;
            var childFrame = childStore.InheritanceFrame;

            Assert.NotNull(parentFrame);
            Assert.NotNull(childFrame);
            Assert.NotSame(parentFrame, childFrame);
            Assert.Same(parentStore, parentFrame!.Owner);
            Assert.Null(parentFrame!.Parent);
            Assert.Same(childStore, childFrame!.Owner);
            Assert.Same(parentFrame, childFrame!.Parent);
        }

        [Fact]
        public void New_InheritanceFrame_Is_Created_After_Setting_Value_On_Child()
        {
            var (parent, parentStore) = CreateTarget();
            var (child, childStore) = CreateTarget(parent);

            child.Foo = "foo";

            var parentFrame = parentStore.InheritanceFrame;
            var childFrame = childStore.InheritanceFrame;

            Assert.NotNull(parentFrame);
            Assert.NotSame(parentFrame, childFrame);
            Assert.Same(parentStore, parentFrame!.Owner);
            Assert.Same(childStore, childFrame!.Owner);
        }

        [Fact]
        public void New_InheritanceFrame_Is_Inherited_By_Empty_Grandchild()
        {
            var (parent, parentStore) = CreateTarget();
            var (child, childStore) = CreateTarget(parent);
            var (_, grandchildStore) = CreateTarget(child);

            child.Foo = "foo";

            var parentFrame = parentStore.InheritanceFrame;
            var childFrame = childStore.InheritanceFrame;
            var grandchildFrame = grandchildStore.InheritanceFrame;

            Assert.NotNull(parentFrame);
            Assert.NotSame(parentFrame, childFrame);
            Assert.Same(childFrame, grandchildFrame);
            Assert.Same(parentStore, parentFrame!.Owner);
            Assert.Same(childStore, childFrame!.Owner);
        }

        [Fact]
        public void New_InheritanceFrame_Is_Inherited_By_Grandchild_Which_Has_Non_Inherited_Value_Set()
        {
            var (parent, parentStore) = CreateTarget();
            var (child, childStore) = CreateTarget(parent);
            var (grandchild, grandchildStore) = CreateTarget(child);

            grandchild.Bar = "bar";
            child.Foo = "foo";

            var parentFrame = parentStore.InheritanceFrame;
            var childFrame = childStore.InheritanceFrame;
            var grandchildFrame = grandchildStore.InheritanceFrame;

            Assert.NotNull(parentFrame);
            Assert.NotSame(parentFrame, childFrame);
            Assert.Same(childFrame, grandchildFrame);
            Assert.Same(parentStore, parentFrame!.Owner);
            Assert.Same(childStore, childFrame!.Owner);
        }

        [Fact]
        public void New_InheritanceFrame_Is_Not_Inherited_By_Grandchild_Which_Has_Inherited_Value_Set()
        {
            var (parent, parentStore) = CreateTarget();
            var (child, childStore) = CreateTarget(parent);
            var (grandchild, grandchildStore) = CreateTarget(child);

            grandchild.Foo = "bar";
            child.Foo = "foo";

            var parentFrame = parentStore.InheritanceFrame;
            var childFrame = childStore.InheritanceFrame;
            var grandchildFrame = grandchildStore.InheritanceFrame;

            Assert.NotNull(parentFrame);
            Assert.NotSame(parentFrame, childFrame);
            Assert.NotSame(childFrame, grandchildFrame);
            Assert.Same(parentStore, parentFrame!.Owner);
            Assert.Same(childStore, childFrame!.Owner);
            Assert.Same(grandchildStore, grandchildFrame!.Owner);
        }

        [Fact]
        public void Child_InheritanceFrame_Is_Null_After_Parent_Removed()
        {
            var (parent, _) = CreateTarget();
            var (child, childStore) = CreateTarget(parent);

            child.Parent = null;

            Assert.Null(childStore.InheritanceFrame);
        }

        [Fact]
        public void Child_InheritanceFrame_Is_Not_Null_After_Parent_Removed_When_Inheriting_Value_Set()
        {
            var (parent, _) = CreateTarget();
            var (child, childStore) = CreateTarget(parent);

            child.Foo = "foo";
            child.Parent = null;

            var childFrame = childStore.InheritanceFrame;
            Assert.NotNull(childFrame);
            Assert.Null(childFrame!.Parent);
        }

        [Fact]
        public void Child_InheritanceFrame_Is_Reparented_When_Inheriting_Value_Set()
        {
            var (parent, _) = CreateTarget();
            var (child, childStore) = CreateTarget(parent);
            var (newParent, newParentStore) = CreateTarget();

            child.Foo = "foo";
            child.Parent = newParent;

            var childFrame = childStore.InheritanceFrame;
            var newParentFrame = newParentStore.InheritanceFrame;

            Assert.NotNull(childFrame);
            Assert.NotNull(newParentFrame);
            Assert.Same(newParentFrame, childFrame!.Parent);
        }

        private static (Class1, ValueStore) CreateTarget(Class1? parent = null)
        {
            var o = new Class1 { Parent = parent };
            return (o, o.UnitTestGetValueStore());
        }

        private class Class1 : AvaloniaObject
        {
            public static readonly StyledProperty<string?> FooProperty =
                AvaloniaProperty.Register<Class1, string?>("Foo", "foodefault", inherits: true);
            public static readonly StyledProperty<string?> BarProperty =
                AvaloniaProperty.Register<Class1, string?>("Foo", "bardefault");
            private Class1? _inheritanceParent;
            private List<Class1> _inheritanceChildren = new();

            public string? Foo
            {
                get => GetValue(FooProperty);
                set => SetValue(FooProperty, value);
            }

            public string? Bar
            {
                get => GetValue(BarProperty);
                set => SetValue(BarProperty, value);
            }

            public Class1? Parent
            {
                get { return _inheritanceParent; }
                set
                {
                    if (_inheritanceParent != value)
                    {
                        _inheritanceParent?._inheritanceChildren.Remove(this);
                        _inheritanceParent = value;
                        _inheritanceParent?._inheritanceChildren.Add(this);
                        InheritanceParentChanged(value);
                    }
                }
            }

            internal override int GetInheritanceChildCount() => _inheritanceChildren.Count;
            internal override AvaloniaObject GetInheritanceChild(int index) => _inheritanceChildren[index];
            private protected override AvaloniaObject? GetInheritanceParent => _inheritanceParent;
        }
    }
}
