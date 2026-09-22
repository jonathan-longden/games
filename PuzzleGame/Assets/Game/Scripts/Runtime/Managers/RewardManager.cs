using PuzzleGame.Core;

namespace PuzzleGame
{
    /// <summary>
    /// Turns a finished run into rewards: records stars/items/unlocks through the
    /// ProgressManager and pays coins through the CurrencyManager.
    /// Coin table (see ProgressRules): normal 100, hard 200, bonus/secret 300,
    /// boss 400; +50 for the move-target star, +100 for the mastery star
    /// (hard/boss: +75 / +150). Each amount is paid once, the first time it is earned.
    /// </summary>
    public sealed class RewardManager
    {
        readonly ProgressManager _progress;
        readonly CurrencyManager _currency;
        readonly SaveManager _save;

        public RewardManager(ProgressManager progress, CurrencyManager currency, SaveManager save)
        {
            _progress = progress;
            _currency = currency;
            _save = save;
        }

        public CompletionResult GrantCompletion(LevelData level, RunStats run, int forcedStars = 0)
        {
            var result = _progress.RecordCompletion(level, run, forcedStars);
            _currency.Add(result.TotalCoins);
            _save.Save(); // completion is the moment progress must never be lost
            return result;
        }
    }
}
