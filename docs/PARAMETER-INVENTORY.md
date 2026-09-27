# CFIP Indicator — v89 Parameter Inventory

Reference: CFIP-PRO `integrations/ctrader/calude-edit-v89.cs`

Total inventoried parameters: **512**.

All remain preserved in the migration surface. Disposition is finalized only after usage/parity audit: ACTIVE, DEPRECATED (with migration path), or REMOVED.

## 01 · Decision

| Label | Property | Type | Default |
|---|---|---|---|
| Minimum Confidence | MinimumConfidence | int | 72 |
| Minimum Edge | MinimumEdge | int | 15 |
| Minimum Smart Quality | MinimumSmartQuality | int | 70 |
| Minimum Structural Confirmations | MinimumStructuralConfirmations | int | 4 |
| Minimum Independent Evidence | MinimumIndependentEvidence | int | 4 |
| Minimum Timeframe Agreement | MinimumTimeframeAgreement | int | 72 |
| Require Higher TF Agreement | RequireHigherTfAgreement | bool | true |
| Require Core Agreement | RequireCoreAgreement | bool | true |
| Require Structural Confirmation | RequireStructuralConfirmation | bool | true |
| Use Advanced Confluence | UseAdvancedConfluence | bool | true |
| Allow Strong Trigger Override | AllowStrongTriggerOverride | bool | true |

## 02 · MTF

| Label | Property | Type | Default |
|---|---|---|---|
| M1 Trigger | UseM1Trigger | bool | false |
| M5 Confirmation | UseM5Confirmation | bool | true |
| M5 Weight | M5Weight | int | 7 |
| M15 Weight | M15Weight | int | 8 |
| M30 Weight | M30Weight | int | 5 |
| H1 Weight | H1Weight | int | 3 |
| H4 Weight | H4Weight | int | 2 |
| D1 Weight | D1Weight | int | 2 |
| W1 Weight | W1Weight | int | 3 |

## 03 · Structure

| Label | Property | Type | Default |
|---|---|---|---|
| Structure Lookback | StructureLookback | int | 40 |
| Swing Strength | SwingStrength | int | 2 |
| Structure Break ATR | StructureBreakAtr | double | 0.05 |
| Use Internal Structure | UseInternalStructure | bool | true |
| Use MSS / CHOCH | UseMssChoch | bool | true |

## 04 · Zones

| Label | Property | Type | Default |
|---|---|---|---|
| Use FVG | UseFvg | bool | true |
| FVG Lookback | FvgLookback | int | 24 |
| Minimum FVG ATR | MinimumFvgAtr | double | 0.08 |
| Use Order Block | UseOrderBlock | bool | true |
| OB Lookback | ObLookback | int | 30 |
| OB Displacement ATR | ObDisplacementAtr | double | 0.60 |
| OB Use Body For Zone | ObUseBodyForZone | bool | false |
| OB Break By Wicks | ObBreakByWicks | bool | false |
| OB Structure Lookback | ObStructureLookback | int | 8 |
| OB Impulse Bars | ObImpulseBars | int | 4 |
| OB Minimum Quality | ObMinimumQuality | int | 68 |
| Require FVG Retest | RequireFvgRetest | bool | true |
| Use 2-Bar Imbalance FVG | UseTwoBarImbalanceFvg | bool | true |
| Require OB Displacement | RequireObDisplacement | bool | true |
| Zone Proximity ATR | ZoneProximityAtr | double | 0.25 |
| Maximum Zone Age | MaximumZoneAgeBars | int | 40 |

## 05 · Liquidity

| Label | Property | Type | Default |
|---|---|---|---|
| Liquidity Lookback | LiquidityLookback | int | 40 |
| Use Equal High / Low | UseEqualHighLow | bool | true |
| Equal Level Tolerance ATR | EqualLevelToleranceAtr | double | 0.12 |
| Use Liquidity Sweep | UseLiquiditySweep | bool | true |
| Liquidity Sweep Minimum Depth ATR | LiquiditySweepMinimumDepthAtr | double | 0.05 |
| Use Displacement | UseDisplacement | bool | true |
| Displacement ATR | DisplacementAtr | double | 0.80 |
| Use Premium / Discount | UsePremiumDiscount | bool | true |
| Use Daily Weekly Liquidity | UseDailyWeeklyLiquidity | bool | true |
| Use Daily Pivots | UseDailyPivots | bool | true |
| Daily Pivot Weight | DailyPivotWeight | int | 18 |

## 06 · Indicators

| Label | Property | Type | Default |
|---|---|---|---|
| Fast EMA | FastEma | int | 9 |
| Slow EMA | SlowEma | int | 21 |
| RSI Period | RsiPeriod | int | 14 |
| ADX Period | AdxPeriod | int | 14 |
| ADX Minimum | AdxMinimum | double | 16 |
| ATR Period | AtrPeriod | int | 14 |
| Use EMA Slope | UseEmaSlope | bool | true |
| Avoid RSI Exhaustion | AvoidRsiExhaustion | bool | true |

## 07 · Entry Precision

