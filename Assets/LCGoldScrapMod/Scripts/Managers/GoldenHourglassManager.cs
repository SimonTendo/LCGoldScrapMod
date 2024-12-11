using Unity.Netcode;
using BepInEx.Logging;

public class GoldenHourglassManager : NetworkBehaviour
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static GoldenHourglassManager instance;

    private float timeSpeedBeforeSlowDown = -1;
    public bool willSetTime = false;
    public bool setTimeToday = false;
    public int timePercentage;
    public int timePercentageGoldFever;
    public GoldenHourglassScript hourglassToBreak;

    private void Awake()
    {
        instance = this;
    }

    public void CheckToSetTime(bool endOfDay = false)
    {
        if (!endOfDay && willSetTime && StartOfRound.Instance.currentLevel.planetHasTime)
        {
            instance.SetTimeSlowClientRpc();
        }
        else if (endOfDay)
        {
            instance.SetTimeNormalClientRpc();
        }
    }

    [ServerRpc]
    public void ToggleTimeOfDayServerRpc(bool hostValue)
    {
        ToggleTimeOfDayClientRpc(hostValue);
        if (hostValue && !StartOfRound.Instance.inShipPhase && StartOfRound.Instance.shipDoorsEnabled && StartOfRound.Instance.currentLevel.planetHasTime)
        {
            instance.SetTimeSlowClientRpc();
        }
    }

    [ClientRpc]
    private void ToggleTimeOfDayClientRpc(bool hostValue)
    {
        instance.willSetTime = hostValue;
        Logger.LogInfo($"Golden Hourglass #{hourglassToBreak?.NetworkObjectId} set to: {instance.willSetTime}");
    }

    [ClientRpc]
    public void SetTimeSlowClientRpc()
    {
        if (setTimeToday)
        {
            Logger.LogDebug($"!!WARNING already set time today, no more GoldenHourglasses will fire");
            return;
        }
        TimeOfDay time = TimeOfDay.Instance;
        if (time != null)
        {
            Logger.LogDebug($"before: {timeSpeedBeforeSlowDown}");
            timeSpeedBeforeSlowDown = time.globalTimeSpeedMultiplier;
            Logger.LogDebug($"after: {timeSpeedBeforeSlowDown}");
            int percentageToUse = RarityManager.CurrentlyGoldFever() ? timePercentageGoldFever : timePercentage;
            Logger.LogDebug($"{percentageToUse}%");
            time.globalTimeSpeedMultiplier = timeSpeedBeforeSlowDown * (float)(percentageToUse / 100f);
            Logger.LogDebug($"global: {time.globalTimeSpeedMultiplier}");
            setTimeToday = true;
            willSetTime = false;
            if (IsServer && hourglassToBreak != null)
            {
                hourglassToBreak.BreakIfActivatedClientRpc();
            }
        }
    }

    [ClientRpc]
    public void SetTimeNormalClientRpc()
    {
        if (willSetTime && hourglassToBreak == null)
        {
            Logger.LogDebug($"WARNING!! day ended with willSetTime {willSetTime} but hourglassToBreak null {hourglassToBreak == null}, resetting manager!");
            setTimeToday = false;
            willSetTime = false;
            timeSpeedBeforeSlowDown = -1;
            return;
        }
        if (!setTimeToday)
        {
            Logger.LogDebug($"time speed has not been set, not touching timeOfDay");
            return;
        }
        TimeOfDay time = TimeOfDay.Instance;
        if (time != null)
        {
            Logger.LogDebug($"before: {timeSpeedBeforeSlowDown}");
            time.globalTimeSpeedMultiplier = timeSpeedBeforeSlowDown;
            Logger.LogDebug($"global: {time.globalTimeSpeedMultiplier}");
            timeSpeedBeforeSlowDown = -1;
            Logger.LogDebug($"after: {timeSpeedBeforeSlowDown}");
            setTimeToday = false;
            willSetTime = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int playerID)
    {
        SyncUponJoinClientRpc(willSetTime, playerID);
        if (hourglassToBreak != null)
        {
            NetworkObject currentHourglass = hourglassToBreak.gameObject.GetComponent<NetworkObject>();
            SyncHourglassUponJoinClientRpc(currentHourglass, playerID);
        }
        else
        {
            Logger.LogDebug($"HourglassManager: hourglassToBreak null = {hourglassToBreak == null}");
        }
    }

    [ClientRpc]
    private void SyncUponJoinClientRpc(bool hostWillSetTime, int playerID)
    {
        if (playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            willSetTime = hostWillSetTime;
            Logger.LogDebug($"HourglassManager: set willSetTime to {willSetTime}");
        }
    }

    [ClientRpc]
    private void SyncHourglassUponJoinClientRpc(NetworkObjectReference hostHourglass, int playerID)
    {
        if (hostHourglass.TryGet(out var networkObject))
        {
            if (playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
            {
                hourglassToBreak = networkObject.GetComponent<GoldenHourglassScript>();
            }
        }
        Logger.LogDebug($"HourglassManager: hostHourglass null = {hourglassToBreak == null}, set to {hourglassToBreak?.gameObject.name} #{hourglassToBreak.NetworkObjectId}");
    }
}
