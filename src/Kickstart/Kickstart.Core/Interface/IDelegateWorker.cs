using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Interface
{
    public interface IDelegateWorker
    {
        void Start<TInput, TResult>(Func<TInput, TResult> onStart, Action<TResult> onComplete, TInput parm);
    }
}
