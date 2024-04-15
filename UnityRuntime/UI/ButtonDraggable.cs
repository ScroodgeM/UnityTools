//this empty line for UTF-8 BOM header

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityTools.UnityRuntime.UI
{
    public enum PointerEventType
    {
        BeginDrag,
        Drag,
        EndDrag,
        Scroll,
        Down,
        Up,
        Click,
        LongTap,
        DoubleClick
    }

    public class ButtonDraggable : Button, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        public event Action<PointerEventType, PointerEventData> OnEvent = (eventType, evenData) => { };

        private new ButtonClickedEvent onClick { get; set; }

        [SerializeField] private bool isDraggable;

        private ScrollRect parentScrollRect;
        private bool forwardToParentScrollRect = false;
        private bool dragStarted = false;

        private bool interruptedLongTap = false;
        private Vector2 startAnchoredPosition;
        private RectTransform rectTransform;
        private Canvas canvas;

        private DateTime? timeToHandleLongTap;
        private DateTime? timeToHandleDoubleClick;

        private Vector2? clickStartPosition;
        private PointerEventData clickLastKnownData;

        private const float longTapTimeout = 0.3f;
        private const float maxDoubleClickTime = .1f;

        public bool ButtonIsDraggable
        {
            get
            {
                return isDraggable;
            }
            set
            {
                // return the button to start position, if being disabled while dragging
                if (value == false && isDraggable == true && dragStarted)
                {
                    rectTransform.anchoredPosition = startAnchoredPosition;
                    dragStarted = false;
                }

                isDraggable = true;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            parentScrollRect = GetComponentInParent<ScrollRect>();
        }

        protected override void Start()
        {
            base.Start();

            rectTransform = GetComponent<RectTransform>();
            canvas = transform.GetComponentInParent<Canvas>();
        }

        private void Update()
        {
            if (
                interruptedLongTap == false &&
                timeToHandleLongTap.HasValue &&
                timeToHandleLongTap.Value < DateTime.UtcNow &&
                dragStarted == false &&
                clickStartPosition.HasValue &&
                clickLastKnownData != null
                )
            {
                OnEvent(PointerEventType.LongTap, clickLastKnownData);
                timeToHandleLongTap = null;
                forwardToParentScrollRect = false;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            clickStartPosition = null;
            clickLastKnownData = null;
            timeToHandleLongTap = null;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (interactable == false) return;

            HandleEvent(PointerEventType.BeginDrag, eventData);

            dragStarted = true;

            // don't scroll parent scroll rect if long tap already happened
            if (parentScrollRect != null && timeToHandleLongTap.HasValue == true)
            {
                forwardToParentScrollRect = true;
            }

            timeToHandleLongTap = null;

            if (forwardToParentScrollRect == true)
            {
                parentScrollRect.OnBeginDrag(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (interactable == false) return;

            if (dragStarted == true && forwardToParentScrollRect == true)
            {
                bool isVerticalDrag = Mathf.Abs(eventData.delta.x) <= Mathf.Abs(eventData.delta.y);
                bool isHorizontalDrag = Mathf.Abs(eventData.delta.x) >= Mathf.Abs(eventData.delta.y);

                if (isHorizontalDrag && parentScrollRect.horizontal == true)
                {
                    dragStarted = false;
                }

                if (isVerticalDrag && parentScrollRect.vertical == true)
                {
                    dragStarted = false;
                }

                if (isHorizontalDrag && parentScrollRect.horizontal == false)
                {
                    forwardToParentScrollRect = false;
                }

                if (isVerticalDrag && parentScrollRect.vertical == false)
                {
                    forwardToParentScrollRect = false;
                }
            }

            if (dragStarted == true)
            {
                HandleEvent(PointerEventType.Drag, eventData);
            }

            if (forwardToParentScrollRect == true)
            {
                parentScrollRect.OnDrag(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (interactable == false) return;

            if (dragStarted == true)
            {
                HandleEvent(PointerEventType.EndDrag, eventData);
                dragStarted = false;
            }

            if (forwardToParentScrollRect == true)
            {
                parentScrollRect.OnEndDrag(eventData);
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (interactable == false) return;

            HandleEvent(PointerEventType.Scroll, eventData);

            if (parentScrollRect != null)
            {
                parentScrollRect.OnScroll(eventData);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (interactable == false) return;

            HandleEvent(PointerEventType.Down, eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            if (interactable == false) return;

            HandleEvent(PointerEventType.Up, eventData);
        }

        [Obsolete("use down->up sequence instead", true)]
        public new void OnPointerClick(PointerEventData eventData) { }

        private void HandleEvent(PointerEventType eType, PointerEventData eData)
        {
            switch (eType)
            {
                case PointerEventType.Down:
                    interruptedLongTap = false;
                    bool anotherPointer = clickLastKnownData != null && eData.pointerId != clickLastKnownData.pointerId;
                    clickStartPosition = eData.position;
                    clickLastKnownData = eData;
                    if (
                        anotherPointer == false &&
                        timeToHandleDoubleClick.HasValue &&
                        DateTime.UtcNow < timeToHandleDoubleClick.Value
                        )
                    {
                        OnEvent(PointerEventType.DoubleClick, eData);
                    }
                    timeToHandleLongTap = DateTime.UtcNow.AddSeconds(longTapTimeout);
                    timeToHandleDoubleClick = DateTime.UtcNow.AddSeconds(maxDoubleClickTime);
                    startAnchoredPosition = rectTransform.anchoredPosition;
                    break;

                case PointerEventType.Up:
                    if (
                        timeToHandleLongTap.HasValue &&
                        dragStarted == false &&
                        clickStartPosition.HasValue
                        )
                    {
                        OnEvent(PointerEventType.Click, clickLastKnownData);
                    }
                    clickStartPosition = null;
                    clickLastKnownData = null;
                    timeToHandleLongTap = null;
                    if (isDraggable && dragStarted)
                    {
                        rectTransform.anchoredPosition = startAnchoredPosition;
                    }
                    break;

                case PointerEventType.Drag:
                    interruptedLongTap = true;
                    if (isDraggable && dragStarted)
                    {
                        rectTransform.anchoredPosition += eData.delta / canvas.scaleFactor;
                    }
                    clickLastKnownData = eData;
                    break;
            }

            OnEvent(eType, eData);
        }
    }
}
