using System;
using System.Diagnostics;
using UnityEngine;
using Vermines.Characters;

namespace Vermines.Core.Player {

    public class PlayerData : IPlayer {

        #region Attributes

        public string UserID => _UserID;

        public string UnityID
        {
            get => _UnityID;
            set => _UnityID = value;
        }

        public string Nickname
        {
            get => _Nickname;
            set
            {
                _Nickname = value;
                IsDirty   = true;
            }
        }

        public Cultist Cultist => GetCultist();

        public int CultistID
        {
            get => _CultistID;
            set
            {
                _CultistID = value;
                IsDirty    = true;
            }
        }

        public bool IsDirty { get; private set; }

        [SerializeField]
        private string _UserID;

        [SerializeField]
        private string _UnityID;

        [SerializeField]
        private string _Nickname;

        [SerializeField]
        private Cultist _Cultist;

        [SerializeField]
        private int _CultistID;

        [SerializeField]
        private bool _IsLocked;

        [SerializeField]
        private int _LastProcessID;

        #endregion

        #region Constructor

        public PlayerData(string userID)
        {
            _UserID = userID;
        }

        #endregion

        #region Getters & Setters

        public bool IsLocked(bool checkProcess = true)
        {
            if (!_IsLocked)
                return false;
            if (checkProcess) {
                try {
                    Process.GetProcessById(_LastProcessID);
                } catch (Exception) {
                    return false; // Process not running.
                }
            }

            return true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// When running multiple instances of the game on same machine we want to lock used player data.
        /// </summary>
        public void Lock()
        {
            _IsLocked      = true;
            _LastProcessID = Process.GetCurrentProcess().Id;
        }

        public void Unlock()
        {
            _IsLocked = false;
        }

        public void ClearDirty()
        {
            IsDirty = false;
        }

        private Cultist GetCultist()
        {
            if (_CultistID < 0)
                return null;
            Cultist cultist = Global.Settings.Cultists.GetCultistByID(_CultistID);

            return cultist;
        }

        #endregion
    }
}
