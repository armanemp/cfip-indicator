// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89ExecutionIdentity
        {
            public string IntentId { get; private set; }
            public string IdempotencyKey { get; private set; }
            public DateTime CreatedUtc { get; private set; }
    
            public CFIPClean89ExecutionIdentity(
                string intentId,
                string idempotencyKey,
                DateTime createdUtc)
            {
                IntentId = intentId ?? string.Empty;
                IdempotencyKey = idempotencyKey ?? string.Empty;
                CreatedUtc = createdUtc;
    
                if (string.IsNullOrWhiteSpace(IdempotencyKey))
                    throw new ArgumentException(
                        "IdempotencyKey is mandatory.",
                        "idempotencyKey");
            }
        }
}
