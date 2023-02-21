using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Nebula.Input;

namespace Nebula.Main
{
    public class MouseButtonEventData
    {
        public PointerEventData buttonData;
        public ButtonState buttonState;
        public Point mousePosition;

        public bool PressedThisFrame()
        {
            return buttonState == ButtonState.Pressed;
        }

        public bool ReleasedThisFrame()
        {
            return buttonState == ButtonState.Released;
        }
    }

    public class PointerEventData
    {
        public IPointerDownHandler pressedEvent;
        public IPointerUpHandler releaseEvent;
        public IPointerClickHandler clickEvent;
        public IPointerEnterHandler enterEvent;
        public IPointerExitHandler exitEvent;

        public Point pressPosition;
        public bool elligibleForClick;
        public int clickCount;
        public float clickTime;
        public float delta;

    }

    public class Input : IControl
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        public static Input Access;

        private List<IPointerEventListener> PointerListeners;

        private MouseState PreviousMousePointerEventData;
        public MouseState MousePointerEventData;

        private MouseButtonEventData leftClickButtonData;
        private MouseButtonEventData rightClickButtonData;
        private MouseButtonEventData middleClickButtonData;

        public void Create(Runtime game)
        {
            Access = this;
            PointerListeners = new List<IPointerEventListener>();
        }

        public void Draw(GameTime gameTime)
        {

        }

        public void Initialise()
        {
            leftClickButtonData = new MouseButtonEventData();
            leftClickButtonData.buttonData = new PointerEventData();
            rightClickButtonData = new MouseButtonEventData();
            rightClickButtonData.buttonData = new PointerEventData();
            middleClickButtonData = new MouseButtonEventData();
            middleClickButtonData.buttonData = new PointerEventData();
        }

        public void LoadContent()
        {

        }

        public void UnloadContent()
        {

        }

        public void Update(GameTime gameTime)
        {
            ProcessMouseData();
        }

        public static void AddPointerEventListener(IPointerEventListener Listener)
        {
            Access.PointerListeners.Add(Listener);
            log.Debug("Adding Listener.. " + Access.PointerListeners.Count);
        }

        private void ProcessMouseData()
        {
            PreviousMousePointerEventData = MousePointerEventData;
            MousePointerEventData = Mouse.GetState();

            leftClickButtonData.buttonState = MousePointerEventData.LeftButton;
            leftClickButtonData.mousePosition = MousePointerEventData.Position;
            rightClickButtonData.buttonState = MousePointerEventData.RightButton;
            rightClickButtonData.mousePosition = MousePointerEventData.Position;
            middleClickButtonData.buttonState = MousePointerEventData.MiddleButton;
            middleClickButtonData.mousePosition = MousePointerEventData.Position;

            List<IPointerEventListener> listenersIntersectingCursor = new List<IPointerEventListener>();
            foreach (var listener in PointerListeners)
            {
                var eventListener = listener.Intersect(MousePointerEventData.Position);
                if (eventListener != null)
                {
                    listenersIntersectingCursor.Add(eventListener);
                }
            }

            IPointerEventListener[] Events = listenersIntersectingCursor.ToArray();
            ProcessMouseButton(leftClickButtonData, Events);
            ProcessMouseButton(rightClickButtonData, Events);
            ProcessMouseButton(middleClickButtonData, Events);

            ProcessMouseOver(leftClickButtonData, Events);
        }

        private void ProcessMouseButton(MouseButtonEventData Data, IPointerEventListener[] Events)
        {
            PointerEventData pointerData = Data.buttonData;
            bool Pressed = Data.PressedThisFrame();
            bool Released = Data.ReleasedThisFrame();
            if (!Pressed && !Released)
            {
                return;
            }
            if (Pressed)
            {
                pointerData.elligibleForClick = true;
                pointerData.delta = 0;
                pointerData.pressPosition = Data.mousePosition;

                IPointerDownHandler pointerDownExecuted = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(Events, Data, ExecuteEvents.pointerDown);
                IPointerClickHandler clickEvent = ExecuteEvents.GetEventListener<IPointerClickHandler>(Events, Data, ExecuteEvents.pointerClick);


                float dT = Time.deltaTime;
                if (clickEvent != null && clickEvent == pointerData.clickEvent)
                {
                    float timeSinceLastClick = dT - pointerData.clickTime;
                    if (timeSinceLastClick < 0.3F)
                    {
                        pointerData.clickCount++;
                    }
                    else
                    {
                        pointerData.clickCount = 1;
                    }
                    pointerData.clickTime = dT;
                }
                else
                {
                    pointerData.clickCount = 0;
                }

                pointerData.pressedEvent = pointerDownExecuted;
                pointerData.clickEvent = clickEvent;

            }
            if (Released)
            {
                IPointerUpHandler pointerUpExecuted = ExecuteEvents.ExecuteHierarchy<IPointerUpHandler>(Events, Data, ExecuteEvents.pointerUp);
                IPointerClickHandler clickEvent = ExecuteEvents.GetEventListener<IPointerClickHandler>(Events, Data, ExecuteEvents.pointerClick);

                if (pointerData.elligibleForClick && clickEvent == pointerData.clickEvent)
                {
                    ExecuteEvents.ExecuteHierarchy<IPointerClickHandler>(Events, Data, ExecuteEvents.pointerClick);
                }
                else
                {
                    pointerData.clickEvent = null;
                }

                pointerData.elligibleForClick = false;
                pointerData.pressedEvent = null;
                pointerData.releaseEvent = pointerUpExecuted;
            }
        }

        private void ProcessMouseOver(MouseButtonEventData Data, IPointerEventListener[] Events)
        {
            PointerEventData pointerData = Data.buttonData;
            IPointerEnterHandler enterEvent = ExecuteEvents.GetEventListener<IPointerEnterHandler>(Events, Data, ExecuteEvents.EventHandle.PointerEnter);
            IPointerExitHandler exitEvent = ExecuteEvents.GetEventListener<IPointerExitHandler>(Events, Data, ExecuteEvents.EventHandle.PointerExit);

            if (pointerData.enterEvent != enterEvent)
            {
                if (pointerData.exitEvent != null)
                {
                    ExecuteEvents.ExecuteEvent(pointerData.exitEvent, Data, ExecuteEvents.EventHandle.PointerExit);
                }
                ExecuteEvents.ExecuteHierarchy<IPointerExitHandler>(Events, Data, ExecuteEvents.EventHandle.PointerExit);
                if (enterEvent != null)
                {
                    ExecuteEvents.ExecuteHierarchy<IPointerEnterHandler>(Events, Data, ExecuteEvents.EventHandle.PointerEnter);
                }
            }

            pointerData.enterEvent = enterEvent;
            pointerData.exitEvent = exitEvent;
        }
    }
}
