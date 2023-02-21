using Microsoft.Xna.Framework;
using Nebula.Main;
using System;

namespace Nebula.Input
{
    public interface IPointerEventListener
    {
        public IPointerEventListener Intersect(Point mousePos);
        public IPointerEventListener Parent { get; }
        public IPointerEventListener[] Children { get; }
    }

    public interface IPointerEventBase : IPointerEventListener
    {

    }

    public interface IPointerEnterHandler : IPointerEventBase
    {
        public bool PointerEnter(MouseButtonEventData Data);
    }

    public interface IPointerExitHandler : IPointerEventBase
    {
        public bool PointerExit(MouseButtonEventData Data);
    }

    public interface IPointerDownHandler : IPointerEventBase
    {
        public bool PointerDown(MouseButtonEventData Data);
    }

    public interface IPointerUpHandler : IPointerEventBase
    {
        public bool PointerUp(MouseButtonEventData Data);
    }

    public interface IPointerClickHandler : IPointerEventBase
    {
        public bool PointerClick(MouseButtonEventData Data);
    }


}