| Label | Property | Type | Default |
|---|---|---|---|
| Minimum Trigger Body ATR | MinimumTriggerBodyAtr | double | 0.12 |
| Minimum Close Location | MinimumCloseLocation | double | 0.65 |
| Maximum Trigger Range ATR | MaximumTriggerRangeAtr | double | 2.5 |
| Require Stable M5 Direction | RequireStableM5Direction | bool | true |
| Stable M5 Bars | StableM5Bars | int | 2 |
| Require Stable M15 Direction | RequireStableM15Direction | bool | true |
| Stable M15 Bars | StableM15Bars | int | 2 |
| Entry Buffer ATR | EntryBufferAtr | double | 0.05 |
| Maximum Entry Extension ATR | MaximumEntryExtensionAtr | double | 0.75 |
| Minimum Retest Quality | MinimumRetestQuality | int | 76 |
| Require Retest Quality | RequireRetestQuality | bool | true |

## 08 · Smart Weights

| Label | Property | Type | Default |
|---|---|---|---|
| Supply / Demand Weight | SupplyDemandWeight | int | 125 |
| FVG Weight | FvgWeight | int | 120 |
| Order Block Weight | OrderBlockWeight | int | 125 |
| Liquidity Pool Weight | LiquidityPoolWeight | int | 120 |
| Equal High / Low Weight | EqualHighLowWeight | int | 115 |
| Swing Structure Weight | SwingStructureWeight | int | 105 |
| MTF Cluster Weight | MtfClusterWeight | int | 130 |
| Previous Day Weight | PreviousDayWeight | int | 115 |
| Previous Week Weight | PreviousWeekWeight | int | 130 |
| Session Weight | SessionWeight | int | 110 |
| HTF Structure Weight | HtfStructureWeight | int | 118 |

## 09 · Risk & Targets

| Label | Property | Type | Default |
|---|---|---|---|
| Minimum SL ATR | MinimumSlAtr | double | 0.55 |
| Maximum SL ATR | MaximumSlAtr | double | 1.80 |
| Fallback SL ATR Multiplier | FallbackSlAtr | double | 1.00 |
| TP1 Minimum RR | Tp1MinimumRR | double | 2.00 |
| TP2 Minimum RR | Tp2MinimumRR | double | 3.20 |
| TP3 Minimum RR | Tp3MinimumRR | double | 4.80 |
| TP4 Minimum RR | Tp4MinimumRR | double | 6.50 |
| Minimum TP Spacing ATR | MinimumTpSpacingAtr | double | 0.40 |
| Target Clearance ATR | TargetClearanceAtr | double | 0.10 |
| Reject Target Obstacle | RejectTargetObstacle | bool | true |
| Maximum Target Extension ATR | MaximumTargetExtensionAtr | double | 4.0 |
| Use HTF Structure For Stop | UseHtfStructureForStop | bool | true |
| Require Structural Stop | RequireStructuralStop | bool | true |
| Adaptive Structural RR | AdaptiveStructuralRR | bool | true |

## 10 · Live Management

| Label | Property | Type | Default |
|---|---|---|---|
| Enable Live Exit Management | EnableLiveExitManagement | bool | true |
| Move SL To Break Even | MoveSlToBreakEven | bool | true |
| Break Even Trigger RR | BreakEvenTriggerRR | double | 0.90 |
| Break Even Buffer Pips | BreakEvenBufferPips | double | 0.5 |
| Use Spread Aware Break Even | UseSpreadAwareBreakEven | bool | true |
| Risk Free Lock Pips | RiskFreeLockPips | double | 0.5 |
| Smart Trail Momentum Bonus ATR | SmartTrailMomentumBonusAtr | double | 0.10 |
| Smart Trail Tighten At RR | SmartTrailTightenAtRR | double | 1.80 |
| Enable Structural SL Repricing | EnableStructuralSlRepricing | bool | true |
| SL Reprice Start RR | SlRepriceStartRR | double | 1.00 |
| SL Reprice Breathing ATR | SlRepriceBreathingAtr | double | 0.85 |
| SL Reprice Step ATR | SlRepriceStepAtr | double | 0.08 |
| Update Unhit Targets | UpdateUnhitTargets | bool | true |
| Target Update Trigger RR | TargetUpdateTriggerRR | double | 1.20 |
| Enable Profit Exhaustion Protection | EnableProfitExhaustionProtection | bool | true |
| Exhaustion Minimum Peak RR | ExhaustionMinimumPeakRR | double | 1.50 |
| Exhaustion Retracement Percent | ExhaustionRetracementPercent | double | 35 |
| Exhaustion Pressure Threshold | ExhaustionPressureThreshold | int | 78 |
| Exhaustion Minimum Opposite Evidence | ExhaustionMinimumOppositeEvidence | int | 3 |

## 11 · Filters

| Label | Property | Type | Default |
|---|---|---|---|
| Use Session Filter | UseSessionFilter | bool | false |
| Session Start UTC | SessionStartUtc | int | 6 |
| Session End UTC | SessionEndUtc | int | 20 |
| Enable End Of Day Alert | EnableEndOfDayAlert | bool | true |
| End Of Day Alert Minutes Before | EndOfDayAlertMinutesBefore | int | 30 |
| Enable End Of Day Auto Close | EnableEndOfDayAutoClose | bool | true |
| Use Spread Filter | UseSpreadFilter | bool | true |
| Maximum Spread ATR | MaximumSpreadAtr | double | 0.15 |
| Cooldown M5 Bars | CooldownM5Bars | int | 3 |
| Avoid Friday Late Entry | AvoidFridayLateEntry | bool | false |
| Friday Cutoff UTC | FridayCutoffUtc | int | 18 |
| Use Volatility Guard | UseVolatilityGuard | bool | true |
| Event Shock Range ATR | EventShockRangeAtr | double | 2.20 |
| Event Shock ATR Expansion | EventShockAtrExpansion | double | 1.55 |
| News Blackout UTC | NewsBlackoutUtc | string | "" |

