namespace Vermines.Core.UI {

    public interface IBackHandler {

        int Priority { get; }
        bool IsActive { get; }

        bool OnBackAction();
    }
}
