namespace Homework
{
    public interface ICaclRankManager
    {
        /// <summary>
        /// 根据数据量设置Duration
        /// </summary>
        /// <returns></returns>
        int GetDuration();

        void Execute();

    }
}
