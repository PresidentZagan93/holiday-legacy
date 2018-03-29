public class InterfaceSelectable : InterfaceObject {

    public bool selected;
    InterfaceSelector selector;

    public InterfaceSelector Selector
    {
        get
        {
            return selector;
        }
        set
        {
            selector = value;
        }
    }

    public virtual void Click()
    {

    }

    public virtual void Select()
    {

    }

    public virtual void Deselect()
    {

    }
}
