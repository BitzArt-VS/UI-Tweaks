namespace BitzArt.UI.Tweaks.Services;

public enum GameStatusDetailType
{
    // ======================= Player health =======================

    PlayerCurrentHealth = 101,
    PlayerMaxHealth = 102,

    PlayerHealthPercentage = 111,

    // ====================== Player satiety ======================

    PlayerCurrentSatiety = 201,
    PlayerMaxSatiety = 202,

    PlayerSatietyPercentage = 211,

    PlayerSatietyHungerRate = 221,

    // ================= Player temporal stability =================

    PlayerTemporalStability = 301,

    // ========================= World time =========================

    // TODO

    // ================ Weather at player's location ================

    // TODO

    // ============= Player location temporal stability =============

    PlayerLocationTemporalStability = 2201
}
