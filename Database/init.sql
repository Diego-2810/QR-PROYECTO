CREATE DATABASE IF NOT EXISTS QrProfileDB;
USE QrProfileDB;

CREATE TABLE IF NOT EXISTS Languages (
    Code VARCHAR(5) PRIMARY KEY,
    Name VARCHAR(50) NOT NULL
);

DROP TABLE IF EXISTS Profiles;

CREATE TABLE IF NOT EXISTS Profiles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    LanguageCode VARCHAR(5) NOT NULL,
    BusinessName VARCHAR(100) NOT NULL,
    CoreValues VARCHAR(200) NOT NULL,
    WhatsApp VARCHAR(50) NOT NULL,
    Services TEXT NOT NULL,
    FOREIGN KEY (LanguageCode) REFERENCES Languages(Code)
);

INSERT IGNORE INTO Languages (Code, Name) VALUES 
('es', 'Español'), 
('en', 'English'), 
('pt', 'Português');

INSERT INTO Profiles (LanguageCode, BusinessName, CoreValues, WhatsApp, Services) 
VALUES 
('es', 'Taxi Alexis', 'Puntualidad, Seguridad y Confort en cada viaje', '1162383883', 'Viajes particulares, Aeropuertos, Traslados laborales, Viajes programados, Servicio para empresas, Servicios de Tour'),
('en', 'Taxi Alexis', 'Punctuality, Safety, and Comfort on every ride', '1162383883', 'Private trips, Airports, Work transfers, Scheduled rides, Corporate service, Tour services'),
('pt', 'Taxi Alexis', 'Pontualidade, Segurança e Conforto em cada viagem', '1162383883', 'Viagens particulares, Aeroportos, Traslados de trabalho, Viagens agendadas, Serviço para empresas, Serviços de tour');
