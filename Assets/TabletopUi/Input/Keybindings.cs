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
                    ""name"": ""kbnormalspeed"",
                    ""type"": ""Button"",
                    ""id"": ""e49fa150-5679-4be1-b0f6-daff5bf7eb2a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""kbfastspeed"",
                    ""type"": ""Button"",
                    ""id"": ""562ce9bb-470f-42be-9374-c8a2faa7a820"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""kbstartrecipe"",
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
                    ""name"": ""kbtoggledebug"",
                    ""type"": ""Button"",
                    ""id"": ""9748c843-b882-44c5-a37a-3a0147b3757e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""kbzoomin"",
                    ""type"": ""Button"",
                    ""id"": ""613ea87a-1750-429b-9d61-e3d6ef847722"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""kbzoomout"",
                    ""type"": ""Button"",
                    ""id"": ""da5aa02a-4834-441a-ab07-bc35c26c9642"",
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
                    ""action"": ""kbnormalspeed"",
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
                    ""action"": ""kbfastspeed"",
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
                    ""action"": ""kbstartrecipe"",
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
                    ""action"": ""kbtoggledebug"",
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
                    ""action"": ""kbzoomin"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c3060b86-7b93-4db4-bca5-efa00865483f"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""kbzoomout"",
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
        m_Default_kbnormalspeed = m_Default.FindAction("kbnormalspeed", throwIfNotFound: true);
        m_Default_kbfastspeed = m_Default.FindAction("kbfastspeed", throwIfNotFound: true);
        m_Default_kbstartrecipe = m_Default.FindAction("kbstartrecipe", throwIfNotFound: true);
        m_Default_kbcollectall = m_Default.FindAction("kbcollectall", throwIfNotFound: true);
        m_Default_kbtoggledebug = m_Default.FindAction("kbtoggledebug", throwIfNotFound: true);
        m_Default_kbzoomin = m_Default.FindAction("kbzoomin", throwIfNotFound: true);
        m_Default_kbzoomout = m_Default.FindAction("kbzoomout", throwIfNotFound: true);
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
    private readonly InputAction m_Default_kbnormalspeed;
    private readonly InputAction m_Default_kbfastspeed;
    private readonly InputAction m_Default_kbstartrecipe;
    private readonly InputAction m_Default_kbcollectall;
    private readonly InputAction m_Default_kbtoggledebug;
    private readonly InputAction m_Default_kbzoomin;
    private readonly InputAction m_Default_kbzoomout;
    public struct DefaultActions
    {
        private @Keybindings m_Wrapper;
        public DefaultActions(@Keybindings wrapper) { m_Wrapper = wrapper; }
        public InputAction @kbpause => m_Wrapper.m_Default_kbpause;
        public InputAction @kbnormalspeed => m_Wrapper.m_Default_kbnormalspeed;
        public InputAction @kbfastspeed => m_Wrapper.m_Default_kbfastspeed;
        public InputAction @kbstartrecipe => m_Wrapper.m_Default_kbstartrecipe;
        public InputAction @kbcollectall => m_Wrapper.m_Default_kbcollectall;
        public InputAction @kbtoggledebug => m_Wrapper.m_Default_kbtoggledebug;
        public InputAction @kbzoomin => m_Wrapper.m_Default_kbzoomin;
        public InputAction @kbzoomout => m_Wrapper.m_Default_kbzoomout;
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
                @kbnormalspeed.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbnormalspeed;
                @kbnormalspeed.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbnormalspeed;
                @kbnormalspeed.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbnormalspeed;
                @kbfastspeed.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbfastspeed;
                @kbfastspeed.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbfastspeed;
                @kbfastspeed.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbfastspeed;
                @kbstartrecipe.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbstartrecipe;
                @kbstartrecipe.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbstartrecipe;
                @kbstartrecipe.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbstartrecipe;
                @kbcollectall.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbcollectall;
                @kbcollectall.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbcollectall;
                @kbcollectall.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbcollectall;
                @kbtoggledebug.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbtoggledebug;
                @kbtoggledebug.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbtoggledebug;
                @kbtoggledebug.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbtoggledebug;
                @kbzoomin.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbzoomin;
                @kbzoomin.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbzoomin;
                @kbzoomin.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbzoomin;
                @kbzoomout.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbzoomout;
                @kbzoomout.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbzoomout;
                @kbzoomout.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnKbzoomout;
            }
            m_Wrapper.m_DefaultActionsCallbackInterface = instance;
            if (instance != null)
            {
                @kbpause.started += instance.OnKbpause;
                @kbpause.performed += instance.OnKbpause;
                @kbpause.canceled += instance.OnKbpause;
                @kbnormalspeed.started += instance.OnKbnormalspeed;
                @kbnormalspeed.performed += instance.OnKbnormalspeed;
                @kbnormalspeed.canceled += instance.OnKbnormalspeed;
                @kbfastspeed.started += instance.OnKbfastspeed;
                @kbfastspeed.performed += instance.OnKbfastspeed;
                @kbfastspeed.canceled += instance.OnKbfastspeed;
                @kbstartrecipe.started += instance.OnKbstartrecipe;
                @kbstartrecipe.performed += instance.OnKbstartrecipe;
                @kbstartrecipe.canceled += instance.OnKbstartrecipe;
                @kbcollectall.started += instance.OnKbcollectall;
                @kbcollectall.performed += instance.OnKbcollectall;
                @kbcollectall.canceled += instance.OnKbcollectall;
                @kbtoggledebug.started += instance.OnKbtoggledebug;
                @kbtoggledebug.performed += instance.OnKbtoggledebug;
                @kbtoggledebug.canceled += instance.OnKbtoggledebug;
                @kbzoomin.started += instance.OnKbzoomin;
                @kbzoomin.performed += instance.OnKbzoomin;
                @kbzoomin.canceled += instance.OnKbzoomin;
                @kbzoomout.started += instance.OnKbzoomout;
                @kbzoomout.performed += instance.OnKbzoomout;
                @kbzoomout.canceled += instance.OnKbzoomout;
            }
        }
    }
    public DefaultActions @Default => new DefaultActions(this);
    public interface IDefaultActions
    {
        void OnKbpause(InputAction.CallbackContext context);
        void OnKbnormalspeed(InputAction.CallbackContext context);
        void OnKbfastspeed(InputAction.CallbackContext context);
        void OnKbstartrecipe(InputAction.CallbackContext context);
        void OnKbcollectall(InputAction.CallbackContext context);
        void OnKbtoggledebug(InputAction.CallbackContext context);
        void OnKbzoomin(InputAction.CallbackContext context);
        void OnKbzoomout(InputAction.CallbackContext context);
    }
}
