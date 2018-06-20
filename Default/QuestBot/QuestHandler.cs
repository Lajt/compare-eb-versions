using System;
using System.Threading.Tasks;

namespace Default.QuestBot
{
    public class QuestHandler
    {
        public static QuestHandler QuestAddedToCache = new QuestHandler(null, null);
        public static QuestHandler AllQuestsDone = new QuestHandler(null, null);

        public readonly Func<Task<bool>> Execute;
        public readonly Action Tick;

        public QuestHandler(Func<Task<bool>> execute, Action tick)
        {
            Execute = execute;
            Tick = tick;
        }
    }
}