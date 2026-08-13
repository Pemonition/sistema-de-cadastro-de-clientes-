-- Sistema de Cadastro de Clientes — Ardena Soluções
-- Script de criação do banco (SQLite). É o mesmo DDL que
-- DatabaseInitializer.Inicializar() executa automaticamente na primeira
-- vez que a aplicação roda; está aqui também como entregável isolado.

CREATE TABLE IF NOT EXISTS Clientes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome TEXT NOT NULL,
    Cpf TEXT NOT NULL UNIQUE,
    Cep TEXT NOT NULL,
    Logradouro TEXT NOT NULL,
    Numero TEXT NOT NULL,
    Complemento TEXT,
    Bairro TEXT NOT NULL,
    Cidade TEXT NOT NULL,
    Estado TEXT NOT NULL
);