## 12 · ALERTS — CORE

| Label | Property | Type | Default |
|---|---|---|---|
| Enable Sound Alerts | EnableSoundAlerts | bool | true |
| Show Popup Alerts | ShowPopupAlerts | bool | false |
| Popup Critical Only | PopupCriticalOnly | bool | true |
| Popup Duration Seconds | PopupDurationSeconds | int | 6 |
| Popup Font Size | PopupFontSize | int | 11 |
| Show Entry Restriction Popup | ShowEntryRestrictionPopup | bool | true |
| Alert On News / Event Guard | AlertOnNewsEventGuard | bool | true |
| Popup Margin | PopupMargin | int | 10 |
| Popup Border Alpha | PopupBorderAlpha | int | 235 |
| Popup Bold | PopupBold | bool | false |
| Popup Font Family | PopupFontFamily | string | "Arial" |
| Alert On Confirmed Signal | AlertOnConfirmedSignal | bool | true |
| Alert On Reaction | AlertOnReaction | bool | true |
| Alert On Early Watch | AlertOnEarlyWatch | bool | true |
| Alert On Level Hit | AlertOnLevelHit | bool | true |
| Alert On Invalidated | AlertOnInvalidated | bool | true |
| Alert Cooldown Seconds | AlertCooldownSeconds | int | 8 |
| Suppress Duplicate Alerts | SuppressDuplicateAlerts | bool | true |
| Enable Email Alerts | EnableEmailAlerts | bool | false |
| Sender Email | SenderEmail | string | "" |
| Receiver Email | ReceiverEmail | string | "" |

## 13 · AUTO TRADING

| Label | Property | Type | Default |
|---|---|---|---|
| Enable Auto Trading | EnableAutoTrading | bool | false |
| Enable Automatic Orders | EnableAutomaticOrders | bool | false |
| Pending Order Mode | PendingOrderMode | CFIPClean89PendingOrderMode | CFIPClean89PendingOrderMode.Adaptive |
| Pending Order Expiry Minutes | PendingOrderExpiryMinutes | int | 120 |
| Pending Entry Buffer ATR | PendingEntryBufferAtr | double | 0.10 |
| Pending Minimum Confidence | PendingMinimumConfidence | int | 84 |
| Pending Minimum Smart Quality | PendingMinimumSmartQuality | int | 78 |
| Pending Minimum Trend Quality | PendingMinimumTrendQuality | int | 72 |
| Pending Auto Cleanup | PendingAutoCleanup | bool | true |
| Auto Trading Reminder | AutoTradingReminder | bool | true |
| Reversal Close Minimum Evidence | ReversalCloseMinimumEvidence | int | 5 |
| Reversal Close Minimum MTF | ReversalCloseMinimumMtf | int | 70 |
| Confirmed Signals Only | ConfirmedSignalsOnly | bool | true |
| Sizing Mode | SizingMode | CFIPClean89SizingMode | CFIPClean89SizingMode.RiskPercentEquity |
| Risk % Equity | RiskPercentEquity | double | 0.50 |
| Fixed Lots | FixedLots | double | 0.01 |
| Minimum Auto Confidence | MinimumAutoConfidence | int | 86 |
| Minimum Auto Smart Quality | MinimumAutoSmartQuality | int | 80 |
| Minimum Auto Level Quality | MinimumAutoLevelQuality | int | 72 |
| Auto TP Stage | AutoTpStage | CFIPClean89TargetStage | CFIPClean89TargetStage.TP1 |
| Enable Dynamic TP Advance | EnableDynamicTpAdvance | bool | true |
| TP Advance Proximity Percent | TpAdvanceProximityPercent | double | 72 |
| Enable Partial Take Profit | EnablePartialTakeProfit | bool | false |
| Partial Close At TP1 Percent | PartialCloseTp1Percent | double | 33 |
| Partial Close At TP2 Percent | PartialCloseTp2Percent | double | 33 |
| Move To Break Even After Partial | MoveToBreakEvenAfterPartial | bool | true |
| Enable Reversal Protection Close | EnableReversalProtectionClose | bool | true |
| Reversal Protection Minimum Quality | ReversalProtectionMinimumQuality | int | 82 |
| Maximum Open Positions | MaximumOpenPositions | int | 1 |
| Enable Daily Loss Limit | EnableDailyLossLimit | bool | true |
| Maximum Daily Loss Percent | MaximumDailyLossPercent | double | 3.0 |
| Use Market Hours Guard | UseMarketHoursGuard | bool | true |
| Use Auto Margin Guard | UseAutoMarginGuard | bool | true |
| Max Auto Margin Usage % | MaxAutoMarginUsagePercent | double | 80 |
| Margin Buffer % | MarginBufferPercent | double | 10 |
| Auto Trade Label | AutoTradeLabel | string | "CFIP-SMART-CLEAN89" |
| Smart Broker Protection | AutoBrokerProtection | bool | true |
| One Order Per Signal | OneOrderPerSignal | bool | true |
| Always Show Safety Buttons | AlwaysShowSafetyButtons | bool | true |
| Action Button Margin | ActionButtonMargin | int | 2 |
| Include Spread In Risk Sizing | IncludeSpreadInRiskSizing | bool | true |
| Managed Actions Only | ManagedActionsOnly | bool | true |
| Action Button Width | ActionButtonWidth | int | 150 |
| Action Button Height | ActionButtonHeight | int | 25 |
| Auto Protect Broker Positions | AutoProtectBrokerPositions | bool | false |
| Managed Position Label | ManagedPositionLabel | string | "" |
| Sync Smart Broker Take Profit | SyncBrokerTakeProfit | bool | true |
| Prevent Broker TP Backward Move | PreventBrokerTpBackwardMove | bool | true |
| Broker Modify Cooldown ms | BrokerModifyCooldownMs | int | 750 |
| Enable Aggressive Auto Entry | EnableAggressiveAutoEntry | bool | false |
| Aggressive Minimum Confidence | AggressiveMinimumConfidence | int | 88 |
| Aggressive Minimum Evidence | AggressiveMinimumEvidence | int | 4 |
| Aggressive Minimum Smart Quality | AggressiveMinimumSmartQuality | int | 78 |
| Aggressive Risk % Equity | AggressiveRiskPercentEquity | double | 0.25 |
| Aggressive TP Stage | AggressiveTpStage | CFIPClean89TargetStage | CFIPClean89TargetStage.TP1 |
| Aggressive Require Smart Agreement | AggressiveRequireSmartAgreement | bool | true |

