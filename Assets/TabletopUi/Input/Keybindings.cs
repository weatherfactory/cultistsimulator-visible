// GENERATED AUTOMATICALLY FROM 'Assets/TabletopUi/Input/Keybindings.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Keybindings : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @Keybindings()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Keybindings"",
    ""maps"": [
        {
            ""name"": ""Default"",
            ""id"": ""a5f15f1d-5e94-4ee1-9d08-4ef12d45a01d"",
            ""actions"": [
                {
                    ""name"": ""kbpause"",
                    ""type"": ""Button"",
                    ""id"": ""295541f4-2845-4bbc-a969-f497df13e6e6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""kbNormalSpeed"",
                    ""type"": ""Button"",
                    ""id"": ""e49fa150-5679-4be1-b0f6-daff5bf7eb2a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Fast Speed"",
                    ""type"": ""Button"",
                    ""id"": ""562ce9bb-470f-42be-9374-c8a2faa7a820"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Start Recipe"",
                    ""type"": ""Button"",
                    ""id"": ""ad2078b0-1f11-4d50-abd9-71aaf89cb2bc"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""kbcollectall"",
                    ""type"": ""Button"",
                    ""id"": ""df10e8a5-e2a5-41cf-bfb8-1e24f0a3dd5b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Toggle Debug"",
                    ""type"": ""Button"",
                    ""id"": ""9748c843-b882-44c5-a37a-3a0147b3757e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Stack Cards"",
                    ""type"": ""Button"",
                    ""id"": ""613ea87a-1750-429b-9d61-e3d6ef847722"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""2ea7fe1d-9dba-4bea-ba5a-bed352a58bce"",
                    ""path"": ""<Keyboard>/n"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""kbNormalSpeed"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d94e6168-fe03-4fe0-be30-d93c374d940b"",
                    ""path"": ""<Keyboard>/m"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fast Speed"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""80ed471a-5b84-4aac-9af3-5a1a272be0b9"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""kbpause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""69fa4b52-4474-4d80-b327-e76c3c75644f"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Start Recipe"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b397d6f3-63c3-4ab0-bdb3-b2d0fae2c7b4"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""kbcollectall"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""251d686b-e46c-4085-ac77-38fe65c2e6ea"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Toggle Debug"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8e7adc8c-6099-4bc2-85ca-32dd3af81d56"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Stack Cards"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Default
        m_Default = asset.FindActionMap("Default", throwIfNotFound: true);
        m_Default_kbpause = m_Default.FindAction("kbpause", throwIfNotFound: true);
        m_Default_kbNormalSpeed = m_Default.FindAction("kbNormalSpeed", throwIfNotFound: true);
        m_Default_FastSpeed = m_Default.FindAction("Fast Speed", throwIfNotFound: true);
        m_Default_StartRecipe = m_Default.FindAction("Start Recipe", throwIfNotFound: true);
        m_Default_kbcollectall = m_Default.FindAction("kbcollectall", throwIfNotFound: true);
        m_Default_ToggleDebug = m_Default.FindAction("Toggle Debug", throwIfNotFound: true);
        m_Default_StackCards = m_Default.FindAction("Stack Cards", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Default
    private readonly InputActionMap m_Default;
    private IDefaultActions m_DefaultActionsCallbackInterface;
    private readonly InputAction m_Default_kbpause;
    private readonly InputAction m_Default_kbNormalSpeed;
    private readonly InputAction m_Default_FastSpeed;
    private readonly InputAction m_Default_StartRecipe;
    private readonly InputAction m_Default_kbcollectall;
    private readonly InputAction m_Default_ToggleDebug;
    private readonly InputAction m_Default_StackCards;
    public struct DefaultActions
    {
        private @Keybindings m_Wrapper;
        public DefaultActions(@Keybindings wrapper) { m_Wrapper = wrapper; }
        public InputAction @kbpause => m_Wrapper.m_Default_kbpause;
        public InputAction @kbNormalSpeed => m_Wrapper.m_Default_kbNormalSpeed;
        public InputAction @FastSpeed => m_Wrapper.m_Default_FastSpeed;
        public InputAction @StartRecipe => m_Wrapper.m_Default_StartRecipe;
        public InputAction @kbcollectall => m_Wrapper.m_Default_kbcollectall;
        public InputAction @ToggleDebug => m_Wrapper.m_Default_ToggleDebug;
        public InputAction @StackCards => m_Wrapper.m_Default_StackCards;
        public InputActionMap Get() { return m_Wrapper.m_Default; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DefaultActions set) { return set.Get(); }
        public void SetCallbacks(IDefaultActions instance)
        {
            if (m_Wrapper.m_DefaultActionsCallbackInterface != null)
            {
                @kbpause.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbpause;
                @kbpause.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbpause;
                @kbpause.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbpause;
                @kbNormalSpeed.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbNormalSpeed;
                @kbNormalSpeed.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbNormalSpeed;
                @kbNormalSpeed.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbNormalSpeed;
                @FastSpeed.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnFastSpeed;
                @FastSpeed.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnFastSpeed;
                @FastSpeed.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnFastSpeed;
                @StartRecipe.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnStartRecipe;
                @StartRecipe.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnStartRecipe;
                @StartRecipe.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnStartRecipe;
                @kbcollectall.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbcollectall;
                @kbcollectall.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbcollectall;
                @kbcollectall.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbcollectall;
                @ToggleDebug.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnToggleDebug;
                @ToggleDebug.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnToggleDebug;
                @ToggleDebug.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnToggleDebug;
                @StackCards.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnStackCards;
                @StackCards.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnStackCards;
                @StackCards.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnStackCards;
            }
            m_Wrapper.m_DefaultActionsCallbackInterface = instance;
            if (instance != null)
            {
                @kbpause.started += instance.OnKbpause;
                @kbpause.performed += instance.OnKbpause;
                @kbpause.canceled += instance.OnKbpause;
                @kbNormalSpeed.started += instance.OnKbNormalSpeed;
                @kbNormalSpeed.performed += instance.OnKbNormalSpeed;
                @kbNormalSpeed.canceled += instance.OnKbNormalSpeed;
                @FastSpeed.started += instance.OnFastSpeed;
                @FastSpeed.performed += instance.OnFastSpeed;
                @FastSpeed.canceled += instance.OnFastSpeed;
                @StartRecipe.started += instance.OnStartRecipe;
                @StartRecipe.performed += instance.OnStartRecipe;
                @StartRecipe.canceled += instance.OnStartRecipe;
                @kbcollectall.started += instance.OnKbcollectall;
                @kbcollectall.performed += instance.OnKbcollectall;
                @kbcollectall.canceled += instance.OnKbcollectall;
                @ToggleDebug.started += instance.OnToggleDebug;
                @ToggleDebug.performed += instance.OnToggleDebug;
                @ToggleDebug.canceled += instance.OnToggleDebug;
                @StackCards.started += instance.OnStackCards;
                @StackCards.performed += instance.OnStackCards;
                @StackCards.canceled += instance.OnStackCards;
            }
        }
    }
    public DefaultActions @Default => new DefaultActions(this);
    public interface IDefaultActions
    {
        void OnKbpause(InputAction.CallbackContext context);
        void OnKbNormalSpeed(InputAction.CallbackContext context);
        void OnFastSpeed(InputAction.CallbackContext context);
        void OnStartRecipe(InputAction.CallbackContext context);
        void OnKbcollectall(InputAction.CallbackContext context);
        void OnToggleDebug(InputAction.CallbackContext context);
        void OnStackCards(InputAction.CallbackContext context);
    }
}
