namespace OMGG.Chronicle {

    public interface IChronicleBroadcaster {

        void BroadcastChronicle(ChronicleEntry entry);
        void BroadcastChronicleRemoval(string id);
    }
}