## 24 · SMART EXECUTION

| Label | Property | Type | Default |
|---|---|---|---|
| Enable Market Suitability Guard | EnableMarketSuitabilityGuard | bool | true |
| Minimum Market Suitability | MinimumMarketSuitability | int | 68 |
| Hard Market Suitability Gate | HardMarketSuitabilityGate | bool | true |
| Require Session Suitability | RequireSessionSuitability | bool | false |
| Use Smart Risk Scaling | UseSmartRiskScaling | bool | true |
| Minimum Smart Risk Multiplier | MinimumSmartRiskMultiplier | double | 0.55 |
| Full Risk Confidence Threshold | FullRiskConfidenceThreshold | int | 94 |
| Full Risk Suitability Threshold | FullRiskSuitabilityThreshold | int | 88 |
| Penalize Choppy Regime Risk | PenalizeChoppyRegimeRisk | bool | true |
| Suitability Recalculation Seconds | SuitabilityRecalculationSeconds | int | 2 |

## 14 · DISPLAY — CORE

| Label | Property | Type | Default |
|---|---|---|---|
| Show Level Lines | ShowLevelLines | bool | true |
| Full Width Level Lines | FullWidthLevelLines | bool | true |
| Level Line Thickness | LevelLineThickness | int | 1 |
| Show Entry | ShowEntry | bool | true |
| Show Trigger | ShowTrigger | bool | true |
| Show SL | ShowSL | bool | true |
| Show TP1 | ShowTP1 | bool | true |
| Show TP2 | ShowTP2 | bool | true |
| Show TP3 | ShowTP3 | bool | true |
| Show TP4 | ShowTP4 | bool | true |
| Show Signal Arrow | ShowSignalArrow | bool | true |
| Show Early Watch | ShowEarlyWatch | bool | true |
| Show Historical Signals | ShowHistoricalSignals | bool | false |
| Historical Signal Limit | HistoricalSignalLimit | int | 10 |
| Show Unified Panel | ShowUnifiedPanel | bool | true |
| Show Panel Background | ShowPanelBackground | bool | true |
| Panel Position | PanelPosition | CFIPClean89PanelCorner | CFIPClean89PanelCorner.BottomLeft |
| Panel Width | PanelWidth | int | 430 |
| Panel Font Size | PanelFontSize | int | 11 |
| Panel Font Family | PanelFontFamily | string | "Arial" |
| Panel Bold | PanelBold | bool | false |
| Panel Background | PanelBackground | Color | "Black" |
| Panel Background Alpha | PanelBackgroundAlpha | int | 145 |
| Panel Border | PanelBorder | Color | "#3A4656" |
| Panel Border Alpha | PanelBorderAlpha | int | 230 |
| Panel Border Thickness | PanelBorderThickness | int | 1 |
| Panel Corner Radius | PanelCornerRadius | int | 5 |
| Panel Padding | PanelPadding | int | 9 |
| Panel Margin | PanelMargin | int | 8 |
| Panel Row Gap | PanelRowGap | int | 1 |
| Panel Max Height | PanelMaxHeight | int | 650 |
| Panel Row Padding | PanelRowPadding | int | 3 |
| Panel Button Gap | PanelButtonGap | int | 4 |
| Panel Accent Color | PanelAccentColor | Color | "#4A90E2" |
| Panel Section Color | PanelSectionColor | Color | "#8FA3B8" |
| Panel Secondary Text Color | PanelSecondaryTextColor | Color | "#C5CBD3" |
| Panel Muted Text Color | PanelMutedTextColor | Color | "#8A95A5" |
| Panel Warning Color | PanelWarningColor | Color | "Orange" |
| Panel Text Color | PanelTextColor | Color | "White" |
| Entry Line Color | EntryLineColor | Color | "White" |
| Trigger Line Color | TriggerLineColor | Color | "Orange" |
| SL Line Color | SlLineColor | Color | "Red" |
| TP1 Line Color | TpLineColor | Color | "Lime" |
| TP2 Line Color | Tp2LineColor | Color | "SpringGreen" |
| TP3 Line Color | Tp3LineColor | Color | "Turquoise" |
| TP4 Line Color | Tp4LineColor | Color | "Gold" |
| BUY Arrow Color | BuyArrowColor | Color | "Lime" |
| SELL Arrow Color | SellArrowColor | Color | "Red" |

