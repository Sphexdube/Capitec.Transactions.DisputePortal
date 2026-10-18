INSERT INTO "tblTransaction" ("ExternalId", "CustomerId", "OccurredAt", "Merchant", "Description", "AmountMinor", "CurrencyCode", "State")
SELECT
    gen_random_uuid(),
    customer."Id",
    now() - make_interval(days => day_offset.value),
    (ARRAY['Example Market', 'Sample Fuel', 'Demo Pharmacy', 'Fictional Cafe'])[((day_offset.value - 1) % 4) + 1],
    'Synthetic card transaction',
    12500 + (day_offset.value * 925),
    'ZAR',
    'Completed'
FROM "tblCustomer" AS customer
CROSS JOIN generate_series(1, 20) AS day_offset(value)
WHERE customer."DisplayName" IN ('Alex Example', 'Sam Sample');
