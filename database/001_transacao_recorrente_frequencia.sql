-- Adiciona a frequencia (intervalo em meses) nas transacoes recorrentes.
-- Valores: 1 = Mensal, 2 = A cada 2 meses, 3 = A cada 3 meses, 4 = A cada 4 meses,
--          6 = A cada 6 meses, 12 = Anual (enum myfinance_web_dotnet_domain.Enums.Frequencia).
-- Registros existentes passam a ser mensais.

IF COL_LENGTH('TransacaoRecorrente', 'Frequencia') IS NULL
BEGIN
    ALTER TABLE TransacaoRecorrente
        ADD Frequencia INT NOT NULL
            CONSTRAINT DF_TransacaoRecorrente_Frequencia DEFAULT 1
            CONSTRAINT CK_TransacaoRecorrente_Frequencia CHECK (Frequencia IN (1, 2, 3, 4, 6, 12));
END
