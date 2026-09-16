using REP2P;

public interface P2PObservable
{
    public void OnP2PSerialize(P2PStream stream, bool write, P2PMessageInfo info);
}
