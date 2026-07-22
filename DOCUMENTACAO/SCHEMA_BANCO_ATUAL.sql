-- ==================================================
-- SCHEMA DO BANCO DE DADOS ATUAL (SEM DADOS REAIS)
-- Gerado em: 2026-07-17 08:18:49
-- ==================================================

CREATE TABLE Pedidos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NumeroPedidoCliente TEXT NOT NULL UNIQUE,
    NomeCliente TEXT,
    Marketplace TEXT,
    CodigoEtiqueta TEXT,
    Status TEXT,
    JsonItens TEXT,
    EtiquetaMarketplaceZpl TEXT,
    Impresso INTEGER NOT NULL DEFAULT 0,
    Conferido INTEGER NOT NULL DEFAULT 0,
    DataCriacao TEXT,
    DataConferencia TEXT
, Oculto INTEGER NOT NULL DEFAULT 0, DataOcultacao TEXT, DataAtualizacao TEXT, DataPrevisao TEXT, CaminhoZipImportacao TEXT, NomePdfNoZip TEXT, PaginaPdf INTEGER);

CREATE TABLE Configuracoes (
    Chave TEXT PRIMARY KEY,
    Valor TEXT NOT NULL
);

CREATE TABLE AuditoriaAdministrativa (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Acao TEXT NOT NULL,
    NumeroPedidoCliente TEXT,
    DataAcao TEXT NOT NULL
);

CREATE INDEX idx_pedidos_numeropedidocliente ON Pedidos (NumeroPedidoCliente);

CREATE INDEX idx_pedidos_marketplace ON Pedidos (Marketplace);

CREATE INDEX idx_pedidos_codigoetiqueta ON Pedidos (CodigoEtiqueta);

CREATE INDEX idx_pedidos_impresso ON Pedidos (Impresso);

CREATE INDEX idx_pedidos_dataconferencia ON Pedidos (DataConferencia);

CREATE INDEX idx_pedidos_oculto ON Pedidos (Oculto);

CREATE INDEX idx_pedidos_dataprevisao ON Pedidos (DataPrevisao);

