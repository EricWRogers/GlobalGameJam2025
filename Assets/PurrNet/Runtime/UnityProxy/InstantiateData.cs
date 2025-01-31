﻿using System;
using PurrNet.Modules;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace PurrNet
{
    internal enum InstantiateType
    {
        Default,
        Parent,
        PositionRotation,
        PositionRotationParent,
        Scene,
        SceneParent
    }
    
    internal readonly struct InstantiateData<T> where T : Object
    {
        public readonly InstantiateType type;
        public readonly T original;
        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly Transform parent;
        public readonly Scene scene;
        public readonly bool instantiateInWorldSpace;
        
        public InstantiateData(T original)
        {
            type = InstantiateType.Default;
            this.original = original;
            position = Vector3.zero;
            rotation = Quaternion.identity;
            parent = null;
            scene = default;
            instantiateInWorldSpace = false;
        }
        
        public InstantiateData(T original, Transform parent, bool instantiateInWorldSpace)
        {
            type = InstantiateType.Parent;
            this.original = original;
            position = Vector3.zero;
            rotation = Quaternion.identity;
            this.parent = parent;
            scene = default;
            this.instantiateInWorldSpace = instantiateInWorldSpace;
        }
        
        public InstantiateData(T original, Vector3 position, Quaternion rotation)
        {
            type = InstantiateType.PositionRotation;
            this.original = original;
            this.position = position;
            this.rotation = rotation;
            parent = null;
            scene = default;
            instantiateInWorldSpace = false;
        }
        
        public InstantiateData(T original, Vector3 position, Quaternion rotation, Transform parent)
        {
            type = InstantiateType.PositionRotationParent;
            this.original = original;
            this.position = position;
            this.rotation = rotation;
            this.parent = parent;
            scene = default;
            instantiateInWorldSpace = false;
        }
        
        public InstantiateData(T original, Scene scene)
        {
            type = InstantiateType.Scene;
            this.original = original;
            position = Vector3.zero;
            rotation = Quaternion.identity;
            parent = null;
            this.scene = scene;
            instantiateInWorldSpace = false;
        }
        
        public InstantiateData(T original, Transform parent)
        {
            type = InstantiateType.SceneParent;
            this.original = original;
            position = Vector3.zero;
            rotation = Quaternion.identity;
            this.parent = parent;
            scene = default;
            instantiateInWorldSpace = false;
        }
        
        public bool TryGetHierarchy(out HierarchyV2 result)
        {
            var manager = NetworkManager.main;
            
            if (!manager)
            {
                result = default;
                return false;
            }
            
            bool isServer = manager.isServer;
            
            if (!manager.TryGetModule<HierarchyFactory>(isServer, out var factory))
            {
                result = default;
                return false;
            }
            
            if (!manager.TryGetModule<ScenesModule>(isServer, out var scenes))
            {
                result = default;
                return false;
            }
            
            var sceneCopy = scene;

            if (!sceneCopy.IsValid())
                sceneCopy = parent ? parent.gameObject.scene : SceneManager.GetActiveScene();
            
            if (!scenes.TryGetSceneID(sceneCopy, out var sceneID))
            {
                result = default;
                return false;
            }
            
            return factory.TryGetHierarchy(sceneID, out result);
        }
        
        public T Instantiate()
        {
            return type switch
            {
                InstantiateType.Default => UnityProxy.InstantiateDirectly(original),
                InstantiateType.Parent => UnityProxy.InstantiateDirectly(original, parent, instantiateInWorldSpace),
                InstantiateType.PositionRotation => UnityProxy.InstantiateDirectly(original, position, rotation),
                InstantiateType.PositionRotationParent => UnityProxy.InstantiateDirectly(original, position, rotation, parent),
#if UNITY_2023_1_OR_NEWER
                InstantiateType.Scene => UnityProxy.InstantiateDirectly(original, scene),
#endif
                InstantiateType.SceneParent => UnityProxy.InstantiateDirectly(original, parent),
                _ => default
            };
        }

        public void ApplyToExisting(GameObject go, GameObject prefab)
        {
            var trs = go.transform;
            switch (type)
            {
                case InstantiateType.PositionRotation:
                    trs.SetPositionAndRotation(position, rotation);
                    break;
                case InstantiateType.PositionRotationParent:
                    trs.SetPositionAndRotation(position, rotation);
                    trs.SetParent(parent);
                    break;
                case InstantiateType.Parent:
                    if (instantiateInWorldSpace)
                    {
                        trs.SetParent(parent, true);
                        trs.SetPositionAndRotation(
                            prefab.transform.position,
                            prefab.transform.rotation
                        );
                    }
                    else
                    {
                        trs.SetParent(parent);
                        trs.SetLocalPositionAndRotation(
                            prefab.transform.localPosition,
                            prefab.transform.localRotation
                        );
                    }
                    break;
                case InstantiateType.SceneParent:
                    trs.SetPositionAndRotation(
                        prefab.transform.position,
                        prefab.transform.rotation
                    );
                    trs.SetParent(parent);
                    break;
                case InstantiateType.Default:
                case InstantiateType.Scene: 
                    trs.SetPositionAndRotation(
                        prefab.transform.position,
                        prefab.transform.rotation
                    );
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}