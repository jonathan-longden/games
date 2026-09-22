using System;

namespace PuzzleGame
{
    /// <summary>
    /// The single wallet. Every coin that enters or leaves the game goes through here.
    /// Coins are a reward/progression currency only; no puzzle ever needs them.
    /// </summary>
    public sealed class CurrencyManager
    {
        readonly SaveManager _save;

        /// <summary>(new balance, delta)</summary>
        public event Action<int, int> Changed;

        public CurrencyManager(SaveManager save) { _save = save; }

        public int Coins => _save.Data.coins;
        public int LifetimeCoins => _save.Data.lifetimeCoins;

        public void Add(int amount)
        {
            if (amount <= 0) return;
            _save.Data.coins += amount;
            _save.Data.lifetimeCoins += amount;
            _save.MarkDirty();
            Changed?.Invoke(Coins, amount);
        }

        /// <summary>For future spending (shop, cosmetics). Nothing calls this yet.</summary>
        public bool TrySpend(int amount)
        {
            if (amount <= 0 || Coins < amount) return false;
            _save.Data.coins -= amount;
            _save.MarkDirty();
            Changed?.Invoke(Coins, -amount);
            return true;
        }
    }
}
