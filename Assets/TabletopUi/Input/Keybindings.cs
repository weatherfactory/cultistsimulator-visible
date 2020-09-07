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
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
                    ""id"": ""295541f4-2845-4bbc-a969-f497df13e6e6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""NormalSpeed"",
                    ""type"": ""Button"",
                    ""id"": ""e49fa150-5679-4be1-b0f6-daff5bf7eb2a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""FastSpeed"",
                    ""type"": ""Button"",
                    ""id"": ""562ce9bb-470f-42be-9374-c8a2faa7a820"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""StartRecipe"",
                    ""type"": ""Button"",
                    ""id"": ""ad2078b0-1f11-4d50-abd9-71aaf89cb2bc"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""CollectAll"",
                    ""type"": ""Button"",
                    ""id"": ""df10e8a5-e2a5-41cf-bfb8-1e24f0a3dd5b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ToggleDebug"",
                    ""type"": ""Button"",
                    ""id"": ""9748c843-b882-44c5-a37a-3a0147b3757e"",
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
                    ""action"": ""NormalSpeed"",
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
                    ""action"": ""FastSpeed"",
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
                    ""action"": ""Pause"",
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
                    ""action"": ""StartRecipe"",
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
                    ""action"": ""CollectAll"",
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
                    ""action"": ""ToggleDebug"",
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
        m_Default_Pause = m_Default.FindAction("Pause", throwIfNotFound: true);
        m_Default_NormalSpeed = m_Default.FindAction("NormalSpeed", throwIfNotFound: true);
        m_Default_FastSpeed = m_Default.FindAction("FastSpeed", throwIfNotFound: true);
        m_Default_StartRecipe = m_Default.FindAction("StartRecipe", throwIfNotFound: true);
        m_Default_CollectAll = m_Default.FindAction("CollectAll", throwIfNotFound: true);
        m_Default_ToggleDebug = m_Default.FindAction("ToggleDebug", throwIfNotFound: true);
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
    private readonly InputAction m_Default_Pause;
    private readonly InputAction m_Default_NormalSpeed;
    private readonly InputAction m_Default_FastSpeed;
    private readonly InputAction m_Default_StartRecipe;
    private readonly InputAction m_Default_CollectAll;
    private readonly InputAction m_Default_ToggleDebug;
    public struct DefaultActions
    {
        private @Keybindings m_Wrapper;
        public DefaultActions(@Keybindings wrapper) { m_Wrapper = wrapper; }
        public InputAction @Pause => m_Wrapper.m_Default_Pause;
        public InputAction @NormalSpeed => m_Wrapper.m_Default_NormalSpeed;
        public InputAction @FastSpeed => m_Wrapper.m_Default_FastSpeed;
        public InputAction @StartRecipe => m_Wrapper.m_Default_StartRecipe;
        public InputAction @CollectAll => m_Wrapper.m_Default_CollectAll;
        public InputAction @ToggleDebug => m_Wrapper.m_Default_ToggleDebug;
        public InputActionMap Get() { return m_Wrapper.m_Default; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DefaultActions set) { return set.Get(); }
        public void SetCallbacks(IDefaultActions instance)
        {
            if (m_Wrapper.m_DefaultActionsCallbackInterface != null)
            {
                @Pause.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnPause;
                @Pause.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnPause;
                @Pause.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnPause;
                @NormalSpeed.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnNormalSpeed;
                @NormalSpeed.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnNormalSpeed;
                @NormalSpeed.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnNormalSpeed;
                @FastSpeed.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnFastSpeed;
                @FastSpeed.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnFastSpeed;
                @FastSpeed.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnFastSpeed;
                @StartRecipe.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnStartRecipe;
                @StartRecipe.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnStartRecipe;
                @StartRecipe.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnStartRecipe;
                @CollectAll.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnCollectAll;
                @CollectAll.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnCollectAll;
                @CollectAll.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnCollectAll;
                @ToggleDebug.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnToggleDebug;
                @ToggleDebug.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnToggleDebug;
                @ToggleDebug.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnToggleDebug;
            }
            m_Wrapper.m_DefaultActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Pause.started += instance.OnPause;
                @Pause.performed += instance.OnPause;
                @Pause.canceled += instance.OnPause;
                @NormalSpeed.started += instance.OnNormalSpeed;
                @NormalSpeed.performed += instance.OnNormalSpeed;
                @NormalSpeed.canceled += instance.OnNormalSpeed;
                @FastSpeed.started += instance.OnFastSpeed;
                @FastSpeed.performed += instance.OnFastSpeed;
                @FastSpeed.canceled += instance.OnFastSpeed;
                @StartRecipe.started += instance.OnStartRecipe;
                @StartRecipe.performed += instance.OnStartRecipe;
                @StartRecipe.canceled += instance.OnStartRecipe;
                @CollectAll.started += instance.OnCollectAll;
                @CollectAll.performed += instance.OnCollectAll;
                @CollectAll.canceled += instance.OnCollectAll;
                @ToggleDebug.started += instance.OnToggleDebug;
                @ToggleDebug.performed += instance.OnToggleDebug;
                @ToggleDebug.canceled += instance.OnToggleDebug;
            }
        }
    }
    public DefaultActions @Default => new DefaultActions(this);
    public interface IDefaultActions
    {
        void OnPause(InputAction.CallbackContext context);
        void OnNormalSpeed(InputAction.CallbackContext context);
        void OnFastSpeed(InputAction.CallbackContext context);
        void OnStartRecipe(InputAction.CallbackContext context);
        void OnCollectAll(InputAction.CallbackContext context);
        void OnToggleDebug(InputAction.CallbackContext context);
    }
}
