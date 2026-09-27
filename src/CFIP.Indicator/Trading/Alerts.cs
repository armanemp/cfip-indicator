using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        // ============================================================
        
                private void SendUnifiedAlert(
                    string key,
                    string message,
                    int direction,
                    bool critical)
                {
                    if (string.IsNullOrWhiteSpace(
                            message))
                        return;
        
                    DateTime now =
                        TimeInUtc;
        
                    if (direction == 0)
                        direction =
                            GetAuthoritativeDirection();
        
                    bool restrictionAlert =
                        key.StartsWith(
                            "RESTRICT|",
                            StringComparison.OrdinalIgnoreCase);
        
                    if (restrictionAlert &&
                        !string.IsNullOrWhiteSpace(
                            _lastRestrictionMessage) &&
                        message.IndexOf(
                            _lastRestrictionMessage,
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        return;
        
                    if (SuppressDuplicateAlerts)
                    {
                        int cooldownSeconds =
                            (key.StartsWith(
                                "SMART|",
                                StringComparison.OrdinalIgnoreCase) ||
                             key.StartsWith(
                                "REACTION|",
                                StringComparison.OrdinalIgnoreCase))
                                ? SmartAlertCooldownSeconds
                                : AlertCooldownSeconds;
        
                        if (_alertCooldowns.TryGetValue(
                                key,
                                out DateTime lastSent) &&
                            (now - lastSent).TotalSeconds <
                            Math.Max(
                                1,
                                cooldownSeconds))
                            return;
                    }
        
                    _alertCooldowns[key] = now;
        
                    // Light housekeeping so this dictionary can't grow forever over
                    // a long-running session — unique keys (per-plan TP/SL, daily
                    // markers) accumulate over days/weeks otherwise.
                    if (_alertCooldowns.Count > 500)
                    {
                        List<string> stale =
                            new List<string>();
        
                        foreach (KeyValuePair<string, DateTime> entry in
                                 _alertCooldowns)
                        {
                            if ((now - entry.Value).TotalHours > 24)
                                stale.Add(entry.Key);
                        }
        
                        foreach (string staleKey in stale)
                            _alertCooldowns.Remove(staleKey);
                    }
        
                    _lastAlertMessage =
                        message;
        
                    _lastAlertDirection =
                        direction;
        
                    _lastAlertCritical =
                        critical;
        
                    _lastAlertUtc =
                        now;
        
                    if (EnableSoundAlerts)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(
                                    SoundFilePath))
                            {
                                Notifications.PlaySound(
                                    SoundFilePath);
                            }
                            else
                            {
                                Notifications.PlaySound(
                                    ResolveAlertSoundType(
                                        key,
                                        critical));
                            }
                        }
                        catch (Exception ex)
                        {
                            Print(
                                "CFIP sound alert failed: {0}",
                                ex.Message);
                        }
                    }
        
                    if (EnableEmailAlerts &&
                        !string.IsNullOrWhiteSpace(
                            SenderEmail) &&
                        !string.IsNullOrWhiteSpace(
                            ReceiverEmail))
                    {
                        try
                        {
                            Notifications.SendEmail(
                                SenderEmail,
                                ReceiverEmail,
                                "CFIP SMART  " +
                                SymbolName,
                                message);
                        }
                        catch (Exception ex)
                        {
                            Print(
                                "CFIP email failed: {0}",
                                ex.Message);
                        }
                    }
        
                    bool restrictionPopup =
                        restrictionAlert &&
                        ShowEntryRestrictionPopup;
        
                    if (ShowPopupAlerts &&
                        (restrictionPopup ||
                         (!restrictionAlert &&
                          (!PopupCriticalOnly ||
                           critical))))
                    {
                        ShowPopup(
                            message);
                    }
                }
        
                private void CheckEndOfDayAlert(
                    DateTime nowUtc)
                {
                    DateTime dayClose =
                        new DateTime(
                            nowUtc.Year,
                            nowUtc.Month,
                            nowUtc.Day,
                            ClampInt(
                                SessionEndUtc,
                                0,
                                23),
                            0,
                            0,
                            DateTimeKind.Utc);
        
                    if (EnableEndOfDayAlert)
                    {
                        DateTime warnStart =
                            dayClose.AddMinutes(
                                -Math.Max(
                                    5,
                                    EndOfDayAlertMinutesBefore));
        
                        if (nowUtc >= warnStart &&
                            nowUtc <= dayClose &&
                            _lastEndOfDayAlertDate.Date !=
                            nowUtc.Date &&
                            HasManagedOpenPosition())
                        {
                            _lastEndOfDayAlertDate =
                                nowUtc.Date;
        
                            int minutesLeft =
                                Math.Max(
                                    0,
                                    (int)Math.Round(
                                        (dayClose -
                                         nowUtc).TotalMinutes));
        
                            SendUnifiedAlert(
                                "DAYEND|" +
                                nowUtc.Date.ToString(
                                    "yyyyMMdd"),
                                "Day-trading session closes in ~" +
                                minutesLeft +
                                " min (" +
                                ClampInt(
                                    SessionEndUtc,
                                    0,
                                    23).ToString("00") +
                                ":00 UTC) - " +
                                (EnableEndOfDayAutoClose
                                    ? "positions will be closed automatically at " +
                                      "the session close."
                                    : "review/close open positions manually. " +
                                      "This indicator never closes trades " +
                                      "automatically."),
                                0,
                                true);
                        }
                    }
        
                    // ENHANCEMENT: optional actual auto-close at the day's close,
                    // separate from the warning above and defaulting ON per
                    // request. Fires once per day, only once we've reached the
                    // configured close time, and only if a managed position is
                    // still open at that point.
                    if (EnableEndOfDayAutoClose &&
                        nowUtc >= dayClose &&
                        _lastEndOfDayCloseDate.Date !=
                        nowUtc.Date &&
                        HasManagedOpenPosition())
                    {
                        _lastEndOfDayCloseDate =
                            nowUtc.Date;
        
                        CloseAllPositions();
        
                        SendUnifiedAlert(
                            "DAYEND-CLOSED|" +
                            nowUtc.Date.ToString(
                                "yyyyMMdd"),
                            "CFIP closed all managed positions at day-trading " +
                            "session end (" +
                            ClampInt(
                                SessionEndUtc,
                                0,
                                23).ToString("00") +
                            ":00 UTC).",
                            0,
                            true);
                    }
                }
        
                private bool HasManagedOpenPosition()
                {
                    foreach (Position position in Positions)
                    {
                        if (position == null ||
                            position.SymbolName !=
                            SymbolName)
                            continue;
        
                        if (!IsManagedPosition(position))
                            continue;
        
                        return true;
                    }
        
                    return false;
                }
        
                        private void CheckReversalProtection()
                {
                    if (!EnableReversalProtectionClose ||
                        _decision == null ||
                        _decision.Direction == 0 ||
                        (_plan != null &&
                         _lifecycleState !=
                            LifecycleState.LivePosition))
                        return;
        
                    foreach (Position position in Positions)
                    {
                        if (position == null ||
                            position.SymbolName != SymbolName)
                            continue;
        
                        if (!IsManagedPosition(position))
                            continue;
        
                        int positionDirection =
                            position.TradeType == TradeType.Buy ? 1 : -1;
        
                        if (_decision.Direction == positionDirection ||
                            _decision.SmartQuality < ReversalProtectionMinimumQuality ||
                            _decision.IndependentEvidence < Math.Max(2, ReversalCloseMinimumEvidence) ||
                            _decision.TimeframeAgreement < Math.Max(50, ReversalCloseMinimumMtf))
                            continue;
        
                        if (!IsDecisiveOppositeDirection(positionDirection) ||
                            position.NetProfit <= 0)
                            continue;
        
                        double protectedProfit =
                            position.NetProfit;
        
                        if (_plan != null &&
                            _plan.IsLivePosition)
                        {
                            SetLifecycleState(
                                LifecycleState.ExitRequested,
                                "REVERSAL PROTECTION");
                        }
        
                        if (!TryClosePosition(
                                position,
                                "REVERSAL PROTECTION"))
                        {
                            if (_plan != null &&
                                _plan.IsLivePosition)
                            {
                                SetLifecycleState(
                                    LifecycleState.RecoveryRequired,
                                    "REVERSAL PROTECTION • EXIT REJECTED");
                            }
        
                            continue;
                        }
        
                        SendUnifiedAlert(
                            "REVERSAL-CLOSE|" +
                            position.Id,
                            "CFIP REVERSAL EXIT REQUESTED | #" +
                            position.Id +
                            " | protected +" +
                            protectedProfit.ToString("F2") +
                            " | Q " +
                            _decision.SmartQuality +
                            " | MTF " +
                            _decision.TimeframeAgreement +
                            " | EVID " +
                            _decision.IndependentEvidence,
                            positionDirection,
                            true);
                    }
                }
        
                private bool IsDecisiveOppositeDirection(int positionDirection)
                {
                    if (_m5Frame == null ||
                        _m5Bars == null ||
                        positionDirection == 0)
                        return false;
        
                    int opposite = positionDirection * -1;
        
                    bool m5Structural =
                        opposite == 1
                            ? (_m5Frame.MssBull || _m5Frame.ChochBull)
                            : (_m5Frame.MssBear || _m5Frame.ChochBear);
        
                    bool m5Force =
                        opposite == 1
                            ? (_m5Frame.DisplacementBull &&
                               _m5Frame.LiquidityBull)
                            : (_m5Frame.DisplacementBear &&
                               _m5Frame.LiquidityBear);
        
                    bool m15Aligned =
                        _m15Frame != null &&
                        _m15Frame.Direction == opposite;
        
                    bool m15Structure =
                        m15Aligned &&
                        (opposite == 1
                            ? (_m15Frame.MssBull || _m15Frame.ChochBull)
                            : (_m15Frame.MssBear || _m15Frame.ChochBear));
        
                    int evidence = 0;
        
                    if (opposite == 1)
                    {
                        if (_m5Frame.MssBull) evidence++;
                        if (_m5Frame.ChochBull) evidence++;
                        if (_m5Frame.DisplacementBull) evidence++;
                        if (_m5Frame.LiquidityBull) evidence++;
                    }
                    else
                    {
                        if (_m5Frame.MssBear) evidence++;
                        if (_m5Frame.ChochBear) evidence++;
                        if (_m5Frame.DisplacementBear) evidence++;
                        if (_m5Frame.LiquidityBear) evidence++;
                    }
        
                    if (!m5Structural ||
                        evidence < Math.Max(1, ReversalCloseMinimumEvidence))
                        return false;
        
                    if (RequireReversalForce &&
                        !m5Force)
                        return false;
        
                    if (RequireM15ReversalForOpposite &&
                        (!m15Aligned || !m15Structure))
                        return false;
        
                    return true;
                }
        
        private bool ExecutePartialClose(
                    double percentOfOriginal,
                    string tag)
                {
                    if (!EnablePartialTakeProfit ||
                        _plan == null ||
                        _plan.OriginalVolume <= 0 ||
                        percentOfOriginal <= 0)
                        return true;
        
                    foreach (Position position in Positions)
                    {
                        if (position == null ||
                            position.SymbolName !=
                            SymbolName ||
                            !IsManagedPosition(position))
                            continue;
        
                        double closeVolume =
                            Symbol.NormalizeVolumeInUnits(
                                _plan.OriginalVolume *
                                percentOfOriginal /
                                100.0,
                                RoundingMode.Down);
        
                        closeVolume =
                            Math.Min(
                                closeVolume,
                                position.VolumeInUnits);
        
                        if (closeVolume <
                            Symbol.VolumeInUnitsMin)
                            return false;
        
                        double remainder =
                            position.VolumeInUnits -
                            closeVolume;
        
                        if (remainder > 0 &&
                            remainder <
                            Symbol.VolumeInUnitsMin)
                            closeVolume =
                                position.VolumeInUnits;
        
                        try
                        {
                            bool closingEverything =
                                closeVolume >=
                                position.VolumeInUnits;
        
                            TradeResult closeResult =
                                closingEverything
                                    ? ClosePosition(position)
                                    : ClosePosition(
                                        position,
                                        closeVolume);
        
                            if (closeResult == null ||
                                !closeResult.IsSuccessful)
                            {
                                Print(
                                    "CFIP partial close rejected ({0}): {1}",
                                    tag,
                                    closeResult != null &&
                                    closeResult.Error.HasValue
                                        ? closeResult.Error.Value.ToString()
                                        : "UNKNOWN");
                                return false;
                            }
        
                            if (MoveToBreakEvenAfterPartial &&
                                !closingEverything)
                            {
                                bool shouldMove =
                                    !position.StopLoss.HasValue ||
                                    BetterStop(
                                        _plan.Direction,
                                        position.EntryPrice,
                                        position.StopLoss.Value);
        
                                if (shouldMove)
                                {
                                    TryModifyStopLoss(
                                        position,
                                        position.EntryPrice,
                                        "PARTIAL BREAK-EVEN");
                                }
                            }
        
                            if (EnableLevelHitAlerts &&
                                AlertOnLevelHit)
                            {
                                SendUnifiedAlert(
                                    "PARTIAL|" +
                                    tag +
                                    "|" +
                                    _plan.CreatedM5,
                                    "CFIP partial close at " +
                                    tag +
                                    " - closed " +
                                    percentOfOriginal.ToString(
                                        "F0") +
                                    "% of original size",
                                    _plan.Direction,
                                    false);
                            }
        
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Print(
                                "CFIP partial close failed ({0}): {1}",
                                tag,
                                ex.Message);
                            return false;
                        }
                    }
        
                    return false;
                }
        
                private bool DailyLossLimitHit(
                    DateTime nowUtc)
                {
                    if (!EnableDailyLossLimit)
                        return false;
        
                    if (_dailyLossBaselineDate.Date !=
                        nowUtc.Date)
                    {
                        _dailyLossBaselineDate =
                            nowUtc.Date;
        
                        _dailyStartEquity =
                            Account.Equity;
        
                        _dailyLossLimitAlerted =
                            false;
                    }
        
                    if (_dailyStartEquity <= 0)
                        return false;
        
                    double lossPercent =
                        (_dailyStartEquity -
                         Account.Equity) /
                        _dailyStartEquity *
                        100.0;
        
                    if (lossPercent <
                        Math.Max(
                            0.5,
                            MaximumDailyLossPercent))
                        return false;
        
                    if (!_dailyLossLimitAlerted)
                    {
                        _dailyLossLimitAlerted =
                            true;
        
                        SendUnifiedAlert(
                            "DAILYLOSS|" +
                            nowUtc.Date.ToString(
                                "yyyyMMdd"),
                            "Daily loss limit reached (" +
                            lossPercent.ToString("F2") +
                            "% >= " +
                            MaximumDailyLossPercent.ToString(
                                "F2") +
                            "%) - new auto-trade entries are blocked for the " +
                            "rest of the day. Open positions are left untouched.",
                            0,
                            true);
                    }
        
                    return true;
                }
        
                private SoundType ResolveAlertSoundType(
                    string key,
                    bool critical)
                {
                    if (!UseSemanticAlertSounds)
                        return AlertSoundType;
        
                    if (key.StartsWith(
                            "SL|",
                            StringComparison.OrdinalIgnoreCase) ||
                        key.StartsWith(
                            "INVALID",
                            StringComparison.OrdinalIgnoreCase) ||
                        key.StartsWith(
                            "RESTRICT|",
                            StringComparison.OrdinalIgnoreCase) ||
                        key.StartsWith(
                            "REVERSAL|",
                            StringComparison.OrdinalIgnoreCase))
                        return SoundType.NegativeNotification;
        
                    if (key.StartsWith(
                            "TP",
                            StringComparison.OrdinalIgnoreCase) ||
                        key.StartsWith(
                            "AUTO",
                            StringComparison.OrdinalIgnoreCase) ||
                        key.StartsWith(
                            "SIGNAL|",
                            StringComparison.OrdinalIgnoreCase) ||
                        key.StartsWith(
                            "HIGH|",
                            StringComparison.OrdinalIgnoreCase) ||
                        key.StartsWith(
                            "SMART|",
                            StringComparison.OrdinalIgnoreCase))
                        return SoundType.PositiveNotification;
        
                    return critical
                        ? SoundType.Confirmation
                        : SoundType.Announcement;
                }
        
                        private bool IsInsideSessionWindow(DateTime utc)
                {
                    int start =
                        ClampInt(SessionStartUtc, 0, 23) * 60;
                    int end =
                        ClampInt(SessionEndUtc, 0, 23) * 60;
                    int now =
                        utc.Hour * 60 + utc.Minute;
        
                    if (start == end)
                        return true;
        
                    return start < end
                        ? now >= start && now < end
                        : now >= start || now < end;
                }
        
                private double AverageAtr(Bars bars, int index, int lookback)
                {
                    if (bars == null || index < 1)
                        return 0;
        
                    int count =
                        Math.Max(2, Math.Min(lookback, index));
                    int start =
                        Math.Max(1, index - count + 1);
        
                    double sum = 0;
                    int samples = 0;
        
                    for (int i = start; i <= index; i++)
                    {
                        double value = Atr(bars, i);
        
                        if (value <= 0)
                            continue;
        
                        sum += value;
                        samples++;
                    }
        
                    return samples > 0 ? sum / samples : 0;
                }
        
                private int CalculateMarketSuitability(
                    int closedM5,
                    int direction,
                    out string reason)
                {
                    reason = "OK";
        
                    if (direction != 1 && direction != -1)
                    {
                        reason = "NO DIRECTION";
                        return 0;
                    }
        
                    if (_m5Bars == null ||
                        _m5Frame == null ||
                        _m15Frame == null)
                    {
                        reason = "MTF DATA";
                        return 0;
                    }
        
                    if (!Symbol.IsTradingEnabled)
                    {
                        reason = "SYMBOL TRADING DISABLED";
                        return 0;
                    }
        
                    if (Symbol.MarketHours == null ||
                        !Symbol.MarketHours.IsOpened())
                    {
                        reason = "MARKET CLOSED";
                        return 0;
                    }
        
                    DateTime nowUtc = TimeInUtc;
                    int score = 50;
        
                    bool inSession =
                        IsInsideSessionWindow(nowUtc);
        
                    score += inSession ? 10 : -8;
        
                    if (UseSessionFilter &&
                        !SessionAllowed(nowUtc))
                    {
                        reason = "SESSION FILTER";
                        return 25;
                    }
        
                    if (RequireSessionSuitability &&
                        !inSession)
                    {
                        reason = "SESSION SUITABILITY";
                        return 30;
                    }
        
                    if (!FridayAllowed(nowUtc))
                    {
                        reason = "FRIDAY CUTOFF";
                        return 25;
                    }
        
                    string newsReason;
        
                    if (NewsBlocked(nowUtc, out newsReason))
                    {
                        reason = newsReason;
                        return 20;
                    }
        
                    if (VolatilityBlocked(_m5Bars, closedM5))
                    {
                        reason = "EVENT SHOCK / VOLATILITY";
                        return 25;
                    }
        
                    double atr =
                        Atr(_m5Bars, closedM5);
        
                    if (atr <= 0)
                    {
                        reason = "ATR UNAVAILABLE";
                        return 0;
                    }
        
                    double spreadRatio =
                        Math.Max(0, Symbol.Ask - Symbol.Bid) /
                        Math.Max(atr, Symbol.TickSize);
        
                    if (spreadRatio <= MaximumSpreadAtr * 0.50)
                        score += 18;
                    else if (spreadRatio <= MaximumSpreadAtr)
                        score += 8;
                    else
                        score -= 22;
        
                    double averageAtr =
                        AverageAtr(_m5Bars, closedM5 - 1, 20);
        
                    double volatilityRatio =
                        averageAtr > 0 ? atr / averageAtr : 1.0;
        
                    if (volatilityRatio >=
                            Math.Max(0.75, HealthyAtrMinimumRatio) &&
                        volatilityRatio <=
                            Math.Max(1.10, HealthyAtrMaximumRatio))
                        score += 12;
                    else if (volatilityRatio < 0.60 ||
                             volatilityRatio > 2.25)
                        score -= 15;
                    else
                        score += 4;
        
                    if (_m5Frame.Direction == direction)
                        score += 9;
                    else if (_m5Frame.Direction == -direction)
                        score -= 10;
        
                    if (_m15Frame.Direction == direction)
                        score += 9;
                    else if (_m15Frame.Direction == -direction)
                        score -= 10;
        
                    if (_m30Frame != null)
                    {
                        if (_m30Frame.Direction == direction)
                            score += 5;
                        else if (_m30Frame.Direction == -direction)
                            score -= 6;
                    }
        
                    if (_h1Frame != null)
                    {
                        if (_h1Frame.Direction == direction)
                            score += 4;
                        else if (_h1Frame.Direction == -direction)
                            score -= 5;
                    }
        
                    if (_h4Frame != null)
                    {
                        if (_h4Frame.Direction == direction)
                            score += 3;
                        else if (_h4Frame.Direction == -direction)
                            score -= 4;
                    }
        
                    double averageAdx =
                        (_m5Frame.Adx + _m15Frame.Adx) / 2.0;
        
                    if (averageAdx >= 25)
                        score += 12;
                    else if (averageAdx >= 20)
                        score += 7;
                    else if (averageAdx < 15)
                        score -= 8;
        
                    if (_m5Frame.Choppy && _m15Frame.Choppy)
                        score -= 18;
                    else if (_m5Frame.Choppy || _m15Frame.Choppy)
                        score -= 7;
        
                    if (_decision != null &&
                        _decision.TimeframeAgreement >=
                        MinimumTimeframeAgreement)
                        score += 8;
                    else if (_decision != null &&
                             _decision.TimeframeAgreement < 60)
                        score -= 8;
        
                    if (_decision != null &&
                        _decision.IndependentEvidence >=
                        MinimumIndependentEvidence)
                        score += 5;
        
                    if (UseDailyPivots &&
                        _d1Bars != null &&
                        _d1Bars.Count >= 3)
                    {
                        int d1Index =
                            ClosedIndex(_d1Bars, nowUtc);
        
                        if (d1Index > 0)
                        {
                            int previous = d1Index - 1;
                            double pivot =
                                (_d1Bars.HighPrices[previous] +
                                 _d1Bars.LowPrices[previous] +
                                 _d1Bars.ClosePrices[previous]) / 3.0;
        
                            double market =
                                direction == 1
                                    ? Symbol.Ask
                                    : Symbol.Bid;
        
                            if ((direction == 1 && market >= pivot) ||
                                (direction == -1 && market <= pivot))
                                score += 5;
                            else
                                score -= 3;
                        }
                    }
        
                    score = ClampInt(score, 0, 100);
        
                    reason =
                        score >= Math.Max(50, MinimumMarketSuitability)
                            ? "SUITABILITY " + score
                            : "SUITABILITY " + score +
                              " < " + MinimumMarketSuitability;
        
                    return score;
                }
        
                private int RefreshMarketSuitability(
                    int closedM5,
                    int direction,
                    bool force)
                {
                    DateTime nowUtc = TimeInUtc;
        
                    int throttleSeconds =
                        Math.Max(1, SuitabilityRecalculationSeconds);
        
                    bool due =
                        force ||
                        closedM5 != _marketSuitabilityM5 ||
                        _marketSuitabilityDirection != direction ||
                        (nowUtc - _lastMarketSuitabilityUtc).TotalSeconds >=
                        throttleSeconds;
        
                    if (!due)
                        return _marketSuitabilityScore;
        
                    string reason;
        
                    int score =
                        CalculateMarketSuitability(
                            closedM5,
                            direction,
                            out reason);
        
                    _marketSuitabilityM5 = closedM5;
                    _marketSuitabilityDirection = direction;
                    _marketSuitabilityScore = score;
                    _marketSuitabilityReason = reason;
                    _marketSuitabilityState =
                        score >= Math.Max(50, MinimumMarketSuitability)
                            ? "SUITABLE"
                            : "UNSUITABLE";
                    _lastMarketSuitabilityUtc = nowUtc;
        
                    return score;
                }
        
                private bool PassesMarketSuitability(
                    int closedM5,
                    int direction,
                    out string reason)
                {
                    int score =
                        RefreshMarketSuitability(
                            closedM5,
                            direction,
                            false);
        
                    reason = _marketSuitabilityReason;
        
                    if (!EnableMarketSuitabilityGuard)
                        return true;
        
                    bool hardContextBlock =
                        _marketSuitabilityReason ==
                            "SYMBOL TRADING DISABLED" ||
                        _marketSuitabilityReason ==
                            "MARKET CLOSED" ||
                        _marketSuitabilityReason ==
                            "SESSION FILTER" ||
                        _marketSuitabilityReason ==
                            "SESSION SUITABILITY" ||
                        _marketSuitabilityReason ==
                            "FRIDAY CUTOFF" ||
                        _marketSuitabilityReason ==
                            "NEWS BLACKOUT" ||
                        _marketSuitabilityReason ==
                            "EVENT SHOCK / VOLATILITY";
        
                    if (hardContextBlock &&
                        HardMarketSuitabilityGate)
                        return false;
        
                    if (score <
                            Math.Max(
                                50,
                                MinimumMarketSuitability) &&
                        HardMarketSuitabilityGate)
                        return false;
        
                    return true;
                }
        
                private double SuitabilityRiskMultiplier()
                {
                    if (!UseSmartRiskScaling)
                        return 1.0;
        
                    double floor =
                        ClampDouble(
                            MinimumSmartRiskMultiplier,
                            0.25,
                            1.0);
        
                    double confidence =
                        _decision == null
                            ? 0
                            : ClampDouble(
                                _decision.Confidence,
                                0,
                                100);
        
                    double confidenceScale =
                        ClampDouble(
                            (confidence - 70.0) /
                            Math.Max(
                                1.0,
                                Math.Max(
                                    70,
                                    FullRiskConfidenceThreshold) - 70.0),
                            0,
                            1);
        
                    double suitabilityScale =
                        ClampDouble(
                            _marketSuitabilityScore /
                            (double)Math.Max(
                                60,
                                FullRiskSuitabilityThreshold),
                            0,
                            1);
        
                    double scale =
                        floor +
                        (1.0 - floor) *
                        (0.60 * confidenceScale +
                         0.40 * suitabilityScale);
        
                    if (PenalizeChoppyRegimeRisk &&
                        _m5Frame != null &&
                        _m15Frame != null &&
                        (_m5Frame.Choppy ||
                         _m15Frame.Choppy))
                        scale *= 0.82;
        
                    return ClampDouble(
                        scale,
                        floor,
                        1.0);
                }
        
                private double EffectiveAutoRiskPercent()
                {
                    double baseRisk =
                        Math.Max(
                            0.05,
                            RiskPercentEquity);
        
                    if (!UseSmartRiskScaling)
                        return baseRisk;
        
                    return ClampDouble(
                        baseRisk * SuitabilityRiskMultiplier(),
                        0.05,
                        baseRisk);
                }
        
                private double EffectiveAggressiveRiskPercent()
                {
                    double baseRisk =
                        Math.Max(
                            0.05,
                            AggressiveRiskPercentEquity);
        
                    if (!UseSmartRiskScaling)
                        return baseRisk;
        
                    return ClampDouble(
                        baseRisk * SuitabilityRiskMultiplier(),
                        0.05,
                        baseRisk);
                }
        
                private bool PassesAutoTradeSafetyGuards(
                    TradeType tradeType,
                    double volume,
                    out string reason)
                {
                    reason = "";
        
                    if (!IsFinitePositive(volume))
                    {
                        reason = "INVALID VOLUME";
                        return false;
                    }
        
                    if (UseMarketHoursGuard)
                    {
                        if (!Symbol.IsTradingEnabled)
                        {
                            reason = "SYMBOL TRADING DISABLED";
                            return false;
                        }
        
                        if (Symbol.MarketHours == null ||
                            !Symbol.MarketHours.IsOpened())
                        {
                            reason = "MARKET CLOSED";
                            return false;
                        }
                    }
        
                    if (UseAutoMarginGuard)
                    {
                        double freeMargin =
                            Account.FreeMargin;
        
                        if (!IsFinitePositive(freeMargin))
                        {
                            reason = "NO FREE MARGIN";
                            return false;
                        }
        
                        double estimatedMargin =
                            Symbol.GetEstimatedMargin(
                                tradeType,
                                volume);
        
                        if (!IsFinitePositive(estimatedMargin))
                        {
                            reason = "MARGIN ESTIMATE FAILED";
                            return false;
                        }
        
                        double maximumUsage =
                            Math.Max(
                                10,
                                Math.Min(
                                    100,
                                    MaxAutoMarginUsagePercent -
                                    Math.Max(
                                        0,
                                        Math.Min(
                                            40,
                                            MarginBufferPercent))));
        
                        double allowedMargin =
                            freeMargin *
                            maximumUsage /
                            100.0;
        
                        if (estimatedMargin >
                            allowedMargin)
                        {
                            reason =
                                "MARGIN " +
                                estimatedMargin.ToString("F0") +
                                " > " +
                                allowedMargin.ToString("F0");
                            return false;
                        }
                    }
        
                    return true;
                }
        
                private void SetAutoTradingState(
                    string state,
                    string reason)
                {
                    _autoTradingState =
                        string.IsNullOrWhiteSpace(state)
                            ? "WAIT"
                            : state.Trim();
        
                    _autoTradingReason =
                        string.IsNullOrWhiteSpace(reason)
                            ? ""
                            : reason.Trim();
                }
        
                private string AutoTradingPanelLine()
                {
                    if (!AutoTradingEnabled)
                        return
                            "AUTO TRADING  •  OFF  •  MANUAL REVIEW" +
                            (AutomaticOrdersEnabled
                                ? "  •  ORDERS ON"
                                : "  •  ORDERS OFF");
        
                    string state =
                        string.IsNullOrWhiteSpace(_autoTradingState)
                            ? "ARMED"
                            : _autoTradingState;
        
                    return
                        "AUTO TRADING  •  ON  •  " +
                        state +
                        "  •  " +
                        (HasTradingPermission() ? "PERM OK" : "PERM OFF") +
                        "  •  " +
                        (AutomaticOrdersEnabled ? "ORDERS ON" : "ORDERS OFF") +
                        "  •  SMART EXEC " +
                        _marketSuitabilityScore +
                        "/100";
                }
        
        private Color AutoTradingPanelColor()
                {
                    if (!AutoTradingEnabled)
                        return PanelMutedTextColor;
        
                    if (string.Equals(
                            _autoTradingState,
                            "EXECUTED",
                            StringComparison.OrdinalIgnoreCase))
                        return TpLineColor;
        
                    if (string.Equals(
                            _autoTradingState,
                            "BLOCKED",
                            StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(
                            _autoTradingState,
                            "ERROR",
                            StringComparison.OrdinalIgnoreCase))
                        return PanelWarningColor;
        
                    return PanelAccentColor;
                }
        
                private double SelectStructuralAutoTarget(
                    int closedM5,
                    int direction,
                    double entry,
                    double stop,
                    double atr,
                    TargetStage stage)
                {
                    if (atr <= 0 ||
                        !IsFinitePositive(entry) ||
                        !IsFinitePositive(stop))
                        return 0;
        
                    double risk =
                        Math.Abs(entry - stop);
        
                    if (risk <= 0)
                        return 0;
        
                    List<Level> levels =
                        BuildTargetLevels(
                            closedM5,
                            direction,
                            entry,
                            atr);
        
                    List<Level> selected =
                        SelectTargets(
                            levels,
                            closedM5,
                            entry,
                            risk,
                            direction,
                            atr);
        
                    int stageIndex =
                        ClampInt(
                            (int)stage,
                            0,
                            3);
        
                    // All execution paths use the same progressive fallback policy:
                    // requested stage -> nearest available earlier stage -> synthetic
                    // RR target (when explicitly permitted). This keeps pending,
                    // aggressive and market execution behavior identical.
                    for (int i = stageIndex;
                         i >= 0;
                         i--)
                    {
                        if (i < selected.Count &&
                            selected[i] != null &&
                            IsFinitePositive(
                                selected[i].Price))
                            return NormalizePrice(
                                selected[i].Price);
                    }
        
                    double fallbackRR =
                        stageIndex == 0
                            ? Math.Max(
                                FallbackTp1RR,
                                MinimumRequiredRR())
                            : stageIndex == 1
                                ? Math.Max(
                                    FallbackTp2RR,
                                    Tp2MinimumRR)
                                : stageIndex == 2
                                    ? Math.Max(
                                        FallbackTp3RR,
                                        Tp3MinimumRR)
                                    : Math.Max(
                                        FallbackTp4RR,
                                        Tp4MinimumRR);
        
                    bool requiresHtf =
                        stageIndex == 0
                            ? RequireHtfRewardForTp1
                            : RequireHtfRewardForTp2Plus;
        
                    if (AllowSyntheticTargetFallback &&
                        !requiresHtf &&
                        fallbackRR > 0)
                    {
                        return NormalizePrice(
                            direction == 1
                                ? entry + risk * fallbackRR
                                : entry - risk * fallbackRR);
                    }
        
                    return 0;
                }
        
                private void ReconcileLivePlanToActualFill(
                    Position position,
                    int closedM5)
                {
                    if (_plan == null ||
                        position == null ||
                        _m5Bars == null ||
                        closedM5 < 20)
                        return;
        
                    int direction =
                        position.TradeType == TradeType.Buy
                            ? 1
                            : -1;
        
                    double actualEntry =
                        NormalizePrice(
                            position.EntryPrice);
        
                    if (!IsFinitePositive(actualEntry))
                        return;
        
                    double atr =
                        Atr(
                            _m5Bars,
                            closedM5);
        
                    if (atr <= 0)
                        return;
        
                    _plan.Entry =
                        actualEntry;
        
                    // The broker may fill at a slightly different price than the
                    // executable quote. Rebuild the complete structural ladder from
                    // the actual fill so chart, plan and broker protection converge.
                    if (RebuildSmartExecutionLevels(
                            closedM5,
                            direction,
                            actualEntry,
                            atr))
                    {
                        _plan.Entry =
                            actualEntry;
        
                        return;
                    }
        
                    double currentStop =
                        _plan.Stop;
        
                    if (!IsValidStop(
                            direction,
                            actualEntry,
                            currentStop))
                    {
                        string stopSource;
                        int stopQuality;
        
                        double rebuiltStop =
                            BuildStructuralStop(
                                closedM5,
                                direction,
                                actualEntry,
                                atr,
                                out stopSource,
                                out stopQuality);
        
                        if (IsFinitePositive(rebuiltStop) &&
                            IsValidStop(
                                direction,
                                actualEntry,
                                rebuiltStop))
                        {
                            currentStop =
                                rebuiltStop;
        
                            _plan.StopSource =
                                stopSource;
        
                            _plan.StopQuality =
                                stopQuality;
                        }
                        else if (position.StopLoss.HasValue &&
                                 IsValidStop(
                                     direction,
                                     actualEntry,
                                     position.StopLoss.Value))
                        {
                            currentStop =
                                NormalizePrice(
                                    position.StopLoss.Value);
        
                            _plan.StopSource =
                                "BROKER FILL PROTECTION";
        
                            _plan.StopQuality =
                                70;
                        }
                    }
        
                    if (!IsValidStop(
                            direction,
                            actualEntry,
                            currentStop))
                        return;
        
                    _plan.Stop =
                        NormalizePrice(
                            currentStop);
        
                    _plan.Risk =
                        Math.Max(
                            Symbol.PipSize,
                            Math.Abs(
                                _plan.Entry -
                                _plan.Stop));
        
                    List<Level> levels =
                        BuildTargetLevels(
                            closedM5,
                            direction,
                            _plan.Entry,
                            atr);
        
                    List<Level> selected =
                        SelectTargets(
                            levels,
                            closedM5,
                            _plan.Entry,
                            _plan.Risk,
                            direction,
                            atr);
        
                    double oldTp1 = _plan.Tp1;
                    double oldTp2 = _plan.Tp2;
                    double oldTp3 = _plan.Tp3;
                    double oldTp4 = _plan.Tp4;
        
                    double tp1 =
                        SelectTarget(
                            selected,
                            0,
                            _plan.Entry,
                            _plan.Risk,
                            direction,
                            Math.Max(
                                FallbackTp1RR,
                                MinimumRequiredRR()));
        
                    double tp2 =
                        SelectTarget(
                            selected,
                            1,
                            _plan.Entry,
                            _plan.Risk,
                            direction,
                            Math.Max(
                                FallbackTp2RR,
                                Tp2MinimumRR));
        
                    double tp3 =
                        SelectTarget(
                            selected,
                            2,
                            _plan.Entry,
                            _plan.Risk,
                            direction,
                            Math.Max(
                                FallbackTp3RR,
                                Tp3MinimumRR));
        
                    double tp4 =
                        SelectTarget(
                            selected,
                            3,
                            _plan.Entry,
                            _plan.Risk,
                            direction,
                            Math.Max(
                                FallbackTp4RR,
                                Tp4MinimumRR));
        
                    _plan.Tp1 =
                        IsValidTarget(
                            direction,
                            _plan.Entry,
                            tp1)
                            ? NormalizePrice(tp1)
                            : oldTp1;
        
                    _plan.Tp2 =
                        IsValidTarget(
                            direction,
                            _plan.Entry,
                            tp2)
                            ? NormalizePrice(tp2)
                            : oldTp2;
        
                    _plan.Tp3 =
                        IsValidTarget(
                            direction,
                            _plan.Entry,
                            tp3)
                            ? NormalizePrice(tp3)
                            : oldTp3;
        
                    _plan.Tp4 =
                        IsValidTarget(
                            direction,
                            _plan.Entry,
                            tp4)
                            ? NormalizePrice(tp4)
                            : oldTp4;
        
                    ApplyTargetMeta(
                        levels,
                        _plan.Tp1,
                        atr,
                        out _plan.Tp1Source,
                        out _plan.Tp1Quality);
        
                    ApplyTargetMeta(
                        levels,
                        _plan.Tp2,
                        atr,
                        out _plan.Tp2Source,
                        out _plan.Tp2Quality);
        
                    ApplyTargetMeta(
                        levels,
                        _plan.Tp3,
                        atr,
                        out _plan.Tp3Source,
                        out _plan.Tp3Quality);
        
                    ApplyTargetMeta(
                        levels,
                        _plan.Tp4,
                        atr,
                        out _plan.Tp4Source,
                        out _plan.Tp4Quality);
        
                    _plan.HtfTargetCount =
                        CountHtfTargetsInPlan(
                            _plan);
        
                    _runtimeTpStageIndex =
                        -1;
        
                    _runtimeTpStagePlanCreatedM5 =
                        -1;
        
                    RecalculatePlanRR();
                }
        
                // ============================================================
        
                private void SetAutoTradingRuntimeState(bool enabled, string reason)
                {
                    _autoTradingEnabledRuntime = enabled;
                    _autoExecutionBlockReason =
                        string.IsNullOrWhiteSpace(reason)
                            ? (enabled ? "NOT EVALUATED" : "DISABLED")
                            : reason;
                    SyncQuickExecutionControls();
                }
        
                private void SetAutomaticOrdersRuntimeState(bool enabled, string reason)
                {
                    _automaticOrdersEnabledRuntime = enabled;
                    _autoOrdersBlockReason =
                        string.IsNullOrWhiteSpace(reason)
                            ? (enabled ? "NOT EVALUATED" : "DISABLED")
                            : reason;
                    SyncQuickExecutionControls();
                }
        
                private bool RequestLivePlanExit(
                    int closedM5,
                    string reason)
                {
                    if (_plan == null ||
                        !_plan.IsLivePosition)
                        return false;
        
                    Position position =
                        GetManagedLivePositionForPlan();
        
                    if (position == null)
                    {
                        _plan = null;
                        _activeBrokerStop = 0;
                        _activeBrokerTarget = 0;
                        _executionModel = null;
        
                        SetLifecycleState(
                            LifecycleState.Closed,
                            reason +
                            " • POSITION ALREADY CLOSED");
        
                        RemovePlanObjects();
                        return true;
                    }
        
                    SetLifecycleState(
                        LifecycleState.ExitRequested,
                        reason);
        
                    if (!TryClosePosition(
                            position,
                            reason))
                    {
                        SetLifecycleState(
                            LifecycleState.RecoveryRequired,
                            reason +
                            " • EXIT REJECTED");
        
                        _autoExecutionBlockReason =
                            reason +
                            " • EXIT REJECTED";
        
                        return false;
                    }
        
                    _lastExitM5 =
                        Math.Max(
                            _lastExitM5,
                            closedM5);
        
                    return true;
                }
        
                private void SetLifecycleState(
                    LifecycleState state,
                    string reason)
                {
                    _lifecycleState = state;
                    _lifecycleReason =
                        string.IsNullOrWhiteSpace(reason)
                            ? state.ToString().ToUpperInvariant()
                            : reason;
                }
        
                private Position GetManagedPositionById(long positionId)
                {
                    if (positionId <= 0)
                        return null;
        
                    foreach (Position position in Positions)
                    {
                        if (position != null &&
                            position.Id == positionId &&
                            IsManagedPosition(position))
                            return position;
                    }
        
                    return null;
                }
        
                private bool TryModifyStopLoss(
                    Position position,
                    double price,
                    string context)
                {
                    if (position == null ||
                        !IsFinitePositive(price))
                        return false;
        
                    double normalized =
                        NormalizePrice(price);
        
                    if (!IsFinitePositive(normalized))
                        return false;
        
                    try
                    {
                        TradeResult result =
                            position.ModifyStopLossPrice(normalized);
        
                        if (result == null ||
                            !result.IsSuccessful)
                        {
                            Print(
                                "CFIP SL mutation rejected ({0}).",
                                context);
                            return false;
                        }
        
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Print(
                            "CFIP SL mutation failed ({0}): {1}",
                            context,
                            ex.Message);
                        return false;
                    }
                }
        
                private bool TryModifyTakeProfit(
                    Position position,
                    double price,
                    string context)
                {
                    if (position == null ||
                        !IsFinitePositive(price))
                        return false;
        
                    double normalized =
                        NormalizePrice(price);
        
                    if (!IsFinitePositive(normalized))
                        return false;
        
                    try
                    {
                        TradeResult result =
                            position.ModifyTakeProfitPrice(normalized);
        
                        if (result == null ||
                            !result.IsSuccessful)
                        {
                            Print(
                                "CFIP TP mutation rejected ({0}).",
                                context);
                            return false;
                        }
        
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Print(
                            "CFIP TP mutation failed ({0}): {1}",
                            context,
                            ex.Message);
                        return false;
                    }
                }
        
                private bool TryClosePosition(
                    Position position,
                    string context,
                    double? volumeInUnits = null)
                {
                    if (position == null)
                        return false;
        
                    try
                    {
                        TradeResult result =
                            volumeInUnits.HasValue
                                ? ClosePosition(
                                    position,
                                    volumeInUnits.Value)
                                : ClosePosition(position);
        
                        if (result == null ||
                            !result.IsSuccessful)
                        {
                            Print(
                                "CFIP close rejected ({0}).",
                                context);
                            return false;
                        }
        
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Print(
                            "CFIP close failed ({0}): {1}",
                            context,
                            ex.Message);
                        return false;
                    }
                }
        
                private bool TryCancelPendingOrder(
                    PendingOrder order,
                    string context)
                {
                    if (order == null)
                        return false;
        
                    try
                    {
                        TradeResult result =
                            CancelPendingOrder(order);
        
                        if (result == null ||
                            !result.IsSuccessful)
                        {
                            Print(
                                "CFIP pending cancel rejected ({0}).",
                                context);
                            return false;
                        }
        
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Print(
                            "CFIP pending cancel failed ({0}): {1}",
                            context,
                            ex.Message);
                        return false;
                    }
                }
        
                private bool EnsureBrokerProtectionForPosition(
                    Position position,
                    double stop,
                    double target,
                    string context,
                    int direction)
                {
                    bool stopOk =
                        TryModifyStopLoss(
                            position,
                            stop,
                            context + " • SL");
        
                    bool targetOk =
                        TryModifyTakeProfit(
                            position,
                            target,
                            context + " • TP");
        
                    bool protectedOk =
                        stopOk &&
                        targetOk;
        
                    _brokerProtectionRecoveryRequired =
                        !protectedOk;
        
                    if (!protectedOk)
                    {
                        SetLifecycleState(
                            LifecycleState.RecoveryRequired,
                            context +
                            " • BROKER PROTECTION REJECTED");
        
                        SendUnifiedAlert(
                            "PROTECTION-REJECTED|" +
                            position.Id,
                            "CFIP BROKER PROTECTION REJECTED | #" +
                            position.Id,
                            direction,
                            true);
                    }
        
                    return protectedOk;
                }
        
                private bool IsExecutionPlanConsistent(
                    int direction,
                    double entry,
                    double stop,
                    double target)
                {
                    return direction != 0 &&
                           IsFinitePositive(entry) &&
                           IsFinitePositive(stop) &&
                           IsFinitePositive(target) &&
                           IsValidStop(direction, entry, stop) &&
                           IsValidTarget(direction, entry, target);
                }
        
                // Single structural source of truth for executable Entry/SL/TP.
                private bool RebuildSmartExecutionLevels(
                    int closedM5,
                    int direction,
                    double executionEntry,
                    double atr)
                {
                    if (_plan == null ||
                        _m5Bars == null ||
                        direction == 0 ||
                        !IsFinitePositive(executionEntry) ||
                        atr <= 0)
                        return false;
        
                    executionEntry = NormalizePrice(executionEntry);
        
                    string stopSource;
                    int stopQuality;
        
                    double stop = BuildStructuralStop(
                        closedM5,
                        direction,
                        executionEntry,
                        atr,
                        out stopSource,
                        out stopQuality);
        
                    if (!IsFinitePositive(stop) ||
                        !IsValidStop(direction, executionEntry, stop))
                        return false;
        
                    double risk = Math.Max(
                        Symbol.PipSize,
                        Math.Abs(executionEntry - stop));
        
                    List<Level> levels = BuildTargetLevels(
                        closedM5,
                        direction,
                        executionEntry,
                        atr);
        
                    List<Level> selected = SelectTargets(
                        levels,
                        closedM5,
                        executionEntry,
                        risk,
                        direction,
                        atr);
        
                    double tp1 = SelectTarget(
                        selected, 0, executionEntry, risk, direction,
                        Math.Max(FallbackTp1RR, MinimumRequiredRR()));
                    double tp2 = SelectTarget(
                        selected, 1, executionEntry, risk, direction,
                        Math.Max(FallbackTp2RR, Tp2MinimumRR));
                    double tp3 = SelectTarget(
                        selected, 2, executionEntry, risk, direction,
                        Math.Max(FallbackTp3RR, Tp3MinimumRR));
                    double tp4 = SelectTarget(
                        selected, 3, executionEntry, risk, direction,
                        Math.Max(FallbackTp4RR, Tp4MinimumRR));
        
                    if (!IsValidTarget(direction, executionEntry, tp1))
                        return false;
        
                    _plan.Entry = executionEntry;
                    _plan.Stop = NormalizePrice(stop);
                    _plan.Risk = risk;
                    _plan.StopSource = stopSource;
                    _plan.StopQuality = stopQuality;
        
                    _plan.Tp1 = IsValidTarget(direction, executionEntry, tp1)
                        ? NormalizePrice(tp1) : 0;
                    _plan.Tp2 = IsValidTarget(direction, executionEntry, tp2)
                        ? NormalizePrice(tp2) : 0;
                    _plan.Tp3 = IsValidTarget(direction, executionEntry, tp3)
                        ? NormalizePrice(tp3) : 0;
                    _plan.Tp4 = IsValidTarget(direction, executionEntry, tp4)
                        ? NormalizePrice(tp4) : 0;
        
                    ApplyTargetMeta(levels, _plan.Tp1, atr,
                        out _plan.Tp1Source, out _plan.Tp1Quality);
                    ApplyTargetMeta(levels, _plan.Tp2, atr,
                        out _plan.Tp2Source, out _plan.Tp2Quality);
                    ApplyTargetMeta(levels, _plan.Tp3, atr,
                        out _plan.Tp3Source, out _plan.Tp3Quality);
                    ApplyTargetMeta(levels, _plan.Tp4, atr,
                        out _plan.Tp4Source, out _plan.Tp4Quality);
        
                    _plan.HtfTargetCount = CountHtfTargetsInPlan(_plan);
                    _runtimeTpStageIndex = -1;
                    _runtimeTpStagePlanCreatedM5 = -1;
                    RecalculatePlanRR();
        
                    double effectiveTarget =
                        AutoTarget(_plan, EffectiveAutoTpStage());
        
                    return IsExecutionPlanConsistent(
                        direction, executionEntry, _plan.Stop, effectiveTarget);
                }
        
                private bool TryPrepareExecutablePlan(
                    int closedM5,
                    double executionEntry,
                    out double stopPips,
                    out double targetPips,
                    out double target)
                {
                    stopPips =
                        0;
        
                    targetPips =
                        0;
        
                    target =
                        0;
        
                    if (_plan == null || _m5Bars == null || closedM5 < 20)
                        return false;
        
                    double atr = Atr(_m5Bars, closedM5);
                    if (atr <= 0)
                        return false;
        
                    if (!RebuildSmartExecutionLevels(
                            closedM5, _plan.Direction, executionEntry, atr))
                        return false;
        
                    double stop =
                        _plan.Stop;
        
                    target =
                        AutoTarget(
                            _plan,
                            EffectiveAutoTpStage());
        
                    if (!IsExecutionPlanConsistent(
                            _plan.Direction,
                            executionEntry,
                            stop,
                            target))
                        return false;
        
                    target =
                        NormalizePrice(
                            target);
        
                    stopPips =
                        Math.Abs(
                            executionEntry -
                            stop) /
                        Symbol.PipSize;
        
                    targetPips =
                        Math.Abs(
                            target -
                            executionEntry) /
                        Symbol.PipSize;
        
                    return stopPips > 0 &&
                           targetPips > 0 &&
                           IsExecutionPlanConsistent(
                               _plan.Direction,
                               executionEntry,
                               stop,
                               target);
                }
        
                private void TryAutoTrade(
                    int closedM5)
                {
                    _lastAutoTradeAttemptUtc =
                        TimeInUtc;
        
                    if (!AutoTradingEnabled)
                    {
                        _lastAutoPlanAttemptM5 =
                            -1;
        
                        SetAutoTradingState(
                            "OFF",
                            "DISABLED");
                        return;
                    }
        
                    PendingOrder existingPending =
                        GetManagedPendingOrder();
        
                    if (existingPending != null)
                    {
                        _autoExecutionBlockReason =
                            "PENDING ORDER EXISTS";
                        SetAutoTradingState(
                            "ARMED",
                            "WAITING FOR PENDING ORDER");
                        return;
                    }
        
                    if (DailyLossLimitHit(
                            TimeInUtc))
                    {
                        _autoExecutionBlockReason =
                            "DAILY LOSS LIMIT";
                        SetAutoTradingState(
                            "BLOCKED",
                            "DAILY LOSS LIMIT REACHED");
                        return;
                    }
        
                    if (_plan == null)
                    {
                        EnsureSignalPlan(
                            closedM5,
                            ConfirmedSignalsOnly
                                ? DecisionPolicyMode.Confirmed
                                : DecisionPolicyMode.Soft);
        
                        if (_plan == null)
                        {
                            if (_executionModel != null &&
                                _executionModel.Mode ==
                                    ExecutionMode.WaitingForTrigger)
                            {
                                _autoExecutionBlockReason =
                                    "WAITING FOR TRIGGER";
        
                                SetAutoTradingState(
                                    "ARMED",
                                    "WAITING FOR TRIGGER");
                            }
                            else
                            {
                                _autoExecutionBlockReason =
                                    "NO ELIGIBLE PLAN";
        
                                SetAutoTradingState(
                                    "ARMED",
                                    ConfirmedSignalsOnly
                                        ? "WAITING FOR CONFIRMED PLAN"
                                        : "WAITING FOR SMART-ELIGIBLE PLAN");
                            }
        
                            return;
                        }
                    }
        
                    if (_decision == null ||
                        _decision.Direction == 0)
                    {
                        _autoExecutionBlockReason =
                            "NO DECISION";
                        SetAutoTradingState(
                            "ARMED",
                            "WAITING FOR DECISION");
                        return;
                    }
        
                    if (!_decision.EntryAllowed)
                    {
                        bool liveGate =
                            !ConfirmedSignalsOnly &&
                            IsLiveExecutionGateReason(
                                _decision.BlockReason);
        
                        if (!liveGate)
                        {
                            _autoExecutionBlockReason =
                                string.IsNullOrWhiteSpace(
                                    _decision.BlockReason)
                                    ? "WAITING FOR CONFIRMATION"
                                    : _decision.BlockReason;
        
                            SetAutoTradingState(
                                "ARMED",
                                string.IsNullOrWhiteSpace(
                                    _decision.BlockReason)
                                    ? "WAITING FOR CONFIRMATION"
                                    : _decision.BlockReason);
        
                            return;
                        }
                    }
        
                    if (OneOrderPerSignal &&
                        _lastAutoM5 == closedM5)
                    {
                        _autoExecutionBlockReason =
                            "ALREADY TRADED THIS M5";
                        SetAutoTradingState(
                            "ARMED",
                            "ALREADY TRADED THIS M5");
                        return;
                    }
        
                    if (_decision.Confidence <
                        MinimumAutoConfidence)
                    {
                        _autoExecutionBlockReason =
                            "CONF " +
                            _decision.Confidence +
                            " < " +
                            MinimumAutoConfidence;
                        SetAutoTradingState(
                            "BLOCKED",
                            "CONF " +
                            _decision.Confidence +
                            " < " +
                            MinimumAutoConfidence);
                        return;
                    }
        
                    if (_decision.SmartQuality <
                        MinimumAutoSmartQuality)
                    {
                        _autoExecutionBlockReason =
                            "SMART Q " +
                            _decision.SmartQuality +
                            " < " +
                            MinimumAutoSmartQuality;
                        SetAutoTradingState(
                            "BLOCKED",
                            "SMART Q " +
                            _decision.SmartQuality +
                            " < " +
                            MinimumAutoSmartQuality);
                        return;
                    }
        
                    string suitabilityReason;
        
                    if (!PassesMarketSuitability(
                            closedM5,
                            _plan.Direction,
                            out suitabilityReason))
                    {
                        _autoExecutionBlockReason =
                            "SUITABILITY • " +
                            suitabilityReason;
                        SetAutoTradingState(
                            "BLOCKED",
                            suitabilityReason);
                        return;
                    }
        
                    int levelQuality =
                        Math.Min(
                            _plan.StopQuality,
                            _plan.Tp1Quality);
        
                    if (levelQuality <
                        MinimumAutoLevelQuality)
                    {
                        _autoExecutionBlockReason =
                            "LEVEL Q " +
                            levelQuality +
                            " < " +
                            MinimumAutoLevelQuality;
                        SetAutoTradingState(
                            "BLOCKED",
                            "LEVEL Q " +
                            levelQuality +
                            " < " +
                            MinimumAutoLevelQuality);
                        return;
                    }
        
                    if (ManagedPositionCount() >=
                        Math.Max(
                            1,
                            MaximumOpenPositions))
                    {
                        _autoExecutionBlockReason =
                            "MAX OPEN POSITIONS";
                        SetAutoTradingState(
                            "BLOCKED",
                            "MAX OPEN POSITIONS");
                        return;
                    }
        
                    double plannedEntry = _plan.Entry;
        
                    double entry =
                        NormalizePrice(
                            _plan.Direction == 1
                                ? Symbol.Ask
                                : Symbol.Bid);
        
                    double atr =
                        Atr(
                            _m5Bars,
                            closedM5);
        
                    if (atr <= 0)
                    {
                        SetAutoTradingState(
                            "BLOCKED",
                            "ATR UNAVAILABLE");
                        return;
                    }
        
                    if (Math.Abs(
                            entry -
                            plannedEntry) >
                        atr *
                        MaximumEntryExtensionAtr)
                    {
                        SetAutoTradingState(
                            "BLOCKED",
                            "ENTRY EXTENSION");
                        return;
                    }
        
                    double stopPips;
                    double targetPips;
                    double target;
        
                    if (!TryPrepareExecutablePlan(
                            closedM5,
                            entry,
                            out stopPips,
                            out targetPips,
                            out target))
                    {
                        SetAutoTradingState(
                            "BLOCKED",
                            "INVALID SMART EXECUTION PLAN");
                        return;
                    }
        
                    string executableEntryReason;
        
                    if (!IsExecutableMarketEntry(
                            _plan,
                            entry,
                            out executableEntryReason))
                    {
                        _autoExecutionBlockReason =
                            executableEntryReason;
                        SetAutoTradingState(
                            "ARMED",
                            executableEntryReason);
                        return;
                    }
        
                    double effectiveStopPips =
                        EffectiveRiskStopPips(stopPips);
        
                    double volume =
                        CalculateVolume(
                            effectiveStopPips);
        
                    volume =
                        AdjustVolumeForMargin(
                            _plan.Direction == 1
                                ? TradeType.Buy
                                : TradeType.Sell,
                            volume);
        
                    if (volume <
                        Symbol.VolumeInUnitsMin)
                    {
                        SetAutoTradingState(
                            "BLOCKED",
                            "VOLUME BELOW MINIMUM");
                        return;
                    }
        
                    try
                    {
                        TradeType type =
                            _plan.Direction == 1
                                ? TradeType.Buy
                                : TradeType.Sell;
        
                        if (!EnsureTradingPermission())
                        {
                            _autoExecutionBlockReason =
                                "TRADING PERMISSION";
                            SetAutoTradingState(
                                "BLOCKED",
                                "TRADING PERMISSION NOT GRANTED");
                            return;
                        }
        
                        string guardReason;
        
                        if (!PassesAutoTradeSafetyGuards(
                                type,
                                volume,
                                out guardReason))
                        {
                            _autoExecutionBlockReason =
                                guardReason;
        
                            SetAutoTradingState(
                                "BLOCKED",
                                guardReason);
                            return;
                        }
        
                        ExecutionIntent marketIntent =
                            BuildExecutionIntent(
                                _plan.Direction,
                                ConfirmedSignalsOnly
                                    ? DecisionPolicyMode.Confirmed
                                    : DecisionPolicyMode.Soft,
                                ExecutionIntentKind.Market,
                                entry,
                                _plan.EntryTrigger,
                                _plan.EntryZoneLow,
                                _plan.EntryZoneHigh,
                                _plan.Stop,
                                target,
                                volume,
                                closedM5,
                                "NORMAL MARKET");
        
                        string intentReason;
        
                        if (!ValidateExecutionIntent(
                                marketIntent,
                                entry,
                                out intentReason))
                        {
                            _autoExecutionBlockReason =
                                intentReason;
                            SetAutoTradingState(
                                "BLOCKED",
                                intentReason);
                            return;
                        }
        
                        TradeResult result =
                            ExecuteMarketOrder(
                                type,
                                SymbolName,
                                volume,
                                NormalizeLabel(),
                                stopPips,
                                targetPips,
                                "CFIP SMART73",
                                false);
        
                        if (result == null)
                        {
                            SetAutoTradingState(
                                "ERROR",
                                "NULL TRADE RESULT");
                            return;
                        }
        
                        if (!result.IsSuccessful ||
                            result.Position == null)
                        {
                            SetAutoTradingState(
                                "ERROR",
                                result.Error.HasValue
                                    ? result.Error.Value.ToString()
                                    : "TRADE REJECTED");
                            return;
                        }
        
                        _lastAutoM5 =
                            closedM5;
        
                        _autoExecutionBlockReason =
                            "EXECUTED";
        
                        _plan.OriginalVolume =
                            result.Position.VolumeInUnits;
        
                        _plan.IsLivePosition = true;
                        _plan.PositionId = result.Position.Id;
        
                        SetLifecycleState(
                            LifecycleState.LivePosition,
                            "MARKET ENTRY • FILLED");
        
                        ReconcileLivePlanToActualFill(
                            result.Position,
                            closedM5);
        
                        string fillExecutionReason;
        
                        if (!IsExecutableFillPrice(
                                _plan,
                                result.Position.EntryPrice,
                                out fillExecutionReason))
                        {
                            SetLifecycleState(
                                LifecycleState.ExitRequested,
                                "MARKET FILL MISMATCH");
        
                            _autoExecutionBlockReason =
                                "FILL MISMATCH • " +
                                fillExecutionReason;
        
                            SetAutoTradingState(
                                "ERROR",
                                fillExecutionReason);
        
                            bool closed =
                                TryClosePosition(
                                    result.Position,
                                    "MARKET FILL MISMATCH");
        
                            if (!closed)
                            {
                                SetLifecycleState(
                                    LifecycleState.RecoveryRequired,
                                    "MARKET FILL MISMATCH • CLOSE REJECTED");
                            }
        
                            SendUnifiedAlert(
                                "FILL-MISMATCH|" +
                                result.Position.Id,
                                "CFIP ACCEPTED BROKER FILL OUTSIDE EXECUTION ENVELOPE | #" +
                                result.Position.Id +
                                " | " +
                                fillExecutionReason,
                                _plan.Direction,
                                true);
        
                            return;
                        }
        
                        double structuralTarget =
                            AutoTarget(
                                _plan,
                                EffectiveAutoTpStage());
        
                        if (IsValidTarget(
                                _plan.Direction,
                                result.Position.EntryPrice,
                                structuralTarget))
                        {
                            target =
                                NormalizePrice(
                                    structuralTarget);
                        }
                        else if (result.Position.TakeProfit.HasValue &&
                                 IsValidTarget(
                                     _plan.Direction,
                                     result.Position.EntryPrice,
                                     result.Position.TakeProfit.Value))
                        {
                            target =
                                NormalizePrice(
                                    result.Position.TakeProfit.Value);
                        }
                        else if (IsValidTarget(
                                     _plan.Direction,
                                     result.Position.EntryPrice,
                                     _plan.Tp1))
                        {
                            target =
                                NormalizePrice(
                                    _plan.Tp1);
                        }
                        else
                        {
                            _autoExecutionBlockReason =
                                "POST-FILL TARGET INVALID";
        
                            SetAutoTradingState(
                                "ERROR",
                                "POSITION OPENED • NO VALID TARGET");
        
                            if (!RequestLivePlanExit(
                                    closedM5,
                                    "POST-FILL TARGET INVALID"))
                            {
                                SetLifecycleState(
                                    LifecycleState.RecoveryRequired,
                                    "POST-FILL TARGET INVALID • CLOSE REJECTED");
                            }
        
                            return;
                        }
        
                        if (AutoBrokerProtection)
                        {
                            EnsureBrokerProtectionForPosition(
                                result.Position,
                                _plan.Stop,
                                target,
                                "NEW MARKET ENTRY",
                                _plan.Direction);
                        }
        
                        SetAutoTradingState(
                            "EXECUTED",
                            "POSITION #" +
                            result.Position.Id);
        
                        SendUnifiedAlert(
                            "AUTO|" +
                            closedM5,
                            "CFIP AUTO " +
                            (_plan.Direction == 1
                                ? "BUY"
                                : "SELL") +
                            " EXECUTED | #" +
                            result.Position.Id +
                            " | ENTRY " +
                            Price(
                                result.Position.EntryPrice) +
                            " | SL " +
                            Price(
                                _plan.Stop) +
                            " | TP " +
                            Price(
                                target),
                            _plan.Direction,
                            true);
                    }
                    catch (Exception ex)
                    {
                        SetAutoTradingState(
                            "ERROR",
                            ex.Message);
        
                        Print(
                            "CFIP auto trade failed: {0}",
                            ex.Message);
                    }
                }
        
                private double AdjustVolumeForMargin(
                    TradeType tradeType,
                    double volume)
                {
                    if (!IsFinitePositive(volume) ||
                        !UseAutoMarginGuard)
                        return volume;
        
                    try
                    {
                        double freeMargin =
                            Math.Max(
                                0,
                                Account.FreeMargin);
        
                        double usage =
                            Math.Max(
                                10,
                                Math.Min(
                                    100,
                                    MaxAutoMarginUsagePercent -
                                    Math.Max(
                                        0,
                                        Math.Min(
                                            40,
                                            MarginBufferPercent))));
        
                        double allowed =
                            freeMargin *
                            usage /
                            100.0;
        
                        if (allowed <= 0)
                            return 0;
        
                        double estimated =
                            Symbol.GetEstimatedMargin(
                                tradeType,
                                volume);
        
                        if (!IsFinitePositive(estimated) ||
                            estimated <= allowed)
                            return Symbol.NormalizeVolumeInUnits(
                                volume,
                                RoundingMode.Down);
        
                        double reduced =
                            Symbol.NormalizeVolumeInUnits(
                                volume *
                                allowed /
                                estimated,
                                RoundingMode.Down);
        
                        while (reduced >= Symbol.VolumeInUnitsMin)
                        {
                            double check =
                                Symbol.GetEstimatedMargin(
                                    tradeType,
                                    reduced);
        
                            if (!IsFinitePositive(check) ||
                                check <= allowed)
                                break;
        
                            reduced =
                                Symbol.NormalizeVolumeInUnits(
                                    reduced -
                                    Symbol.VolumeInUnitsStep,
                                    RoundingMode.Down);
                        }
        
                        return reduced >= Symbol.VolumeInUnitsMin
                            ? reduced
                            : 0;
                    }
                    catch (Exception ex)
                    {
                        Print(
                            "CFIP margin sizing failed: {0}",
                            ex.Message);
                        return 0;
                    }
                }
        
                private double CalculateVolume(
                    double stopPips)
                {
                    try
                    {
                        double volume;
        
                        if (SizingMode ==
                            SizingMode.FixedLots)
                        {
                            volume =
                                Symbol.QuantityToVolumeInUnits(
                                    Math.Max(
                                        0.001,
                                        FixedLots));
                        }
                        else
                        {
                            double riskAmount =
                                Math.Max(
                                    0,
                                    Account.Equity) *
                                EffectiveAutoRiskPercent() /
                                100.0;
        
                            if (riskAmount <= 0)
                                return 0;
        
                            volume =
                                Symbol.VolumeForFixedRisk(
                                    riskAmount,
                                    stopPips,
                                    RoundingMode.Down);
                        }
        
                        if (!IsFinitePositive(
                                volume))
                            return 0;
        
                        volume =
                            Symbol.NormalizeVolumeInUnits(
                                volume,
                                RoundingMode.Down);
        
                        if (volume <
                            Symbol.VolumeInUnitsMin)
                            return 0;
        
                        if (volume >
                            Symbol.VolumeInUnitsMax)
                        {
                            volume =
                                Symbol.NormalizeVolumeInUnits(
                                    Symbol.VolumeInUnitsMax,
                                    RoundingMode.Down);
                        }
        
                        return volume;
                    }
                    catch (Exception ex)
                    {
                        Print(
                            "CFIP volume calculation failed: {0}",
                            ex.Message);
        
                        return 0;
                    }
                }
        
                private int ManagedPositionCount()
                {
                    int count = 0;
        
                    foreach (Position position in Positions)
                    {
                        if (IsManagedPosition(position))
                            count++;
                    }
        
                    return count;
                }
        
                private bool IsAutoPlanValid(
                    int direction,
                    double entry,
                    double stop,
                    double target)
                {
                    return
                        (direction == 1 ||
                         direction == -1) &&
                        IsFinitePositive(entry) &&
                        IsValidStop(
                            direction,
                            entry,
                            stop) &&
                        IsValidTarget(
                            direction,
                            entry,
                            target);
                }
        
                private double AutoTarget(
                    Plan plan,
                    TargetStage stage)
                {
                    if (stage ==
                            TargetStage.TP4 &&
                        plan.Tp4 > 0)
                        return plan.Tp4;
        
                    if (stage ==
                            TargetStage.TP3 &&
                        plan.Tp3 > 0)
                        return plan.Tp3;
        
                    if (stage ==
                            TargetStage.TP2 &&
                        plan.Tp2 > 0)
                        return plan.Tp2;
        
                    return plan.Tp1;
                }
        
                private int _runtimeTpStageIndex = -1;
        
                private int _runtimeTpStagePlanCreatedM5 = -1;
        
                private TargetStage EffectiveAutoTpStage()
                {
                    if (!EnableDynamicTpAdvance ||
                        _plan == null)
                        return AutoTpStage;
        
                    int baseStage =
                        (int)AutoTpStage;
        
                    // A new plan resets the ratchet back to the configured base
                    // stage — this is a per-trade advance, not a permanent state.
                    if (_runtimeTpStagePlanCreatedM5 !=
                        _plan.CreatedM5)
                    {
                        _runtimeTpStagePlanCreatedM5 =
                            _plan.CreatedM5;
        
                        _runtimeTpStageIndex =
                            baseStage;
                    }
        
                    if (_runtimeTpStageIndex <
                        baseStage)
                        _runtimeTpStageIndex =
                            baseStage;
        
                    double risk =
                        Math.Max(
                            Symbol.PipSize,
                            _plan.Risk);
        
                    while (_runtimeTpStageIndex < 3)
                    {
                        double currentTarget =
                            AutoTarget(
                                _plan,
                                (TargetStage)
                                _runtimeTpStageIndex);
        
                        double nextTarget =
                            AutoTarget(
                                _plan,
                                (TargetStage)
                                (_runtimeTpStageIndex +
                                 1));
        
                        // AutoTarget() falls back to Tp1 for a stage with no valid
                        // price, so confirm the "next" stage is a genuinely farther
                        // level before treating it as something to advance to.
                        bool nextIsFarther =
                            _plan.Direction == 1
                                ? nextTarget >
                                  currentTarget
                                : nextTarget <
                                  currentTarget;
        
                        if (!nextIsFarther)
                            break;
        
                        double distanceToCurrent =
                            Math.Abs(
                                currentTarget -
                                _plan.Entry);
        
                        if (distanceToCurrent <=
                            risk * 0.1)
                            break;
        
                        double covered =
                            _plan.Direction == 1
                                ? _lastMarket -
                                  _plan.Entry
                                : _plan.Entry -
                                  _lastMarket;
        
                        double progressPercent =
                            covered /
                            distanceToCurrent *
                            100.0;
        
                        if (progressPercent <
                            Math.Max(
                                50,
                                TpAdvanceProximityPercent))
                            break;
        
                        _runtimeTpStageIndex++;
                    }
        
                    return
                        (TargetStage)
                        _runtimeTpStageIndex;
                }
        
                private void SynchronizeLiveBrokerState()
                {
                    _activeBrokerStop = 0;
                    _activeBrokerTarget = 0;
        
                    if (_plan == null ||
                        !_plan.IsLivePosition)
                        return;
        
                    Position position =
                        GetManagedLivePositionForPlan();
        
                    if (position == null)
                        return;
        
                    int direction =
                        position.TradeType == TradeType.Buy
                            ? 1
                            : -1;
        
                    if (IsFinitePositive(position.EntryPrice))
                        _plan.Entry =
                            NormalizePrice(position.EntryPrice);
        
                    if (position.StopLoss.HasValue &&
                        IsFinitePositive(position.StopLoss.Value) &&
                        IsValidStop(
                            direction,
                            position.EntryPrice,
                            position.StopLoss.Value))
                    {
                        _activeBrokerStop =
                            NormalizePrice(position.StopLoss.Value);
                    }
        
                    if (position.TakeProfit.HasValue &&
                        IsFinitePositive(position.TakeProfit.Value))
                    {
                        _activeBrokerTarget =
                            NormalizePrice(position.TakeProfit.Value);
                    }
                }
        
                private double GetActiveBrokerStopPrice()
                {
                    if (_plan != null &&
                        _plan.IsLivePosition &&
                        IsFinitePositive(_activeBrokerStop))
                        return _activeBrokerStop;
        
                    return _plan == null
                        ? 0
                        : _plan.Stop;
                }
        
                private double GetActiveBrokerTargetPrice()
                {
                    return
                        _plan != null &&
                        _plan.IsLivePosition &&
                        IsFinitePositive(_activeBrokerTarget)
                            ? _activeBrokerTarget
                            : 0;
                }
        
                private string BrokerTargetStageText(double target)
                {
                    if (!IsFinitePositive(target) ||
                        _plan == null)
                        return "CUSTOM / NONE";
        
                    if (SamePrice(target, _plan.Tp1))
                        return "TP1";
                    if (SamePrice(target, _plan.Tp2))
                        return "TP2";
                    if (SamePrice(target, _plan.Tp3))
                        return "TP3";
                    if (SamePrice(target, _plan.Tp4))
                        return "TP4";
        
                    return "CUSTOM";
                }
        
                private string NormalizeLabel()
                {
                    return
                        string.IsNullOrWhiteSpace(
                            AutoTradeLabel)
                            ? "CFIP-SMART-CLEAN66"
                            : AutoTradeLabel.Trim();
                }
        
                        private void CloseAllPositions()
                {
                    bool allClosedOrAbsent = true;
        
                    foreach (Position position in Positions)
                    {
                        if (!IsManagedPosition(position))
                            continue;
        
                        if (!TryClosePosition(
                                position,
                                "END OF DAY"))
                            allClosedOrAbsent = false;
                    }
        
                    if (GetManagedPosition() == null)
                    {
                        _plan = null;
                        _executionModel = null;
                        _activeBrokerStop = 0;
                        _activeBrokerTarget = 0;
        
                        SetLifecycleState(
                            LifecycleState.Closed,
                            "END OF DAY • CLOSED");
        
                        RemovePlanObjects();
                    }
                    else if (!allClosedOrAbsent)
                    {
                        SetLifecycleState(
                            LifecycleState.RecoveryRequired,
                            "END OF DAY • CLOSE REJECTED");
                    }
                    else
                    {
                        SetLifecycleState(
                            LifecycleState.ExitRequested,
                            "END OF DAY • EXIT REQUESTED");
                    }
                }
        
                        private void CancelAllOrders()
                {
                    bool allCancelledOrAbsent = true;
        
                    foreach (PendingOrder order in PendingOrders)
                    {
                        if (!IsManagedPendingOrder(order))
                            continue;
        
                        if (!TryCancelPendingOrder(
                                order,
                                "PENDING CIRCUIT BREAKER"))
                            allCancelledOrAbsent = false;
                    }
        
                    if (GetManagedPendingOrder() == null)
                    {
                        RemoveManagedPendingOrderObjects();
                    }
                    else if (!allCancelledOrAbsent)
                    {
                        SetLifecycleState(
                            LifecycleState.RecoveryRequired,
                            "PENDING CANCEL REJECTED");
                    }
                }
        
                private int FreshTriggerEvidence(
                    Bars bars,
                    int index,
                    int direction)
                {
                    if (bars == null || index < 5)
                        return 0;
        
                    int evidence = 0;
        
                    double atr =
                        Atr(
                            bars,
                            index);
        
                    double body =
                        Math.Abs(
                            bars.ClosePrices[index] -
                            bars.OpenPrices[index]);
        
                    if (direction == 1 &&
                        bars.ClosePrices[index] >
                        bars.OpenPrices[index])
                        evidence++;
        
                    if (direction == -1 &&
                        bars.ClosePrices[index] <
                        bars.OpenPrices[index])
                        evidence++;
        
                    if (atr > 0 &&
                        body >=
                        atr *
                        MinimumTriggerBodyAtr)
                        evidence++;
        
                    if (direction == 1 &&
                        bars.ClosePrices[index] >
                        Highest(
                            bars,
                            Math.Max(
                                0,
                                index - 5),
                            index - 1))
                        evidence++;
        
                    if (direction == -1 &&
                        bars.ClosePrices[index] <
                        Lowest(
                            bars,
                            Math.Max(
                                0,
                                index - 5),
                            index - 1))
                        evidence++;
        
                    return evidence;
                }
        
                private int StructuralSequence(
                    Bars bars,
                    int index,
                    int direction)
                {
                    if (bars == null ||
                        index < 8)
                        return 0;
        
                    double atr =
                        Atr(
                            bars,
                            index);
        
                    int result = 0;
        
                    if (direction == 1)
                    {
                        if (BullStructure(
                                bars,
                                index,
                                atr))
                            result++;
        
                        if (BullMss(
                                bars,
                                index,
                                atr))
                            result++;
        
                        if (BullDisplacement(
                                bars,
                                index,
                                atr))
                            result++;
                    }
                    else
                    {
                        if (BearStructure(
                                bars,
                                index,
                                atr))
                            result++;
        
                        if (BearMss(
                                bars,
                                index,
                                atr))
                            result++;
        
                        if (BearDisplacement(
                                bars,
                                index,
                                atr))
                            result++;
                    }
        
                    return result;
                }
        
                private int EntryLocationQuality(
                    Bars bars,
                    int index,
                    int direction)
                {
                    if (bars == null ||
                        index < 10)
                        return 0;
        
                    double atr =
                        Atr(
                            bars,
                            index);
        
                    if (atr <= 0)
                        return 0;
        
                    int quality = 40;
        
                    Zone zone =
                        FindNearestOpposingZone(
                            bars,
                            index,
                            direction,
                            atr);
        
                    if (zone != null)
                    {
                        double price =
                            bars.ClosePrices[index];
        
                        if (price >=
                                zone.Low -
                                atr *
                                ZoneProximityAtr &&
                            price <=
                                zone.High +
                                atr *
                                ZoneProximityAtr)
                            quality += 30;
        
                        if (zone.Quality >= 80)
                            quality += 15;
                    }
        
                    if (PremiumDiscountBias(
                            bars,
                            index) ==
                        direction)
                        quality += 10;
        
                    return ClampInt(
                        quality,
                        0,
                        100);
                }
        
                private double ProxyExpectedValue(
                    int quality,
                    double rr)
                {
                    double winRate =
                        Clamp(
                            quality / 100.0,
                            0.05,
                            0.95);
        
                    return
                        winRate * rr -
                        (1.0 - winRate);
                }
        
                private bool NoTradeRegimeBlocked(
                    string regime,
                    int quality)
                {
                    if (quality <
                        NoTradeMinimumSmartQuality)
                        return true;
        
                    if (BlockCompressionRegime &&
                        regime == "COMPRESSION")
                        return true;
        
                    if (BlockWeakRangeTransition &&
                        (regime == "RANGE" ||
                         regime == "TRANSITION") &&
                        quality <
                        SmartRegimeQualityFloor)
                        return true;
        
                    return false;
                }
        
                private int CalibratedConfidence(
                    int baseConfidence,
                    int direction)
                {
                    if (!UseEmpiricalCalibration ||
                        !EnableConfidenceCalibration ||
                        !EnableOutcomeTelemetry ||
                        !_directionSamples.ContainsKey(
                            direction))
                        return baseConfidence;
        
                    int totalSamples =
                        _directionSamples.Values.Sum();
        
                    if (totalSamples <
                        Math.Max(
                            1,
                            CalibrationMinimumSamples))
                        return baseConfidence;
        
                    int samples =
                        _directionSamples[direction];
        
                    if (samples <
                        CalibrationDirectionalMinimumSamples)
                        return baseConfidence;
        
                    int wins =
                        _directionWins.ContainsKey(
                            direction)
                            ? _directionWins[direction]
                            : 0;
        
                    double rate =
                        samples <= 0
                            ? 0.5
                            : (double)wins /
                              samples;
        
                    int adjustment =
                        ClampInt(
                            (int)Math.Round(
                                (rate - 0.5) *
                                2.0 *
                                CalibrationMaxConfidenceAdjustment),
                            -CalibrationMaxConfidenceAdjustment,
                            CalibrationMaxConfidenceAdjustment);
        
                    return ClampInt(
                        baseConfidence +
                        adjustment,
                        0,
                        100);
                }
        
                private void RegisterOutcome(
                    int direction,
                    bool win)
                {
                    if (!EnableOutcomeTelemetry)
                        return;
        
                    if (!_directionSamples.ContainsKey(
                            direction))
                        _directionSamples[direction] = 0;
        
                    if (!_directionWins.ContainsKey(
                            direction))
                        _directionWins[direction] = 0;
        
                    _directionSamples[direction]++;
        
                    if (win)
                        _directionWins[direction]++;
                }
        
                private string CalibrationText()
                {
                    int total =
                        _wins +
                        _losses;
        
                    return
                        total <= 0
                            ? "W0/L0"
                            : (100.0 *
                               _wins /
                               total)
                              .ToString("F0") +
                              "%";
                }
        
                private Prediction BuildEarlyPrediction(
                    int closedM5)
                {
                    Prediction p =
                        new Prediction();
        
                    if (!EnableEarlyPrediction ||
                        _m5Frame == null ||
                        _m15Frame == null ||
                        closedM5 < 10)
                        return p;
        
                    double buy =
                        _m5Frame.BullScore * 0.55 +
                        _m15Frame.BullScore * 0.45;
        
                    double sell =
                        _m5Frame.BearScore * 0.55 +
                        _m15Frame.BearScore * 0.45;
        
                    if (UseLiquidityForecast)
                    {
                        if (_m5Frame.LiquidityBull)
                            buy += 8;
        
                        if (_m5Frame.LiquidityBear)
                            sell += 8;
                    }
        
                    if (_m5Frame.VolumeBull)
                        buy += 2;
        
                    if (_m5Frame.VolumeBear)
                        sell += 2;
        
                    if (_m5Frame.VwapBull)
                        buy += 1;
        
                    if (_m5Frame.VwapBear)
                        sell += 1;
        
                    p.Direction =
                        buy >= sell
                            ? 1
                            : -1;
        
                    double total =
                        Math.Max(
                            1,
                            buy + sell);
        
                    p.Confidence =
                        ClampInt(
                            (int)Math.Round(
                                100.0 *
                                Math.Max(
                                    buy,
                                    sell) /
                                total),
                            0,
                            100);
        
                    if (p.Confidence <
                        Math.Max(
                            MinimumEarlyConfidence,
                            EarlySetupConfidence))
                        return p;
        
                    double atr =
                        Atr(
                            _m5Bars,
                            closedM5);
        
                    if (atr <= 0)
                        return p;
        
                    ExecutionModel predictionExecution =
                        BuildExecutionModel(
                            closedM5,
                            p.Direction);
        
                    p.Mode =
                        predictionExecution == null
                            ? ExecutionMode.None
                            : predictionExecution.Mode;
        
                    if (predictionExecution != null &&
                        predictionExecution.ZoneHigh >
                        predictionExecution.ZoneLow)
                    {
                        p.ZoneLow =
                            NormalizePrice(
                                predictionExecution.ZoneLow);
        
                        p.ZoneHigh =
                            NormalizePrice(
                                predictionExecution.ZoneHigh);
        
                        p.Trigger =
                            NormalizePrice(
                                predictionExecution.Trigger);
        
                        p.Entry =
                            predictionExecution.Ready &&
                            IsFinitePositive(
                                predictionExecution.ActualEntry)
                                ? NormalizePrice(
                                    predictionExecution.ActualEntry)
                                : p.Mode ==
                                  ExecutionMode.WaitingForTrigger
                                    ? NormalizePrice(
                                        predictionExecution.Trigger)
                                    : NormalizePrice(
                                        _m5Bars.ClosePrices[
                                            closedM5]);
                    }
                    else
                    {
                        p.Entry =
                            NormalizePrice(
                                _m5Bars.ClosePrices[
                                    closedM5]);
        
                        p.ZoneLow =
                            p.Entry -
                            atr * 0.30;
        
                        p.ZoneHigh =
                            p.Entry +
                            atr * 0.30;
        
                        p.Trigger =
                            p.Direction == 1
                                ? p.ZoneHigh +
                                  atr * EntryBufferAtr
                                : p.ZoneLow -
                                  atr * EntryBufferAtr;
                    }
        
                    string stopSource;
                    int stopQuality;
        
                    p.StopLoss =
                        BuildStructuralStop(
                            closedM5,
                            p.Direction,
                            p.Entry,
                            atr,
                            out stopSource,
                            out stopQuality);
        
                    if (!IsFinitePositive(
                            p.StopLoss) &&
                        AllowExecutionFrameStopFallback)
                    {
                        p.StopLoss =
                            p.Direction == 1
                                ? p.Entry -
                                  atr * FallbackSlAtr
                                : p.Entry +
                                  atr * FallbackSlAtr;
                    }
        
                    double risk =
                        Math.Abs(
                            p.Entry -
                            p.StopLoss);
        
                    if (risk <= 0)
                        return p;
        
                    List<Level> candidates =
                        BuildTargetLevels(
                            closedM5,
                            p.Direction,
                            p.Entry,
                            atr);
        
                    List<Level> selected =
                        SelectTargets(
                            candidates,
                            closedM5,
                            p.Entry,
                            risk,
                            p.Direction,
                            atr);
        
                    p.Target1 =
                        SelectTarget(
                            selected,
                            0,
                            p.Entry,
                            risk,
                            p.Direction,
                            Tp1MinimumRR);
        
                    p.Target2 =
                        SelectTarget(
                            selected,
                            1,
                            p.Entry,
                            risk,
                            p.Direction,
                            Tp2MinimumRR);
        
                    p.Target3 =
                        SelectTarget(
                            selected,
                            2,
                            p.Entry,
                            risk,
                            p.Direction,
                            Tp3MinimumRR);
        
                    p.Target4 =
                        SelectTarget(
                            selected,
                            3,
                            p.Entry,
                            risk,
                            p.Direction,
                            Tp4MinimumRR);
        
                    p.Target =
                        p.Target1;
        
                    p.Reason =
                        (p.Direction == 1
                            ? "BUY"
                            : "SELL") +
                        " EARLY | CONF " +
                        p.Confidence;
        
                    return p;
                }
        
                private void RenderPredictionObjects(
                    Prediction prediction,
                    int closedM5)
                {
                    RemovePredictionObjects();
        
                    if (!ShowPredictionObjects ||
                        !EnableEarlyPrediction ||
                        prediction == null ||
                        prediction.Direction == 0 ||
                        prediction.Confidence <
                        Math.Max(
                            MinimumEarlyConfidence,
                            EarlySetupConfidence))
                        return;
        
                    int start =
                        MapM5ToChart(
                            Math.Max(
                                0,
                                closedM5 -
                                Math.Max(
                                    2,
                                    PredictionLookaheadBars / 2)),
                            Bars.Count - 1);
        
                    int end =
                        MapM5ToChart(
                            Math.Min(
                                _m5Bars.Count - 1,
                                closedM5 +
                                Math.Max(
                                    2,
                                    PredictionLookaheadBars)),
                            Bars.Count - 1);
        
                    if (end <= start)
                        end =
                            Math.Min(
                                Bars.Count - 1,
                                start + 4);
        
                    if (ShowPredictionZone &&
                        prediction.ZoneHigh >
                        prediction.ZoneLow)
                    {
                        ChartRectangle zone =
                            Chart.DrawRectangle(
                                P + "PRED_ZONE",
                                start,
                                prediction.ZoneHigh,
                                end,
                                prediction.ZoneLow,
                                PredictionColor,
                                1,
                                LineStyle.Solid);
        
                        zone.IsFilled = true;
        
                        zone.Color =
                            Color.FromArgb(
                                35,
                                PredictionColor);
        
                        zone.IsInteractive = false;
                    }
        
                    if (prediction.Entry > 0)
                        DrawPredictionLine(
                            P + "PRED_ENTRY",
                            prediction.Entry);
        
                    if (prediction.StopLoss > 0)
                        DrawPredictionLine(
                            P + "PRED_STOP",
                            prediction.StopLoss);
        
                    if (prediction.Trigger > 0)
                        DrawPredictionLine(
                            P + "PRED_TRIGGER",
                            prediction.Trigger);
        
                    if (ShowPredictionTargets)
                    {
                        if (prediction.Target1 > 0)
                            DrawPredictionLine(
                                P + "PRED_TARGET1",
                                prediction.Target1);
        
                        if (prediction.Target2 > 0)
                            DrawPredictionLine(
                                P + "PRED_TARGET2",
                                prediction.Target2);
        
                        if (prediction.Target3 > 0)
                            DrawPredictionLine(
                                P + "PRED_TARGET3",
                                prediction.Target3);
        
                        if (prediction.Target4 > 0)
                            DrawPredictionLine(
                                P + "PRED_TARGET4",
                                prediction.Target4);
                    }
        
                    RenderPredictionLabels(
                        prediction,
                        closedM5);
                }
        
                private void RenderPredictionLabels(
                    Prediction prediction,
                    int closedM5)
                {
                    if (prediction == null ||
                        Bars == null ||
                        Bars.Count < 2)
                        return;
        
                    int bar =
                        MapM5ToChart(
                            closedM5,
                            Bars.Count - 1);
        
                    bar =
                        Math.Max(
                            0,
                            Math.Min(
                                Bars.Count - 1,
                                bar));
        
                    DrawPlanLabel(
                        P + "PRED_ENTRY_LABEL",
                        "ENTRY " +
                        Price(prediction.Entry),
                        bar,
                        prediction.Entry,
                        EntryLineColor);
        
                    if (prediction.StopLoss > 0)
                        DrawPlanLabel(
                            P + "PRED_STOP_LABEL",
                            "SL " +
                            Price(prediction.StopLoss),
                            bar,
                            prediction.StopLoss,
                            SlLineColor);
        
                    if (prediction.Trigger > 0 &&
                        !SamePrice(
                            prediction.Trigger,
                            prediction.Entry))
                        DrawPlanLabel(
                            P + "PRED_TRIGGER_LABEL",
                            "TRIGGER " +
                            Price(prediction.Trigger),
                            bar,
                            prediction.Trigger,
                            TriggerLineColor);
        
                    if (ShowPredictionTargets)
                    {
                        if (prediction.Target1 > 0 &&
                            !SamePrice(
                                prediction.Target1,
                                prediction.Entry))
                            DrawPlanLabel(
                                P + "PRED_TARGET1_LABEL",
                                "TP1 " +
                                Price(prediction.Target1),
                                bar,
                                prediction.Target1,
                                TpLineColor);
        
                        if (prediction.Target2 > 0 &&
                            !SamePrice(
                                prediction.Target2,
                                prediction.Target1))
                            DrawPlanLabel(
                                P + "PRED_TARGET2_LABEL",
                                "TP2 " +
                                Price(prediction.Target2),
                                bar,
                                prediction.Target2,
                                Tp2LineColor);
        
                        if (prediction.Target3 > 0 &&
                            !SamePrice(
                                prediction.Target3,
                                prediction.Target2))
                            DrawPlanLabel(
                                P + "PRED_TARGET3_LABEL",
                                "TP3 " +
                                Price(prediction.Target3),
                                bar,
                                prediction.Target3,
                                Tp3LineColor);
        
                        if (prediction.Target4 > 0 &&
                            !SamePrice(
                                prediction.Target4,
                                prediction.Target3))
                            DrawPlanLabel(
                                P + "PRED_TARGET4_LABEL",
                                "TP4 " +
                                Price(prediction.Target4),
                                bar,
                                prediction.Target4,
                                Tp4LineColor);
                    }
                }
        
                private Color PredictionLineColor(
                    string name)
                {
                    if (name.IndexOf(
                            "PRED_TRIGGER",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        return TriggerLineColor;
        
                    if (name.IndexOf(
                            "PRED_STOP",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        return SlLineColor;
        
                    if (name.IndexOf(
                            "PRED_TARGET1",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        return TpLineColor;
        
                    if (name.IndexOf(
                            "PRED_TARGET2",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        return Tp2LineColor;
        
                    if (name.IndexOf(
                            "PRED_TARGET3",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        return Tp3LineColor;
        
                    if (name.IndexOf(
                            "PRED_TARGET4",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        return Tp4LineColor;
        
                    if (name.IndexOf(
                            "PRED_ENTRY",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        return EntryLineColor;
        
                    return PredictionColor;
                }
        
                private void DrawPredictionLine(
                    string name,
                    double price)
                {
                    if (!IsFinitePositive(price) ||
                        Bars == null ||
                        Bars.Count < 2)
                    {
                        Chart.RemoveObject(name);
                        return;
                    }
        
                    try
                    {
                        int anchor =
                            MapM5ToChart(
                                _m5Bars == null
                                    ? Bars.Count - 1
                                    : Math.Max(
                                        1,
                                        _m5Bars.Count - 1),
                                Bars.Count - 1);
        
                        anchor =
                            Math.Max(
                                0,
                                Math.Min(
                                    Bars.Count - 1,
                                    anchor));
        
                        int left =
                            Math.Max(
                                0,
                                anchor -
                                Math.Max(
                                    1,
                                    LineLengthBars));
        
                        int right =
                            Math.Min(
                                Bars.Count - 1,
                                anchor +
                                Math.Max(
                                    1,
                                    Math.Max(
                                        LineForwardBars,
                                        PredictionLookaheadBars)));
        
                        if (right <= left)
                        {
                            Chart.RemoveObject(name);
                            return;
                        }
        
                        double normalized =
                            NormalizePrice(price);
        
                        if (!IsFinitePositive(normalized))
                        {
                            Chart.RemoveObject(name);
                            return;
                        }
        
                        ChartTrendLine line =
                            Chart.FindObject(name)
                            as ChartTrendLine;
        
                        LineStyle lineStyle =
                            LineStyle.Solid;
        
                        if (name.IndexOf(
                                "PRED_TRIGGER",
                                StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            lineStyle =
                                LineStyle.Solid;
                        }
                        else if (name.IndexOf(
                                    "PRED_TARGET",
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            lineStyle =
                                LineStyle.Solid;
                        }
        
                        if (line == null)
                        {
                            ChartObject existing =
                                Chart.FindObject(name);
        
                            if (existing != null)
                                Chart.RemoveObject(name);
        
                            line =
                                Chart.DrawTrendLine(
                                    name,
                                    left,
                                    normalized,
                                    right,
                                    normalized,
                                    PredictionLineColor(name),
                                    Math.Max(
                                        1,
                                        LevelLineThickness),
                                    lineStyle);
                        }
        
                        if (line == null)
                            return;
        
                        line.Time1 =
                            Bars.OpenTimes[left];
                        line.Y1 =
                            normalized;
                        line.Time2 =
                            Bars.OpenTimes[right];
                        line.Y2 =
                            normalized;
                        line.Color =
                            PredictionLineColor(name);
                        line.Thickness =
                            Math.Max(
                                1,
                                LevelLineThickness);
                        line.LineStyle =
                            lineStyle;
                        line.ExtendToInfinity =
                            false;
                        line.IsInteractive =
                            false;
                    }
                    catch (Exception ex)
                    {
                        Print(
                            "CFIP prediction render failed: {0}",
                            ex.Message);
                    }
                }
        
                private void RemovePredictionObjects()
                {
                    Chart.RemoveObject(
                        P + "PRED_ZONE");
        
                    Chart.RemoveObject(
                        P + "PRED_ENTRY");
        
                    Chart.RemoveObject(
                        P + "PRED_ENTRY_LABEL");
        
                    Chart.RemoveObject(
                        P + "PRED_STOP");
        
                    Chart.RemoveObject(
                        P + "PRED_STOP_LABEL");
        
                    Chart.RemoveObject(
                        P + "PRED_TRIGGER");
        
                    Chart.RemoveObject(
                        P + "PRED_TRIGGER_LABEL");
        
                    Chart.RemoveObject(
                        P + "PRED_TARGET1");
        
                    Chart.RemoveObject(
                        P + "PRED_TARGET1_LABEL");
        
                    Chart.RemoveObject(
                        P + "PRED_TARGET2");
        
                    Chart.RemoveObject(
                        P + "PRED_TARGET2_LABEL");
        
                    Chart.RemoveObject(
                        P + "PRED_TARGET3");
        
                    Chart.RemoveObject(
                        P + "PRED_TARGET3_LABEL");
        
                    Chart.RemoveObject(
                        P + "PRED_TARGET4");
        
                    Chart.RemoveObject(
                        P + "PRED_TARGET4_LABEL");
                }
        
                private void EmitContextAlerts(
                    int closedM5)
                {
                    if (_lastContextM5 ==
                        closedM5 ||
                        _m5Frame == null)
                        return;
        
                    _lastContextM5 =
                        closedM5;
        
                    if (AlertOnBos &&
                        (_m5Frame.StructureBull ||
                         _m5Frame.StructureBear))
                    {
                        if (ShowContextEventMarker)
                        {
                            int bar =
                                MapM5ToChart(
                                    closedM5,
                                    Bars.Count - 1);
        
                            DrawIcon(
                                P + "BOS_MARKER",
                                _m5Frame.StructureBull
                                    ? ChartIconType.UpArrow
                                    : ChartIconType.DownArrow,
                                bar,
                                _m5Frame.StructureBull
                                    ? Bars.LowPrices[bar]
                                    : Bars.HighPrices[bar],
                                _m5Frame.StructureBull
                                    ? BuyArrowColor
                                    : SellArrowColor);
                        }
        
                        int direction =
                            _m5Frame.StructureBull
                                ? 1
                                : -1;
        
                        SendUnifiedAlert(
                            "BOS|" +
                            closedM5 +
                            "|" +
                            direction,
                            "CFIP BOS | " +
                            (direction == 1
                                ? "BUY"
                                : "SELL"),
                            direction,
                            false);
                    }
        
                    if (AlertOnMssChoch &&
                        (_m5Frame.MssBull ||
                         _m5Frame.MssBear ||
                         _m5Frame.ChochBull ||
                         _m5Frame.ChochBear))
                    {
                        int direction =
                            _m5Frame.MssBull ||
                            _m5Frame.ChochBull
                                ? 1
                                : -1;
        
                        SendUnifiedAlert(
                            "MSS|" +
                            closedM5 +
                            "|" +
                            direction,
                            "CFIP MSS/CHOCH | " +
                            (direction == 1
                                ? "BUY"
                                : "SELL"),
                            direction,
                            false);
                    }
        
                    if (AlertOnLiquiditySweep &&
                        (_m5Frame.LiquidityBull ||
                         _m5Frame.LiquidityBear))
                    {
                        int direction =
                            _m5Frame.LiquidityBull
                                ? 1
                                : -1;
        
                        SendUnifiedAlert(
                            "SWEEP|" +
                            closedM5 +
                            "|" +
                            direction,
                            "CFIP LIQUIDITY SWEEP | " +
                            (direction == 1
                                ? "BUY"
                                : "SELL"),
                            direction,
                            false);
                    }
        
                    if (AlertOnEarlySetup &&
                        _prediction != null &&
                        _prediction.Confidence >=
                        MinimumEarlyConfidence &&
                        _prediction.Confidence <
                        MinimumConfidence &&
                        _lastEarlyAlertM5 !=
                        closedM5)
                    {
                        SendUnifiedAlert(
                            "EARLY|" +
                            closedM5 +
                            "|" +
                            _prediction.Direction,
                            _prediction.Reason,
                            _prediction.Direction,
                            false);
        
                        _lastEarlyAlertM5 =
                            closedM5;
                    }
                }
        
                private bool CheckLiveReversalAgainstPlan(
                    int closedM5)
                {
                    if (_plan == null ||
                        !_plan.IsLivePosition ||
                        _m5Frame == null ||
                        !EnableLiveStructuralReversal ||
                        !EnableFastReversalIntelligence)
                        return false;
        
                    int opposite =
                        _plan.Direction * -1;
        
                    bool structural =
                        opposite == 1
                            ? (_m5Frame.MssBull ||
                               _m5Frame.ChochBull)
                            : (_m5Frame.MssBear ||
                               _m5Frame.ChochBear);
        
                    bool force =
                        opposite == 1
                            ? (_m5Frame.DisplacementBull &&
                               _m5Frame.LiquidityBull)
                            : (_m5Frame.DisplacementBear &&
                               _m5Frame.LiquidityBear);
        
                    int reversalConfidence =
                        _reaction != null
                            ? Math.Max(
                                _reaction.Confidence,
                                _m5Frame.Quality)
                            : _m5Frame.Quality;
        
                    if (reversalConfidence <
                        Math.Max(
                            50,
                            LiveReversalMinimumConfidence))
                        return false;
        
                    if (!structural ||
                        _m5Frame.Quality <
                        LiveReversalStructuralScore ||
                        _m5Frame.Evidence <
                        LiveReversalMinimumEvidence)
                        return false;
        
                    if (RequireReversalForce &&
                        !force)
                        return false;
        
                    if (!AllowReversalAgainstStaleHtf &&
                        _m15Frame != null &&
                        _m15Frame.Direction ==
                        _plan.Direction)
                        return false;
        
                    if (RequireM15ReversalForOpposite &&
                        _m15Frame != null &&
                        _m15Frame.Direction !=
                        opposite)
                        return false;
        
                    if (StructuralSequence(
                            _m5Bars,
                            closedM5,
                            opposite) <
                        MinimumOppositeM5Structure)
                        return false;
        
                    if (PreventRapidDirectionFlip &&
                        _lastSignalM5 >= 0 &&
                        closedM5 -
                        _lastSignalM5 <
                        OppositeSignalCooldownM5)
                        return false;
        
                    int flipBars =
                        Math.Max(
                            1,
                            SmartFlipConfirmationBars);
        
                    if (flipBars > 1 &&
                        !StableDirection(
                            _m5Bars,
                            closedM5,
                            opposite,
                            flipBars))
                        return false;
        
                    SendUnifiedAlert(
                        "REVERSAL|" +
                        closedM5 +
                        "|" +
                        opposite,
                        "CFIP ACTIVE PLAN REVERSAL | " +
                        (opposite == 1
                            ? "BUY"
                            : "SELL") +
                        " REVERSAL | Q " +
                        _m5Frame.Quality,
                        opposite,
                        true);
        
                    Position livePosition =
                        GetManagedLivePositionForPlan();
        
                    if (livePosition != null)
                    {
                        if (!EnableReversalProtectionClose ||
                            livePosition.NetProfit <= 0)
                        {
                            SetAutoTradingState(
                                "BLOCKED",
                                "REVERSAL DETECTED • POSITION RETAINED");
                            return false;
                        }
        
                        double protectedProfit =
                            livePosition.NetProfit;
        
                        SetLifecycleState(
                            LifecycleState.ExitRequested,
                            "ACTIVE PLAN REVERSAL");
        
                        if (!TryClosePosition(
                                livePosition,
                                "ACTIVE PLAN REVERSAL"))
                        {
                            SetLifecycleState(
                                LifecycleState.RecoveryRequired,
                                "ACTIVE PLAN REVERSAL • EXIT REJECTED");
        
                            SetAutoTradingState(
                                "ERROR",
                                "REVERSAL CLOSE REJECTED");
        
                            return false;
                        }
        
                        _lastExitM5 =
                            Math.Max(
                                _lastExitM5,
                                closedM5);
        
                        SetAutoTradingState(
                            "EXECUTED",
                            "REVERSAL EXIT REQUESTED #" +
                            livePosition.Id);
        
                        SendUnifiedAlert(
                            "REVERSAL-CLOSE|" +
                            livePosition.Id,
                            "CFIP ACTIVE PLAN REVERSAL • EXIT REQUESTED | #" +
                            livePosition.Id +
                            " | protected +" +
                            protectedProfit.ToString("F2"),
                            _plan.Direction,
                            true);
        
                        return true;
                    }
        
                    SetLifecycleState(
                        LifecycleState.Closed,
                        "REVERSAL • POSITION ABSENT");
        
                    _plan = null;
                    _executionModel = null;
                    RemovePlanObjects();
        
                    return true;
                }
        
                private Position GetManagedLivePositionForPlan()
                {
                    if (_plan == null ||
                        !_plan.IsLivePosition ||
                        _plan.PositionId <= 0)
                        return null;
        
                    foreach (Position position in Positions)
                    {
                        if (position != null &&
                            position.SymbolName == SymbolName &&
                            position.Id == _plan.PositionId)
                            return position;
                    }
        
                    return null;
                }
        
                private double CalculateAggressiveVolume(
                    double stopPips)
                {
                    try
                    {
                        if (stopPips <= 0)
                            return 0;
        
                        double amount =
                            Math.Max(
                                0,
                                Account.Equity) *
                            EffectiveAggressiveRiskPercent() /
                            100.0;
        
                        if (amount <= 0)
                            return 0;
        
                        double volume =
                            Symbol.VolumeForFixedRisk(
                                amount,
                                stopPips,
                                RoundingMode.Down);
        
                        return
                            Symbol.NormalizeVolumeInUnits(
                                volume,
                                RoundingMode.Down);
                    }
                    catch
                    {
                        return 0;
                    }
                }
        
                
        
                        private void ProtectBrokerPositions(
                    int closedM5)
                {
                    if (!AutoBrokerProtection &&
                        !AutoProtectBrokerPositions)
                        return;
        
                    if (_plan == null ||
                        !_plan.IsLivePosition ||
                        _lifecycleState ==
                            LifecycleState.ExitRequested)
                        return;
        
                    if ((TimeInUtc -
                         _lastBrokerModifyUtc).TotalMilliseconds <
                        Math.Max(
                            100,
                            BrokerModifyCooldownMs))
                        return;
        
                    string label =
                        string.IsNullOrWhiteSpace(
                            ManagedPositionLabel)
                            ? NormalizeLabel()
                            : ManagedPositionLabel.Trim();
        
                    foreach (Position position in Positions)
                    {
                        if (position == null ||
                            position.SymbolName !=
                            SymbolName)
                            continue;
        
                        bool byPlanId =
                            _plan.PositionId > 0 &&
                            position.Id ==
                            _plan.PositionId;
        
                        bool byManagedLabel =
                            AutoProtectBrokerPositions &&
                            position.Label ==
                            label;
        
                        if (!byPlanId &&
                            !byManagedLabel)
                            continue;
        
                        bool mutationRequired = false;
                        bool mutationSucceeded = true;
        
                        int positionDirection =
                            position.TradeType == TradeType.Buy
                                ? 1
                                : -1;
        
                        if (IsValidStop(
                                positionDirection,
                                position.EntryPrice,
                                _plan.Stop))
                        {
                            double normalizedStop =
                                NormalizePrice(_plan.Stop);
        
                            bool materiallyDifferent =
                                !position.StopLoss.HasValue ||
                                Math.Abs(
                                    position.StopLoss.Value -
                                    normalizedStop) >=
                                Math.Max(
                                    Symbol.TickSize,
                                    Symbol.PipSize * 0.25);
        
                            if (materiallyDifferent)
                            {
                                mutationRequired = true;
                                mutationSucceeded =
                                    TryModifyStopLoss(
                                        position,
                                        normalizedStop,
                                        "LIVE PROTECTION • SL") &&
                                    mutationSucceeded;
                            }
                        }
        
                        if (SyncBrokerTakeProfit)
                        {
                            double target =
                                AutoTarget(
                                    _plan,
                                    EffectiveAutoTpStage());
        
                            if (IsValidTarget(
                                    positionDirection,
                                    position.EntryPrice,
                                    target))
                            {
                                bool move = true;
        
                                if (PreventBrokerTpBackwardMove &&
                                    position.TakeProfit.HasValue)
                                {
                                    double current =
                                        position.TakeProfit.Value;
        
                                    move =
                                        positionDirection == 1
                                            ? target >= current
                                            : target <= current;
                                }
        
                                double normalizedTarget =
                                    NormalizePrice(target);
        
                                bool materiallyDifferent =
                                    move &&
                                    (!position.TakeProfit.HasValue ||
                                     Math.Abs(
                                         position.TakeProfit.Value -
                                         normalizedTarget) >=
                                     Math.Max(
                                         Symbol.TickSize,
                                         Symbol.PipSize * 0.25));
        
                                if (materiallyDifferent)
                                {
                                    mutationRequired = true;
                                    mutationSucceeded =
                                        TryModifyTakeProfit(
                                            position,
                                            normalizedTarget,
                                            "LIVE PROTECTION • TP") &&
                                        mutationSucceeded;
                                }
                            }
                        }
        
                        if (mutationRequired &&
                            !mutationSucceeded)
                        {
                            _brokerProtectionRecoveryRequired = true;
        
                            SetLifecycleState(
                                LifecycleState.RecoveryRequired,
                                "BROKER PROTECTION MUTATION REJECTED");
        
                            SendUnifiedAlert(
                                "PROTECTION-SYNC-FAILED|" +
                                position.Id,
                                "CFIP BROKER PROTECTION SYNC REJECTED | #" +
                                position.Id,
                                positionDirection,
                                true);
                        }
                        else if (!_brokerProtectionRecoveryRequired)
                        {
                            SetLifecycleState(
                                LifecycleState.LivePosition,
                                "LIVE POSITION • BROKER STATE SYNCHRONIZED");
                        }
        
                        _lastBrokerModifyUtc =
                            TimeInUtc;
        
                        break;
                    }
                }
        
                private void TryAggressiveAutoTrade(
                    int closedM5)
                {
                    if (!AutoTradingEnabled ||
                        !EnableAggressiveAutoEntry ||
                        _lifecycleState ==
                            LifecycleState.ExitRequested ||
                        _plan != null ||
                        _reaction == null ||
                        !_reaction.EntryAllowed ||
                        _reaction.Direction == 0)
                        return;
        
                    if (GetManagedPendingOrder() != null)
                    {
                        _autoExecutionBlockReason =
                            "PENDING ORDER EXISTS";
                        return;
                    }
        
                    if (DailyLossLimitHit(
                            TimeInUtc))
                        return;
        
                    if (OneOrderPerSignal &&
                        _lastAutoM5 ==
                        closedM5)
                        return;
        
                    if (_reaction.Confidence <
                        AggressiveMinimumConfidence ||
                        _reaction.IndependentEvidence <
                        AggressiveMinimumEvidence)
                        return;
        
                    if (AggressiveRequireSmartAgreement &&
                        (_decision == null ||
                         _decision.Direction !=
                         _reaction.Direction ||
                         _decision.SmartQuality <
                         AggressiveMinimumSmartQuality))
                        return;
        
                    string aggressiveSuitabilityReason;
        
                    if (!PassesMarketSuitability(
                            closedM5,
                            _reaction.Direction,
                            out aggressiveSuitabilityReason))
                    {
                        SetAutoTradingState(
                            "BLOCKED",
                            "AGGRESSIVE • " +
                            aggressiveSuitabilityReason);
                        return;
                    }
        
                    if (ManagedPositionCount() >=
                        Math.Max(
                            1,
                            MaximumOpenPositions))
                        return;
        
                    double entry =
                        NormalizePrice(
                            _reaction.Direction == 1
                                ? Symbol.Ask
                                : Symbol.Bid);
        
                    double atr =
                        Atr(
                            _m5Bars,
                            closedM5);
        
                    if (atr <= 0)
                    {
                        _autoExecutionBlockReason =
                            "AGGRESSIVE • ATR UNAVAILABLE";
                        return;
                    }
        
                    string source;
                    int quality;
        
                    double stop =
                        BuildStructuralStop(
                            closedM5,
                            _reaction.Direction,
                            entry,
                            atr,
                            out source,
                            out quality);
        
                    if (!IsFinitePositive(stop))
                    {
                        _autoExecutionBlockReason =
                            "AGGRESSIVE • INVALID SL";
                        return;
                    }
        
                    double target =
                        SelectStructuralAutoTarget(
                            closedM5,
                            _reaction.Direction,
                            entry,
                            stop,
                            atr,
                            AggressiveTpStage);
        
                    if (!IsAutoPlanValid(
                            _reaction.Direction,
                            entry,
                            stop,
                            target))
                    {
                        SetAutoTradingState(
                            "BLOCKED",
                            "NO VALID STRUCTURAL TARGET");
                        return;
                    }
        
                    double stopPips =
                        Math.Abs(
                            entry -
                            stop) /
                        Symbol.PipSize;
        
                    double tpPips =
                        Math.Abs(
                            target -
                            entry) /
                        Symbol.PipSize;
        
                    double effectiveStopPips =
                        stopPips;
        
                    if (IncludeSpreadInRiskSizing)
                        effectiveStopPips +=
                            Math.Max(
                                0,
                                (Symbol.Ask - Symbol.Bid) /
                                Math.Max(
                                    Symbol.PipSize,
                                    1e-9));
        
                    double volume =
                        CalculateAggressiveVolume(
                            effectiveStopPips);
        
                    volume =
                        AdjustVolumeForMargin(
                            _reaction.Direction == 1
                                ? TradeType.Buy
                                : TradeType.Sell,
                            volume);
        
                    if (volume <
                        Symbol.VolumeInUnitsMin)
                    {
                        _autoExecutionBlockReason =
                            "AGGRESSIVE • VOLUME BELOW MINIMUM";
                        return;
                    }
        
                    try
                    {
                        TradeType type =
                            _reaction.Direction == 1
                                ? TradeType.Buy
                                : TradeType.Sell;
        
                        if (!EnsureTradingPermission())
                        {
                            _autoExecutionBlockReason =
                                "TRADING PERMISSION";
                            SetAutoTradingState(
                                "BLOCKED",
                                "TRADING PERMISSION NOT GRANTED");
                            return;
                        }
        
                        string guardReason;
        
                        if (!PassesAutoTradeSafetyGuards(
                                type,
                                volume,
                                out guardReason))
                        {
                            SetAutoTradingState(
                                "BLOCKED",
                                guardReason);
                            return;
                        }
        
                        ExecutionIntent aggressiveIntent =
                            BuildExecutionIntent(
                                _reaction.Direction,
                                DecisionPolicyMode.Aggressive,
                                ExecutionIntentKind.Market,
                                entry,
                                0,
                                0,
                                0,
                                stop,
                                target,
                                volume,
                                closedM5,
                                "AGGRESSIVE MARKET");
        
                        string aggressiveIntentReason;
        
                        if (!ValidateExecutionIntent(
                                aggressiveIntent,
                                entry,
                                out aggressiveIntentReason))
                        {
                            SetAutoTradingState(
                                "BLOCKED",
                                aggressiveIntentReason);
                            return;
                        }
        
                        TradeResult result =
                            ExecuteMarketOrder(
                                type,
                                SymbolName,
                                volume,
                                NormalizeLabel(),
                                stopPips,
                                tpPips,
                                "CFIP SMART73",
                                false);
        
                        if (result == null ||
                            !result.IsSuccessful ||
                            result.Position == null)
                        {
                            _autoExecutionBlockReason =
                                result != null &&
                                result.Error.HasValue
                                    ? "AGGRESSIVE • " +
                                      result.Error.Value.ToString()
                                    : "AGGRESSIVE • TRADE REJECTED";
                            SetAutoTradingState(
                                "ERROR",
                                _autoExecutionBlockReason);
                            return;
                        }
        
                        double actualFill =
                            NormalizePrice(
                                result.Position.EntryPrice);
        
                        string aggressiveFillReason;
        
                        if (!ValidateActualMarketFill(
                                aggressiveIntent,
                                actualFill,
                                atr,
                                out aggressiveFillReason))
                        {
                            SetLifecycleState(
                                LifecycleState.RecoveryRequired,
                                "AGGRESSIVE FILL MISMATCH");
        
                            _plan =
                                CreateManagedPlanFromExecution(
                                    _reaction.Direction,
                                    actualFill,
                                    stop,
                                    target,
                                    closedM5,
                                    result.Position.VolumeInUnits);
        
                            _plan.PositionId =
                                result.Position.Id;
        
                            ReconcileLivePlanToActualFill(
                                result.Position,
                                closedM5);
        
                            return;
                        }
        
                        string actualStopSource;
                        int actualStopQuality;
        
                        double actualStop =
                            BuildStructuralStop(
                                closedM5,
                                _reaction.Direction,
                                actualFill,
                                atr,
                                out actualStopSource,
                                out actualStopQuality);
        
                        double actualTarget =
                            SelectStructuralAutoTarget(
                                closedM5,
                                _reaction.Direction,
                                actualFill,
                                actualStop,
                                atr,
                                AggressiveTpStage);
        
                        if (!IsExecutionPlanConsistent(
                                _reaction.Direction,
                                actualFill,
                                actualStop,
                                actualTarget))
                        {
                            SetLifecycleState(
                                LifecycleState.RecoveryRequired,
                                "AGGRESSIVE POST-FILL REBUILD FAILED");
                            return;
                        }
        
                        _lastAutoM5 =
                            closedM5;
        
                        _autoExecutionBlockReason =
                            "EXECUTED";
        
                        _plan =
                            CreateManagedPlanFromExecution(
                                _reaction.Direction,
                                result.Position.EntryPrice,
                                stop,
                                target,
                                closedM5,
                                result.Position.VolumeInUnits);
        
                        _plan.PositionId =
                            result.Position.Id;
        
                        SetLifecycleState(
                            LifecycleState.LivePosition,
                            "AGGRESSIVE ENTRY • FILLED");
        
                        ReconcileLivePlanToActualFill(
                            result.Position,
                            closedM5);
        
                        double maximumFillDistance =
                            Math.Max(
                                Symbol.TickSize * 2,
                                Math.Max(
                                    Symbol.PipSize * 0.5,
                                    Math.Max(
                                        (Symbol.Ask - Symbol.Bid) * 2,
                                        atr *
                                        Math.Max(
                                            0.10,
                                            MaximumEntryExtensionAtr))));
        
                        if (Math.Abs(
                                result.Position.EntryPrice -
                                entry) >
                            maximumFillDistance)
                        {
                            SetLifecycleState(
                                LifecycleState.ExitRequested,
                                "AGGRESSIVE FILL MISMATCH");
        
                            bool closed =
                                TryClosePosition(
                                    result.Position,
                                    "AGGRESSIVE FILL MISMATCH");
        
                            if (!closed)
                            {
                                SetLifecycleState(
                                    LifecycleState.RecoveryRequired,
                                    "AGGRESSIVE FILL MISMATCH • CLOSE REJECTED");
                            }
        
                            SendUnifiedAlert(
                                "FILL-MISMATCH|" +
                                result.Position.Id,
                                "CFIP AGGRESSIVE FILL OUTSIDE EXECUTION ENVELOPE | #" +
                                result.Position.Id,
                                _reaction.Direction,
                                true);
        
                            return;
                        }
        
                        EnrichLivePlanTargets(closedM5);
        
                        if (AutoBrokerProtection)
                        {
                            EnsureBrokerProtectionForPosition(
                                result.Position,
                                actualStop,
                                actualTarget,
                                "AGGRESSIVE ENTRY",
                                _reaction.Direction);
                        }
        
                        SendUnifiedAlert(
                            "AUTO-REACTION|" +
                            closedM5,
                            "CFIP AUTO REACTION " +
                            (_reaction.Direction == 1
                                ? "BUY"
                                : "SELL") +
                            " EXECUTED | #" +
                            result.Position.Id +
                            " | ENTRY " +
                            Price(
                                result.Position.EntryPrice) +
                            " | SL " +
                            Price(actualStop) +
                            " | TP " +
                            Price(actualTarget),
                            _reaction.Direction,
                            true);
                    }
                    catch (Exception ex)
                    {
                        Print(
                            "CFIP aggressive auto trade failed: {0}",
                            ex.Message);
                    }
                }
        
                private void MonitorOutcome(
                    int closedM5)
                {
                    if (!EnableOutcomeTelemetry ||
                        _plan == null ||
                        !_plan.IsLivePosition ||
                        OutcomeMaximumM5Bars <= 0)
                        return;
        
                    if (_outcomeTelemetryTimedOut ||
                        closedM5 -
                        _plan.CreatedM5 <
                        OutcomeMaximumM5Bars)
                        return;
        
                    // Telemetry timeout is an observation boundary, not a position
                    // lifecycle boundary. Keep broker ownership and live protection
                    // active until the position is actually closed.
                    _outcomeTelemetryTimedOut = true;
        
                    SendUnifiedAlert(
                        "OUTCOME-TIMEOUT|" +
                        _plan.PositionId,
                        "CFIP OUTCOME WINDOW ELAPSED | POSITION #" +
                        _plan.PositionId +
                        " remains under live management",
                        _plan.Direction,
                        false);
                }
        
                private void DrawOutcomeMarker(
                    string label,
                    double price,
                    bool success)
                {
                    if (!ShowContextEventMarker ||
                        Bars == null ||
                        Bars.Count < 2 ||
                        !IsFinitePositive(price))
                        return;
        
                    try
                    {
                        string name =
                            P +
                            "OUTCOME_" +
                            label.Replace(
                                " ",
                                "_") +
                            "_" +
                            Bars.Count +
                            "_" +
                            _outcomeSequence++;
        
                        ChartText marker =
                            Chart.DrawText(
                                name,
                                label,
                                Bars.OpenTimes[
                                    Bars.Count - 1],
                                NormalizePrice(price),
                                success
                                    ? TpLineColor
                                    : SlLineColor);
        
                        marker.FontSize =
                            Math.Max(
                                8,
                                PanelFontSize);
        
                        marker.IsBold = true;
                        marker.IsInteractive = false;
        
                        _outcomeSequence =
                            Math.Max(
                                0,
                                _outcomeSequence);
                    }
                    catch
                    {
                    }
                }
        
                // ============================================================
    }
}
