using UnityEngine;
using UnityEngine.Events;

public abstract class Controller : MonoBehaviour
{
    protected Vector2 stickL;
    protected Vector2 stickR;

    public Vector2 StickL { get => stickL; }
    public Vector2 StickR { get => stickR; }

    public Vector3 StickL3 => new Vector3(StickL.x, 0, StickL.y);
    public Vector3 StickR3 => new Vector3(StickR.x, 0, StickR.y);

    public Button A = new Button();
    public Button B = new Button();
    public Button X = new Button();
    public Button Y = new Button();

    public class Button
    {
        bool isPressed = false;
        public UnityEvent OnPressDown = new UnityEvent();
        public UnityEvent OnPressUp = new UnityEvent();


        public bool IsPressed
        {
            get => isPressed;

            set
            {
                if (isPressed == value)
                    return;

                if (!isPressed && value)
                    OnPressDown.Invoke();

                if (isPressed && !value)
                    OnPressUp.Invoke();

                isPressed = value;
            }
        }
    }
}
