using UnityEngine;

public class PlayerManager : CharacterManager
{
    protected override void SubscribeEvent()
    {
        base.SubscribeEvent();
        isDead.OnValueChanged += HandleBanner;
        targetCharacterManager.isDead.OnValueChanged += HandleBanner;
    }

    private void HandleBanner(bool value)
    {
        if (value == false) return;
        var ui = FindFirstObjectByType<VictoryDefeatUI>();
        if (ui == null) return;
        // 본인이 죽은 경우 패배, 타겟이 죽은 경우 승리
        if (targetCharacterManager != null && targetCharacterManager.isDead.Value)
        {
            ui.ShowVictory(3f);
        }
        else
        {
            ui.ShowDefeat(3f);
        }
    }
}
