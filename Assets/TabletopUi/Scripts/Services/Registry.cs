using System;
using System.Collections.Generic;
using Assets.Core;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.CS.TabletopUI
{
    //public interface IRegisterable
    //{
        
    //}

    public class Registry
    {
        private static Dictionary<Type, System.Object> registered=new Dictionary<Type, object>();

        public static T Retrieve<T>() where T: class
        {
            T got = registered[typeof(T)] as T;
            Assert.IsNotNull(got,typeof(T).GetType().Name + " never registered");
            return got;
        }

        public void Register<T>(T toRegister) where T: class
        {
            registered[typeof(T)] = toRegister;
        }

    }
}
