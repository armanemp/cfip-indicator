using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool RestrictionAlertEnabled(
                                    string reason)
                                {
                                    if (string.IsNullOrWhiteSpace(
                                            reason))
                                        return AlertOnEntryRestriction;
                        
                                    switch (reason)
                                    {
                                        case "NEWS BLACKOUT":
                                            return AlertOnNewsEventGuard ||
                                                   AlertOnNewsEvent ||
                                                   AlertOnEntryRestriction;
                        
                                        case "SESSION":
                                            return AlertOnSessionBlock ||
                                                   AlertOnEntryRestriction;
                        
                                        case "SPREAD":
                                            return AlertOnSpreadBlock ||
                                                   AlertOnEntryRestriction;
                        
                                        case "FRIDAY":
                                            return AlertOnFridayBlock ||
                                                   AlertOnEntryRestriction;
                        
                                        case "REGIME NO-TRADE":
                                        case "CHOP":
                                            return AlertOnRegimeNoTrade ||
                                                   AlertOnEntryRestriction;
                        
                                        case "COOLDOWN":
                                        case "DIRECTION FLIP":
                                            return AlertOnCooldownBlock ||
                                                   AlertOnEntryRestriction;
                        
                                        default:
                                            return AlertOnEntryRestriction;
                                    }
                                }
    }
}
