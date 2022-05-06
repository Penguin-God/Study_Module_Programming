public struct QuestSaveData
{
    public string codeName; // codeName은 Database에서 원본 Quset를 찾기 위한 Key 역할
    public QuestState state;
    public int currentTaskGroupIndex;
    public int[] taskSuccessCounts;
}