## 15 · CONTROL — ADVANCED

| Label | Property | Type | Default |
|---|---|---|---|
| Live Trigger Score | LiveTriggerScore | int | 4 |
| Precision Trigger Score | PrecisionTriggerScore | int | 5 |
| Allow Strong M5 Trigger Override | AllowStrongM5TriggerOverride | bool | true |
| M5 Only Confirmed Trigger | M5OnlyConfirmedTrigger | bool | true |
| Allow M15 Neutral Pullback | AllowM15NeutralPullback | bool | true |
| Higher TF Penalty | HigherTfPenalty | int | 7 |
| Use Zone Confluence | UseZoneConfluence | bool | true |
| Use Higher TF Liquidity Targets | UseHigherTfLiquidityTargets | bool | true |
| Minimum HTF Target RR | MinimumHtfTargetRR | double | 2.50 |
| Structural TP RR Step | StructuralTpRrStep | double | 0.50 |
| Minimum Trade RR | MinimumTradeRR | double | 2.00 |
| Use RR Filter | UseRRFilter | bool | true |
| Avoid Late Entry | AvoidLateEntry | bool | true |
| Use Precision Execution Model | UsePrecisionExecutionModel | bool | true |
| Stop Buffer ATR | StopBufferAtr | double | 0.10 |
| Require HTF Targets | RequireHtfTargets | bool | false |
| Maximum Structural Stop ATR | MaximumStructuralStopAtr | double | 2.25 |
| Target Obstacle Lookback Bars | TargetObstacleLookbackBars | int | 8 |
| Allow Direct Displacement Override | AllowDirectDisplacementOverride | bool | true |
| Direct Displacement Override Score | DirectDisplacementOverrideScore | int | 6 |
| Maximum Setup Age Bars | MaximumSetupAgeBars | int | 8 |
| Allow Synthetic Target Fallback | AllowSyntheticTargetFallback | bool | true |
| HTF Stop Buffer ATR | HtfStopBufferAtr | double | 0.15 |
| Block New Signal While Active | BlockNewSignalWhileActive | bool | true |
| Cooldown Bars | CooldownBars | int | 3 |
| Use News Event Guard | UseNewsEventGuard | bool | true |
| Use Volatility Event Guard | UseVolatilityEventGuard | bool | true |
| Event Guard Cooldown Bars | EventGuardCooldownBars | int | 3 |
| Use Regime No-Trade Guard | UseRegimeNoTradeGuard | bool | true |
| Show Reaction Arrow | ShowReactionArrow | bool | true |
| Show Historical Arrows | ShowHistoricalArrows | bool | true |
| Enable Dynamic Structural Stop Alias | EnableDynamicSlTrail | bool | true |
| Structural Stop Breathing ATR | TrailDistanceAtr | double | 0.85 |
| Structural Stop Step ATR | TrailStepAtr | double | 0.08 |
| Target Update Step ATR | TargetUpdateStepAtr | double | 0.20 |
| Use Swing Structure In Structural Stop | UseSwingStructureInTrail | bool | true |
| Smart Minimum Independent Evidence | SmartMinimumIndependentEvidence | int | 4 |
| Smart Stop Zone Bonus | SmartStopZoneBonus | int | 10 |
| Smart Liquidity Pool Bonus | SmartLiquidityPoolBonus | int | 12 |
| Smart Trail Minimum RR | SmartTrailMinimumRR | double | 1.00 |
| Smart Use Closed-Bar Decision (Safety-Enforced) | SmartUseClosedBarDecision | bool | true |
| Smart Target Nearest Bias | SmartTargetNearestBias | double | 0.65 |
| Require Smart Consensus | RequireSmartConsensus | bool | true |
| Smart Strong Setup Quality | SmartStrongSetupQuality | int | 82 |
| Smart Strong Setup Edge | SmartStrongSetupEdge | int | 10 |
| Smart Flip Confirmation Bars | SmartFlipConfirmationBars | int | 2 |
| Allow Smart Soft Gate | AllowSmartSoftGate | bool | true |
| Enable Fast Reversal Intelligence | EnableFastReversalIntelligence | bool | true |
| Fast Reversal Minimum Quality | FastReversalMinimumQuality | int | 74 |
| Fast Reversal Lookback Bars | FastReversalLookbackBars | int | 6 |
| Fast Reversal Minimum Zone Quality | FastReversalMinimumZoneQuality | int | 60 |
| Allow Fast M5 Reversal Before M15 | AllowFastM5ReversalBeforeM15 | bool | true |
| Retest Lookback Bars | RetestLookbackBars | int | 8 |
| Retest Max Bars After Displacement | RetestMaxBarsAfterDisplacement | int | 6 |
| Retest Zone Tolerance ATR | RetestZoneToleranceAtr | double | 0.25 |
| Retest Rejection Body ATR | RetestRejectionBodyAtr | double | 0.12 |
| Require Retest Close Confirmation | RequireRetestCloseConfirmation | bool | true |
| Use Extended Liquidity Map | UseExtendedLiquidityMap | bool | true |
| Use Session Liquidity Targets | UseSessionLiquidityTargets | bool | true |
| Liquidity Target Minimum Score | LiquidityTargetMinimumScore | int | 72 |
| Target Obstacle Buffer ATR | TargetObstacleBufferAtr | double | 0.10 |
| Require Obstacle Free TP1 | RequireObstacleFreeTp1 | bool | true |

