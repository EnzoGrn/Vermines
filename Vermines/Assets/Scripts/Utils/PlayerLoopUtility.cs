using System.Collections.Generic;
using System;
using UnityEngine.LowLevel;
using UnityEngine;

namespace Vermines.Utils {

    public static class PlayerLoopUtility {

        #region Getters & Setters

        public static void SetDefaultPlayerLoopSystem()
        {
            PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
        }

        public static bool HasPlayerLoopSystem(Type playerLoopSystem)
        {
            if (playerLoopSystem == null)
                return false;
            PlayerLoopSystem loopSystem = PlayerLoop.GetCurrentPlayerLoop();
            int subSystemCount = loopSystem.subSystemList.Length;

            for (int i = 0; i < subSystemCount; i++) {
                PlayerLoopSystem       subSystem     = loopSystem.subSystemList[i];
                List<PlayerLoopSystem> subSubSystems = new(subSystem.subSystemList);

                for (int j = 0; j < subSubSystems.Count; j++) {
                    if (subSubSystems[j].type == playerLoopSystem)
                        return true;
                }
            }

            return false;
        }

        #endregion

        #region Methods

        public static bool AddPlayerLoopSystems(Type playerLoopSystemType, Type targetLoopSystemType, PlayerLoopSystem.UpdateFunction updateFunction, int position = -1)
        {
            if (playerLoopSystemType == null || targetLoopSystemType == null || updateFunction == null)
                return false;
            PlayerLoopSystem loopSystem = PlayerLoop.GetCurrentPlayerLoop();
            int subSystemCount = loopSystem.subSystemList.Length;

            for (int i = 0; i < subSystemCount; i++) {
                PlayerLoopSystem subSystem = loopSystem.subSystemList[i];

                if (subSystem.type == targetLoopSystemType) {
                    PlayerLoopSystem targetSystem = new() {
                        type           = playerLoopSystemType,
                        updateDelegate = updateFunction
                    };
                    List<PlayerLoopSystem> subSubSystems = new(subSystem.subSystemList);

                    if (position >= 0) {
                        if (position > subSubSystems.Count)
                            throw new ArgumentOutOfRangeException(nameof(position));
                        subSubSystems.Insert(position, targetSystem);
                    } else {
                        subSubSystems.Add(targetSystem);
                    }

                    subSystem.subSystemList     = subSubSystems.ToArray();
                    loopSystem.subSystemList[i] = subSystem;

                    PlayerLoop.SetPlayerLoop(loopSystem);

                    return true;
                }
            }

            Debug.LogWarning($"Failed to add Player Loop System: {playerLoopSystemType.FullName} to {targetLoopSystemType.FullName}.");

            return false;
        }

        public static bool AddPlayerLoopSystems(Type playerLoopSystemType, Type targetSubSystemType, PlayerLoopSystem.UpdateFunction updateFunctionBefore, PlayerLoopSystem.UpdateFunction updateFunctionAfter)
        {
            if (playerLoopSystemType == null || targetSubSystemType == null || (updateFunctionBefore == null && updateFunctionAfter == null))
                return false;
            PlayerLoopSystem loopSystem = PlayerLoop.GetCurrentPlayerLoop();
            int subSystemCount = loopSystem.subSystemList.Length;

            for (int i = 0; i < subSystemCount; i++) {
                PlayerLoopSystem subSystem = loopSystem.subSystemList[i];
                int subSubSystemCount = subSystem.subSystemList.Length;

                for (int j = 0; j < subSubSystemCount; j++) {
                    PlayerLoopSystem subSubSystem = subSystem.subSystemList[j];

                    if (subSubSystem.type == targetSubSystemType) {
                        List<PlayerLoopSystem> subSubSystems = new(subSystem.subSystemList);
                        int currentPosition = j;

                        if (updateFunctionBefore != null) {
                            PlayerLoopSystem playerLoopSystem = new() {
                                type           = playerLoopSystemType,
                                updateDelegate = updateFunctionBefore
                            };

                            subSubSystems.Insert(currentPosition, playerLoopSystem);

                            currentPosition++;
                        }

                        if (updateFunctionAfter != null) {
                            currentPosition++;

                            PlayerLoopSystem playerLoopSystem = new() {
                                type           = playerLoopSystemType,
                                updateDelegate = updateFunctionAfter
                            };

                            subSubSystems.Insert(currentPosition, playerLoopSystem);
                        }
                        subSystem.subSystemList     = subSubSystems.ToArray();
                        loopSystem.subSystemList[i] = subSystem;

                        PlayerLoop.SetPlayerLoop(loopSystem);

                        return true;
                    }
                }
            }

            Debug.LogWarning($"Failed to add Player Loop System: {playerLoopSystemType.FullName}.");

            return false;
        }

        public static bool RemovePlayerLoopSystems(Type playerLoopSystemType)
        {
            if (playerLoopSystemType == null)
                return false;
            bool setPlayerLoop = false;

            PlayerLoopSystem loopSystem = PlayerLoop.GetCurrentPlayerLoop();
            int subSystemCount = loopSystem.subSystemList.Length;

            for (int i = 0; i < subSystemCount; i++) {
                PlayerLoopSystem subSystem = loopSystem.subSystemList[i];

                if (subSystem.subSystemList == null)
                    continue;
                bool removedFromSubSystem = false;

                List<PlayerLoopSystem> subSubSystems = new(subSystem.subSystemList);

                for (int j = subSubSystems.Count - 1; j >= 0; j--) {
                    if (subSubSystems[j].type == playerLoopSystemType) {
                        subSubSystems.RemoveAt(j);

                        removedFromSubSystem = true;
                    }
                }

                if (removedFromSubSystem) {
                    setPlayerLoop = true;

                    subSystem.subSystemList     = subSubSystems.ToArray();
                    loopSystem.subSystemList[i] = subSystem;
                }
            }

            if (setPlayerLoop)
                PlayerLoop.SetPlayerLoop(loopSystem);
            return setPlayerLoop;
        }

        #endregion

        #region Debug

        public static void DumpPlayerLoopSystem()
        {
            Debug.LogWarning("===== DumpPlayerLoopSystem =====");

            PlayerLoopSystem loopSystem = PlayerLoop.GetCurrentPlayerLoop();
            int subSystemCount = loopSystem.subSystemList.Length;

            for (int i = 0; i <subSystemCount; i++) {
                PlayerLoopSystem subSystem = loopSystem.subSystemList[i];

                Debug.LogWarning(subSystem.type.FullName);

                List<PlayerLoopSystem> subSubSystems = new(subSystem.subSystemList);

                for (int j = 0; j < subSubSystems.Count; j++)
                    Debug.Log("    " + subSubSystems[j].type.FullName);
            }

            Debug.LogWarning("================================");
        }

        #endregion
    }
}
