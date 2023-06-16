-- Criação da tabela Categoria
CREATE TABLE Categoria (
  CategoriaId INT PRIMARY KEY AUTO_INCREMENT,
  Nome VARCHAR(100)
);

-- Criação da tabela Usuario
CREATE TABLE Usuario (
  UsuarioId INT PRIMARY KEY AUTO_INCREMENT,
  Nome VARCHAR(100),
  Email VARCHAR(100),
  Senha VARCHAR(100)
);

-- Criação da tabela Projeto
CREATE TABLE Projeto (
  ProjetoId INT PRIMARY KEY AUTO_INCREMENT,
  UsuarioRespId INT,
  Nome VARCHAR(100),
  Descricao VARCHAR(255),
  Status VARCHAR(50),
  CategoriaId INT,
  FOREIGN KEY (UsuarioRespId) REFERENCES Usuario(UsuarioId),
  FOREIGN KEY (CategoriaId) REFERENCES Categoria(CategoriaId)
);

-- Criação da tabela ProjetoUsuario
CREATE TABLE ProjetoUsuario (
  ProjetoId INT,
  UsuarioId INT,
  Cargo VARCHAR(100),
  FOREIGN KEY (ProjetoId) REFERENCES Projeto(ProjetoId),
  FOREIGN KEY (UsuarioId) REFERENCES Usuario(UsuarioId),
  PRIMARY KEY (ProjetoId, UsuarioId)
);

-- Criação da tabela Tarefa
CREATE TABLE Tarefa (
  TarefaId INT PRIMARY KEY AUTO_INCREMENT,
  ProjetoId INT,
  UsuarioId INT,
  Nome VARCHAR(100),
  Descricao VARCHAR(255),
  Status VARCHAR(50),
  CategoriaId INT,
  FOREIGN KEY (ProjetoId) REFERENCES Projeto(ProjetoId),
  FOREIGN KEY (UsuarioId) REFERENCES Usuario(UsuarioId),
  FOREIGN KEY (CategoriaId) REFERENCES Categoria(CategoriaId)
);
