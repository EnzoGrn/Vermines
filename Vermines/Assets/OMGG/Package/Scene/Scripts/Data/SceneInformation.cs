using UnityEngine;
using System;

namespace OMGG.Scene.Data {

    using OMGG.Scene.Attribute;

    [Serializable]
    public partial struct SceneInformation {

        /// <summary>
        /// The display name of the scene.
        /// </summary>
        public string Name;

        /// <summary>
        /// The path to the scene asset.
        /// </summary>
        [ScenePath]
        public string ScenePath;

        /// <summary>
        /// Gets the filename of the <see cref="ScenePath" /> to set as Unity scene to load.
        /// </summary>
        public readonly string SceneName => ScenePath == null ? null : System.IO.Path.GetFileNameWithoutExtension(ScenePath);

        /// <summary>
        /// The sprite displayed as scene preview in the scene selection UI.
        /// </summary>
        public Sprite Preview;
    }
};
