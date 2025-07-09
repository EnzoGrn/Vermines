using System.Collections;

namespace Vermines.Menu.Tools {

    public interface ILoadingSteps {

        string StepName { get; }

        IEnumerator Execute();
    }
}
