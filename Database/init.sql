USE defaultdb;

CREATE TABLE IF NOT EXISTS Languages (
    Code VARCHAR(5) PRIMARY KEY,
    Name VARCHAR(50) NOT NULL
);

CREATE TABLE IF NOT EXISTS Profiles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    LanguageCode VARCHAR(5) NOT NULL,
    BusinessName VARCHAR(100) NOT NULL,
    CoreValues VARCHAR(200) NOT NULL,
    WhatsApp VARCHAR(50) NOT NULL,
    Services TEXT NOT NULL,
    FOREIGN KEY (LanguageCode) REFERENCES Languages (Code)
);

CREATE TABLE IF NOT EXISTS Usuarios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    GoogleId VARCHAR(255) NULL UNIQUE,
    Nombre VARCHAR(255) NOT NULL,
    Email VARCHAR(255) NOT NULL UNIQUE,
    FotoUrl VARCHAR(2048) NULL,
    FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS Taxistas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UsuarioId INT NOT NULL UNIQUE,
    NombreServicio VARCHAR(120) NOT NULL,
    NombreCompleto VARCHAR(120) NOT NULL DEFAULT '',
    Telefono VARCHAR(40) NOT NULL DEFAULT '',
    FotoUrl VARCHAR(2048) NULL,
    Vehiculo VARCHAR(120) NOT NULL DEFAULT '',
    Patente VARCHAR(30) NOT NULL DEFAULT '',
    DatosPago VARCHAR(250) NOT NULL DEFAULT '',
    Servicios TEXT NOT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Taxistas_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios (Id) ON DELETE CASCADE
);

INSERT IGNORE INTO
    Languages (Code, Name)
VALUES ('es', 'Español'),
    ('en', 'English'),
    ('pt', 'Português');

INSERT INTO
    Profiles (
        LanguageCode,
        BusinessName,
        CoreValues,
        WhatsApp,
        Services
    )
SELECT 'es', 'Taxi Alexis', 'Puntualidad, Seguridad y Confort en cada viaje', '1162383883', 'Viajes particulares, Aeropuertos, Traslados laborales, Viajes programados, Servicio para empresas, Servicios de Tour'
WHERE
    NOT EXISTS (
        SELECT 1
        FROM Profiles
        WHERE
            LanguageCode = 'es'
    )
UNION ALL
SELECT 'en', 'Taxi Alexis', 'Punctuality, Safety, and Comfort on every ride', '1162383883', 'Private trips, Airports, Work transfers, Scheduled rides, Corporate service, Tour services'
WHERE
    NOT EXISTS (
        SELECT 1
        FROM Profiles
        WHERE
            LanguageCode = 'en'
    )
UNION ALL
SELECT 'pt', 'Taxi Alexis', 'Pontualidade, Segurança e Conforto em cada viagem', '1162383883', 'Viagens particulares, Aeroportos, Traslados de trabalho, Viagens agendadas, Serviço para empresas, Serviços de tour'
WHERE
    NOT EXISTS (
        SELECT 1
        FROM Profiles
        WHERE
            LanguageCode = 'pt'
    );