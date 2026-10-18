INSERT INTO "tblTransaction" ("ExternalId", "CustomerId", "OccurredAt", "Merchant", "Description", "AmountMinor", "CurrencyCode", "State")
SELECT
    md5(customer."ExternalId"::text || ':history:' || history_day.days::text)::uuid,
    customer."Id",
    now() - make_interval(days => history_day.days),
    (ARRAY['Example Market', 'Sample Fuel', 'Demo Pharmacy', 'Fictional Cafe'])[((history_day.days - 1) % 4) + 1],
    'Synthetic card transaction',
    16700 + (history_day.days * 725),
    'ZAR',
    'Completed'
FROM "tblCustomer" AS customer
CROSS JOIN (VALUES (35), (65), (85)) AS history_day(days)
WHERE customer."DisplayName" IN ('Alex Example', 'Sam Sample')
    AND NOT EXISTS
    (
        SELECT 1
        FROM "tblTransaction" AS existing_transaction
        WHERE existing_transaction."ExternalId" = md5(
            customer."ExternalId"::text || ':history:' || history_day.days::text
        )::uuid
    );