## 20 · Confluence Extensions

| Label | Property | Type | Default |
|---|---|---|---|
| Use Volume Expansion | UseVolumeExpansion | bool | false |
| Volume Expansion Ratio | VolumeExpansionRatio | double | 1.15 |
| Use MACD Bias | UseMacdBias | bool | false |
| MACD Fast Period | MacdFastPeriod | int | 12 |
| MACD Slow Period | MacdSlowPeriod | int | 26 |
| Use VWAP Bias | UseVwapBias | bool | false |
| VWAP Lookback Bars | VwapLookbackBars | int | 48 |
| Use Healthy Volatility | UseHealthyVolatility | bool | false |
| Healthy ATR Minimum Ratio | HealthyAtrMinimumRatio | double | 0.85 |
| Healthy ATR Maximum Ratio | HealthyAtrMaximumRatio | double | 1.80 |

## 21 · Complete Intelligence

| Label | Property | Type | Default |
|---|---|---|---|
| Early Setup Confidence | EarlySetupConfidence | int | 52 |
| Enable Live Reaction | EnableLiveReaction | bool | true |
| Live Reaction Watch Threshold | LiveReactionWatchThreshold | int | 56 |
| Live Reaction Threshold | LiveReactionThreshold | int | 68 |
| Live Reaction Strong Threshold | LiveReactionStrongThreshold | int | 82 |
| Minimum Live Reaction Evidence | MinimumLiveReactionEvidence | int | 3 |
| Use Volume Expansion Evidence | UseVolumeExpansionEvidence | bool | true |
| Use MACD Evidence | UseMacdEvidence | bool | true |
| Use VWAP Evidence | UseVwapEvidence | bool | true |
| Use Healthy Volatility Evidence | UseHealthyVolatilityEvidence | bool | true |
| Minimum Smart Direction Share | MinimumSmartDirectionShare | int | 57 |
| Adaptive Smart Thresholds | AdaptiveSmartThresholds | bool | true |
| Smart Regime Buffer | SmartRegimeBuffer | int | 6 |
| Smart Score Temperature | SmartScoreTemperature | double | 12.0 |
| Smart Consensus Threshold | SmartConsensusThreshold | int | 57 |
| Adaptive Regime Weighting | AdaptiveRegimeWeighting | bool | true |
| Use Multi TF Level Map | UseMultiTfLevelMap | bool | true |
| Smart Level Cluster ATR | SmartLevelClusterAtr | double | 0.10 |
| Smart Target Quality | SmartTargetQuality | int | 58 |
| Smart Stop Quality | SmartStopQuality | int | 55 |
| Smart Exit Pressure Threshold | SmartExitPressureThreshold | int | 65 |
| Smart Alert Cooldown Seconds | SmartAlertCooldownSeconds | int | 8 |
| Smart Weekly Context | SmartWeeklyContext | bool | true |
| Block Same-Bar Reentry After Exit | BlockSameBarReentryAfterExit | bool | true |
| Allow Execution Frame Stop Fallback | AllowExecutionFrameStopFallback | bool | true |
| Use Historical Choppiness Guard | UseHistoricalChoppinessGuard | bool | true |
| Alert On News Event | AlertOnNewsEvent | bool | true |
| Alert On Session Block | AlertOnSessionBlock | bool | false |
| Alert On Spread Block | AlertOnSpreadBlock | bool | false |
| Alert On Friday Block | AlertOnFridayBlock | bool | false |
| Alert On Regime No Trade | AlertOnRegimeNoTrade | bool | false |
| Alert On Cooldown Block | AlertOnCooldownBlock | bool | false |
| Alert On Exit Plan Update | AlertOnExitPlanUpdate | bool | true |
| Show Engine Status | ShowEngineStatus | bool | true |
| Panel State Hold Seconds | PanelStateHoldSeconds | int | 2 |
| Show Level Prices In Unified Panel | ShowLevelPricesInUnifiedPanel | bool | true |
| Show Trade Plan Panel | ShowTradePlanPanel | bool | true |
| Use Authoritative Signal State | UseAuthoritativeSignalState | bool | true |
| Require Plan Integrity | RequirePlanIntegrity | bool | true |
| Maximum Spread / Stop Risk Ratio | MaximumSpreadToStopRiskRatio | double | 0.18 |
| Minimum Smart Target Quality For TP1 | MinimumSmartTargetQualityForTp1 | int | 55 |
| Prediction Color | PredictionColor | Color | "#4A90E2" |
| Strong BUY Arrow Color | StrongBuyArrowColor | Color | "Lime" |
| Strong SELL Arrow Color | StrongSellArrowColor | Color | "Red" |
| Confirmed BUY Arrow Color | ConfirmedBuyArrowColor | Color | "Lime" |
| Confirmed SELL Arrow Color | ConfirmedSellArrowColor | Color | "Red" |
| Caution BUY Arrow Color | CautionBuyArrowColor | Color | "#9AA7B4" |
| Caution SELL Arrow Color | CautionSellArrowColor | Color | "#9AA7B4" |
| Blocked / Reaction Arrow Color | BlockedReactionArrowColor | Color | "#9AA7B4" |
| Fallback TP1 RR | FallbackTp1RR | double | 2.0 |
| Fallback TP2 RR | FallbackTp2RR | double | 3.2 |
| Fallback TP3 RR | FallbackTp3RR | double | 4.8 |
| Fallback TP4 RR | FallbackTp4RR | double | 6.5 |

