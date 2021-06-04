using UnityEngine;
using UnityEngine.UIElements;
namespace WaynGroup.Mgm.Ability.UI
{
    public class DragableAbility : MouseManipulator
    {
        #region Init
        private Vector2 m_Start;
        private StyleLength m_topStart;
        private StyleLength m_leftStart;
        protected bool m_Active;
        protected bool m_Dragging;

        public DragableAbility()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            m_Active = false;
            m_Dragging = false;
        }
        #endregion

        #region Registrations
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }
        #endregion

        #region OnMouseDown
        protected void OnMouseDown(MouseDownEvent e)
        {
            if (m_Active)
            {
                e.StopImmediatePropagation();
                return;
            }

            if (CanStartManipulation(e))
            {
                m_Start = e.mousePosition;
                m_topStart = target.style.top;
                m_leftStart = target.style.left;
                m_Active = true;
                target.CaptureMouse();
                e.StopPropagation();
            }
        }
        #endregion

        #region OnMouseMove
        protected void OnMouseMove(MouseMoveEvent e)
        {
            if (!m_Active || !target.HasMouseCapture())
                return;
            m_Dragging = true;
            Vector2 diff = e.mousePosition - m_Start;

            target.style.top = m_topStart.value.value + diff.y;
            target.style.left = m_leftStart.value.value + diff.x;

            e.StopPropagation();
        }
        #endregion

        #region OnMouseUp
        protected void OnMouseUp(MouseUpEvent e)
        {

            if (!m_Dragging) ((AbilityUIElement)target).ExecuteAction();

            if (!m_Active || !target.HasMouseCapture() || !CanStopManipulation(e))
                return;
            m_Dragging = false;
            m_Active = false;

            target.style.top = m_topStart;
            target.style.left = m_leftStart;
            VisualElement actionSlot = AbilityUIData.Instance.FindDropArea(e.mousePosition);



            if (target.parent is ActionSlot)
            {
                if (actionSlot == null)
                {
                    target.SetEnabled(false);
                    target.parent.Remove(target);
                }
                else
                {
                    AbilityUIElement swap = actionSlot.Q<AbilityUIElement>();
                    if (swap != null)
                    {
                        target.parent.Add(swap);
                        swap.style.top = 0;
                        swap.style.left = 0;
                        swap.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                        swap.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                    }

                    actionSlot.Clear();
                    actionSlot.Add(target);
                    target.style.top = 0;
                    target.style.left = 0;
                    target.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                    target.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                }
            }
            else if (actionSlot != null)
            {

                AbilityUIElement copy = ((AbilityUIElement)target).Clone();
                actionSlot.Clear();
                actionSlot.Add(copy);
                copy.style.top = 0;
                copy.style.left = 0;
                copy.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                copy.style.height = new StyleLength(new Length(100, LengthUnit.Percent));

            }



            target.ReleaseMouse();
            e.StopPropagation();
        }
        #endregion
    }


}
