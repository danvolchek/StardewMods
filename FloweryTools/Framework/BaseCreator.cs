using FloweryTools.Framework.Flowerers;

namespace FloweryTools.Framework
{
    internal abstract class BaseCreator
    {
        protected FlowerHelper helper;

        public BaseCreator(FlowerHelper helper)
        {
            this.helper = helper;
        }
    }
}
