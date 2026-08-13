-- Sistema de Cadastro de Clientes — Ardena Soluções
-- Variante MySQL do schema (equivalente a database/schema.sql, que é a
-- versão SQLite). Rode isto num banco já criado, ex.:
--   CREATE DATABASE cadastro_clientes;
--   USE cadastro_clientes;
--   -- depois cole o CREATE TABLE abaixo (ou rode este arquivo inteiro no Workbench)

CREATE TABLE IF NOT EXISTS Clientes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(200) NOT NULL,
    Cpf VARCHAR(11) NOT NULL UNIQUE,
    Cep VARCHAR(8) NOT NULL,
    Logradouro VARCHAR(200) NOT NULL,
    Numero VARCHAR(20) NOT NULL,
    Complemento VARCHAR(100) NULL,
    Bairro VARCHAR(100) NOT NULL,
    Cidade VARCHAR(100) NOT NULL,
    Estado CHAR(2) NOT NULL
);
