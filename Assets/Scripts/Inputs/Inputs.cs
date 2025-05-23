using UnityEngine;
using UnityEngine.InputSystem;

namespace Wolfheat.Inputs
{
    public class Inputs : MonoBehaviour
    {
        public Controls Controls { get; set; }
        public InputAction Actions { get; set; }

        public static Inputs Instance { get; private set; }

        private void OnEnable()
        {
            // Enable this when you want to use the loading of a saved file
            //SavingUtility.LoadingComplete += LoadingComplete;

#if UNITY_EDITOR
    Controls.Player.T.performed += TPressed;
    Controls.Player.Y.performed += YPressed;
#endif
        }

        private void Start()
        {
            LoadingComplete();
            
        }

        private void LoadingComplete()
        {
            Debug.Log("Loading Complete");
        }

        private void OnDisable()
        {
            Controls.Player.T.performed -= TPressed;
            Controls.Player.Y.performed -= YPressed;
            Controls.Disable();
        }

        public void YPressed(InputAction.CallbackContext context)
        {
            Debug.Log("Y pressed");
        }
        
        public void TPressed(InputAction.CallbackContext context)
        {
            Debug.Log("T Pressed");            
        }

        // Start is called before the first frame update
        void Awake()
        {
            Debug.Log("Created Inputs Controller");
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            Controls = new Controls();
            Controls.Enable();
        }
    }
}