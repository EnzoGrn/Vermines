using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace OMGG.Menu.Configuration {

    using OMGG.Menu.Tools;
    using OMGG.Scene.Data;

    [CreateAssetMenu(menuName = "OMGG/Menu/Menu configuration")]
    [ScriptHelp(BackColor = ScriptHeaderBackColor.Orange)]
    public class ServerConfig : FusionScriptableObject {

        /// <summary>
        /// The maximum player count allowed for all game modes.
        /// </summary>
        [InlineHelp, SerializeField]
        protected int _MaxPlayers = 4;

        public int MaxPlayerCount => _MaxPlayers;

        /// <summary>
        /// Force 60 FPS during menu animations.
        /// </summary>
        [InlineHelp, SerializeField]
        protected bool _AdaptFramerateForMobilePlatform = true;

        public bool AdaptFramerateForMobilePlatform => _AdaptFramerateForMobilePlatform;

        /// <summary>
        /// Static list of regions selectable in the related dropdown menu.
        /// An empty entry symbolizes best region option.
        /// An empty list will hide is related drowpdown on the menu.
        /// </summary>
        /// <remarks>
        /// The default values will instantiate every region available in the Photon Fusion server (asia, eu, sa, us).
        /// </remarks>
        [InlineHelp, SerializeField]
        protected List<string> _AvailableRegions = new() {
            "asia",
            "eu",
            "sa",
            "us"
        };

        public List<string> AvailableRegions => _AvailableRegions;

        /// <summary>
        /// Static list of scenes available.
        /// See <see cref="SceneInformation"/> for more details.
        /// </summary>
        [InlineHelp, SerializeField]
        protected List<SceneInformation> _AvailableScenes = new();

        public List<SceneInformation> AvailableScenes => _AvailableScenes;

        /// <summary>
        /// The <see cref="MachineID"/> ScriptableObject that stores local ids to use as an option for application version.
        /// Designed as a convenient development feature.
        /// Can be null.
        /// </summary>
        [InlineHelp, SerializeField]
        protected MachineID _MachineId;

        public virtual string MachineId => _MachineId != null ? _MachineId.ID : null;

        /// <summary>
        /// The <see cref="CodeGenerator"/> ScriptableObject that is required for code generation.
        /// Also used to create random player names.
        /// </summary>
        [InlineHelp, SerializeField]
        protected CodeGenerator _CodeGenerator;

        public CodeGenerator CodeGenerator => _CodeGenerator;
    };
};
