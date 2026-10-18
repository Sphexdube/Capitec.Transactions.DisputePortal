CREATE TABLE "lupCurrency"
(
    "Code"              CHAR(3)  NOT NULL,
    "MinorUnitExponent" SMALLINT NOT NULL,

    CONSTRAINT "PK_Currency" PRIMARY KEY ("Code"),
    CONSTRAINT "CK_Currency_MinorUnitExponent" CHECK ("MinorUnitExponent" BETWEEN 0 AND 4)
);
