using UnityEngine;
using Fusion;

namespace OMGG.Menu.Configuration {

    /// <summary>
    /// A scriptable object that has an id used by the OMGG.Menu as an application version.
    /// Mostly a developement feature to ensure to only meet compatible clients in the Photon matchmaking.
    /// </summary>
    [CreateAssetMenu(menuName = "OMGG/Menu/Machine ID")]
    public class MachineID : FusionScriptableObject {

        /// <summary>
        /// An id that should be unique to this machine, used by the OMGG.Menu as an application version.
        /// An explicit asset importer is used to create local ids during import <see cref="Importer.MachineIDImporter" />.
        /// </summary>
        [InlineHelp]
        public string ID;
    }
};