## 23 · Structural Execution

| Label | Property | Type | Default |
|---|---|---|---|
| Require Precision Entry | RequirePrecisionEntry | bool | true |
| Execution Zone ATR | ExecutionZoneAtr | double | 0.35 |
| Minimum Entry Quality | MinimumEntryQuality | int | 72 |
| Maximum Entry Distance ATR | MaximumEntryDistanceAtr | double | 0.45 |
| Allow Precision Breakout Entry | AllowPrecisionBreakoutEntry | bool | true |
| Precision Breakout Buffer ATR | PrecisionBreakoutBufferAtr | double | 0.08 |
| Minimum Targets For Plan | MinimumTargetsForPlan | int | 1 |
| Require HTF Reward For TP1 | RequireHtfRewardForTp1 | bool | false |
| Require HTF Reward For TP2+ | RequireHtfRewardForTp2Plus | bool | true |
| Minimum HTF Reward Quality | MinimumHtfRewardQuality | int | 68 |
| Maximum Reward RR | MaximumRewardRR | double | 12.0 |
| HTF Reward Bonus | HtfRewardBonus | int | 22 |
| Liquidity Reward Bonus | LiquidityRewardBonus | int | 14 |
| Zone Reward Bonus | ZoneRewardBonus | int | 10 |
| Preferred Stop Risk ATR | PreferredStopRiskAtr | double | 1.00 |
| Stop Risk Balance Weight | StopRiskBalanceWeight | int | 18 |
| Minimum Structural Stop Quality | MinimumStructuralStopQuality | int | 65 |
| Structural Stop Management Only | StructuralStopManagementOnly | bool | true |
| Structural Target Updates Only | StructuralTargetUpdatesOnly | bool | true |

## 22 · Safety & Precision

| Label | Property | Type | Default |
|---|---|---|---|
| Use M5 Structure For Initial Stop | UseM5StructureForStop | bool | true |
| Use Zone Mitigation Guard | UseZoneMitigationGuard | bool | true |
| Require Order Block Retest | RequireObRetest | bool | false |
| FVG Invalidate On Full Fill (Safety-Enforced) | FvgInvalidateOnFullFill | bool | true |
| FVG Partial Mitigation | EnableFvgPartialMitigation | bool | true |
| FVG Break By Wicks | FvgBreakByWicks | bool | false |
| Use Semantic Alert Sounds | UseSemanticAlertSounds | bool | true |
| Show Spread Diagnostics | ShowSpreadDiagnostics | bool | true |

## 15 · INTELLIGENCE — EARLY

| Label | Property | Type | Default |
|---|---|---|---|
| Enable Early Prediction | EnableEarlyPrediction | bool | true |
| Minimum Early Confidence | MinimumEarlyConfidence | int | 56 |
| Prediction Lookahead Bars | PredictionLookaheadBars | int | 10 |
| Show Prediction Zone | ShowPredictionZone | bool | true |
| Show Prediction Targets | ShowPredictionTargets | bool | true |
| Use Liquidity Forecast | UseLiquidityForecast | bool | true |
| Alert On Early Setup | AlertOnEarlySetup | bool | true |
| Alert On BOS | AlertOnBos | bool | true |
| Alert On MSS / CHOCH | AlertOnMssChoch | bool | true |
| Alert On Liquidity Sweep | AlertOnLiquiditySweep | bool | true |
| Enable Outcome Telemetry | EnableOutcomeTelemetry | bool | true |
| Outcome Maximum M5 Bars | OutcomeMaximumM5Bars | int | 72 |
| Enable Confidence Calibration | EnableConfidenceCalibration | bool | true |
| Use Empirical Calibration | UseEmpiricalCalibration | bool | true |
| Calibration Directional Minimum Samples | CalibrationDirectionalMinimumSamples | int | 6 |
| Calibration Minimum Samples | CalibrationMinimumSamples | int | 5 |
| Calibration Max Confidence Adjustment | CalibrationMaxConfidenceAdjustment | int | 8 |
| Show Outcome Diagnostics | ShowOutcomeDiagnostics | bool | true |

## 16 · Accuracy

| Label | Property | Type | Default |
|---|---|---|---|
| Use Smart Entry Quality Filter | UseSmartEntryQualityFilter | bool | true |
| Smart Quality Threshold | SmartQualityThreshold | int | 70 |
| Require Fresh M5 Trigger | RequireFreshM5Trigger | bool | true |
| Minimum Fresh Trigger Evidence | MinimumFreshTriggerEvidence | int | 3 |
| Use False Signal Guard | UseFalseSignalGuard | bool | true |
| False Signal Adverse R | FalseSignalAdverseR | double | 1.10 |
| False Signal Watch Bars | FalseSignalWatchBars | int | 3 |
| Invalidate On False Signal | InvalidateOnFalseSignal | bool | true |
| Enable Setup Invalidation | EnableSetupInvalidation | bool | true |
| Invalidation Structure ATR | InvalidationStructureAtr | double | 0.10 |
| Invalidation Zone Close ATR | InvalidationZoneCloseAtr | double | 0.10 |
| Invalidation Max Adverse R | InvalidationMaxAdverseR | double | 0.75 |
| Require MTF Flip For Invalidation | RequireMtfFlipForInvalidation | bool | true |
| Allow Reversal Against Stale HTF | AllowReversalAgainstStaleHtf | bool | true |
| Enable Live Structural Reversal | EnableLiveStructuralReversal | bool | true |
| Live Reversal Minimum Confidence | LiveReversalMinimumConfidence | int | 68 |
| Live Reversal Minimum Evidence | LiveReversalMinimumEvidence | int | 3 |
| Live Reversal Structural Score | LiveReversalStructuralScore | int | 72 |
| Require Reversal Force | RequireReversalForce | bool | true |
| Opposite Signal Cooldown M5 | OppositeSignalCooldownM5 | int | 5 |
| Prevent Rapid Direction Flip | PreventRapidDirectionFlip | bool | true |
| Require M15 Reversal For Opposite | RequireM15ReversalForOpposite | bool | true |
| Allow Opposite While Active | AllowOppositeWhileActive | bool | false |
| Minimum Opposite M5 Structure | MinimumOppositeM5Structure | int | 2 |
| Exit Reentry Cooldown M5 | ExitReentryCooldownM5 | int | 3 |
| Use Structural Sequence Gate | UseStructuralSequenceGate | bool | true |
| Minimum Structural Sequence | MinimumStructuralSequence | int | 2 |
| Require Entry Location Confluence | RequireEntryLocationConfluence | bool | true |
| Minimum Entry Location Quality | MinimumEntryLocationQuality | int | 64 |
| Use Proxy Expected Value Gate | UseProxyExpectedValueGate | bool | true |
| Minimum Proxy Expected Value | MinimumProxyExpectedValue | double | 0.20 |

## 17 · Smart Engine

| Label | Property | Type | Default |
|---|---|---|---|
| Enable Smart Decision Engine | EnableSmartDecisionEngine | bool | true |
| Smart Minimum Timeframe Agreement | SmartMinimumTimeframeAgreement | int | 72 |
| Smart Target Minimum RR | SmartTargetMinimumRR | double | 1.50 |
| Smart Target Max Candidates | SmartTargetMaxCandidates | int | 32 |
| Smart Regime Quality Floor | SmartRegimeQualityFloor | int | 55 |
| No Trade Minimum Smart Quality | NoTradeMinimumSmartQuality | int | 55 |
| Block Compression Regime | BlockCompressionRegime | bool | true |
| Block Weak Range Transition | BlockWeakRangeTransition | bool | true |

## 12 · ALERTS — ADVANCED

| Label | Property | Type | Default |
|---|---|---|---|
| Alert On Live Reaction | AlertOnLiveReaction | bool | true |
| Alert On Smart Decision | AlertOnSmartDecision | bool | true |
| Enable Level Hit Alerts | EnableLevelHitAlerts | bool | true |
| Alert On TP1 | AlertOnTp1 | bool | true |
| Alert On TP2 | AlertOnTp2 | bool | true |
| Alert On TP3 | AlertOnTp3 | bool | true |
| Alert On TP4 | AlertOnTp4 | bool | true |
| Alert On SL | AlertOnSl | bool | true |
| Alert On False Signal Risk | AlertOnFalseSignalRisk | bool | true |
| Alert On Entry Restriction | AlertOnEntryRestriction | bool | false |
| Alert On High Confidence Entry | AlertOnHighConfidenceEntry | bool | true |
| High Confidence Threshold | HighConfidenceThreshold | int | 82 |
| Alert Sound Type | AlertSoundType | cAlgo.API.SoundType | SoundType.PositiveNotification |
| Sound File Path | SoundFilePath | string | "" |
| Popup Position | PopupPosition | CFIPClean89PanelCorner | CFIPClean89PanelCorner.TopRight |
| Popup Width | PopupWidth | int | 430 |
| Keep Popup Until Next Alert | KeepPopupUntilNextAlert | bool | false |
| Show Popup Close Button | ShowPopupCloseButton | bool | true |
| Popup Background | PopupBackgroundColor | Color | "Black" |
| Popup Background Alpha | PopupBackgroundAlpha | int | 235 |
| Popup Border | PopupBorderColor | Color | "#3A4656" |
| Popup Border Thickness | PopupBorderThickness | int | 1 |
| Popup Corner Radius | PopupCornerRadius | int | 5 |
| Popup Padding | PopupPadding | int | 8 |
| Popup Text Color | PopupTextColor | Color | "White" |

## 14 · DISPLAY — ADVANCED

| Label | Property | Type | Default |
|---|---|---|---|
| Line Length Bars | LineLengthBars | int | 40 |
| Line Forward Bars | LineForwardBars | int | 10 |
| Show Signal Labels | ShowSignalLabels | bool | true |
| Show Level Price Labels | ShowLevelPriceLabels | bool | true |
| Show Context Event Marker | ShowContextEventMarker | bool | true |
| Label Left Offset Bars | LabelLeftOffsetBars | int | 2 |
| Show Prediction Objects | ShowPredictionObjects | bool | true |
| Arrow Offset ATR | ArrowOffsetAtr | double | 0.18 |
| Minimum Arrow Offset Pips | MinimumArrowOffsetPips | double | 2.0 |
| Show Early Arrow | ShowEarlyArrow | bool | true |

## 14 · DISPLAY — PANEL

| Label | Property | Type | Default |
|---|---|---|---|
| Show Panel Toggle Button | ShowPanelToggleButton | bool | true |
| Panel Toggle Width | PanelToggleWidth | int | 26 |
| Panel Toggle Height | PanelToggleHeight | int | 26 |

